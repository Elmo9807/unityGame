using UnityEngine;


[System.Serializable]
public class PlayerSaveData
{
    public int currency;
    public int maxHealth;
    public float meleeDmg;
    public float rangedDmg;
    public bool hasDash;
    public bool hasBow;
    public bool hasHealingPot;
    public bool startWithDash;
    public bool startWithBow;
    public bool startWithPot;
    public int currentHealth;

    public PlayerSaveData(Player player, PowerupManager powerupManager, HealthTracker healthTracker)
    {
        hasBow = player.hasBow;
        hasDash = player.hasDash;
        hasHealingPot = player.hasHealingPotion;
        currency = powerupManager.playerCurrency;
        maxHealth = healthTracker.maxHealth;
        meleeDmg = player.meleeAttackDamage;
        rangedDmg = player.bowAttackDamage;
        startWithDash = powerupManager.startWithDash;
        startWithBow = powerupManager.startWithBow;
        startWithPot = powerupManager.startWithHealingPotion;
    }

}
