using NUnit.Framework;
using UnityEngine;
<<<<<<< Updated upstream
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using UnityEditor;
=======
>>>>>>> Stashed changes

public class Enemy : MonoBehaviour
{
<<<<<<< Updated upstream
    public string Name { get; private set; }

    public int Health { get; private set; }

    private List<(GameEffect effect, Action onTick)> activeEffects = new List<(GameEffect, Action)>();

    public event Action OnDeath;

    public Enemy(string name, int health)
    {
        Name = name;
        Health = health;
=======
    public string Name;
    public int Health;
    public int MaxHealth = 100;
    public float Speed = 5f;
    public float detectionRadius = 10f;

    public event System.Action OnDeath;
    public delegate void HealthChangeHandler(int currentHealth, int maxHealth);
    public event HealthChangeHandler OnHealthChanged;

    public Transform player;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Please ensure the Player object is tagged with 'Player'.");
        }

        // Initialize health
        Health = MaxHealth;
        OnHealthChanged?.Invoke(Health, MaxHealth);
>>>>>>> Stashed changes
    }

    protected virtual void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRadius)
            {
                MoveTowardsTarget(player.position);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
<<<<<<< Updated upstream

        if(Health <= 0)
=======
        OnHealthChanged?.Invoke(Health, MaxHealth);
        if (Health <= 0)
>>>>>>> Stashed changes
        {
            Die();
        }
    }

<<<<<<< Updated upstream
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
=======
    protected virtual void Die()
>>>>>>> Stashed changes
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
<<<<<<< Updated upstream
=======

    // Base movement method that can be overridden
    public virtual void MoveTowardsTarget(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
    }

    // Base attack method that can be overridden
    public virtual void Attack()
    {
        // Base implementation does nothing
    }
>>>>>>> Stashed changes
}

