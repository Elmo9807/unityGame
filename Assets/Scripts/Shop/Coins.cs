using UnityEngine;

public class Coins : MonoBehaviour
{
    public int value = 1;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PowerupManager powerupManager = FindFirstObjectByType<PowerupManager>();
        if (collision.gameObject.CompareTag("Player"))
        {
            powerupManager.AddGold(value);
            Destroy(gameObject);
        }
    }
}
