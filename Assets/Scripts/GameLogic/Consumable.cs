using UnityEngine;

public class Consumable : Item
{
    public GameEffect Effect { get; set; }

    public float Duration { get; set; }

    public override void Use(Player player)
    {
        Effect.ApplyTo(player);
    }
}
