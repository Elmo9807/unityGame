using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Properties")]
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private float attackRange = 0.5f;

    [Header("Attack Setup")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;

    // Placeholder for animations when we get them
    [SerializeField] private Animator animator;

    private float nextAttackTime = 0f;

    void Update()
    {
        // Atk cooldown
        if (Time.time >= nextAttackTime)
        {
            // Check for attack input (you can change this to any key)
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Z))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void Attack()
    {
        Debug.Log("Attack started");

        // animation placeholder for when we actually have animations
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        Collider2D[] atkEnemy = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        Debug.Log("Found " + atkEnemy.Length + " enemies in range");

        // Enemy damaging
        foreach (Collider2D enemy in atkEnemy)
        {
            Debug.Log("Trying to damage enemy: " + enemy.name);

            // health-damage check
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
            }
            else
            {
                Debug.Log("Enemy doesn't have IDamageable component");
            }
        }
    }

    // Draw attack range in editor
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

// IDamageable implemented to force damage
public interface IDamageable
{
    void TakeDamage(float damage);
}