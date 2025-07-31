using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public enum EquipmentType
{
    MELEE,
    FIREARM,
    BLESSING,
    COSMETIC
}

public class EquipmentSelectable : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public GameObject[] navigationIndicators;
    public int currentIndex = 0;
    public EquipmentType equipmentType;
    public IWeapon selectedWeapon;
    public Player player;
    public GameObject selectionCursor;

    [Header("UI")]
    public Image selectedEquipmentSprite;
    public TMP_Text selectedEuipmentName;

    private EquipmentMenu parentMenu;

    public bool Selected { get; private set; }


    void Start()
    {
        Deselect();
        parentMenu = transform.parent.GetComponent<EquipmentMenu>();
    }

    public void Navigate(int index)
    {
        currentIndex += index;
        if(equipmentType == EquipmentType.MELEE)
        {
            if(currentIndex < 0)
                currentIndex = player.inventory.meleeWeapons.Count - 1;
            else if(currentIndex >= player.inventory.meleeWeapons.Count)
                currentIndex = 0;

            selectedWeapon = player.inventory.meleeWeapons[currentIndex];
            print("Navigate to index " + currentIndex + " for " + selectedWeapon.Name);
            parentMenu.HighlightEquipment();
        }
        else if(equipmentType == EquipmentType.FIREARM)
        {
            if(currentIndex < 0)
                currentIndex = player.inventory.firearms.Count - 1;
            else if(currentIndex >= player.inventory.firearms.Count)
                currentIndex = 0;

            selectedWeapon = player.inventory.firearms[currentIndex];
            print("Navigate to index " + currentIndex + " for " + selectedWeapon.Name);
            parentMenu.HighlightEquipment();
        }

        Vector3 cursorPos = selectionCursor.transform.localPosition;
        cursorPos.x = -150f + (currentIndex * 75f);
        selectionCursor.transform.localPosition = cursorPos;
    }

    public void OnSelect(BaseEventData eventData)
    {
        Select();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Deselect();
    }

    public void Select()
    {
        if(parentMenu == null)
            parentMenu = transform.parent.GetComponent<EquipmentMenu>();

        Selected = true;
        //currentIndex = 0;
        parentMenu.selectedEquipment = this;
        Navigate(0);

        foreach(GameObject indicator in navigationIndicators)
        {
            indicator.SetActive(true);
        }
    }

    public void Deselect()
    {
        Selected = false;
        foreach(GameObject indicator in navigationIndicators)
        {
            indicator.SetActive(false);
        }
    }
}
