using UnityEngine;

public class Coins : MonoBehaviour
{
    public int value = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Find the PowerupManager
            PowerupManager powerupManager = FindFirstObjectByType<PowerupManager>();
            HealthTracker health = collision.gameObject.GetComponent<HealthTracker>();
            // Check if we found a PowerupManager before using it
            if (powerupManager != null)
            {
                powerupManager.AddGold(value);
                Debug.Log($"Added {value} gold to player");
            }
            else
            {
                Debug.LogWarning("PowerupManager not found - couldn't add gold");
            }

            if (health != null)
            {
                health.Heal(10);
            }
            else
            {
                Debug.LogWarning("HealthTracker not found - couldn't heal player!");
            }

            AudioManager.instance.PlayOneShot(FMODEvents.instance.CoinPickup, this.transform.position);
            // Destroy the coin regardless of whether we found the PowerupManager
            Destroy(gameObject);
        }
    }
}