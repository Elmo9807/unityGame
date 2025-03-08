using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject[] itemSlots;
    void Start()
    {
        inventoryPanel.SetActive(false);
    }

    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void UpdateInvDisplay()
    {
        //add later when UI elements are fleshed
    }
}