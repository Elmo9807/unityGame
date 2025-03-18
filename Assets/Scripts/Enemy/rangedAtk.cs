using Unity.VisualScripting;
using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    public float speed = 5f; //speed of projectile
    public int damage = 10; //damage done to player or surface
    public float lifetime = 3f; //projectile flight time before despawn

    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized; //normalise vector of projectile
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthTracker playerHealth = other.GetComponent<HealthTracker>(); //fetch HealthTracker information, assign in playerHealth, assign damage if applicable and/or destroy player gameObj when HP threshold is met
            if(playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall")) //handle when projectile strikes walls, floor etc, not finished yet
        {
            Destroy(gameObject);
        }
    }
}
