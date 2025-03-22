using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    public string Name;
    public int Health;
    public int MaxHealth = 100;
    public float Speed = 5f;
    public float detectionRadius = 10f;

    [Header("Movement Settings")]
    public float jumpForce = 12f;
    public float jumpCooldown = 0.5f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;
    public float groundCheckDistance = 0.3f;
    public float wallCheckDistance = 0.5f;
    public float ledgeCheckDistance = 1f;
    public float enemyCheckDistance = 1.5f;

    [Header("Debug")]
    [SerializeField] protected bool showDebugLogs = true;

    public event System.Action OnDeath;
    public delegate void HealthChangeHandler(int currentHealth, int maxHealth);
    public event HealthChangeHandler OnHealthChanged;

    protected Transform _playerTransform;
    protected GameObject _playerObject;
    protected PlayerController _playerController;
    protected Vector3 lastKnownPlayerPosition = Vector3.zero;
    protected float lastPlayerCheckTime = 0f;
    protected float playerCheckInterval = 0.05f;

    protected Animator animator;
    protected Rigidbody2D rb;

    protected bool isGrounded;
    protected bool wallAhead;
    protected bool ledgeAhead;
    protected bool enemyAhead;
    protected bool isFacingRight = true;
    protected bool isCircumnavigating = false;
    protected float circumnavigationDirection = 1f;

    protected float lastJumpTime = 0f;
    protected float lastPathChangeTime = 0f;
    protected float pathChangeCooldown = 0.5f;

    protected bool IsPlayerValid => _playerObject != null && _playerTransform != null;

    protected float currentPlayerDistance = 0f;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true);
        }

        if (obstacleLayer == 0)
        {
            obstacleLayer = LayerMask.GetMask("Enemy");
        }
    }

    protected virtual void Start()
    {
        FindPlayer();

        Health = MaxHealth;
        OnHealthChanged?.Invoke(Health, MaxHealth);

        if (showDebugLogs)
            Debug.Log($"{Name} initialized with {Health} health");
    }

    protected virtual void EnforceGravity()
    {
        if (rb != null && !isGrounded && rb.linearVelocity.y > 0 && Time.time > lastJumpTime + 0.5f)
        {
            rb.AddForce(Vector2.down * 10f, ForceMode2D.Force);
        }
    }

    protected virtual void Update()
    {
        if (Time.time > lastPlayerCheckTime + playerCheckInterval)
        {
            if (!IsPlayerValid)
            {
                FindPlayer();
            }
            else
            {
                lastKnownPlayerPosition = _playerTransform.position;
                currentPlayerDistance = Vector3.Distance(transform.position, _playerTransform.position);
            }

            lastPlayerCheckTime = Time.time;
        }

        if (IsPlayerValid && currentPlayerDistance <= detectionRadius)
        {
            UpdateFacing();
            MoveTowardsTarget(_playerTransform.position);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (rb != null)
        {
            CheckSurroundings();

            if (isGrounded)
            {
                _jumpCount = 0;
            }
            EnforceGravity();
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        Health -= amount;

        if (showDebugLogs)
            Debug.Log($"{Name} took {amount} damage. Health: {Health}/{MaxHealth}");

        OnHealthChanged?.Invoke(Health, MaxHealth);

        if (Health <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(Mathf.RoundToInt(damage));
    }

    protected virtual void Die()
    {
        if (showDebugLogs)
            Debug.Log($"{Name} has been defeated!");

        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    protected void FindPlayer()
    {
        _playerObject = null;
        _playerTransform = null;
        _playerController = null;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            _playerObject = playerObject;
            _playerTransform = playerObject.transform;
            _playerController = playerObject.GetComponent<PlayerController>();

            if (_playerTransform != null)
            {
                lastKnownPlayerPosition = _playerTransform.position;

                if (showDebugLogs)
                    Debug.Log($"{Name} found player: {_playerObject.name} at position {lastKnownPlayerPosition}");
            }
        }
        else
        {
            Debug.LogWarning($"{Name} couldn't find player! Retrying next update...");
        }
    }

    public Vector3 GetPlayerPosition()
    {
        if (!IsPlayerValid)
        {
            FindPlayer();
        }

        return IsPlayerValid ? _playerTransform.position : lastKnownPlayerPosition;
    }

    public bool IsPlayerInRange(float range)
    {
        if (!IsPlayerValid) return false;

        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        return distance <= range;
    }

    public virtual void MoveTowardsTarget(Vector3 targetPosition)
    {
        if (rb != null)
        {
            float directionX = Mathf.Sign(targetPosition.x - transform.position.x);
            rb.linearVelocity = new Vector2(directionX * Speed, rb.linearVelocity.y);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
        }
    }

    protected virtual void UpdateFacing()
    {
        if (!IsPlayerValid) return;

        Vector3 playerPos = _playerTransform.position;

        bool shouldFaceRight = playerPos.x > transform.position.x;

        if ((shouldFaceRight && !isFacingRight) || (!shouldFaceRight && isFacingRight))
        {
            isFacingRight = shouldFaceRight;

            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;

            if (showDebugLogs)
                Debug.Log($"{Name} flipped to face {(shouldFaceRight ? "right" : "left")}");
        }
    }

    protected virtual void CheckSurroundings()
    {
        float facingDirection = isFacingRight ? 1f : -1f;

        isGrounded = Physics2D.BoxCast(
            transform.position,
            new Vector2(0.8f, 0.1f),
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer);

        Vector2 wallCheckPos = new Vector2(transform.position.x + facingDirection * 0.5f, transform.position.y);

        wallAhead = Physics2D.BoxCast(
            wallCheckPos,
            new Vector2(0.1f, 0.8f),
            0f,
            new Vector2(facingDirection, 0),
            wallCheckDistance,
            groundLayer);

        if (wallAhead)
        {
            RaycastHit2D wallHit = Physics2D.Raycast(
                wallCheckPos,
                new Vector2(facingDirection, 0),
                wallCheckDistance,
                groundLayer);

            if (wallHit.collider != null && wallHit.collider.CompareTag("Platform"))
            {
                wallAhead = false;
            }
        }

        Vector2 ledgeCheckPosition = new Vector2(
            transform.position.x + facingDirection * ledgeCheckDistance,
            transform.position.y);
        ledgeAhead = !Physics2D.Raycast(ledgeCheckPosition, Vector2.down, 1.5f, groundLayer);

        if (!ledgeAhead)
        {
            Vector2 furtherLedgeCheck = new Vector2(
                transform.position.x + facingDirection * (ledgeCheckDistance + 0.5f),
                transform.position.y);
            ledgeAhead = !Physics2D.Raycast(furtherLedgeCheck, Vector2.down, 1.5f, groundLayer);
        }

        Vector2 enemyCheckPosition = new Vector2(
            transform.position.x + facingDirection * 0.5f,
            transform.position.y);
        enemyAhead = Physics2D.BoxCast(
            enemyCheckPosition,
            new Vector2(0.1f, 0.8f),
            0f,
            new Vector2(facingDirection, 0),
            enemyCheckDistance,
            obstacleLayer);

        if (showDebugLogs)
        {
            Debug.DrawRay(transform.position + new Vector3(-0.4f, 0, 0), Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
            Debug.DrawRay(transform.position + new Vector3(0.4f, 0, 0), Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

            Debug.DrawRay(transform.position + new Vector3(facingDirection * 0.5f, -0.4f, 0),
                          new Vector2(facingDirection, 0) * wallCheckDistance, wallAhead ? Color.red : Color.green);
            Debug.DrawRay(transform.position + new Vector3(facingDirection * 0.5f, 0.4f, 0),
                          new Vector2(facingDirection, 0) * wallCheckDistance, wallAhead ? Color.red : Color.green);

            Debug.DrawRay(ledgeCheckPosition, Vector2.down * 1.5f, ledgeAhead ? Color.red : Color.green);

            Vector3 enemyCheckPos3D = new Vector3(enemyCheckPosition.x, enemyCheckPosition.y, 0);
            Debug.DrawRay(enemyCheckPos3D + new Vector3(0, -0.4f, 0),
                         new Vector2(facingDirection, 0) * enemyCheckDistance, enemyAhead ? Color.magenta : Color.cyan);
            Debug.DrawRay(enemyCheckPos3D + new Vector3(0, 0.4f, 0),
                         new Vector2(facingDirection, 0) * enemyCheckDistance, enemyAhead ? Color.magenta : Color.cyan);
        }
    }

    protected int _jumpCount = 0;
    protected int _maxJumpCount = 2;

    protected virtual bool TryJump()
    {
        if (rb == null) return false;

        bool canJump = isGrounded || (_jumpCount < _maxJumpCount);

        if (Time.time <= lastJumpTime + jumpCooldown) return false;

        if (canJump)
        {
            float currentXVelocity = rb.linearVelocity.x;

            rb.linearVelocity = new Vector2(currentXVelocity, 0);

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }

            lastJumpTime = Time.time;

            if (!isGrounded)
            {
                _jumpCount++;
            }

            if (showDebugLogs)
            {
                Debug.Log($"{Name} jumped while moving at x velocity: {currentXVelocity}, jump count: {_jumpCount}/{_maxJumpCount}");
            }

            return true;
        }

        return false;
    }

    protected virtual void NavigateAroundObstacle()
    {
        if (rb == null) return;

        rb.linearVelocity = new Vector2(0, circumnavigationDirection * Speed);

        if (Mathf.Abs(rb.linearVelocity.y) > 0 && Time.time > lastPathChangeTime + 0.5f)
        {
            CheckSurroundings();

            if (!wallAhead && !enemyAhead)
            {
                isCircumnavigating = false;
                lastPathChangeTime = Time.time;

                if (showDebugLogs)
                {
                    Debug.Log($"Successfully navigated around obstacle, resuming chase");
                }
            }
        }

        if (Time.time > lastPathChangeTime + 1.5f)
        {
            circumnavigationDirection *= -1;
            lastPathChangeTime = Time.time;

            if (showDebugLogs)
            {
                Debug.Log($"Changing circumnavigation direction to: {(circumnavigationDirection > 0 ? "up" : "down")}");
            }
        }

        if (Time.time > lastPathChangeTime + 3f)
        {
            if (isGrounded)
            {
                float facingDirection = isFacingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(facingDirection * Speed * 0.5f, rb.linearVelocity.y);

                TryJump();
                isCircumnavigating = false;

                if (showDebugLogs)
                {
                    Debug.Log($"Giving up on circumnavigation, trying to jump instead");
                }
            }
        }
    }

    public virtual void Attack()
    {
        if (showDebugLogs)
            Debug.Log($"{Name} tried to attack but has no attack implementation");
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isCircumnavigating)
        {
            isCircumnavigating = true;
            circumnavigationDirection = Random.value > 0.5f ? 1f : -1f;
            lastPathChangeTime = Time.time;

            if (showDebugLogs)
            {
                Debug.Log($"Collided with entity, starting circumnavigation with direction: {(circumnavigationDirection > 0 ? "up" : "down")}");
            }
        }
    }
}