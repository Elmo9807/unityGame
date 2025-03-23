using UnityEngine;

// Helper class to load and configure weapon prefabs
public class WeaponLoader : MonoBehaviour
{
    [Header("Weapon Prefabs")]
    public GameObject arrowPrefab;

    private static WeaponLoader _instance;

    public static WeaponLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject loaderObject = new GameObject("WeaponLoader");
                _instance = loaderObject.AddComponent<WeaponLoader>();
                DontDestroyOnLoad(loaderObject);

                // Try to find arrow prefab in scene, must be set to Resource folder
                _instance.arrowPrefab = Resources.Load<GameObject>("PlayerProjectile");
                Debug.Log($"Loaded arrowPrefab: {(_instance.arrowPrefab != null ? "Success" : "Failed")}");

                if (_instance.arrowPrefab == null)
                {
                    Debug.LogWarning("Arrow prefab not found in Resources. Will need manual assignment.");
                }
            }
            return _instance;
        }
    }

    public Bow CreateBow(string bowType = "basic")
    {
        Bow bow = new Bow();
        bow.arrowPrefab = arrowPrefab;

        // Configure different bow types with appropriate damage multipliers, we can mess around with this in the shop too
        switch (bowType.ToLower())
        {
            case "basic":
                bow.Name = "Basic Bow";
                bow.Description = "A simple wooden bow that fires arrows.";
                bow.Damage = 15; // Keep for compatibility
                bow.damageMultiplier = 1.0f;
                bow.arrowSpeed = 15f;
                bow.cooldownTime = 0.7f;
                bow.CurrencyValue = 20;
                break;

            case "hunting":
                bow.Name = "Hunting Bow";
                bow.Description = "A well-crafted bow that provides better damage.";
                bow.Damage = 22; // Keep for compatibility
                bow.damageMultiplier = 1.5f;
                bow.arrowSpeed = 16f;
                bow.cooldownTime = 0.6f;
                bow.CurrencyValue = 40;
                break;

            case "longbow":
                bow.Name = "Longbow";
                bow.Description = "A powerful longbow with exceptional range and damage.";
                bow.Damage = 30; // Keep for compatibility
                bow.damageMultiplier = 2.0f;
                bow.arrowSpeed = 20f;
                bow.cooldownTime = 1.0f;
                bow.CurrencyValue = 75;
                break;

            default:
                bow.Name = "Basic Bow";
                bow.Description = "A simple wooden bow that fires arrows.";
                bow.Damage = 15;
                bow.damageMultiplier = 1.0f;
                bow.arrowSpeed = 15f;
                bow.cooldownTime = 0.7f;
                bow.CurrencyValue = 20;
                break;
        }

        return bow;
    }

    public HealingPotion CreateHealingPotion(string potionType = "standard")
    {
        HealingPotion potion = new HealingPotion();

        switch (potionType.ToLower())
        {
            case "minor":
                potion.Name = "Minor Healing Potion";
                potion.Description = "Restores a small amount of health.";
                potion.HealthGain = 20;
                potion.CurrencyValue = 15;
                break;

            case "major":
                potion.Name = "Major Healing Potion";
                potion.Description = "Restores a significant amount of health.";
                potion.HealthGain = 50;
                potion.CurrencyValue = 40;
                break;

            default: // standard healing pot
                potion.Name = "Healing Potion";
                potion.Description = "Restores health.";
                potion.HealthGain = 30;
                potion.CurrencyValue = 25;
                break;
        }

        potion.Icon = "potion_icon";
        return potion;
    }
}