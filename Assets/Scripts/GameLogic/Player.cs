using UnityEngine;
using System.Collections.Generic;

public class Player
{
    public Inventory inventory;
    public Weapon equippedWeapon;

    private int health;
    public int Health
    {
        get { return health; }
        private set { health = Mathf.Clamp(value, 0, MaxHealth); }
    }

    public int MaxHealth { get; private set; } = 100;
    public float StrengthModifier { get; private set; } = 1.0f;

    private List<GameEffect> activeEffects = new List<GameEffect>();
    private Transform playerTransform;

    private bool isDebugLogging = true;

    public delegate void HealthChangeHandler(int currentHealth, int maxHealth);
    public event HealthChangeHandler OnHealthChanged;

    public Player(Transform transform = null)
    {
        playerTransform = transform;
        Health = MaxHealth;
        inventory = new Inventory();

        if (isDebugLogging)
        {
            Debug.Log($"[Player] New Player instance created with health: {Health}/{MaxHealth}");
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        int oldHealth = Health;
        Health = Mathf.Max(Health - damage, 0);

        if (isDebugLogging)
        {
            Debug.Log($"[Player] TakeDamage called with {damage}. Health: {oldHealth} -> {Health}");
        }

        // Only raise event if health actually changed
        if (oldHealth != Health)
        {
            if (isDebugLogging && OnHealthChanged == null)
            {
                Debug.LogWarning("[Player] OnHealthChanged event has NO subscribers!");
            }

            OnHealthChanged?.Invoke(Health, MaxHealth);
        }
    }

    public void EquipWeapon(Weapon weapon)
    {
        equippedWeapon = weapon;
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        int oldHealth = Health;
        Health = Mathf.Min(Health + amount, MaxHealth);

        if (isDebugLogging)
        {
            Debug.Log($"[Player] Heal called with {amount}. Health: {oldHealth} -> {Health}");
        }

        // Only raise event if health actually changed
        if (oldHealth != Health)
        {
            if (isDebugLogging && OnHealthChanged == null)
            {
                Debug.LogWarning("[Player] OnHealthChanged event has NO subscribers!");
            }

            OnHealthChanged?.Invoke(Health, MaxHealth);
        }
    }

    public void ApplyEffect(GameEffect effect)
    {
        activeEffects.Add(effect);
        effect.ApplyTo(this);
    }

    public void Attack(Enemy target)
    {
        if (equippedWeapon != null)
        {
            if (equippedWeapon is Sword sword)
            {
                sword.PerformSlash(this, target);
            }
            else
            {
                target.TakeDamage(equippedWeapon.Damage);
            }
        }
    }

    public void UpdateEffects(float deltaTime)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            GameEffect effect = activeEffects[i];
            effect.Duration -= deltaTime;

            if (effect.Duration <= 0)
            {
                activeEffects.RemoveAt(i);
            }
        }
    }
}