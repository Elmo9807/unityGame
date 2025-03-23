using UnityEngine;

public class Bow : Weapon
{
    public GameObject arrowPrefab;
    public float arrowSpeed = 15f;
    public float cooldownTime = 0.5f;
    private float lastShotTime = 0f;

    
    public float damageMultiplier = 1.0f;

    public Bow()
    {
        Name = "Hunting Bow";
        Description = "A simple wooden bow that fires arrows.";
        Damage = 15;  
        AttackSpeed = 2.0f;
        Icon = "bow_icon";
        CurrencyValue = 35;
        damageMultiplier = 1.0f;  
    }

    public override void PerformAttack(Player player, Transform playerTransform)
    {
        if (Time.time >= lastShotTime + cooldownTime)
        {
            
            Vector2 direction;

            
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            if (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f)
            {
                
                direction = new Vector2(horizontalInput, verticalInput).normalized;
            }
            else
            {
                
                bool isFacingRight = playerTransform.localScale.x > 0;
                direction = isFacingRight ? Vector2.right : Vector2.left;
            }

            
            Vector3 spawnPosition = playerTransform.position + new Vector3(direction.x * 0.5f, direction.y * 0.5f, 0);

            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            
            if (arrowPrefab == null)
            {
                Debug.LogError("Arrow prefab is null! Make sure it's assigned in the Bow class.");
                return;
            }

            
            GameObject arrowObject = Object.Instantiate(arrowPrefab, spawnPosition, rotation);

            
            PlayerArrow arrow = arrowObject.GetComponent<PlayerArrow>();
            if (arrow != null)
            {
                
                
                float actualMultiplier = damageMultiplier > 0 ? damageMultiplier : (Damage / 15f); 
                arrow.damageMultiplier = actualMultiplier;
                arrow.speed = arrowSpeed;
                arrow.SetDirection(direction);

                Debug.Log($"Created arrow with damage multiplier: {actualMultiplier}, base damage: {arrow.baseDamage}, " +
                          $"final damage: {arrow.GetFinalDamage()}");

                
                arrowObject.layer = LayerMask.NameToLayer("Projectile");

                
                arrowObject.tag = "PlayerProjectile";

                
                Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
                Collider2D arrowCollider = arrowObject.GetComponent<Collider2D>();

                if (playerCollider != null && arrowCollider != null)
                {
                    Physics2D.IgnoreCollision(arrowCollider, playerCollider);
                }

                Debug.Log($"Player fired an arrow in direction: {direction}, " +
                          $"rotation: {rotation.eulerAngles.z} degrees");
            }
            else
            {
                Debug.LogError("Arrow prefab does not have PlayerArrow component!");
            }

            lastShotTime = Time.time;
        }
    }
}