using UnityEngine;

public class Weapon : Item
{
    public int Damage { get; set; }
    public float AttackSpeed { get; set; }
    public WeaponType Type { get; set; }

    public override void Use(Player player)
    {
        player.EquipWeapon(this);
    }
}
