using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Player
{
    public Inventory inventory;
    public Weapon equippedWeapon;
    public int Health { get; private set; }
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
    }

    public void TakeDamage(int damage)
    {
        int oldHealth = Health;
        Health = Mathf.Max(Health - damage, 0);

        if (isDebugLogging)
        {
            Debug.Log($"[Player] TakeDamage called with {damage}. Health: {oldHealth} -> {Health}");

            if (OnHealthChanged == null)
            {
                Debug.LogError("[Player] OnHealthChanged event has NO subscribers!");
            }
            else
            {
                Debug.Log("[Player] OnHealthChanged has subscribers, invoking...");
            }
        }

        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    public void EquipWeapon(Weapon weapon)
    {
        equippedWeapon = weapon;
    }

    public void Heal(int amount)
    {
        int oldHealth = Health;
        Health = Mathf.Min(Health + amount, MaxHealth);

        if (isDebugLogging)
        {
            Debug.Log($"[Player] Heal called with {amount}. Health: {oldHealth} -> {Health}");

            if (OnHealthChanged == null)
            {
                Debug.LogError("[Player] OnHealthChanged event has NO subscribers!");
            }
        }

        OnHealthChanged?.Invoke(Health, MaxHealth);
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
