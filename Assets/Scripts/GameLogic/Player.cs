using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Player
{
    public Inventory inventory;
    public Weapon equippedWeapon;
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public float StrengthModifier { get; private set; } = 1.0f;

    private List<GameEffect> activeEffects = new List<GameEffect>();

    public Player()
    {
        MaxHealth = 100;
        Health = MaxHealth;
        inventory = new Inventory();
    }

    public void EquipWeapon(Weapon weapon)
    {
        equippedWeapon = weapon;
    }

    public void Heal(int amount)
    { 
        Health = Mathf.Min(Health + amount, MaxHealth);
    }

    public void ApplyEffect(GameEffect effect)
    {
        activeEffects.Add(effect);
        effect.ApplyTo(this);
    }

    public void Attack(Enemy target)
    {
        if(equippedWeapon != null)
        {
            if(equippedWeapon is Sword sword)
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
        for(int i = activeEffects.Count - 1; i >= 0; i--)
        {
            GameEffect effect = activeEffects[i];
            effect.Duration -= deltaTime;

            if(effect.Duration <= 0)
            {
                activeEffects.RemoveAt(i);
            }
        }
    }
}
