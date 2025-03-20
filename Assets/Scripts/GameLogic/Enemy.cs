using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using UnityEditor;

public class Enemy
{
    public string Name { get; private set; }

    public int Health { get; private set; }

    private List<(GameEffect effect, Action onTick)> activeEffects = new List<(GameEffect, Action)>();

    public event Action OnDeath;

    public Enemy(string name, int health)
    {
        Name = name;
        Health = health;
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;

        if(Health <= 0)
        {
            Die();
        }
    }

    //public void ApplyEffect(GameEffect effect)
    //{
    //    Action onTick = null;
    //    if(effect is BleedingEffect)
    //    {
    //        onTick = () => TakeDamage(2);
    //    }
    //    activeEffects.Add((effect, onTick));
    //} 

    private void Die()
    {
        OnDeath?.Invoke();
    }
}

