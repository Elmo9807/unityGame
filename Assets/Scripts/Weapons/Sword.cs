using UnityEngine;

public class Sword : Weapon
{
    public float slashRange = 1.5f;

    public Sword()
    {
        Name = "Basic Sword";
        Description = "A simple steel sword.";
        Damage = 20;
        AttackSpeed = 1.0f;
        Icon = "sword_icon";
        CurrencyValue = 25;
    }

    public void PerformSlash(Player player, Enemy target)
    {
        target.TakeDamage(Damage);
        Debug.Log($"Sword slash hit enemy for {Damage} damage!");
    }

    public override void PerformAttack(Player player, Transform playerTransform)
    {
        bool isFacingRight = playerTransform.localScale.x > 0;
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;

        Vector2 attackPos = (Vector2)playerTransform.position + direction * slashRange;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, slashRange, LayerMask.GetMask("Enemy"));

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(Damage);
                Debug.Log($"Sword hit {enemy.name} for {Damage} damage!");
            }
        }
    }
}