using UnityEngine;
using System.Collections.Generic;

public class Player
{
    #region Powerup Flags
    public bool hasBow;
    public bool hasSword;
    public bool hasHealingPotion;
    public bool hasDoubleJump;
    public bool hasDash;
    #endregion

    #region Stats
    public float meleeAttackDamage = 20f;
    public float bowAttackDamage = 15f;
    public int healingAmount = 30;

    private int health;
    public int Health
    {
        get { return health; }
        private set { health = Mathf.Clamp(value, 0, MaxHealth); }
    }

    public int MaxHealth { get; private set; } = 100;
    #endregion

    private List<GameEffect> activeEffects = new List<GameEffect>();

    public delegate void HealthChangeHandler(int currentHealth, int maxHealth);
    public event HealthChangeHandler OnHealthChanged;
    public delegate void DeathEventHandler();
    public event System.Action OnDeath;

    public Player()
    {
        Health = MaxHealth;

        hasBow = false;
        hasSword = true;
        hasHealingPotion = false;
        hasDoubleJump = false;
        hasDash = false;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        int oldHealth = Health;
        Health = Mathf.Max(Health - damage, 0);

        if (oldHealth != Health)
            OnHealthChanged?.Invoke(Health, MaxHealth);

        if (Health <= 0)
        {
            OnDeath();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        int oldHealth = Health;
        Health = Mathf.Min(Health + amount, MaxHealth);

        if (oldHealth != Health)
            OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    public void UseHealingPotion()
    {
        if (hasHealingPotion)
        {
            Heal(healingAmount);
            hasHealingPotion = false;
        }
    }

    public void ApplyEffect(GameEffect effect)
    {
        activeEffects.Add(effect);
        effect.ApplyTo(this);
    }

    public void UpdateEffects(float deltaTime)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            GameEffect effect = activeEffects[i];
            effect.Duration -= deltaTime;

            if (effect.Duration <= 0)
                activeEffects.RemoveAt(i);
        }
    }
}