using UnityEngine;

public abstract class GameEffect
{
    public float Duration { get; set; }

    public GameEffect(float duration)
    {
        Duration = duration;
    }

    public abstract void ApplyTo(Player player);
}
