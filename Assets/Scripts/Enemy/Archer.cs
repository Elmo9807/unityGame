using UnityEngine;

public class Archer : Enemy
{
    [Header("Archer Settings")]
    public float attackRange = 8f;
    public float idealRange = 5f;
    public float arrowSpeed = 10f;

    private ProjectileAttacker projectileAttacker;
    private Vector3 lastKnownPlayerPosition;
    private float lastUpdateTime;
    private float updateInterval = 0.1f; // Update position tracking 10 times per second

    [Header("Debug")]
    [SerializeField] private bool debugMovement = true;
    [SerializeField] private float currentPlayerDistance = 0f;

    protected override void Start()
    {
        base.Start();

        // Set archer-specific properties
        if (string.IsNullOrEmpty(Name))
            Name = "Archer";

        // Set movement speed
        Speed = 3f;

        // Setup projectile attacker component
        projectileAttacker = GetComponent<ProjectileAttacker>();
        if (projectileAttacker == null)
        {
            projectileAttacker = gameObject.AddComponent<ProjectileAttacker>();
            projectileAttacker.projectilePrefab = Resources.Load<GameObject>("Arrow");
            projectileAttacker.attackCooldown = 2f;
            projectileAttacker.attackRange = attackRange;
            Debug.Log("ProjectileAttacker component added to Archer");
        }
    }

    protected override void Update()
    {

        Debug.Log($"Player null check: {(player == null ? "NULL" : "FOUND")}");

        // Find player if needed
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // Update player position tracking periodically
        if (Time.time > lastUpdateTime + updateInterval)
        {
            lastKnownPlayerPosition = GetFreshPlayerPosition();
            lastUpdateTime = Time.time;
        }

        // Calculate current distance to player
        currentPlayerDistance = Vector2.Distance(transform.position, lastKnownPlayerPosition);

        Debug.Log($"Distance to player: {currentPlayerDistance}, Detection radius: {detectionRadius}, Ideal range: {idealRange}");

        // Update sprite facing direction 
        UpdateFacing();

        // Handle movement and attacks based on distance
        if (currentPlayerDistance <= detectionRadius)
        {
            // Too close - back away
            if (currentPlayerDistance < idealRange - 1f)
            {
                BackAwayFromPlayer();
            }
            // At good attack range - stop and attack
            else if (currentPlayerDistance <= attackRange)
            {
                // Don't move, just attack
                if (projectileAttacker.CanAttack(player))
                {
                    Attack();
                }
            }
            // Too far - move closer 
            else
            {
                MoveToPlayer();
            }
        }
    }

    // CRITICAL: We're explicitly NOT calling the base class MoveTowardsTarget
    private void MoveToPlayer()
    {
        Vector3 targetPos = lastKnownPlayerPosition;

        // Calculate only horizontal direction
        float directionX = Mathf.Sign(targetPos.x - transform.position.x);

        // Move directly using transform position
        Vector3 newPosition = transform.position + new Vector3(directionX * Speed * Time.deltaTime, 0, 0);
        transform.position = newPosition;

        if (debugMovement)
        {
            Debug.Log($"Moving towards player at {targetPos}, new position: {newPosition}, directionX: {directionX}");
            // Draw a line showing movement direction
            Debug.DrawLine(transform.position, new Vector3(transform.position.x + directionX * 2, transform.position.y, transform.position.z), Color.blue, 0.1f);
        }
    }

    private void BackAwayFromPlayer()
    {
        Vector3 targetPos = lastKnownPlayerPosition;

        // Calculate direction away from player (horizontal only)
        float directionX = -Mathf.Sign(targetPos.x - transform.position.x);

        // Move directly with transform
        Vector3 newPosition = transform.position + new Vector3(directionX * Speed * Time.deltaTime, 0, 0);
        transform.position = newPosition;

        if (debugMovement)
        {
            Debug.Log($"Backing away from player at {targetPos}, new position: {newPosition}, directionX: {directionX}");
            // Draw a line showing movement direction
            Debug.DrawLine(transform.position, new Vector3(transform.position.x + directionX * 2, transform.position.y, transform.position.z), Color.red, 0.1f);
        }
    }

    private void UpdateFacing()
    {
        bool shouldFaceRight = lastKnownPlayerPosition.x > transform.position.x;

        // Get current scale and check if we need to flip
        Vector3 currentScale = transform.localScale;
        if ((shouldFaceRight && currentScale.x < 0) || (!shouldFaceRight && currentScale.x > 0))
        {
            // Flip the x scale
            currentScale.x *= -1;
            transform.localScale = currentScale;

            Debug.Log($"Archer flipped to face {(shouldFaceRight ? "right" : "left")}");
        }
    }

    // Get current player position - always fresh [VERY IMPORTANT TO DIFFERENTIATE BETWEEN SPAWN STATE AND ACTIVE STATE]
    private Vector3 GetFreshPlayerPosition()
    {
        GameObject currentPlayer = GameObject.FindGameObjectWithTag("Player");
        if (currentPlayer != null)
        {
            return currentPlayer.transform.position;
        }
        return player != null ? player.position : transform.position;
    }

    // Override the attack method
    public override void Attack()
    {
        Vector3 targetPosition = GetFreshPlayerPosition();

        if (projectileAttacker != null)
        {
            // Shoot arrow at current player position
            projectileAttacker.ShootProjectile(player, "arrow");
            Debug.Log($"Archer shot arrow at {targetPosition}");
        }
    }

    // Override base class movement to do nothing - we handle it directly
    public override void MoveTowardsTarget(Vector3 targetPosition)
    {
      //NO METHODS IN HERE
    }
}