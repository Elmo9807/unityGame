using UnityEngine;

public class PlayerArrow : MonoBehaviour
{
    public float speed = 15f;
    public int baseDamage = 15;
    public float damageMultiplier = 1f; 
    public float maxLifetime = 5f;
    public float maxDistance = 30f;

    private Vector2 direction;
    private Vector3 startPosition;
    private bool hasHitTarget = false;
    private bool initialized = false;

    void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, maxLifetime);
        Debug.Log($"Player arrow spawned with base damage: {baseDamage}, multiplier: {damageMultiplier}, final damage: {GetFinalDamage()}");
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        UpdateRotation();
        initialized = true;
    }

    private void UpdateRotation()
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    public int GetFinalDamage()
    {
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    void Update()
    {
        if (!initialized) return;

        transform.position += (Vector3)direction * speed * Time.deltaTime;

        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void LateUpdate() //keep UpdateRotation in here to stop the arrow from going wonky
    {
        if (initialized)
        {
            UpdateRotation();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHitTarget) return;

        Debug.Log($"PlayerArrow hit: {collision.gameObject.name}, Tag: {collision.tag}");

        if (collision.CompareTag("Enemy"))
        {
            hasHitTarget = true;

            int finalDamage = GetFinalDamage();

            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
                Debug.Log($"Hit enemy for {finalDamage} damage! (Base: {baseDamage}, Multiplier: {damageMultiplier})");
            }

            Destroy(gameObject);
        }
        else if (!collision.CompareTag("Player"))
        {
            hasHitTarget = true;
            Destroy(gameObject);
        }
    }
}