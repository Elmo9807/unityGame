using UnityEngine;
using System.Collections.Generic;

public class HealingPotion : Item
{
    public int HealingPotionPrice { get; } = 10;  // Price of potion
    public int HealthGain { get; } = 30;          // Amount healed
    public int Quantity { get; private set; } = 1;  // Number of potions owned

    public override void Use(Player player)
    {
        Debug.Log($"Using healing potion. Current health: {player.Health}/{player.MaxHealth}, healing for {HealthGain}");
        player.Heal(HealthGain);
        Debug.Log($"After healing. Current health: {player.Health}/{player.MaxHealth}");
    }

    public void BuyPotion()
    {
        Quantity++; // Increase owned quantity when bought
    }
}
