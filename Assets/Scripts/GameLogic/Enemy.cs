using UnityEngine;
using System;
using System.Collections.Generic;

public class Enemy
{
    public string Name { get; private set; }
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }

    private List<(GameEffect effect, Action onTick)> activeEffects = new List<(GameEffect, Action)>();

    public event Action OnDeath;
    public delegate void HealthChangeHandler(int currentHealth, int maxHealth);
    public event HealthChangeHandler OnHealthChanged;

    public Enemy(string name, int health)
    {
        Name = name;
        MaxHealth = health;
        Health = MaxHealth;

        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;

        OnHealthChanged?.Invoke(Health, MaxHealth);

        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }

    public float HealthPercent()
    {
        return (float)Health / MaxHealth;
    }

    /*
    public void ApplyEffect(GameEffect effect)
    {
        Action onTick = null;
        if(effect is BleedingEffect)
        {
            onTick = () => TakeDamage(2);
        }
        activeEffects.Add((effect, onTick));
    }
    */
}

