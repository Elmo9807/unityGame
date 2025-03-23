using UnityEngine;

public class HealingPotion : Item
{
    public int HealthGain { get; set; } = 30;

    public override void Use(Player player)
    {
        Debug.Log($"Using healing potion. Current health: {player.Health}/{player.MaxHealth}, healing for {HealthGain}");

        player.Heal(HealthGain);

        Debug.Log($"After healing. Current health: {player.Health}/{player.MaxHealth}");
    }
}