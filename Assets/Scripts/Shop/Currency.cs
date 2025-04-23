using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance; // Singleton for global access

    public float Souls = 30; // Starting currency
    public Text SoulsTxt;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddSouls(float amount)
    {
        Souls += amount;
        UpdateUI();
    }

    public bool SpendSouls(float amount)
    {
        if (Souls >= amount)
        {
            Souls -= amount;
            UpdateUI();
            return true;
        }
        return false; // Not enough currency
    }

    void UpdateUI()
    {
        SoulsTxt.text = "Souls: " + Souls.ToString();
    }
}
