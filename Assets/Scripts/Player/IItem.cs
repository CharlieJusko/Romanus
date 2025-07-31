using UnityEngine;
using UnityEngine.UI;

public interface IItem
{
    public string Name { get; set; }
    public Image Icon { get; set; }
    public string Description {  get; set; }
    public void Use();
    public void Pickup(Player player);
}
