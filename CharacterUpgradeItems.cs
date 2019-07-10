using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterUpgradeItems : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Canvas canvas;
    CharacterInventoryGUI characterInventory;
    CharacterInventoryPickupWindowGUI inventoryWindow;
    CharacterSkillBarGUI characterGUI;
    TerrainScript terrain;
    WeaponsDatabase Weapons;
    ArmorDatabase Armors;
    WeaponSwitch wepSwitch;
    ArmorSwitch armorSwitch;
    MainGUI mainGUI;
    CharacterStats Stats;
    CharacterMovement movement;

    public GameObject HoverWindowRectPrefab;
    public GameObject PreviousUpgradedIcon;
    public GameObject AfterUpgradedIcon;
    GameObject HoverRectINIT;
    string CurrentPointerEnterName;
    public int CurrentUpgradeIndex;
    int CurrentItemLocationIndex = 0; // current index of item slot
    Color CurrentRarityColor; //hoverwindow
    string AfterRarity; //hoverwindow
    Color AfterRarityColor; //hoverwindow

    int CurrentRarityIndex;
    float RarityCoeff;
    int CurrentSuccessRate;
    int CurrentDestroyRate;

    //                            50, 40, 20, 10, 5        // success rates
    //                            30, 30, 35, 35, 35        // destroy rates
    // go down a rank if failed, and not destroyed..

    bool NearACraftingStation()
    {
        bool NearCraftingStation = false;

        for (int i = 0; i < wepSwitch.CraftingStations.Count; i++)
        {
            if (Vector3.Distance(wepSwitch.CraftingStations[i].transform.position, terrain.Player.transform.position) < 3)
            {
                if (wepSwitch.CraftingStations[i].name == "Stone Workbench(Clone)")
                {
                    NearCraftingStation = true;
                }
            }
        }

        return NearCraftingStation;
    }

    public void CreateRarity()
    {
        int RandomNumber = Random.Range(0, 101);

        for (int i = 0; i < characterInventory.InventoryManage.Count; i++)
        {
            if (CurrentUpgradeIndex == characterInventory.InventoryManage[i].CurrentInventorySlot)
                CurrentItemLocationIndex = i;
        }

        if (CurrentItemLocationIndex == -1)
            return;

        switch (characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity)
        {
            case "Common":
                if (RandomNumber <= 40)
                {
                    characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity = "Rare";
                    RarityCoeff = 1.04f;
                    mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("Successful upgrade."));
                }
                else
                { 
                    RandomNumber = Random.Range(0, 101);
                    if (RandomNumber <= 15) //destroyed
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText(characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName + " is destroyed in the process."));
                        characterInventory.InventoryManage.RemoveAt(CurrentItemLocationIndex);
                        characterInventory.InventoryButtonsIcons[CurrentUpgradeIndex].sprite = characterInventory.DefaultSprite;
                        characterInventory.InventoryButtonsIcons[CurrentUpgradeIndex].transform.GetComponentInChildren<Mask>().showMaskGraphic = false;
                        characterInventory.InventoryButtonRects[CurrentUpgradeIndex].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
                        CurrentItemLocationIndex = -1;
                        CurrentUpgradeIndex = -1;
                        PreviousUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                        PreviousUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                        AfterUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                        AfterUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                    }
                    else
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("That didn't seem to do anything."));
                    }
                }
                break;
            case "Rare":
                if (RandomNumber <= 25)
                {
                    characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity = "Epic";
                    RarityCoeff = 1.08f;
                    mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("Successful upgrade."));
                }
                else
                { 
                    RandomNumber = Random.Range(0, 101);
                    if (RandomNumber <= 20) //destroyed
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText(characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName + " is destroyed in the process."));
                        characterInventory.InventoryManage.RemoveAt(CurrentItemLocationIndex);
                        characterInventory.InventoryButtonsIcons[CurrentUpgradeIndex].sprite = characterInventory.DefaultSprite;
                        characterInventory.InventoryButtonsIcons[CurrentUpgradeIndex].transform.GetComponentInChildren<Mask>().showMaskGraphic = false;
                        characterInventory.InventoryButtonRects[CurrentUpgradeIndex].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
                        CurrentItemLocationIndex = -1;
                        CurrentUpgradeIndex = -1;
                        PreviousUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                        PreviousUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                        AfterUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                        AfterUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                    }
                    else
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("That didn't seem to do anything."));
                    }
                }
                break;
            case "Epic":
                if (RandomNumber <= 20)
                {
                    characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity = "Unique";
                    RarityCoeff = 1.12f;
                    mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("Successful upgrade."));
                }
                else
                { 
                    RandomNumber = Random.Range(0, 101);
                    if (RandomNumber <= 25) //destroyed
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText(characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName + " is destroyed in the process."));
                        characterInventory.InventoryManage.RemoveAt(CurrentItemLocationIndex);
                        characterInventory.InventoryButtonsIcons[CurrentUpgradeIndex].sprite = characterInventory.DefaultSprite;
                        characterInventory.InventoryButtonsIcons[CurrentUpgradeIndex].transform.GetComponentInChildren<Mask>().showMaskGraphic = false;
                        characterInventory.InventoryButtonRects[CurrentUpgradeIndex].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
                        CurrentItemLocationIndex = -1;
                        CurrentUpgradeIndex = -1;
                        PreviousUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                        PreviousUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                        AfterUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                        AfterUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                    }
                    else
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("That didn't seem to do anything."));
                    }
                }
                break;
            case "Unique":
                if (RandomNumber <= 15)
                {
                    characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity = "Legendary";
                    RarityCoeff = 1.16f;
                    mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("Successful upgrade."));
                }
                else
                { 
                    RandomNumber = Random.Range(0, 101);
                    if (RandomNumber <= 30) //destroyed
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText(characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName + " is destroyed in the process."));
                        characterInventory.InventoryManage.RemoveAt(CurrentItemLocationIndex);
                        characterInventory.InventoryButtonsIcons[CurrentUpgradeIndex].sprite = characterInventory.DefaultSprite;
                        characterInventory.InventoryButtonsIcons[CurrentUpgradeIndex].transform.GetComponentInChildren<Mask>().showMaskGraphic = false;
                        characterInventory.InventoryButtonRects[CurrentUpgradeIndex].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
                        CurrentItemLocationIndex = -1;
                        CurrentUpgradeIndex = -1;
                        PreviousUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                        PreviousUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                        AfterUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                        AfterUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                    }
                    else
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("That didn't seem to do anything."));
                    }
                }
                break;
            case "Legendary":
                if (RandomNumber <= 10)
                {
                    characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity = "Mythic";
                    RarityCoeff = 1.20f;
                    mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("Successful upgrade."));
                }
                else
                { 
                    RandomNumber = Random.Range(0, 101);
                    if (RandomNumber <= 30) //destroyed
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText(characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName + " is destroyed in the process."));
                        characterInventory.InventoryManage.RemoveAt(CurrentItemLocationIndex);
                        characterInventory.InventoryButtonsIcons[CurrentUpgradeIndex].sprite = characterInventory.DefaultSprite;
                        characterInventory.InventoryButtonsIcons[CurrentUpgradeIndex].transform.GetComponentInChildren<Mask>().showMaskGraphic = false;
                        CurrentItemLocationIndex = -1;
                        CurrentUpgradeIndex = -1;
                        PreviousUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                        PreviousUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                        AfterUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                        AfterUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                    }
                    else
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("That didn't seem to do anything."));
                    }
                }
                break;
            default: // mythic
                RarityCoeff = 1.20f;
                break;
        }
        if (CurrentItemLocationIndex != -1 && characterInventory.InventoryManage[CurrentItemLocationIndex].CritRate > 0) // upgrade weapon
        {
            for (int i = 0; i < Weapons.WeaponList.Count; i++)
            {
                if (characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName == Weapons.WeaponList[i].WeaponName)
                {
                    characterInventory.InventoryManage[CurrentItemLocationIndex].DamageOrValue = Mathf.Round(Weapons.WeaponList[i].WeaponDamage * RarityCoeff);
                    characterInventory.InventoryManage[CurrentItemLocationIndex].WeaponAttackSpeed = Mathf.Round(Weapons.WeaponList[i].WeaponAttackSpeed * RarityCoeff);
                    characterInventory.InventoryManage[CurrentItemLocationIndex].CritRate = Mathf.Round(Weapons.WeaponList[i].CritRate * RarityCoeff);
                    characterInventory.InventoryManage[CurrentItemLocationIndex].ArmorPenetration = Mathf.Round(Weapons.WeaponList[i].ArmorPenetration * RarityCoeff);
                }
            }

            HoverRarity(characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity, "Weapon");
            transform.Find("UpgradeChance").GetComponentInChildren<Text>().text = "Success: " + CurrentSuccessRate.ToString() + "% \n" + "Destroy(Failure): " + CurrentDestroyRate.ToString() + "% ";
        }
        else if (CurrentItemLocationIndex != -1 && characterInventory.InventoryManage[CurrentItemLocationIndex].Defense > 0) // upgrade armor
        {
            for (int i = 0; i < Armors.ArmorList.Count; i++)
            {
                if (characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName == Armors.ArmorList[i].ArmorName)
                {
                    characterInventory.InventoryManage[CurrentItemLocationIndex].Health = Armors.ArmorList[i].BonusHealth * RarityCoeff;
                    characterInventory.InventoryManage[CurrentItemLocationIndex].Stamina = Armors.ArmorList[i].BonusStamina * RarityCoeff;
                    characterInventory.InventoryManage[CurrentItemLocationIndex].Defense = Mathf.Round(Armors.ArmorList[i].DefenseValues * RarityCoeff);
                }
            }
            HoverRarity(characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity, "Armor");
            transform.Find("UpgradeChance").GetComponentInChildren<Text>().text = "Success: " + CurrentSuccessRate.ToString() + "% \n" + "Destroy(Failure): " + CurrentDestroyRate.ToString() + "% ";
        }
    }

    public void HoverRarity(string Rarity, string type) 
    {
        switch (Rarity)
        {
            case "Common":
                AfterRarity = "Rare " + type;
                CurrentSuccessRate = 40;
                CurrentDestroyRate = 15;
                AfterRarityColor = Color.blue;
                CurrentRarityColor = Color.white;
                RarityCoeff = 1.04f;
                break;
            case "Rare":
                AfterRarity = "Epic " + type;
                CurrentSuccessRate = 25;
                CurrentDestroyRate = 20;
                AfterRarityColor = Color.magenta;
                CurrentRarityColor = Color.blue;
                RarityCoeff = 1.08f;
                break;
            case "Epic":
                AfterRarity = "Unique " + type;
                CurrentSuccessRate = 20;
                CurrentDestroyRate = 25;
                AfterRarityColor = Color.red;
                CurrentRarityColor = Color.magenta;
                RarityCoeff = 1.12f;
                break;
            case "Unique":
                AfterRarity = "Legendary " + type;
                CurrentSuccessRate = 15;
                CurrentDestroyRate = 30;
                AfterRarityColor = Color.green;
                CurrentRarityColor = Color.red;
                RarityCoeff = 1.16f;
                break;
            case "Legendary":
                AfterRarity = "Mythic " + type;
                CurrentSuccessRate = 10;
                CurrentDestroyRate = 30;
                AfterRarityColor = Color.cyan;
                CurrentRarityColor = Color.green;
                RarityCoeff = 1.20f;
                break;
            case "Mythic":
                AfterRarity = "Mythic " + type;
                CurrentSuccessRate = 0;
                CurrentDestroyRate = 0;
                AfterRarityColor = Color.cyan;
                CurrentRarityColor = Color.cyan;
                RarityCoeff = 1.20f;
                break;
        }
    }

    IEnumerator PointerStay() 
    {
        yield return null;

        if (inventoryWindow.CurrentUpgradeItemSlot != -1)
        {
            for (int i = 0; i < characterInventory.InventoryManage.Count; i++) // upgrade item
            {
                if (inventoryWindow.CurrentUpgradeItemSlot == characterInventory.InventoryManage[i].CurrentInventorySlot && 
                    characterInventory.InventoryManage[i].isASecondary == 0 && (characterInventory.InventoryManage[i].DamageOrValue > 0 ||
                    characterInventory.InventoryManage[i].Defense > 0) && characterInventory.InventoryManage[i].Rarity != "Mythic")
                {
                    //if we are wearing the item to be upgraded, then destroy and make icon into uitexture
                    if (inventoryWindow.CurrentUpgradeItemSlot == armorSwitch.CurrentHelmetIteration)
                    {
                        characterInventory.InventoryButtonRects[characterInventory.InventoryManage[i].CurrentInventorySlot].transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.66f);
                        armorSwitch.DestroyObject(armorSwitch.CurrentHelmet, 1);
                    }
                    else if (inventoryWindow.CurrentUpgradeItemSlot == armorSwitch.CurrentChestplateIteration)
                    {
                        characterInventory.InventoryButtonRects[characterInventory.InventoryManage[i].CurrentInventorySlot].transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.66f);
                        armorSwitch.DestroyObject(armorSwitch.CurrentChestplate, 0);
                    }
                    else if (inventoryWindow.CurrentUpgradeItemSlot == armorSwitch.CurrentLegsIteration)
                    {
                        characterInventory.InventoryButtonRects[characterInventory.InventoryManage[i].CurrentInventorySlot].transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.66f);
                        armorSwitch.DestroyObject(armorSwitch.CurrentLegs, 2);
                    }
                    else if (inventoryWindow.CurrentUpgradeItemSlot == wepSwitch.CurrentWeaponItemSlot)
                    {
                        characterInventory.InventoryButtonRects[characterInventory.InventoryManage[i].CurrentInventorySlot].transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.66f);
                        wepSwitch.DropObject(wepSwitch.CurrentWeaponItemSlot);
                    }

                    PreviousUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.InventoryManage[i].tSprite;
                    PreviousUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;

                    AfterUpgradedIcon.transform.GetComponent<Image>().sprite = characterInventory.InventoryManage[i].tSprite;
                    AfterUpgradedIcon.transform.GetComponent<Mask>().showMaskGraphic = true;
                    if (characterInventory.InventoryManage[i].Defense > 0)
                        HoverRarity(characterInventory.InventoryManage[i].Rarity, "Armor");
                    else
                        HoverRarity(characterInventory.InventoryManage[i].Rarity, "Weapon");
                    transform.Find("UpgradeChance").GetComponentInChildren<Text>().text = "Success: " + CurrentSuccessRate.ToString() + "% \n" + "Destroy(Failure): " + CurrentDestroyRate.ToString() + "% ";
                    CurrentPointerEnterName = string.Empty;
                    CurrentUpgradeIndex = inventoryWindow.CurrentUpgradeItemSlot;
                    inventoryWindow.CurrentUpgradeItemSlot = -1;
                    inventoryWindow.UpgradeIndex = -1;
                    break;                   
                }
                else if (inventoryWindow.CurrentUpgradeItemSlot == characterInventory.InventoryManage[i].CurrentInventorySlot &&
                    characterInventory.InventoryButtonsIcons[characterInventory.InventoryManage[i].CurrentInventorySlot].sprite.name.Contains("Shard")) // remove shard and then attempt upgrade
                {
                    if (characterInventory.InventoryManage[i].Rarity + " Weapon" == AfterRarity ||
                        characterInventory.InventoryManage[i].Rarity + " Armor" == AfterRarity)
                    {
                        if (characterGUI.NearACraftingStation("Shard") == false || characterGUI.EquippedCraftingTool("Blacksmithing") == false)
                        {
                            break;
                        }

                        GameObject shard = PhotonNetwork.Instantiate(characterInventory.InventoryManage[i].SlotName, characterGUI.CurrentCraftingStation.transform.position + new Vector3(0, 2, 0), Quaternion.identity, 0);
                        shard.GetComponent<Collider>().isTrigger = true;
                        shard.GetComponent<Rigidbody>().isKinematic = true;
                        shard.transform.SetParent(characterGUI.CurrentCraftingStation.transform);
                        shard.transform.localPosition = new Vector3(-0.5f,0.5f,0.8f);
                        shard.transform.localRotation = Quaternion.Euler(-90, 0, 0);

                        GameObject weapon = PhotonNetwork.Instantiate(characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName, characterGUI.CurrentCraftingStation.transform.position + new Vector3(0, 2, 0), Quaternion.identity, 0);
                        weapon.GetComponent<Collider>().isTrigger = true;
                        weapon.GetComponent<Rigidbody>().isKinematic = true;
                        weapon.transform.SetParent(characterGUI.CurrentCraftingStation.transform);
                        if (characterInventory.InventoryManage[i].Rarity + " Armor" == AfterRarity)
                        {
                            if (characterInventory.InventoryManage[i].SlotName.Contains("Chest"))
                            {
                                weapon.transform.localPosition = new Vector3(-0.447f, -1.059f, 0.012f);
                                weapon.transform.localRotation = Quaternion.Euler(-90, 0, 90);
                            }
                            else if (characterInventory.InventoryManage[i].SlotName.Contains("Helmet"))
                            {
                                weapon.transform.localPosition = new Vector3(-0.9f, -1.862f, 0.6f);
                                weapon.transform.localRotation = Quaternion.Euler(0, 0, 0);
                            }
                            else if (characterInventory.InventoryManage[i].SlotName.Contains("Legs"))
                            {
                                weapon.transform.localPosition = new Vector3(-0.9f, -1.862f, 0.6f);
                                weapon.transform.localRotation = Quaternion.Euler(0, 0, 0);
                            }
                        }
                        else
                        {
                            weapon.transform.localPosition = new Vector3(-0.9f, 0.5f, 0.4f);
                            weapon.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                        }

                        mainGUI.timeElapsedProgressBar = 4.9f + Time.time;
                        mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.055f, "Upgrading " + characterInventory.InventoryManage[i].SlotName));
                        characterGUI.StartCoroutine("CheckMovement");
                        //if (Tools.ToolList[a].ToolId != 1 && Tools.ToolList[a].ToolId != 850)
                        movement.NetworkSetHandAnimations("Crafting", 3);

                        //characterGUI.StartCoroutine(characterGUI.CraftingComponents("Tools", a, 1)); // crafting components shown on table/furnace
                        yield return new WaitForSeconds(3.925f);

                        movement.NetworkSetHandAnimations("Crafting", 0); // crafting animation 0
                        mainGUI.CheckMovementProgressBar = false;

                        PhotonNetwork.Destroy(shard);
                        PhotonNetwork.Destroy(weapon);

                        characterInventory.InventoryButtonsIcons[characterInventory.InventoryManage[i].CurrentInventorySlot].sprite = characterInventory.DefaultSprite;
                        characterInventory.InventoryButtonsIcons[characterInventory.InventoryManage[i].CurrentInventorySlot].transform.GetComponentInChildren<Mask>().showMaskGraphic = false;
                        characterInventory.InventoryButtonRects[characterInventory.InventoryManage[i].CurrentInventorySlot].transform.GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
                        characterInventory.InventoryManage.RemoveAt(i);

                        CreateRarity();
                    }
                    else
                    {
                        mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("You must use the correct shard."));
                    }

                    break;
                }
            }
        }

        if (CurrentPointerEnterName != PreviousUpgradedIcon.name)
            StopCoroutine(PointerStay());
        else if (CurrentPointerEnterName == PreviousUpgradedIcon.name)
            StartCoroutine(PointerStay());


    }

    public void OnPointerEnter(PointerEventData data) // hovered over upgrade window
    {
        if (data.pointerEnter.name == "UpgradeItemPrevious")
        {
            CurrentPointerEnterName = data.pointerEnter.name;
            StartCoroutine(PointerStay());
            inventoryWindow.UpgradeIndex = 1;

            if (data.pointerEnter.GetComponentInChildren<Image>().sprite != characterInventory.DefaultSprite)
            {
                float x = (10 + HoverRectINIT.transform.GetComponent<RectTransform>().rect.width) * terrain.canvas.GetComponent<Canvas>().scaleFactor;
                float y = (95 + HoverRectINIT.transform.GetComponent<RectTransform>().rect.height / 2) * terrain.canvas.GetComponent<Canvas>().scaleFactor;
                HoverRectINIT.transform.position = data.pointerEnter.GetComponent<RectTransform>().position + new Vector3(x, y, 0);
                HoverRectINIT.GetComponentInChildren<Image>().enabled = true;
                HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().enabled = true;
                HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().enabled = true;
                HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().enabled = true;

                for (int i = 0; i < characterInventory.InventoryManage.Count; i++)
                {
                    if (CurrentUpgradeIndex == characterInventory.InventoryManage[i].CurrentInventorySlot)
                        CurrentItemLocationIndex = i;
                }

                for (int i = 0; i < Weapons.WeaponList.Count; i++)
                {
                    if (characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName == Weapons.WeaponList[i].WeaponName)
                    {
                        HoverRarity(characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity, "Weapon");
                        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName;
                        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = CurrentRarityColor;
                        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity;
                        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().color = CurrentRarityColor;
                        HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().text = "Level/Crafting Requirement: " + Weapons.WeaponList[i].LevelRank;
                        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().text = "+" + characterInventory.InventoryManage[CurrentItemLocationIndex].DamageOrValue + " Weapon Damage";
                        HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
                        if (Weapons.WeaponList[i].IsASecondaryWeapon == 0)
                        {
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().text = "+" + characterInventory.InventoryManage[CurrentItemLocationIndex].ArmorPenetration.ToString() + " Armor Penetration" + "\n";
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().text = "+" + characterInventory.InventoryManage[CurrentItemLocationIndex].WeaponAttackSpeed.ToString() + " Attack Speed";
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().text = "+" + characterInventory.InventoryManage[CurrentItemLocationIndex].CritRate.ToString() + " Critical Chance";
                        }
                        else
                        {
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = false;
                        }
                    }
                }
                for (int i = 0; i < Armors.ArmorList.Count; i++)
                {
                    if (characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName == Armors.ArmorList[i].ArmorName)
                    {
                        HoverRarity(characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity, "Armor");
                        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = Armors.ArmorList[i].ArmorName;
                        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = CurrentRarityColor;
                        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity + "Armor";
                        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().color = CurrentRarityColor;
                        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
                        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
                        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().text = "Level/Crafting Requirement: " + Armors.ArmorList[i].LevelRank;
                        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().text = "+" + characterInventory.InventoryManage[CurrentItemLocationIndex].Defense + " Defense";
                        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().text = "+" + Mathf.Round(characterInventory.InventoryManage[CurrentItemLocationIndex].Health * Stats.tempHealth) + " Health";
                        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().text = "+" + Mathf.Round(characterInventory.InventoryManage[CurrentItemLocationIndex].Stamina * Stats.tempStamina) + " Stamina";
                    }
                }
            }
        }

        else if (data.pointerEnter.name == "UpgradeItemAfter")
        {
            if (data.pointerEnter.GetComponentInChildren<Image>().sprite != characterInventory.DefaultSprite)
            {
                float x = (10 + HoverRectINIT.transform.GetComponent<RectTransform>().rect.width) * terrain.canvas.GetComponent<Canvas>().scaleFactor;
                float y = (95 + HoverRectINIT.transform.GetComponent<RectTransform>().rect.height / 2) * terrain.canvas.GetComponent<Canvas>().scaleFactor;
                HoverRectINIT.transform.position = data.pointerEnter.GetComponent<RectTransform>().position + new Vector3(x, y, 0);
                HoverRectINIT.GetComponentInChildren<Image>().enabled = true;
                HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().enabled = true;
                HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().enabled = true;
                HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().enabled = true;

                for (int i = 0; i < characterInventory.InventoryManage.Count; i++)
                {
                    if (CurrentUpgradeIndex == characterInventory.InventoryManage[i].CurrentInventorySlot)
                        CurrentItemLocationIndex = i;
                }

                for (int i = 0; i < Weapons.WeaponList.Count; i++)
                {
                    if (characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName == Weapons.WeaponList[i].WeaponName)
                    {
                        HoverRarity(characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity, "Weapon");
                        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName;
                        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = AfterRarityColor;
                        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = AfterRarity;
                        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().color = AfterRarityColor;
                        HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().text = "Level/Crafting Requirement: " + Weapons.WeaponList[i].LevelRank;
                        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().text = "+" + Mathf.Round(Weapons.WeaponList[i].WeaponDamage * RarityCoeff) + " Weapon Damage";
                        HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
                        if (Weapons.WeaponList[i].IsASecondaryWeapon == 0)
                        {
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().text = "+" + Mathf.Round(Weapons.WeaponList[i].ArmorPenetration * RarityCoeff) + " Armor Penetration" + "\n";
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().text = "+" + Mathf.Round(Weapons.WeaponList[i].WeaponAttackSpeed * RarityCoeff) + " Attack Speed";
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().text = "+" + Mathf.Round(Weapons.WeaponList[i].CritRate * RarityCoeff) + " Critical Chance";
                        }
                        else
                        {
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = false;
                        }
                    }
                }
                for (int i = 0; i < Armors.ArmorList.Count; i++)
                {
                    if (characterInventory.InventoryManage[CurrentItemLocationIndex].SlotName == Armors.ArmorList[i].ArmorName)
                    {
                        HoverRarity(characterInventory.InventoryManage[CurrentItemLocationIndex].Rarity, "Armor");
                        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = Armors.ArmorList[i].ArmorName;
                        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = AfterRarityColor;
                        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = AfterRarity;
                        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().color = AfterRarityColor;
                        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
                        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
                        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = true;
                        HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().text = "Level/Crafting Requirement: " + Armors.ArmorList[i].LevelRank;
                        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().text = "+" + Mathf.Round(Armors.ArmorList[i].DefenseValues * RarityCoeff) + " Defense";
                        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().text = "+" + Mathf.Round(Stats.tempHealth * Armors.ArmorList[i].BonusHealth * RarityCoeff) + " Health";
                        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().text = "+" + Mathf.Round(Stats.tempStamina * Armors.ArmorList[i].BonusStamina * RarityCoeff) + " Stamina";
                    }
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (HoverRectINIT != null)
        {
            HoverRectINIT.GetComponentInChildren<Image>().enabled = false;
            inventoryWindow.UpgradeIndex = -1;
            CurrentPointerEnterName = string.Empty;
            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().enabled = false;
            HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().enabled = false;
            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().enabled = false;
            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = false;
            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = false;
            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = false;
            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
            HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
        }
    }


    void Start ()
    {
        canvas = GameObject.Find("Canvas").GetComponentInChildren<Canvas>();
        characterInventory = canvas.GetComponentInChildren<CharacterInventoryGUI>();
        terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
        inventoryWindow = characterInventory.GetComponentInChildren<CharacterInventoryPickupWindowGUI>();
        Weapons = terrain.Player.GetComponentInChildren<WeaponsDatabase>();
        Armors = terrain.Player.GetComponentInChildren<ArmorDatabase>();
        wepSwitch = terrain.Player.GetComponentInChildren<WeaponSwitch>();
        armorSwitch = terrain.Player.GetComponentInChildren<ArmorSwitch>();
        Stats = terrain.Player.GetComponentInChildren<CharacterStats>();
        mainGUI = canvas.GetComponent<MainGUI>();
        movement = terrain.Player.GetComponentInChildren<CharacterMovement>();
        characterGUI = canvas.GetComponentInChildren<CharacterSkillBarGUI>();

        CurrentItemLocationIndex = -1;

        PreviousUpgradedIcon = transform.Find("ParentUpgradeItemPrevious").Find("UpgradeItemPrevious").gameObject;
        AfterUpgradedIcon = transform.Find("ParentUpgradeItemAfter").Find("UpgradeItemAfter").gameObject;
        transform.Find("Confirm").GetComponent<Button>().onClick.AddListener(() => CreateRarity());

        HoverRectINIT = Instantiate(HoverWindowRectPrefab, transform.position, transform.rotation) as GameObject;
        HoverRectINIT.transform.SetParent(transform);
        HoverRectINIT.transform.localScale = new Vector3(1, 1, 1);
        HoverRectINIT.GetComponentInChildren<Image>().enabled = false;
        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;

    }
	

	void Update ()
    {
	
	}
}
