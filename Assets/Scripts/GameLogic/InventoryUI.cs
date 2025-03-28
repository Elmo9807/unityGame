using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform weaponSlotsParent;
    public Transform potionSlotsParent;
    public Transform consumableSlotsParent;
    public GameObject itemSlotPrefab;

    [Header("Player Reference")]
    public PlayerController playerController;

    private Player playerData;
    private bool isInventoryOpen = false;

    private List<GameObject> weaponSlots = new List<GameObject>();
    private List<GameObject> potionSlots = new List<GameObject>();
    private List<GameObject> consumableSlots = new List<GameObject>();

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("InventoryUI: Inventory panel not assigned!");
        }

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();

            if (playerController == null)
            {
                Debug.LogError("InventoryUI: Could not find PlayerController!");
            }
        }

        if (weaponSlotsParent == null)
        {
            GameObject weaponSlotsObj = new GameObject("WeaponSlots");
            weaponSlotsObj.transform.SetParent(transform);
            weaponSlotsParent = weaponSlotsObj.transform;
        }

        if (potionSlotsParent == null)
        {
            GameObject potionSlotsObj = new GameObject("PotionSlots");
            potionSlotsObj.transform.SetParent(transform);
            potionSlotsParent = potionSlotsObj.transform;
        }

        if (consumableSlotsParent == null)
        {
            GameObject consumableSlotsObj = new GameObject("ConsumableSlots");
            consumableSlotsObj.transform.SetParent(transform);
            consumableSlotsParent = consumableSlotsObj.transform;
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            RefreshInventory();
        }
    }

    public void RefreshInventory()
    {
        if (playerController == null) return;

        playerData = playerController.GetPlayerData();
        if (playerData == null) return;

        ClearSlots(weaponSlots);
        FillWeaponSlots();

        ClearSlots(potionSlots);
        FillPotionSlots();

        ClearSlots(consumableSlots);
        FillConsumableSlots();
    }

    private void ClearSlots(List<GameObject> slots)
    {
        foreach (GameObject slot in slots)
        {
            Destroy(slot);
        }
        slots.Clear();
    }

    private void FillWeaponSlots()
    {
        if (playerData.inventory == null) return;

        for (int i = 0; i < playerData.inventory.weapons.Count; i++)
        {
            Weapon weapon = playerData.inventory.weapons[i];
            GameObject slotObject = CreateItemSlot(weaponSlotsParent);

            Text nameText = slotObject.GetComponentInChildren<Text>();
            if (nameText != null)
            {
                nameText.text = weapon.Name;
            }

            Button button = slotObject.GetComponent<Button>();
            if (button != null)
            {
                int index = i;
                button.onClick.AddListener(() => {
                    playerData.inventory.EquipWeapon(index, playerData);
                    RefreshInventory();
                });
            }

            weaponSlots.Add(slotObject);
        }
    }

    private void FillPotionSlots()
    {
        if (playerData.inventory == null) return;

        for (int i = 0; i < playerData.inventory.healingPotions.Count; i++)
        {
            HealingPotion potion = playerData.inventory.healingPotions[i];
            GameObject slotObject = CreateItemSlot(potionSlotsParent);

            Text nameText = slotObject.GetComponentInChildren<Text>();
            if (nameText != null)
            {
                nameText.text = $"{potion.Name} (+{potion.HealthGain})";
            }

            Button button = slotObject.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => {
                    playerData.inventory.UseHealingPotion(playerData);
                    RefreshInventory();
                });
            }

            potionSlots.Add(slotObject);
        }
    }

    private void FillConsumableSlots()
    {
        if (playerData.inventory == null) return;

        for (int i = 0; i < playerData.inventory.consumables.Count; i++)
        {
            Item consumable = playerData.inventory.consumables[i];
            GameObject slotObject = CreateItemSlot(consumableSlotsParent);

            Text nameText = slotObject.GetComponentInChildren<Text>();
            if (nameText != null)
            {
                nameText.text = consumable.Name;
            }

            Button button = slotObject.GetComponent<Button>();
            if (button != null)
            {
                int index = i;
                button.onClick.AddListener(() => {
                    playerData.inventory.UseConsumable(playerData);
                    RefreshInventory();
                });
            }

            consumableSlots.Add(slotObject);
        }
    }

    private GameObject CreateItemSlot(Transform parent)
    {
        GameObject slotObject;

        if (itemSlotPrefab != null)
        {
            slotObject = Instantiate(itemSlotPrefab, parent);
        }
        else
        {
            slotObject = new GameObject("ItemSlot");
            slotObject.transform.SetParent(parent, false);

            RectTransform rectTransform = slotObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 30);

            Image background = slotObject.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Button button = slotObject.AddComponent<Button>();
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
            button.colors = colors;

            GameObject textObject = new GameObject("ItemText");
            textObject.transform.SetParent(slotObject.transform, false);

            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);

            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.white;
        }

        return slotObject;
    }
}