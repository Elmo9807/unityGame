using UnityEngine;
using UnityEngine.UI;

public class ButtonInfo : MonoBehaviour
{
    public Text PriceTxt;
    public int itemId;
    public GameObject powerupManager;

    // Update is called once per frame
    void Update()
    {
        PriceTxt.text = "Price: " + powerupManager.GetComponent<PowerupManager>().shopItems[2, itemId].ToString();
    }
}
