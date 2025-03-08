using UnityEngine;

public class Inventory
{
    private Weapon[] weaponSlots = new Weapon[2];
    private HealingPotion healthSlot;
    private Consumable consumableSlot;

    public bool AddWeapon(Weapon weapon, int slot)
    {
        if (slot < 0 || slot >= weaponSlots.Length)
            return false;

        weaponSlots[slot] = weapon;
        return true;
    }

    public bool SetHealthItem(HealingPotion item)
    {
        healthSlot = item;
        return true;
    }

    public bool SetConsumable(Consumable item)
    {
        consumableSlot = item;
        return true;
    }

    public void UseWeapon(int slot, Player player)
    {
        if (weaponSlots[slot] != null)
            weaponSlots[slot].Use(player);
    }
    public void UseHealingPotion(Player player)
    {
        if(healthSlot != null)
        {
            healthSlot.Use(player);
            healthSlot = null;
        }
    }

    public void UseConsumable(Player player)
    {
        if(consumableSlot != null)
        {
            consumableSlot.Use(player);
            consumableSlot = null;
        }
    }

    public Weapon GetWeapon(int slot)
    {
        if (slot < 0 || slot >= weaponSlots.Length)
            return null;

        return weaponSlots[slot];
    }

    public HealingPotion GetHealthItem()
    {
        return healthSlot;
    }

    public Consumable GetConsumable()
    {
        return consumableSlot;
    }
}
