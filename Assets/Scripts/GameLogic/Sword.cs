using UnityEngine;

public class Sword : Weapon
{
    public Sword()
    {
        Name = "Gladius";
        Description = "A simple, straight thrusting sword, good when wielded with a shield";
        Icon = "icons/gladius.png";
        CurrencyValue = 15;
        Damage = 34;
        AttackSpeed = 1.5f;
        Type = WeaponType.Melee;
    }

    public override void Use(Player player)
    {
        base.Use(player);
    }

    public void PerformSlash(Player player, Enemy target)
    {
        int damageDealt = (int)(Damage * player.StrengthModifier);

        target.TakeDamage(damageDealt);
    }
}
