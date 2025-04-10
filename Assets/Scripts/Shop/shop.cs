using UnityEngine;
using UnityEngine.UI;

public class shop : MonoBehaviour
{
    public GameObject shopTxt;
    public bool playerIsClose;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerIsClose)
        {
            if (GameManager.Instance.shopPanel.activeInHierarchy)
            {
                GameManager.Instance.HideShopUi();
            }
            else
            {
                GameManager.Instance.ShowShopUi();
            }
        }

        if (GameManager.Instance.shopPanel.activeInHierarchy)
        {
            shopTxt.SetActive(false);
        }

        if (!playerIsClose)
        {
            GameManager.Instance.HideShopUi();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsClose = true;
            shopTxt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsClose = false;
            shopTxt.SetActive(false);
        }
    }
}
