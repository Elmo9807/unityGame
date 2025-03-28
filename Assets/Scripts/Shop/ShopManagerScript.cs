using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopManagerScript : MonoBehaviour
{
    public Dictionary<int, HealingPotion> shopItems = new Dictionary<int, HealingPotion>();

    void Start()
    {
        shopItems.Add(1, new HealingPotion()); // Add HealingPotion to shop
    }

    public void Buy()
    {
        GameObject ButtonRef = EventSystem.current.currentSelectedGameObject;
        if (ButtonRef == null) return;

        ButtonInfo buttonInfo = ButtonRef.GetComponent<ButtonInfo>();
        if (buttonInfo == null) return;

        int itemID = buttonInfo.ItemID;
        if (!shopItems.ContainsKey(itemID)) return;

        HealingPotion potion = shopItems[itemID];
        
        if (CurrencyManager.Instance.SpendSouls(potion.HealingPotionPrice))
        {
            potion.BuyPotion(); // Increase quantity in the HealingPotion class
            buttonInfo.QuantityTxt.text = "Owned: " + potion.Quantity;
        }
    }
}
