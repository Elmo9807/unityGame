using UnityEngine;

public class hpScript : MonoBehaviour, IDamageable
{
    public float health = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
