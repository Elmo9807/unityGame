using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonInfo : MonoBehaviour
{
    public int ItemID;
    public Text PriceTxt;
    public Text QuantityTxt;
    public GameObject ShopManager;

    void Update()
    {
        ShopManagerScript shopManager = ShopManager.GetComponent<ShopManagerScript>();

        if (shopManager.shopItems.ContainsKey(ItemID))
        {
            HealingPotion potion = (HealingPotion)shopManager.shopItems[ItemID];
            PriceTxt.text = "Souls needed: " + potion.HealingPotionPrice;
            QuantityTxt.text = "Owned: " + potion.Quantity;
        }
    }
}
