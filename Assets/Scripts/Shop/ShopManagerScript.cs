using UnityEngine;
using UnityEngine.UI ; 
using UnityEngine.EventSystems ; 
public class ShopManagerScript : MonoBehaviour
{
    
    public int[,] shopItems = new int[5, 5] ; //intializes rows and cols for shop 
    public float coins ;  
    public Text CoinsTxt ; 


    void Start()
    {
        CoinsTxt.text = "Coins:"  + coins.ToString();

        //These lines are creating different IDs for each item in the shop 
        shopItems[1,1] = 1 ; 
        shopItems[1,2] = 2 ;
        shopItems[1,3] = 3 ;
        shopItems[1,4] = 4 ;

        //Price 
        shopItems[2,1] = 10 ;
        shopItems[2,2] = 20 ;
        shopItems[2,3] = 30 ;
        shopItems[2,4] = 40 ;

        //Quantity 
        shopItems[3,1] = 0 ;
        shopItems[3,2] = 0 ;
        shopItems[3,3] = 0 ;
        shopItems[3,4] = 0 ;

    }

    
    public void Buy()
    {
        GameObject ButtonRef = GameObject.FindGameObjectsWithTag("Event").GetComponent<EventSystem>().currentSelectedGameObject; 

        if (coins >= shopItems[2, ButtonRef.GetComponent<ButtonInfo>().itemID])
        {
            coins -= shopItems[2, ButtonRef.GetComponent<ButtonInfo>().itemID] ; 
            shopItems[3, ButtonRef.GetComponent<ButtonInfo>().itemID]++ ; //Updates the quantity of item 
            CoinsTxt.text = "Coins:"  + coins.ToString(); // updates the coin counter 
            //Updates Text everytime something is bought 
            ButtonRef.GetComponent<ButtonInfo>().QuantityTxt.text = shopItems[3, ButtonRef.GetComponent<ButtonInfo>().itemID].ToString(); 


        }
    }
}
