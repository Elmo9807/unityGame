using UnityEngine;
using UnityEngine.UI;

public class PowerupManager : MonoBehaviour
{
    #region Powerup Settings
    [SerializeField] private bool startWithBow = false;
    [SerializeField] private bool startWithDoubleJump = false;
    [SerializeField] private bool startWithDash = false;
    [SerializeField] private bool startWithHealingPotion = false;
    #endregion

    #region UI References
    [SerializeField] private GameObject powerupUI;
    [SerializeField] private Image bowIcon;
    [SerializeField] private Image dashIcon;
    [SerializeField] private Image doubleJumpIcon;
    [SerializeField] private Image healingPotionIcon;
    #endregion

    #region Shop
    [SerializeField] private int playerCurrency = 1000;
    [SerializeField] private Text currencyText;
    [SerializeField] private int bowPrice = 100;
    [SerializeField] private int dashPrice = 150;
    [SerializeField] private int doubleJumpPrice = 120;
    [SerializeField] private int healingPotionPrice = 50;
    #endregion

    private PlayerController playerController;
    private Player playerData;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
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
        if (currencyText != null)
            currencyText.text = "Currency: " + currencyText;
    }

    public void BuyBow()
    {
        BuyPowerup(ref playerData.hasBow, bowPrice);
    }

    public void BuyDash()
    {
        BuyPowerup(ref playerData.hasDash, dashPrice);
    }

    public void BuyDoubleJump()
    {
        BuyPowerup(ref playerData.hasDoubleJump, doubleJumpPrice);
    }

    public void BuyHealingPotion()
    {
        BuyPowerup(ref playerData.hasHealingPotion, healingPotionPrice);
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

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        playerCurrency += amount;
        UpdateGoldText();
    }
}