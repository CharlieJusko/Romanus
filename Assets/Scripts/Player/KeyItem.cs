using UnityEngine;
using UnityEngine.UI;

public class KeyItem : IItem
{
    [SerializeField] string itemName;
    [SerializeField] Image uiIcon;
    [SerializeField] string description;

    public string Name { get => itemName; set => itemName = value; }
    public Image Icon { get => uiIcon; set => uiIcon = value; }
    public string Description { get => description; set => description = value; }

    public void Pickup(Player player)
    {
        player.inventory.keyItems.Add(this);
        //throw new System.NotImplementedException();
    }

    public void Use()
    {
        //throw new System.NotImplementedException();
    }
}
