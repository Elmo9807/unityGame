using System;
using UnityEngine;
using System.Collections;
using FMOD.Studio;


public class Grunt : Enemy
{
    [Header("Grunt Stats")]
    public int attackDamage = 15;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float preferredDistance = 1.5f;

    [Header("Movement")]
    public float chargeSpeed = 8f;
    public float obstacleJumpForce = 8f;

    
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    
    private bool isJumping = false;
    private bool isCharging = false;
    private float navigationCheckCooldown = 0.5f;
    private float lastNavigationCheck = 0f;

    // Audio
    private EventInstance GruntFootstep;

    protected override void Start()
    {
        
        MaxHealth = 100;
        Speed = 4f;
        Name = "Grunt";
        base.Start();
    }

    protected override void Update()
    {
        
        base.Update();

        
        if (IsPlayerValid && currentPlayerDistance <= detectionRadius)
        {
            
            if (isAttacking) return;

            
            if (currentPlayerDistance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                if (showDebugLogs)
                    Debug.Log($"{Name} is in attack range, preparing to attack");

                
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }

                Attack();
                return;
            }

            
            if (currentPlayerDistance <= attackRange)
            {
                
                isFacingRight = _playerTransform.position.x > transform.position.x;
                UpdateFacing();

                
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
                return;
            }

            
            if (Time.time >= lastNavigationCheck + navigationCheckCooldown)
            {
                CheckObstacles();
                lastNavigationCheck = Time.time;
            }

            
            MoveTowardsPlayer();
        }
    }

    protected override void FixedUpdate()
    {
        animator.SetFloat("xVelocity", Math.Abs(rb.linearVelocity.x)); // speed of running animation is based off of enemy's x velocity
        base.FixedUpdate();
    }

    private void MoveTowardsPlayer()
    {
        if (!IsPlayerValid || rb == null) return;

        
        float directionToPlayer = Mathf.Sign(_playerTransform.position.x - transform.position.x);

        
        isFacingRight = directionToPlayer > 0;
        UpdateFacing();

        
        float currentSpeed = isCharging ? chargeSpeed : Speed;

        
        if (currentPlayerDistance <= attackRange * 1.5f)
        {
            currentSpeed *= 0.6f;
        }

        
        rb.linearVelocity = new Vector2(directionToPlayer * currentSpeed, rb.linearVelocity.y);
    }

    private void CheckObstacles()
    {
        if (!isGrounded || rb == null) return;

        
        if (wallAhead)
        {
            if (showDebugLogs)
                Debug.Log($"{Name} detected wall, attempting to jump");

            StartCoroutine(JumpOverObstacle());
            return;
        }

        
        if (ledgeAhead)
        {
            
            bool playerIsAcross = (isFacingRight && _playerTransform.position.x > transform.position.x) ||
                                (!isFacingRight && _playerTransform.position.x < transform.position.x);

            
            if (playerIsAcross)
            {
                if (showDebugLogs)
                    Debug.Log($"{Name} detected ledge with player across, attempting to jump");

                StartCoroutine(JumpOverLedge());
                return;
            }
            else
            {
                
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    private IEnumerator JumpOverObstacle()
    {
        if (isJumping || !isGrounded) yield break;

        isJumping = true;

        
        float jumpDirection = isFacingRight ? 1f : -1f;

        
        rb.linearVelocity = new Vector2(jumpDirection * Speed * 1.5f, rb.linearVelocity.y);

        
        yield return new WaitForSeconds(0.05f);

        
        bool jumped = TryJump();

        if (jumped && showDebugLogs)
            Debug.Log($"{Name} jumped over obstacle");

        
        yield return new WaitForSeconds(0.5f);
        isJumping = false;
    }

    private IEnumerator JumpOverLedge()
    {
        if (isJumping || !isGrounded) yield break;

        isJumping = true;
        isCharging = true;

        
        float jumpDirection = isFacingRight ? 1f : -1f;

        
        rb.linearVelocity = new Vector2(jumpDirection * chargeSpeed, rb.linearVelocity.y);

        
        yield return new WaitForSeconds(0.1f);

        
        if (TryJump())
        {
            
            rb.AddForce(new Vector2(jumpDirection * 5f, 0), ForceMode2D.Impulse);

            if (showDebugLogs)
                Debug.Log($"{Name} jumped over ledge with extra momentum");
        }

        
        yield return new WaitForSeconds(0.5f);
        isJumping = false;

        yield return new WaitForSeconds(0.5f);
        isCharging = false;
    }

    public override void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        AudioManager.instance.PlayOneShot(FMODEvents.instance.GruntAttack, this.transform.position);

        if (showDebugLogs)
            Debug.Log($"{Name} is attacking player");

        
        if (IsPlayerValid && currentPlayerDistance <= attackRange)
        {
            
            if (_playerController != null)
            {
                _playerController.TakeDamage(attackDamage);
            }
            else
            {
                
                IDamageable playerDamageable = _playerObject.GetComponent<IDamageable>();
                if (playerDamageable != null)
                {
                    playerDamageable.TakeDamage(attackDamage);
                }
                else
                {
                    
                    HealthTracker playerHealth = _playerObject.GetComponent<HealthTracker>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);
                    }
                }
            }

            
            Rigidbody2D playerRb = _playerObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                float knockbackDirection = isFacingRight ? 1f : -1f;
                playerRb.AddForce(new Vector2(knockbackDirection, 0.2f) * 5f, ForceMode2D.Impulse);
            }
        }

        
        StartCoroutine(ResetAttackState(0.5f));
    }

    private IEnumerator ResetAttackState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
    }


    private void CallFootsteps() // This is called on certain frames of the walk animation. 
    {
        // Check if grounded
        if (Math.Abs(rb.linearVelocity.y) < 0.1f)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.GruntFootstep, transform.position);
        }
    }

    //void OnGUI()
    //{
    //    GUILayout.Label("Grunt stats");
    //    GUILayout.Label($"Grounded: {isGrounded}");
    //    GUILayout.Label($"Velocity: {rb.linearVelocity.x}");
    //    GUILayout.Label($"Anim XVel: {animator.GetFloat("xVelocity")}");
    //}

    protected override void OnDrawGizmosSelected()
    {
        
        base.OnDrawGizmosSelected();

        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}