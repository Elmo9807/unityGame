using UnityEngine;

public abstract class Weapon : Item
{
    public int Damage { get; set; }
    public float AttackSpeed { get; set; }

    // Attack method when used from inventory
    public override void Use(Player player)
    {
        // Find the player's GameObject transform
        Transform playerTransform = null;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Could not find player object for weapon use!");
            return;
        }

        PerformAttack(player, playerTransform);
    }

    // Abstract method for specific weapon attack behavior
    public abstract void PerformAttack(Player player, Transform playerTransform);
}