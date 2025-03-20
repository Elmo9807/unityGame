using UnityEngine;

public class Archer : Enemy
{
    private ProjectileAttacker projectileAttacker;

    protected override void Start()
    {
        base.Start();
        projectileAttacker = GetComponent<ProjectileAttacker>();
        if (projectileAttacker == null)
        {
            projectileAttacker = gameObject.AddComponent<ProjectileAttacker>();
            projectileAttacker.projectilePrefab = Resources.Load<GameObject>("Arrow");
            projectileAttacker.attackCooldown = 1.5f;
            projectileAttacker.attackRange = 10f;
        }
    }

    protected override void Update()
    {
        if (player == null) return;  // Ensure player exists

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Only proceed if player is within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            if (distanceToPlayer > projectileAttacker.attackRange * 0.8f)
            {
                MoveTowardsTarget(player.position);  // Keep moving if too far
            }
            else
            {
                if (projectileAttacker.CanAttack(player))
                {
                    Attack();
                }
            }
        }
    }

    public override void Attack()
    {
        projectileAttacker.ShootProjectile(player, "arrow");
    }

    // Visualize the detection radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}