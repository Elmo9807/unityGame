using UnityEngine;

public class HealingPotion : Item
{
    public int HealthGain { get; set; } 

    public override void Use(Player player)
    {
        player.Heal(HealthGain);
    }
}
