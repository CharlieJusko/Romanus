using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentMenu : Menu
{
    public Player player;
    public TMP_Text euipmentUIName;
    public Image equipmentUIIcon;
    public TMP_Text equipmentUIDescription;
    public EquipmentSelectable selectedEquipment;


    public override void OnEnterMenu() 
    {
        EquipmentSelectable selectable;

        foreach(Transform child in transform)
        {
            if(child.TryGetComponent(out selectable))
                selectable.Deselect();
        }

        if(firstButton.TryGetComponent(out selectable))
        {
            selectable.Select();
        }
    }

    public void Navigate(int index)
    {
        if(selectedEquipment != null)
        {
            selectedEquipment.Navigate(index);
        }
    }

    public void HighlightEquipment()
    {
        if(selectedEquipment != null)
        {
            euipmentUIName.text = selectedEquipment.selectedWeapon.Name;
            equipmentUIIcon.sprite = selectedEquipment.selectedWeapon.Icon;
            equipmentUIDescription.text = selectedEquipment.selectedWeapon.Description;
        }
    }

    #region Button Clicks
    public void MeleeEquipmentClick()
    {
        //Weapon melee = player.inventory.meleeWeapons[0];
        euipmentUIName.text = selectedEquipment.selectedWeapon.Name;
        equipmentUIIcon.sprite = selectedEquipment.selectedWeapon.Icon;
        equipmentUIDescription.text = selectedEquipment.selectedWeapon.Description;

        selectedEquipment.selectedEquipmentSprite.sprite = selectedEquipment.selectedWeapon.Icon;
        selectedEquipment.selectedEuipmentName.text = selectedEquipment.selectedWeapon.Name;

        // TODO
        var targetWeapon = player.inventory.meleeWeapons[selectedEquipment.currentIndex];
        if (targetWeapon.Name != player.meleeWeapons[0].Name)
            player.inventory.EquipMelee(targetWeapon);
    }

    public void FirearmEquipmentClick()
    {
        //Firearm firearm = player.inventory.firearms[0];
        euipmentUIName.text = selectedEquipment.selectedWeapon.Name;
        equipmentUIIcon.sprite = selectedEquipment.selectedWeapon.Icon;
        equipmentUIDescription.text = selectedEquipment.selectedWeapon.Description;

        selectedEquipment.selectedEquipmentSprite.sprite = selectedEquipment.selectedWeapon.Icon;
        selectedEquipment.selectedEuipmentName.text = selectedEquipment.selectedWeapon.Name;

        // TODO
        var targetWeapon = player.inventory.firearms[selectedEquipment.currentIndex];
        if(targetWeapon.Name != player.firearms[0].Name)
            player.inventory.EquipFirearm(targetWeapon);
    }
    #endregion
}
