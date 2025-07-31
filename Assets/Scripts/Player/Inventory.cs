using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Inventory
{
    [Header("Ammo")]
    public int ammunition;

    [Header("Favor")]
    public int favor;
    public TMP_Text favorUI;
    public TMP_Text favorAdditionUI;
    [Range(0.01f, 0.5f)]
    public float favorAddTime;
    public float favorAddTotalTime;

    [Space(5)]
    [Header("Key Items")]
    public List<KeyItem> keyItems = new List<KeyItem>();

    [Space(5)]
    [Header("Weapons")]
    public List<Weapon> meleeWeapons = new List<Weapon>();
    public List<Firearm> firearms = new List<Firearm>();

    [Space(5)]
    [Header("UI")]
    [Header("Equipment")]
    [SerializeField] protected Image meleeUIIcon;
    [SerializeField] protected Image firearmUIIcon;
    [SerializeField] protected List<Image> foundFirearms = new List<Image>();

    [Header("Ammo")]
    [SerializeField] protected TMP_Text totalAmmoUI;


    public void Start()
    {
        favorAdditionUI.gameObject.SetActive(false);
        GameEvents.current.FavorSummation(favor);
    }

    public void Update()
    {
        for(int i = 0; i < firearms.Count; i++)
        {
            foundFirearms[i].sprite = firearms[i].Icon;
        }
    }

    public void UseAmmo()
    {
        ammunition--;
        totalAmmoUI.text = ammunition.ToString();
    }

    public IEnumerator AddFavorUI(int amount)
    {
        favorAdditionUI.gameObject.SetActive(true);
        favorAdditionUI.text = "+" + amount.ToString();

        int target = favor + amount;
        float waitTime = 0f;
        while(waitTime <= 1.5f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        float addTime = 0f;
        while(favor < target)
        {
            favor++;
            favorUI.text = favor.ToString();
            yield return new WaitForSeconds(favorAddTime);

            addTime += favorAddTime;
            if(addTime >= favorAddTotalTime)
            {
                favor = target;
                favorUI.text = favor.ToString();
                break;
            }
        }

        favorAdditionUI.text = "";
        favorAdditionUI.gameObject.SetActive(false);

        GameEvents.current.FavorSummation(favor);
    }

    //public void GenerateAmmoUI(int clipSize)
    //{
    //    ClearAmmoUI();
    //    float width = ammoUIDistance * (clipSize + 1);  // 1 extra space at front and back
    //    float posBoxX = (ammoUIRange - width) / 2;

    //    ammoUIBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(width, ammoUIBackground.GetComponent<RectTransform>().sizeDelta.y);
    //    ammoUIBackground.GetComponent<RectTransform>().localPosition = new Vector3(posBoxX, 0, 0);

    //    float rightMostPosX = posBoxX + ((clipSize / 2) * ammoUIDistance) - (ammoUIDistance / 2);
    //    for(int i = 0; i < clipSize; i++)
    //    {
    //        float x = rightMostPosX - (i * ammoUIDistance);
    //        var ammo = GameObject.Instantiate(ammoUIPrefab, ammoUIContainer.GetComponent<RectTransform>());
    //        ammo.GetComponent<RectTransform>().localPosition = new Vector3(x, 0, 0);
    //        ammoUI.Add(ammo);
    //    }

    //}

    //void ClearAmmoUI()
    //{
    //    foreach(GameObject ammo in ammoUI)
    //    {
    //        GameObject.Destroy(ammo);
    //    }

    //    ammoUI.Clear();
    //}

    public void SelectMelee(Weapon weapon)
    {
        meleeUIIcon.sprite = weapon.Icon;
        //EquipMelee(weapon);
    }

    public void SelectFirearm(Firearm weapon)
    {
        firearmUIIcon.sprite = weapon.Icon;
        //EquipFirearm(weapon);
    }

    public void EquipMelee(Weapon weapon)
    {
        Player player = GameObject.FindFirstObjectByType<Player>();
        foreach(Weapon melee in player.meleeWeapons)
        {
            GameObject.Destroy(melee.gameObject);
        }

        List<Weapon> spawnedEquipment = new List<Weapon>();
        if(weapon.WieldingType == WieldingType.SINGLE)
            spawnedEquipment.Add(GameObject.Instantiate(weapon.gameObject).GetComponent<Weapon>());
        else
        {
            spawnedEquipment.Add(GameObject.Instantiate(weapon.gameObject).GetComponent<Weapon>());
            spawnedEquipment.Add(GameObject.Instantiate(weapon.gameObject).GetComponent<Weapon>());
        }

        player.meleeWeapons = spawnedEquipment.ToArray();
        player.EquipMelee();
        //player.GetComponent<Animator>().runtimeAnimatorController = weapon.overrideController;
        SelectMelee(spawnedEquipment[0]);
    }

    public void EquipFirearm(Firearm weapon)
    {
        Player player = GameObject.FindFirstObjectByType<Player>();
        foreach(Firearm firearm in player.firearms)
        {
            ammunition += firearm.currentAmmoCount; // Save ammo thats left in clip
            GameObject.Destroy(firearm.gameObject);
        }

        //int clipSize = weapon.clipSize;
        List<Firearm> spawnedEquipment = new List<Firearm>();
        if(weapon.WieldingType == WieldingType.SINGLE)
        {
            spawnedEquipment.Add(GameObject.Instantiate(weapon.gameObject).GetComponent<Firearm>());
            spawnedEquipment[0].currentAmmoCount = 0;
        }
        else
        {
            spawnedEquipment.Add(GameObject.Instantiate(weapon.gameObject).GetComponent<Firearm>());
            spawnedEquipment[0].currentAmmoCount = 0;
            spawnedEquipment.Add(GameObject.Instantiate(weapon.gameObject).GetComponent<Firearm>());
            spawnedEquipment[1].currentAmmoCount = 0;
            //clipSize = weapon.clipSize * 2;
        }

        player.firearms = spawnedEquipment.ToArray();
        player.EquipFirearms();
        //player.ReloadFirearms();
        player.GetComponent<Animator>().runtimeAnimatorController = weapon.overrideController;
        player.GetComponent<Animator>().SetLayerWeight(2, weapon.rightArmMaskStrength);
        player.GetComponent<Animator>().SetLayerWeight(3, weapon.upperBodyMaskStrength);
        SelectFirearm(spawnedEquipment[0]);
        //GenerateAmmoUI(clipSize);
    }
}
