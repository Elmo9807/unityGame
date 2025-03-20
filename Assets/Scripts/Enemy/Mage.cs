using UnityEngine;

public class Mage : Enemy
{
    public float levitationHeight = 2f;
    private ProjectileAttacker projectileAttacker;

    protected override void Start()
    {
        base.Start();
        projectileAttacker = GetComponent<ProjectileAttacker>();
        if (projectileAttacker == null)
        {
            projectileAttacker = gameObject.AddComponent<ProjectileAttacker>();
            projectileAttacker.projectilePrefab = Resources.Load<GameObject>("Fireball");
            projectileAttacker.attackCooldown = 2f;
        }
    }

    private void Update()
    {
        if (player != null)
        {
            // Custom flying movement
            Fly(player.position);

            // Attack logic
            if (projectileAttacker.CanAttack(player))
            {
                Attack();
            }
        }
    }

    public override void Attack()
    {
        projectileAttacker.ShootProjectile(player, "fireball");
    }

    private void Fly(Vector3 targetPosition)
    {
        // Override movement to implement flying
        Vector3 flyingPosition = new Vector3(targetPosition.x, targetPosition.y + levitationHeight, targetPosition.z);
        MoveTowardsTarget(flyingPosition);
    }
}