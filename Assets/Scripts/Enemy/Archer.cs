using UnityEngine;

public class Archer : Enemy
{
    [Header("Archer Settings")]
    public float attackRange = 8f;
    public float idealRange = 5f;
    public float retreatRange = 3f; 
    public float arrowSpeed = 10f;
    public float retreatSpeed = 5f; 
    public float minDistanceFromEdge = 1.5f; 
    
    private ProjectileAttacker projectileAttacker;
    private bool isRetreating = false;
    private bool hasLineOfSight = false;
    private float lastLineOfSightCheckTime = 0f;
    private float lineOfSightCheckInterval = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool debugMovement = true;

    protected override void Start()
    {
        
        MaxHealth = 80; 
        Speed = 3f; 
        if (string.IsNullOrEmpty(Name))
            Name = "Archer";

        _maxJumpCount = 2;

        base.Start(); 

        
        projectileAttacker = GetComponent<ProjectileAttacker>();
        if (projectileAttacker == null)
        {
            projectileAttacker = gameObject.AddComponent<ProjectileAttacker>();
            projectileAttacker.projectilePrefab = Resources.Load<GameObject>("Arrow");
            if (projectileAttacker.projectilePrefab == null)
            {
                Debug.LogError("Arrow prefab not found in Resources! Please add it.");
                
                
                GameObject arrowInScene = GameObject.Find("Arrow");
                if (arrowInScene != null)
                {
                    projectileAttacker.projectilePrefab = arrowInScene;
                    Debug.Log("Found Arrow in scene as fallback");
                }
            }
            projectileAttacker.attackCooldown = 2f;
            projectileAttacker.attackRange = attackRange;
            Debug.Log("ProjectileAttacker component added to Archer");
        }
    }

    protected override void Update()
    {
        
        base.Update();

        
        if (!IsPlayerValid) return;
        
        
        if (Time.time > lastLineOfSightCheckTime + lineOfSightCheckInterval)
        {
            CheckLineOfSightToPlayer();
            lastLineOfSightCheckTime = Time.time;
        }

        if (debugMovement)
        {
            Debug.Log($"Distance to player: {currentPlayerDistance}, Detection radius: {detectionRadius}, Ideal range: {idealRange}");
        }

        
        if (currentPlayerDistance <= detectionRadius)
        {
            
            if (!hasLineOfSight)
            {
                FindPositionWithLineOfSight();
                return;
            }
            
            
            if (currentPlayerDistance < retreatRange)
            {
                isRetreating = true;
                RetreatFromPlayer();
            }
            
            else if (currentPlayerDistance < idealRange - 1f)
            {
                isRetreating = true;
                RetreatFromPlayer();
            }
            
            else if (currentPlayerDistance <= attackRange)
            {
                isRetreating = false;
                
                if (hasLineOfSight && projectileAttacker.CanAttack())
                {
                    Attack();
                }
                else
                {
                    
                    AdjustHorizontalPosition();
                }
            }
            
            else
            {
                isRetreating = false;
                MoveTowardsPlayer();
            }
        }
        else
        {
            isRetreating = false;
            
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        
        if (rb != null && !isGrounded && rb.linearVelocity.y > 0 && Time.time > lastJumpTime + 0.5f)
        {
            
            rb.AddForce(Vector2.down * 10f, ForceMode2D.Force);
        }
    }

    
    private void CheckLineOfSightToPlayer()
    {
        if (!IsPlayerValid) return;
        
        
        Vector2 dirToPlayer = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
        float distToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
        
        
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            dirToPlayer,
            distToPlayer,
            groundLayer); 
        
        
        hasLineOfSight = (hit.collider == null);
        
        if (debugMovement)
        {
            Debug.DrawRay(transform.position, dirToPlayer * distToPlayer, 
                         hasLineOfSight ? Color.green : Color.red, 0.1f);
            
            if (hit.collider != null)
            {
                Debug.Log($"Line of sight blocked by: {hit.collider.name} at distance {hit.distance}");
            }
        }
    }
    
    
    private void FindPositionWithLineOfSight()
    {
        if (!IsPlayerValid) return;
        
        
        Vector2 rightCheck = (Vector2)transform.position + new Vector2(1f, 0);
        Vector2 dirFromRight = ((Vector2)_playerTransform.position - rightCheck).normalized;
        
        RaycastHit2D hitRight = Physics2D.Raycast(
            rightCheck,
            dirFromRight,
            attackRange,
            groundLayer);
            
        bool rightHasLOS = (hitRight.collider == null);
        
        
        Vector2 leftCheck = (Vector2)transform.position + new Vector2(-1f, 0);
        Vector2 dirFromLeft = ((Vector2)_playerTransform.position - leftCheck).normalized;
        
        RaycastHit2D hitLeft = Physics2D.Raycast(
            leftCheck,
            dirFromLeft,
            attackRange,
            groundLayer);
            
        bool leftHasLOS = (hitLeft.collider == null);
        
        
        if (rightHasLOS && !leftHasLOS)
        {
            
            rb.linearVelocity = new Vector2(Speed, rb.linearVelocity.y);
            if (debugMovement) Debug.Log("Moving right to get line of sight");
        }
        else if (leftHasLOS && !rightHasLOS)
        {
            
            rb.linearVelocity = new Vector2(-Speed, rb.linearVelocity.y);
            if (debugMovement) Debug.Log("Moving left to get line of sight");
        }
        else if (!rightHasLOS && !leftHasLOS)
        {
            
            
            if (isGrounded && Time.time > lastJumpTime + jumpCooldown)
            {
                TryJump();
                if (debugMovement) Debug.Log("Jumping to try to get line of sight");
            }
            else
            {
                
                MoveTowardsPlayer();
            }
        }
        else
        {
            
            float rightDist = Vector2.Distance(rightCheck, (Vector2)_playerTransform.position);
            float leftDist = Vector2.Distance(leftCheck, (Vector2)_playerTransform.position);
            
            float rightDeviation = Mathf.Abs(rightDist - idealRange);
            float leftDeviation = Mathf.Abs(leftDist - idealRange);
            
            if (rightDeviation < leftDeviation)
            {
                rb.linearVelocity = new Vector2(Speed, rb.linearVelocity.y);
                if (debugMovement) Debug.Log("Moving right to get ideal range");
            }
            else
            {
                rb.linearVelocity = new Vector2(-Speed, rb.linearVelocity.y);
                if (debugMovement) Debug.Log("Moving left to get ideal range");
            }
        }
    }
    
    
    private void AdjustHorizontalPosition()
    {
        if (!IsPlayerValid || rb == null) return;
        
        
        float horizontalDistance = Mathf.Abs(_playerTransform.position.x - transform.position.x);
        
        
        if (Mathf.Abs(horizontalDistance - idealRange) < 0.5f && hasLineOfSight)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
        
        
        float directionX = (horizontalDistance < idealRange) ? 
            -Mathf.Sign(_playerTransform.position.x - transform.position.x) : 
            Mathf.Sign(_playerTransform.position.x - transform.position.x);
        
        
        bool canMoveInDirection = CheckIfCanMove(directionX);
        
        if (canMoveInDirection)
        {
            rb.linearVelocity = new Vector2(directionX * Speed, rb.linearVelocity.y);
            
            if (debugMovement)
            {
                Debug.Log($"Adjusting position, moving {(directionX > 0 ? "right" : "left")} to get ideal range");
            }
        }
        else
        {
            
            if (isGrounded && Time.time > lastJumpTime + jumpCooldown)
            {
                TryJump();
                rb.linearVelocity = new Vector2(directionX * Speed, rb.linearVelocity.y);
            }
            else
            {
                
                directionX *= -1;
                canMoveInDirection = CheckIfCanMove(directionX);
                
                if (canMoveInDirection)
                {
                    rb.linearVelocity = new Vector2(directionX * Speed, rb.linearVelocity.y);
                }
                else
                {
                    
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
            }
        }
    }
    
    
    private bool CheckIfCanMove(float direction)
    {
        if (rb == null) return false;
        
        
        Vector2 wallCheckPosition = (Vector2)transform.position + new Vector2(direction * 0.5f, 0);
            
        RaycastHit2D wallHit = Physics2D.Raycast(
            wallCheckPosition,
            new Vector2(direction, 0),
            1f,
            groundLayer);
            
        if (wallHit.collider != null)
        {
            if (debugMovement) Debug.Log($"Can't move {(direction > 0 ? "right" : "left")} - wall detected: {wallHit.collider.name}");
            return false;
        }
        
        
        if ((direction > 0 && _playerTransform.position.x < transform.position.x) ||
            (direction < 0 && _playerTransform.position.x > transform.position.x))
        {
            Vector2 ledgeCheckPosition = (Vector2)transform.position + new Vector2(direction * minDistanceFromEdge, -0.1f);
                
            RaycastHit2D groundHit = Physics2D.Raycast(
                ledgeCheckPosition,
                Vector2.down,
                1.5f,
                groundLayer);
                
            if (groundHit.collider == null)
            {
                if (debugMovement) Debug.Log($"Can't move {(direction > 0 ? "right" : "left")} - ledge detected");
                return false;
            }
        }
        
        return true;
    }

    
    private void MoveTowardsPlayer()
    {
        if (!IsPlayerValid || rb == null) return;

        Vector3 targetPos = _playerTransform.position;

        
        float directionX = Mathf.Sign(targetPos.x - transform.position.x);
        
        
        bool canMove = CheckIfCanMove(directionX);
        
        if (canMove)
        {
            
            rb.linearVelocity = new Vector2(directionX * Speed, rb.linearVelocity.y);
            
            
            isFacingRight = directionX > 0;
            UpdateFacing();
        }
        else
        {
            
            if (isGrounded && Time.time > lastJumpTime + jumpCooldown)
            {
                TryJump();
                rb.linearVelocity = new Vector2(directionX * Speed, rb.linearVelocity.y);
            }
            else
            {
                
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }

        if (debugMovement)
        {
            Debug.Log($"Moving towards player at {targetPos}, position: {transform.position}, direction: {directionX}");
            
            Debug.DrawLine(transform.position, new Vector3(transform.position.x + directionX * 2, transform.position.y, transform.position.z), Color.blue, 0.1f);
        }
    }

    
    private void RetreatFromPlayer()
    {
        if (!IsPlayerValid || rb == null) return;

        Vector3 targetPos = _playerTransform.position;

        
        float directionX = -Mathf.Sign(targetPos.x - transform.position.x);

        
        float currentRetreatSpeed = (currentPlayerDistance < retreatRange) ? retreatSpeed : Speed;

        
        bool canRetreat = CheckIfCanMove(directionX);

        if (canRetreat)
        {
            
            isFacingRight = targetPos.x > transform.position.x;
            UpdateFacing();

            
            
            rb.linearVelocity = new Vector2(directionX * currentRetreatSpeed, rb.linearVelocity.y);
        }
        else
        {
            
            if (isGrounded && Time.time > lastJumpTime + jumpCooldown)
            {
                TryJump();
                rb.linearVelocity = new Vector2(directionX * currentRetreatSpeed, rb.linearVelocity.y);
            }
            else
            {
                
                float strafeDir = (Random.value > 0.5f) ? 1f : -1f;
                if (CheckIfCanMove(strafeDir))
                {
                    rb.linearVelocity = new Vector2(strafeDir * Speed, rb.linearVelocity.y);
                    if (debugMovement) Debug.Log("Can't retreat, strafing sideways");
                }
                else
                {
                    
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
            }
        }

        if (debugMovement)
        {
            Debug.Log($"Backing away from player, velocity: {rb.linearVelocity}, isFacingRight: {isFacingRight}");
            
            Debug.DrawLine(transform.position, new Vector3(transform.position.x + directionX * 2, transform.position.y, transform.position.z), Color.red, 0.1f);
        }
    }

    
    public override void Attack()
    {
        
        if (!IsPlayerValid)
        {
            FindPlayer();
            if (!IsPlayerValid) return; 
        }
        
        
        CheckLineOfSightToPlayer();
        
        if (!hasLineOfSight)
        {
            if (debugMovement) Debug.Log("Aborted attack - no line of sight to player");
            return;
        }

        Vector3 targetPosition = _playerTransform.position;

        if (projectileAttacker != null)
        {
            
            UpdateFacing();
            
            
            projectileAttacker.ShootProjectile(_playerTransform, "arrow");
            Debug.Log($"Archer shot arrow at {targetPosition}");
        }
    }

    
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (isRetreating && collision.gameObject.layer == gameObject.layer)
        {
            if (isGrounded && Time.time > lastJumpTime + jumpCooldown)
            {
                TryJump();
                if (debugMovement) Debug.Log("Jumping to avoid collision while retreating");
            }
        }
    }
    
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, idealRange);
        
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, retreatRange);
    }
}