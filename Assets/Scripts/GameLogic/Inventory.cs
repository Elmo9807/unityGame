using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public List<Item> items = new List<Item>();
    public List<Weapon> weapons = new List<Weapon>();
    public List<HealingPotion> healingPotions = new List<HealingPotion>();
    public List<Item> consumables = new List<Item>();

    private int maxWeapons = 3;
    private int maxPotions = 5;
    private int maxConsumables = 10;

    public Inventory()
    {
        // Add a default sword, can be changed later
        Sword defaultSword = new Sword();
        weapons.Add(defaultSword);

        // Add a starting healing potion, can be changed if we want him to spawn without pots
        HealingPotion startingPotion = new HealingPotion
        {
            Name = "Minor Healing Potion",
            Description = "Restores a small amount of health.",
            HealthGain = 20,
            Icon = "potion_icon",
            CurrencyValue = 15
        };
        healingPotions.Add(startingPotion);
    }

    public void AddItem(Item item)
    {
        items.Add(item);
        Debug.Log($"Added {item.Name} to inventory.");
    }

    public void AddWeapon(Weapon weapon)
    {
        if (weapons.Count < maxWeapons)
        {
            weapons.Add(weapon);
            Debug.Log($"Added {weapon.Name} to weapons.");
        }
        else
        {
            Debug.Log("Weapon inventory full!");
        }
    }

    public void AddHealingPotion(HealingPotion potion)
    {
        if (healingPotions.Count < maxPotions)
        {
            healingPotions.Add(potion);
            Debug.Log($"Added {potion.Name} to potions.");
        }
        else
        {
            Debug.Log("Potion inventory full!");
        }
    }

    public void UseWeapon(int index, Player player)
    {
        if (index >= 0 && index < weapons.Count)
        {
            weapons[index].Use(player);
        }
        else
        {
            Debug.Log("Invalid weapon index!");
        }
    }

    public void UseHealingPotion(Player player)
    {
        if (healingPotions.Count > 0)
        {
            HealingPotion potion = healingPotions[0];
            potion.Use(player);

            Debug.Log($"Used {potion.Name} to heal for {potion.HealthGain} health.");
            healingPotions.RemoveAt(0);
        }
        else
        {
            Debug.Log("No healing potions available!");
        }
    }

    public void UseConsumable(Player player)
    {
        if (consumables.Count > 0)
        {
            consumables[0].Use(player);
            consumables.RemoveAt(0);
        }
        else
        {
            Debug.Log("No consumables available!");
        }
    }

    public void EquipWeapon(int index, Player player)
    {
        if (index >= 0 && index < weapons.Count)
        {
            player.EquipWeapon(weapons[index]);
            Debug.Log($"Equipped {weapons[index].Name}");
        }
    }
}