using UnityEngine;
using System.Collections;

public class Grunt : Enemy
{
    [Header("Grunt Attack Settings")]
    public float stabRange = 2f;
    public int stabDamage = 10;
    public float stabCooldown = 1f;
    public GameObject stabEffectPrefab;
    public float knockbackForce = 5f;
    public float preferredAttackDistance = 1.5f;

    [Header("Improved Navigation")]
    public float verticalCheckDistance = 5f;      // How far to check above for obstacles
    public float sidePathDistance = 5f;           // How far to check to sides for clear paths
    public float verticalAlignmentThreshold = 1f; // When grunt is considered "directly" below player 
    public float horizontalClearance = 2f;        // Distance to move horizontally to clear obstacle
    public bool showNavigationGizmos = true;
    public float pathStabilityTime = 1.5f;        // Minimum time to follow a path before reconsidering
    public float stuckThreshold = 0.5f;           // Distance moved to be considered "stuck"

    // State variables
    private float _lastStabTime = 0f;
    private Vector3 _currentPathTarget;
    private bool _hasPathTarget = false;
    private float _pathChoiceTimestamp = 0f;
    private Vector3 _lastPositionCheck;
    private float _stuckCheckTimestamp;
    private bool _movingRight = false;
    private bool _isAttacking = false;
    private bool _isJumping = false;
    private int _consecutiveJumpAttempts = 0;
    private bool _hasReachedPlayer = false;
    private float _timeAtCurrentPosition = 0f;

    // Path finding variables
    private enum PathFindingState { Direct, Circumnavigate, Jump }
    private PathFindingState _currentPathState = PathFindingState.Direct;

    protected override void Start()
    {
        // Set Grunt-specific properties
        MaxHealth = 120;
        Speed = 5f;
        Name = "Grunt";

        base.Start();

        // Initialize tracking variables
        _lastPositionCheck = transform.position;
        _stuckCheckTimestamp = Time.time;
    }

    protected override void Update()
    {
        // Call base update to handle player tracking
        base.Update();

        // Only proceed if player is valid
        if (!IsPlayerValid) return;

        // If we're in the middle of an attack animation, don't move
        if (_isAttacking) return;

        // If we're in the middle of a jump, don't change direction
        if (_isJumping && !isGrounded) return;

        // Update grounded state if we were jumping
        if (_isJumping && isGrounded)
        {
            _isJumping = false;
            _consecutiveJumpAttempts = 0;
        }

        // Specific Grunt behavior based on distance to player
        if (currentPlayerDistance <= detectionRadius)
        {
            // If in stab range and grounded, try to attack
            if (currentPlayerDistance <= stabRange && isGrounded)
            {
                TryStab();
                // Stop moving when in attack range
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
                _hasReachedPlayer = true;
            }
            else
            {
                // Reset flag if we're no longer in range
                if (_hasReachedPlayer && currentPlayerDistance > stabRange * 1.5f)
                {
                    _hasReachedPlayer = false;
                }

                // Otherwise chase player
                ChasePlayer();
            }

            // Check if stuck in one place - only if not already at player
            if (!_hasReachedPlayer)
            {
                CheckIfStuck();
            }
        }
        else
        {
            // Reset navigation when player is out of range
            _hasPathTarget = false;
            _hasReachedPlayer = false;
            _currentPathState = PathFindingState.Direct;

            // Stop moving if player is out of detection range
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // Ensure Grunt doesn't float upward (unless jumping)
        if (rb != null && !isGrounded && rb.linearVelocity.y > 0 && Time.time > lastJumpTime + 0.5f)
        {
            // If we're rising but not from a recent jump, apply downward force
            rb.AddForce(Vector2.down * 12f, ForceMode2D.Force);
        }
    }

    private void CheckIfStuck()
    {

        if (Time.time - _stuckCheckTimestamp < 2f) return;

        float distanceMoved = Vector3.Distance(transform.position, _lastPositionCheck);


        if (distanceMoved < stuckThreshold)
        {
            _timeAtCurrentPosition += 2f;

            if (_timeAtCurrentPosition > 6f)
            {

                _currentPathState = PathFindingState.Jump;
                TryJump();

 
                if (rb != null)
                {
                    float newDir = _movingRight ? -1f : 1f;
                    rb.linearVelocity = new Vector2(newDir * Speed, rb.linearVelocity.y);
                    _movingRight = !_movingRight;

                    Debug.Log("Grunt has been stuck for a while - changing direction and jumping");
                }

                _hasPathTarget = false;
                _currentPathState = PathFindingState.Direct;
                _timeAtCurrentPosition = 0f;
            }
            else if (_timeAtCurrentPosition > 3f)
            {

                if (isGrounded)
                {
                    _currentPathState = PathFindingState.Jump;
                    TryJump();
                    Debug.Log("Grunt appears stuck for 3+ seconds - trying jump");
                }
            }
            else
            {

                if (_hasPathTarget)
                {
                    PickNewPathDirection();
                    Debug.Log("Grunt appears stuck - choosing a new path direction");
                }
                else
                {
                    // Try to find a path
                    _currentPathState = PathFindingState.Circumnavigate;
                    _hasPathTarget = false;
                }
            }
        }
        else
        {
            // We moved enough, reset stuck timer
            _timeAtCurrentPosition = 0f;
        }

        // Update reference position and time
        _lastPositionCheck = transform.position;
        _stuckCheckTimestamp = Time.time;
    }

    private void ChasePlayer()
    {
        if (rb == null || !IsPlayerValid) return;

        // Update facing direction (always face toward player)
        isFacingRight = _playerTransform.position.x > transform.position.x;
        UpdateFacing();

        // State machine for path finding
        switch (_currentPathState)
        {
            case PathFindingState.Direct:
                DirectApproach();
                break;
            case PathFindingState.Circumnavigate:
                NavigateAroundObstacle();
                break;
            case PathFindingState.Jump:
                AttemptJumpOver();
                break;
        }
    }

    private void DirectApproach()
    {
        // Check if player is directly above us with an obstacle in between
        if (IsPlayerDirectlyAbove() && IsVerticalObstacleAbove())
        {
            // Need to navigate around - switch to circumnavigation
            _currentPathState = PathFindingState.Circumnavigate;
            _hasPathTarget = false;
            return;
        }

        // If we have an existing path target, follow it
        if (_hasPathTarget)
        {
            float distanceToTarget = Vector2.Distance(transform.position, _currentPathTarget);

            // If we've reached the target or should pick a new target
            if (distanceToTarget < 0.5f || Time.time - _pathChoiceTimestamp > pathStabilityTime)
            {
                _hasPathTarget = false;
            }
            else
            {
                // Continue moving toward the target
                float directionToTarget = Mathf.Sign(_currentPathTarget.x - transform.position.x);
                rb.linearVelocity = new Vector2(directionToTarget * Speed, rb.linearVelocity.y);

                // Toggle moving right flag based on direction
                _movingRight = directionToTarget > 0;

                // Draw debug line to target
                if (showNavigationGizmos)
                {
                    Debug.DrawLine(transform.position, _currentPathTarget, Color.yellow);
                }

                return;
            }
        }

        // Calculate the optimal attack position
        Vector3 attackPos = CalculateAttackPosition();

        // Get direction to attack position
        float directionToAttackPos = Mathf.Sign(attackPos.x - transform.position.x);

        // Check for obstacles in our path
        if ((wallAhead || enemyAhead) && isGrounded)
        {
            // First attempt a jump if possible
            _currentPathState = PathFindingState.Jump;
            return;
        }
        else if (ledgeAhead)
        {
            // When approaching a ledge, be more cautious
            float distanceToLedge = 0f;

            // Cast ray forward to find exactly where the ledge is
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                transform.position,
                new Vector2(directionToAttackPos, -0.1f),
                5f,
                groundLayer);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    distanceToLedge = hit.distance;
                    break;
                }
            }

            if (distanceToLedge <= 0.5f) // Very close to ledge
            {
                // Check if we can see player and they're close and at similar height
                if (currentPlayerDistance < 5f &&
                    Mathf.Abs(_playerTransform.position.y - transform.position.y) < 3f)
                {
                    // Player is close and at similar height - try jumping across
                    _currentPathState = PathFindingState.Jump;
                    return;
                }
                else
                {
                    // Stop at the ledge if it's unsafe to jump
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                    // Switch to circumnavigation to find an alternate path
                    _currentPathState = PathFindingState.Circumnavigate;
                    _hasPathTarget = false;
                    return;
                }
            }
            else
            {
                // Approach ledge cautiously if not too close yet
                rb.linearVelocity = new Vector2(directionToAttackPos * Speed * 0.5f, rb.linearVelocity.y);
                return;
            }
        }

        // Normal movement - approach the calculated attack position
        float distanceToAttackPos = Mathf.Abs(transform.position.x - attackPos.x);

        // If we're close enough to the attack position, slow down
        if (distanceToAttackPos < 0.5f)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            // Move toward attack position at full speed
            rb.linearVelocity = new Vector2(directionToAttackPos * Speed, rb.linearVelocity.y);
            _movingRight = directionToAttackPos > 0;
        }
    }

    private void AttemptJumpOver()
    {
        if (!isGrounded)
        {
            // If we're in the air, maintain horizontal velocity toward player
            float dirToPlayer = Mathf.Sign(_playerTransform.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(dirToPlayer * Speed, rb.linearVelocity.y);
            return;
        }

        // If we're grounded and in jump state, try to jump
        if (Time.time > lastJumpTime + jumpCooldown)
        {
            // Jump in the direction of the player
            float jumpDirX = Mathf.Sign(_playerTransform.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(jumpDirX * Speed, rb.linearVelocity.y);

            bool jumped = TryJump();
            _isJumping = jumped;

            if (jumped)
            {
                _consecutiveJumpAttempts++;

                // After jump, we'll try direct approach again
                if (_consecutiveJumpAttempts >= 2)
                {
                    // If we've tried jumping twice, switch to circumnavigation
                    _currentPathState = PathFindingState.Circumnavigate;
                    _consecutiveJumpAttempts = 0;
                }
                else
                {
                    // After a single jump, go back to direct approach
                    _currentPathState = PathFindingState.Direct;
                }
            }
            else
            {
                // Couldn't jump - try circumnavigation instead
                _currentPathState = PathFindingState.Circumnavigate;
            }
        }
        else
        {
            // Jump is on cooldown - move toward player while waiting
            float dirToPlayer = Mathf.Sign(_playerTransform.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(dirToPlayer * Speed * 0.5f, rb.linearVelocity.y);
        }
    }

    // Check if player is directly above (or nearly so)
    private bool IsPlayerDirectlyAbove()
    {
        if (!IsPlayerValid) return false;

        // Check horizontal alignment
        float horizontalDistance = Mathf.Abs(_playerTransform.position.x - transform.position.x);

        // Check if player is above
        bool playerIsAbove = _playerTransform.position.y > transform.position.y;

        return horizontalDistance < verticalAlignmentThreshold && playerIsAbove;
    }

    // Check if there's an obstacle directly above us
    private bool IsVerticalObstacleAbove()
    {
        if (!IsPlayerValid) return false;

        // Cast ray upward toward player
        Vector2 playerPos2D = new Vector2(_playerTransform.position.x, _playerTransform.position.y);
        Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 dirToPlayer = (playerPos2D - pos2D).normalized;

        RaycastHit2D hit = Physics2D.Raycast(
            pos2D,
            dirToPlayer,
            verticalCheckDistance,
            groundLayer
        );

        // If there's an obstacle between us and the player
        if (hit.collider != null && hit.distance < currentPlayerDistance)
        {
            // Draw debug line to the hit point
            if (showNavigationGizmos)
            {
                Debug.DrawRay(transform.position, new Vector3(dirToPlayer.x, dirToPlayer.y, 0) * hit.distance, Color.red);
            }

            return true;
        }

        // No obstacle found
        return false;
    }

    // Navigate around a vertical obstacle by moving horizontally first
    protected override void NavigateAroundObstacle()
    {
        if (rb == null) return;

        // Instead of setting vertical velocity directly, try jumping or strafing
        if (isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            // First, give a bit of horizontal velocity in the direction we're facing
            float facingDirection = isFacingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(facingDirection * Speed * 0.5f, rb.linearVelocity.y);

            TryJump();
            isCircumnavigating = false;

            if (showDebugLogs)
            {
                Debug.Log($"Trying to jump over obstacle");
            }
        }
        else
        {
            // If we can't jump, try strafing horizontally instead
            float strafeDir = (circumnavigationDirection > 0) ? 1f : -1f;
            rb.linearVelocity = new Vector2(strafeDir * Speed * 0.5f, rb.linearVelocity.y);

            // If still stuck after a while, try the other direction
            if (Time.time > lastPathChangeTime + 1.5f)
            {
                circumnavigationDirection *= -1; // Reverse direction
                lastPathChangeTime = Time.time;

                if (showDebugLogs)
                {
                    Debug.Log($"Changing circumnavigation direction");
                }
            }
        }
    }
    private void PickNewPathDirection()
    {
        // Determine which direction to move to go around obstacle
        // First, check both left and right to see which path is clearer
        bool rightPathClear = CheckHorizontalPathClear(true);
        bool leftPathClear = CheckHorizontalPathClear(false);

        // If one path is clear and the other isn't, choose the clear one
        if (rightPathClear && !leftPathClear)
        {
            _movingRight = true;
        }
        else if (leftPathClear && !rightPathClear)
        {
            _movingRight = false;
        }
        else if (!rightPathClear && !leftPathClear)
        {
            // If both paths are blocked, just pick randomly
            _movingRight = Random.value > 0.5f;
        }
        else
        {
            // If both paths are clear, prefer the direction of the player
            _movingRight = _playerTransform.position.x > transform.position.x;
        }

        // Calculate target position that should clear the obstacle
        float targetX = transform.position.x + (_movingRight ? horizontalClearance : -horizontalClearance);

        // Set the target path position at the same height as the grunt
        _currentPathTarget = new Vector3(targetX, transform.position.y, transform.position.z);
        _hasPathTarget = true;
        _pathChoiceTimestamp = Time.time;

        Debug.Log($"Grunt choosing {(_movingRight ? "right" : "left")} path to avoid obstacle");
    }

    // Check if there's a clear path horizontally
    private bool CheckHorizontalPathClear(bool checkRight)
    {
        Vector2 checkDirection = checkRight ? Vector2.right : Vector2.left;

        // Cast ray horizontally to check for obstacles
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            checkDirection,
            horizontalClearance,
            groundLayer
        );

        // Draw debug ray
        if (showNavigationGizmos)
        {
            Color rayColor = hit.collider == null ? Color.green : Color.red;
            Debug.DrawRay(transform.position, checkDirection * horizontalClearance, rayColor, 0.2f);
        }

        // Check if we hit something
        if (hit.collider != null)
        {
            return false; // Path is blocked
        }

        // Also check if there would be ground beneath us along this path
        for (float i = 1f; i <= horizontalClearance; i += 1f)
        {
            Vector2 groundCheckPos = (Vector2)transform.position + checkDirection * i;

            // Cast ray downward to check for ground
            RaycastHit2D groundHit = Physics2D.Raycast(
                groundCheckPos + Vector2.up * 0.2f,
                Vector2.down,
                1f,
                groundLayer
            );

            // Draw debug ray
            if (showNavigationGizmos)
            {
                Color rayColor = groundHit.collider != null ? Color.green : Color.red;
                Debug.DrawRay(groundCheckPos + Vector2.up * 0.2f, Vector2.down * 1f, rayColor, 0.2f);
            }

            // If there's no ground, path is not safe
            if (groundHit.collider == null)
            {
                return false;
            }
        }

        // Path has both clearance and ground underneath - it's good!
        return true;
    }

    // Calculate the optimal attack position to maintain preferred distance
    private Vector3 CalculateAttackPosition()
    {
        if (!IsPlayerValid) return transform.position;

        // Calculate direction from player to grunt
        Vector3 dirFromPlayer = (transform.position - _playerTransform.position).normalized;

        // Calculate attack position at preferred distance from player
        Vector3 attackPos = _playerTransform.position + dirFromPlayer * preferredAttackDistance;

        // Use the same Y as the player for simplicity
        attackPos.y = transform.position.y;

        // Keep Z unchanged
        attackPos.z = transform.position.z;

        return attackPos;
    }

    private void TryStab()
    {
        if (Time.time - _lastStabTime >= stabCooldown)
        {
            // Only attack if player is actually in range - double-check distance
            if (IsPlayerValid && Vector3.Distance(transform.position, _playerTransform.position) <= stabRange)
            {
                Attack();
                _lastStabTime = Time.time;
            }
        }
    }

    // Enhanced version of TryJump that returns whether jump was successful
    private new bool TryJump()
    {
        // Only jump if we have a rigidbody and are on ground or haven't exceeded max jumps
        if (rb == null) return false;

        // Check if we're on the ground
        bool canJump = isGrounded || (_jumpCount < _maxJumpCount);

        // Don't jump if we're on cooldown
        if (Time.time <= lastJumpTime + jumpCooldown) return false;

        if (canJump)
        {
            // Store current x velocity to maintain horizontal movement direction
            float currentXVelocity = rb.linearVelocity.x;

            // Reset vertical velocity only, preserving horizontal movement
            rb.linearVelocity = new Vector2(currentXVelocity, 0);

            // Apply upward force while maintaining horizontal direction
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            // Trigger animation if available
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }

            lastJumpTime = Time.time;

            // Increment jump count if we're in the air
            if (!isGrounded)
            {
                _jumpCount++;
            }

            if (showDebugLogs)
            {
                Debug.Log($"{Name} jumped while moving at x velocity: {currentXVelocity}, jump count: {_jumpCount}");
            }

            return true;
        }

        return false;
    }

    public override void Attack()
    {
        // Set attacking flag to prevent movement during animation
        _isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("Stab");
        }

        // Double check that player is valid and in range
        if (!IsPlayerValid)
        {
            _isAttacking = false;
            return;
        }

        // Face the player
        Vector3 direction = (_playerTransform.position - transform.position).normalized;

        // Check if player is still in range
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        if (distanceToPlayer <= stabRange)
        {
            Debug.Log($"Grunt attack initiated at distance {distanceToPlayer}, stab range is {stabRange}");

            // Try direct damage application ONLY via PlayerController to prevent double damage
            if (_playerController != null)
            {
                Debug.Log($"Applying damage via PlayerController component");
                _playerController.TakeDamage(stabDamage);
            }
            else
            {
                // Only use these fallbacks if PlayerController isn't available
                IDamageable playerDamageable = _playerObject.GetComponent<IDamageable>();
                if (playerDamageable != null)
                {
                    Debug.Log($"Applying damage via IDamageable interface");
                    playerDamageable.TakeDamage(stabDamage);
                }
                else
                {
                    // Last fallback to HealthTracker
                    HealthTracker playerHealth = _playerObject.GetComponent<HealthTracker>();
                    if (playerHealth != null)
                    {
                        Debug.Log($"Applying damage via HealthTracker component");
                        playerHealth.TakeDamage(stabDamage);
                    }
                    else
                    {
                        Debug.LogError($"No damage component found on player: {_playerObject.name}");
                    }
                }
            }

            // Spawn stab effect
            if (stabEffectPrefab != null)
            {
                Vector3 spawnPosition = transform.position + direction * (stabRange * 0.5f);
                GameObject stabEffect = Instantiate(stabEffectPrefab, spawnPosition, transform.rotation);
                Destroy(stabEffect, 1f); // Clean up after 1 second
            }

            // Apply knockback to player
            Rigidbody2D playerRb = _playerObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                float knockbackDirX = Mathf.Sign(_playerTransform.position.x - transform.position.x);

                Vector2 knockbackDirection = new Vector2(knockbackDirX, 0.2f);

                playerRb.linearVelocity = knockbackDirection * knockbackForce;

                Debug.Log($"Applied knockback to player: direction={knockbackDirection}, force={knockbackForce}, resulting velocity={playerRb.linearVelocity}");
            }
        }
        else
        {
            Debug.Log($"Grunt's stab missed! Distance to player: {distanceToPlayer}, stab range: {stabRange}");
        }

        // Use coroutine to reset attacking flag after animation time
        StartCoroutine(ResetAttackFlag(0.5f));
    }

    private IEnumerator ResetAttackFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isAttacking = false;
    }

    protected override void OnDrawGizmosSelected()
    {
        // Call the base method to draw the detection radius
        base.OnDrawGizmosSelected();

        // Add Grunt-specific visualization: Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stabRange);

        // Show the vertical check distance
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.up * verticalCheckDistance
        );

        // Show the attack position
        if (Application.isPlaying && IsPlayerValid)
        {
            Gizmos.color = Color.cyan;
            Vector3 attackPos = CalculateAttackPosition();
            Gizmos.DrawSphere(attackPos, 0.2f);
        }

        // Show horizontal path checking area
        if (Application.isPlaying)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawLine(
                transform.position,
                transform.position + Vector3.right * horizontalClearance
            );

            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawLine(
                transform.position,
                transform.position + Vector3.left * horizontalClearance
            );
        }
    }
}