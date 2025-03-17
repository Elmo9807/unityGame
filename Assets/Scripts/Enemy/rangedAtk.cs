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

        Debug.Log("Fireball hit: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by Fireball.");
            HealthTracker playerHealth = other.GetComponent<HealthTracker>(); //fetch HealthTracker information, assign in playerHealth, assign damage if applicable and/or destroy player gameObj when HP threshold is met
            if(playerHealth != null)
            {
                Debug.Log("HealthTracker found, applying damage.");
                playerHealth.TakeDamage(damage);
            } else
            {
                Debug.LogError("HealthTracker component not found on player.");
            }
                Destroy(gameObject);
        }
        else if (other.CompareTag("Wall")) //handle when projectile strikes walls, floor etc, not finished yet
        {
            Debug.Log("Fireball, more like Firewall.");
            Destroy(gameObject);
        }
    }
}
