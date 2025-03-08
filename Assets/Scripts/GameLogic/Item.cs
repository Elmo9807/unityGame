using UnityEngine;

public abstract class Item 
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int CurrencyValue { get; set; }

    public abstract void Use(Player player);
}
