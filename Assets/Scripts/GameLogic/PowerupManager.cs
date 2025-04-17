using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerupManager : MonoBehaviour
{
    #region Powerup Settings
    [SerializeField] public bool startWithBow = false;
    [SerializeField] public bool startWithDoubleJump = false;
    [SerializeField] public bool startWithDash = false;
    [SerializeField] public bool startWithHealingPotion = false;
    #endregion

    #region UI References
    [SerializeField] private GameObject powerupUI;
    [SerializeField] private Image bowIcon;
    [SerializeField] private Image dashIcon;
    [SerializeField] private Image doubleJumpIcon;
    [SerializeField] private Image healingPotionIcon;
    #endregion

    #region Shop
    [SerializeField] public int playerCurrency = 1000;
    [SerializeField] private Text currencyText;
    [SerializeField] private int bowPrice = 100;
    [SerializeField] private int dashPrice = 150;
    [SerializeField] private int doubleJumpPrice = 120;
    [SerializeField] private int healingPotionPrice = 50;
    [SerializeField] public int[,] shopItems = new int[8, 8];
    #endregion

    private PlayerController playerController;
    private Player playerData;
    private HealthTracker healthTracker;

    private void Start()
    {
        // Dylan's code was in start but it didn't always find the player so I made it a coroutine

        /*GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerData = playerController.GetPlayerData();

            if (playerData != null)
            {
                playerData.hasBow = startWithBow;
                playerData.hasDoubleJump = startWithDoubleJump;
                playerData.hasDash = startWithDash;
                playerData.hasHealingPotion = startWithHealingPotion;
            }
        }

        UpdatePowerupUI();
        UpdateGoldText(); */
        StartCoroutine(InitialisePlayerData());

        //ID's
        shopItems[1, 1] = 1; //Dash
        shopItems[1, 2] = 2; //Bow
        shopItems[1, 3] = 3; //Potion
        shopItems[1, 4] = 4; //Max Health
        shopItems[1, 5] = 5; //Melee Dmg Up
        shopItems[1, 6] = 6; //Ranged Dmg Up

        //Price
        shopItems[2, 1] = dashPrice;
        shopItems[2, 2] = bowPrice;
        shopItems[2, 3] = healingPotionPrice;
        shopItems[2, 4] = 15;
        shopItems[2, 5] = 20;
        shopItems[2, 6] = 20;
    }

    private IEnumerator InitialisePlayerData()
    {
        GameObject player = null;

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }

        playerController = player.GetComponent<PlayerController>();
        healthTracker = player.GetComponent<HealthTracker>();

        if (playerController != null)
        {
            playerData = playerController.GetPlayerData();

            if (playerData != null)
            {
                playerData.hasBow = startWithBow;
                playerData.hasDoubleJump = startWithDoubleJump;
                playerData.hasDash = startWithDash;
                playerData.hasHealingPotion = startWithHealingPotion;
            }
        }

        UpdatePowerupUI();
        UpdateGoldText();
    }

    public void UpdatePowerupUI()
    {
        if (powerupUI == null || playerData == null) return;
        if (bowIcon != null) bowIcon.color = playerData.hasBow ? Color.white : Color.gray;
        if (dashIcon != null) dashIcon.color = playerData.hasDash ? Color.white : Color.gray;
        if (doubleJumpIcon != null) doubleJumpIcon.color = playerData.hasDoubleJump ? Color.white : Color.gray;
        if (healingPotionIcon != null) healingPotionIcon.color = playerData.hasHealingPotion ? Color.white : Color.gray;
    }

    private void UpdateGoldText()
    {
        Debug.Log("Player Currency: " + playerCurrency);
        if (currencyText != null)
            currencyText.text = "Currency: " + playerCurrency.ToString();
    }

    public void BuyBow()
    {
        BuyPowerup(ref playerData.hasBow, bowPrice);
        startWithBow = true;
    }

    public void BuyDash()
    {
        BuyPowerup(ref playerData.hasDash, dashPrice);
        startWithDash = true;
        Debug.Log("Player has dash: " + playerData.hasDash);
    }

    public void BuyDoubleJump()
    {
        BuyPowerup(ref playerData.hasDoubleJump, doubleJumpPrice);
    }

    public void BuyHealingPotion()
    {
        BuyPowerup(ref playerData.hasHealingPotion, healingPotionPrice);
        startWithHealingPotion = true;
    }

    private void BuyPowerup(ref bool powerupFlag, int price)
    {
        if (powerupFlag) return;

        if (playerCurrency >= price)
        {
            playerCurrency -= price;
            powerupFlag = true;
            UpdatePowerupUI();
            UpdateGoldText();
        }
    }

    public void BuyMaxHealth(int price)
    {
        if (playerCurrency >= price)
        {
            playerCurrency -= price;
            healthTracker.SetMaxHealth(healthTracker.maxHealth += 15);
            healthTracker.SetHealth(healthTracker.maxHealth);
            UpdateGoldText();
        }
    }

    public void BuyMeleeDmg(int price)
    {
        if (playerCurrency >= price)
        {
            playerCurrency -= price;
            playerData.meleeAttackDamage += 10;
            UpdateGoldText();
        }
    }

    public void BuyRangedDmg(int price)
    {
        if (playerCurrency >= price)
        {
            playerCurrency -= price;
            playerData.bowAttackDamage += 5;
            UpdateGoldText();
        }
    }

    public void Buy()
    {
        GameObject ButtonRef = GameObject.FindGameObjectWithTag("Event").GetComponent<EventSystem>().currentSelectedGameObject;

        if (ButtonRef.GetComponent<ButtonInfo>().itemId == 1) //Dash
        {
            BuyDash();
        }
        else if (ButtonRef.GetComponent<ButtonInfo>().itemId == 2) //Bow
        {
            BuyBow();
        }
        else if (ButtonRef.GetComponent<ButtonInfo>().itemId == 3) //HealingPot
        {
            BuyHealingPotion();
        }
        else if (ButtonRef.GetComponent<ButtonInfo>().itemId == 4) // MaxHealth
        {
            BuyMaxHealth(shopItems[2, 4]);
        }
        else if (ButtonRef.GetComponent<ButtonInfo>().itemId == 5) // MeleeDmg
        {
            BuyMeleeDmg(shopItems[2, 5]);
        }
        else if (ButtonRef.GetComponent<ButtonInfo>().itemId == 6) // RangedDmg
        {
            BuyRangedDmg(shopItems[2, 6]);
        }

        SaveManager.SavePlayer(playerData, this, healthTracker);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        playerCurrency += amount;
        UpdateGoldText();
        SaveManager.SavePlayer(playerData, this, healthTracker);
    }
}