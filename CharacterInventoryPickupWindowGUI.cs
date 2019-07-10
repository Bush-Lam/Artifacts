using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CharacterInventoryPickupWindowGUI : MonoBehaviour /*IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler*/
{

	TerrainScript terrain;
	public Button PickUpWindowRectPrefab;
	Canvas inventory;

	CharacterInventoryGUI characterInventory;
    CharacterMovement character;
    CharacterStats Stats;
	CharacterSkillBarGUI skillbarGUI;
	PotionDatabase Potions;
	MiscellaneousItemsDatabase MiscItems;
	WeaponsDatabase Weapons;
    WeaponSwitch WepSwitch;
	ToolDatabase Tools;
	ArmorDatabase Armors;
    ArmorSwitch armorSwitch;
    CharacterUpgradeItems UpgradeItems;
    Button RectINIT;

    public GameObject HoverWindowRectPrefab;
	public Image HoverTextPrefab;
	
	public GameObject HoverRectINIT;

	public int CurrentPointerButtonIndex;
    public int CurrentUpgradeItemSlot; // current inventory slot to be upgraded
    public int UpgradeIndex; // for upgrade items(hover)

    // put required level, also change color on required lvl/rank level
	public void FunctionOnPointerExit()
	{
		CurrentPointerButtonIndex = -1;
        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Image>().enabled = false;
        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Level").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponentInChildren<Text>().enabled = false;
    }

	public void FunctionOnPointerEnter(GameObject ThisObject) // thisobject = which button is entered
	{
        //change color scheme on skills hover
        if (ThisObject.GetComponentInChildren<Button>().name.Length < 2)
			CurrentPointerButtonIndex = int.Parse(ThisObject.GetComponentInChildren<Button>().name.Substring(0,1)); 
		else
			CurrentPointerButtonIndex = int.Parse(ThisObject.GetComponentInChildren<Button>().name.Substring(0,2));

        if (ThisObject.transform.Find("ImageScript").GetComponentInChildren<Image>().sprite != characterInventory.DefaultSprite)
		{
            float x = (10 + HoverRectINIT.transform.GetComponentInChildren<RectTransform>().rect.width) * terrain.canvas.GetComponentInChildren<Canvas>().scaleFactor;
            float y = (50 + HoverRectINIT.transform.GetComponentInChildren<RectTransform>().rect.height / 2) * terrain.canvas.GetComponentInChildren<Canvas>().scaleFactor;
            HoverRectINIT.transform.position = ThisObject.GetComponentInChildren<RectTransform>().position + new Vector3(x,y,0);
            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Image>().enabled = true;
            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().enabled = true;
            HoverRectINIT.transform.Find("Image").Find("Level").GetComponentInChildren<Text>().enabled = true;
            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().enabled = true;

            for (int i = 0; i < characterInventory.InventoryManage.Count; i++)
            {
                if (CurrentPointerButtonIndex == characterInventory.InventoryManage[i].CurrentInventorySlot) //make all the loops into one
                {
                    for (int j = 0; j < MiscItems.Miscellaneousitems.Count; j++)
                    {
                        if (characterInventory.InventoryManage[i].SlotName == MiscItems.Miscellaneousitems[j].MiscellaneousItemName)
                        {                         
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = ThisObject.transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name;
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren< Text>().color = Color.white;
                            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().text = characterInventory.InventoryManage[i].Rarity + " Craftable";
                            HoverRectINIT.transform.Find("Image").Find("Level").GetComponentInChildren<Text>().text = "Crafting Level: " + MiscItems.Miscellaneousitems[j].LevelRank;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().text = MiscItems.Miscellaneousitems[j].Description;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponentInChildren<Text>().enabled = false;
                            break;
                        }
                    }
                


                    for (int j = 0; j < Weapons.WeaponList.Count; j++)
                    {
                        if (characterInventory.InventoryManage[i].SlotName == Weapons.WeaponList[j].WeaponName)
                        {
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = ThisObject.transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponentInChildren<Text>().enabled = false;

                            switch (characterInventory.InventoryManage[i].Rarity)
                            {
                                case "Common":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.white;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = Color.white;
                                    break;
                                case "Rare":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren< Text>().color = Color.blue;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = new Color(0.5f,0.5f,1);
                                    break;
                                case "Epic":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren< Text>().color = Color.magenta;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = Color.magenta;
                                    break;
                                case "Unique":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren< Text>().color = Color.red;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = Color.red;
                                    break;
                                case "Legendary":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren< Text>().color = Color.green;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = Color.green;
                                    break;
                                case "Mythic":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren< Text>().color = Color.cyan;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = Color.cyan;
                                    break;

                            }
                            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().text = characterInventory.InventoryManage[i].Rarity + " Weapon";
                            HoverRectINIT.transform.Find("Image").Find("Level").GetComponentInChildren<Text>().text = "Level/Crafting Requirement: " + Weapons.WeaponList[j].LevelRank;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().text = "+" + characterInventory.InventoryManage[i].DamageOrValue.ToString() + " Weapon Damage";
                            if (characterInventory.InventoryManage[i].isASecondary == 0)
                            {
                                HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponentInChildren<Text>().text = "+" + characterInventory.InventoryManage[i].ArmorPenetration.ToString() + " Armor Penetration" + "\n";
                                HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().text = "+" + characterInventory.InventoryManage[i].WeaponAttackSpeed.ToString() + " Attack Speed";
                                HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().text = "+" + characterInventory.InventoryManage[i].CritRate.ToString() + " Critical Chance";
                            }
                            else
                            {
                                HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().enabled = false;
                                HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().enabled = false;
                                HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponentInChildren<Text>().enabled = false;
                            }
                            break;
                        }
                    }

                    for (int j = 0; j < Potions.EndLengthHealthPotion; j++)
                    {
                        if (characterInventory.InventoryManage[i].SlotName == Potions.PotionList[j].PotionName)
                        {
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = ThisObject.transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name;
                            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().text = characterInventory.InventoryManage[i].Rarity + " Usable";
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren< Text>().color = Color.white;
                            HoverRectINIT.transform.Find("Image").Find("Level").GetComponentInChildren<Text>().text = "Level/Crafting Requirement: " + Potions.PotionList[j].LevelRank;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().text = "Replenish " + (Stats.tempHealth * characterInventory.InventoryManage[i].DamageOrValue).ToString() + " Health.";
                            break;
                        }
                    }
                    for (int j = Potions.EndLengthHealthPotion; j < Potions.EndLengthStaminaPotion; j++)
                    {
                        if (characterInventory.InventoryManage[i].SlotName == Potions.PotionList[j].PotionName)
                        {
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = ThisObject.transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name;
                            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().text = characterInventory.InventoryManage[i].Rarity + " Usable";
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren< Text>().color = Color.white;
                            HoverRectINIT.transform.Find("Image").Find("Level").GetComponentInChildren<Text>().text = "Level/Crafting Requirement: " + Potions.PotionList[j].LevelRank;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().text = "Replenish " + (Stats.PlayerStamina * characterInventory.InventoryManage[i].DamageOrValue).ToString() + " Stamina.";
                            break;
                        }
                    }

                    for (int j = 0; j < Tools.ToolList.Count; j++)
                    {
                        if (characterInventory.InventoryManage[i].SlotName == Tools.ToolList[j].ToolName)
                        {
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = ThisObject.transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name;
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren< Text>().color = Color.white;
                            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().text = "Common Gathering Tool";
                            HoverRectINIT.transform.Find("Image").Find("Level").GetComponentInChildren<Text>().text = "Level Requirement: " + Tools.ToolList[j].RequiredLevel;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().text = Tools.ToolList[j].Description;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponentInChildren<Text>().enabled = false;
                            break;
                        }
                    }

                    for (int j = 0; j < Armors.ArmorList.Count; j++)
                    {
                        if (characterInventory.InventoryManage[i].SlotName == Armors.ArmorList[j].ArmorName)
                        {
                            switch (characterInventory.InventoryManage[i].Rarity)
                            {
                                case "Common":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.white;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = Color.white;
                                    break;
                                case "Rare":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.blue;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = new Color(0.5f, 0.5f, 1);
                                    break;
                                case "Epic":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.magenta;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = Color.magenta;
                                    break;
                                case "Unique":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.red;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = Color.red;
                                    break;
                                case "Legendary":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.green;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = Color.green;
                                    break;
                                case "Mythic":
                                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.cyan;
                                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().color = Color.cyan;
                                    break;

                            }

                            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = ThisObject.transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponentInChildren<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().text = characterInventory.InventoryManage[i].Rarity + " Armor";
                            HoverRectINIT.transform.Find("Image").Find("Level").GetComponentInChildren<Text>().text = "Level/Crafting Requirement: " + Armors.ArmorList[j].LevelRank;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().text = "+ " + characterInventory.InventoryManage[i].Defense.ToString() + " Defense";
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().text = "+ " + Mathf.Round(characterInventory.InventoryManage[i].Health * Stats.tempHealth).ToString() + " Health";
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().text = "+ " + Mathf.Round(characterInventory.InventoryManage[i].Stamina * Stats.tempStamina).ToString() + " Stamina";
                            break;
                        }
                    }
                }
			}
		}
	}

	public void FunctionOnPointerClick(BaseEventData Basedata)
	{
		PointerEventData data = ( PointerEventData )Basedata;

        if (data.clickCount == 2 && character.Attacking == false && terrain.canvas.GetComponentInChildren<MainGUI>().timeElapsedProgressBar <= Time.time)
        {

            if (CurrentPointerButtonIndex >= 0 && CurrentPointerButtonIndex <= 24)
                characterInventory.CurrentInventoryItemSlot = CurrentPointerButtonIndex;

            for (int i = 0; i < characterInventory.InventoryManage.Count; i++)
            {
                if (characterInventory.InventoryManage[i].CurrentInventorySlot == characterInventory.CurrentInventoryItemSlot
                    && characterInventory.InventoryManage[i].isASecondary == 0
                    && characterInventory.CurrentInventoryItemSlot == WepSwitch.CurrentWeaponItemSlot) // for weapons that are primary - clicking an already equipped item
                {
                    CurrentPointerButtonIndex = 100;
                    characterInventory.InventoryButtonRects[WepSwitch.CurrentWeaponItemSlot].transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.66f);
                    WepSwitch.DestroyObject(WepSwitch.CurrentObject, 0);
                    if (WepSwitch.CurrentProjectileObject != null)
                        WepSwitch.DestroyObject(WepSwitch.CurrentProjectileObject, 1);
                    character.SendFloats("ArmJump", 0);
                    return;

                }
                else if (characterInventory.InventoryManage[i].CurrentInventorySlot == characterInventory.CurrentInventoryItemSlot
                     && (characterInventory.CurrentInventoryItemSlot == armorSwitch.CurrentChestplateIteration ||
                     characterInventory.CurrentInventoryItemSlot == armorSwitch.CurrentHelmetIteration ||
                     characterInventory.CurrentInventoryItemSlot == armorSwitch.CurrentLegsIteration)) // for armors - clicking an already equipped item
                {
                    CurrentPointerButtonIndex = 100;
                    characterInventory.InventoryButtonRects[characterInventory.CurrentInventoryItemSlot].transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.66f);
                    armorSwitch.DropObject(characterInventory.CurrentInventoryItemSlot); // unequip
                    return;

                }
            }

            for (int x = 0; x < Potions.PotionList.Count; x++)
            {
                if (Potions.PotionList[x].PotionName == characterInventory.InventoryButtonRects[characterInventory.CurrentInventoryItemSlot].GetComponentInChildren<Text>().text)
                {
                    skillbarGUI.UsePotions(Potions.PotionList[x].PotionName);
                    return;
                }
            }

            characterInventory.SetWeaponValues();
            characterInventory.SetArmorValues();
            characterInventory.SetToolValues();

            if (UpgradeItems.CurrentUpgradeIndex == characterInventory.CurrentInventoryItemSlot) // update upgrade icons when equipping an item
            {
                UpgradeItems.PreviousUpgradedIcon.transform.GetComponentInChildren<Image>().sprite = characterInventory.DefaultSprite;
                UpgradeItems.PreviousUpgradedIcon.transform.GetComponentInChildren<Mask>().showMaskGraphic = true;
                UpgradeItems.AfterUpgradedIcon.transform.GetComponentInChildren<Image>().sprite = characterInventory.DefaultSprite;
                UpgradeItems.AfterUpgradedIcon.transform.GetComponentInChildren<Mask>().showMaskGraphic = true;
            }
        }
        
	}

	public void FunctionOnPointerDown (GameObject ThisObject)
	{
	    // no picking up inv slot items when making stuff/atking stuff/gathering
		if (ThisObject.transform.GetComponentInChildren<Mask>().showMaskGraphic == true && character.Attacking == false && terrain.canvas.GetComponentInChildren<MainGUI>().timeElapsedProgressBar <= Time.time)
		{			
			RectINIT.transform.SetParent(inventory.transform);
			RectINIT.GetComponentInChildren<RectTransform>().sizeDelta = ThisObject.transform.GetComponentInChildren<RectTransform>().sizeDelta;
			RectINIT.GetComponentInChildren<RectTransform>().localScale = ThisObject.transform.GetComponentInChildren<RectTransform>().localScale;
			RectINIT.GetComponentInChildren<Image>().sprite = ThisObject.transform.Find("ImageScript").GetComponentInChildren<Image>().sprite;
			characterInventory.CurrentInventoryItemSlot = int.Parse(ThisObject.transform.name);
			RectINIT.GetComponentInChildren<RectTransform>().position = Input.mousePosition;
            StartCoroutine(PointingDown(RectINIT, ThisObject));
		}
	}

	IEnumerator PointingDown(Button RECTINIT, GameObject PreviousButtonLocation)
	{
		if (Input.GetMouseButton(0))
		{
			yield return null;

            terrain.canvas.GetComponentInChildren< MainGUI>().DisableAttackWhenMenuItemPressed = true; // no attacking when holding item
            RECTINIT.GetComponentInChildren<Image>().enabled = true;
            RECTINIT.GetComponentInChildren<RectTransform>().position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            RectINIT.transform.SetAsLastSibling();
			StartCoroutine(PointingDown(RECTINIT, PreviousButtonLocation));
		}

		else 
		{
            RECTINIT.GetComponentInChildren<Image>().enabled = false;
            if (UpgradeIndex == 1) // make sure it is on the upgrade icon
                CurrentUpgradeItemSlot = characterInventory.CurrentInventoryItemSlot; //put item onto upgrade window

            for (int i =0; i < Potions.PotionList.Count; i++)
			{
				if (characterInventory.InventoryButtonsIcons[characterInventory.CurrentInventoryItemSlot].sprite.name == Potions.PotionList[i].PotionName)
				{
                    skillbarGUI.CurrentSkillPickUpSprite = characterInventory.InventoryButtonsIcons[characterInventory.CurrentInventoryItemSlot].sprite; // transition potion from inventory to skillbar
                }	
			}

            if (CurrentPointerButtonIndex != -1 && CurrentPointerButtonIndex != 100)
            {
                for (int i = 0; i < characterInventory.InventoryManage.Count; i++)
                {
                    if (characterInventory.InventoryButtonsIcons[CurrentPointerButtonIndex].sprite == characterInventory.DefaultSprite &&
                    characterInventory.InventoryManage[i].CurrentInventorySlot == int.Parse(PreviousButtonLocation.name)) // drop item into another inventory slot or upgrading an item
                    {
                        if (UpgradeItems.CurrentUpgradeIndex == characterInventory.InventoryManage[i].CurrentInventorySlot) // update upgrade icons
                        {
                            UpgradeItems.PreviousUpgradedIcon.transform.GetComponentInChildren<Image>().sprite = characterInventory.DefaultSprite;
                            UpgradeItems.PreviousUpgradedIcon.transform.GetComponentInChildren<Mask>().showMaskGraphic = true;
                            UpgradeItems.AfterUpgradedIcon.transform.GetComponentInChildren<Image>().sprite = characterInventory.DefaultSprite;
                            UpgradeItems.AfterUpgradedIcon.transform.GetComponentInChildren<Mask>().showMaskGraphic = true;
                        }

                        PreviousButtonLocation.transform.Find("ImageScript").GetComponentInChildren<Image>().sprite = characterInventory.DefaultSprite;
                        PreviousButtonLocation.transform.GetComponentInChildren<Mask>().showMaskGraphic = false;

                        characterInventory.InventoryManage[i].CurrentInventorySlot = CurrentPointerButtonIndex;
                        characterInventory.InventoryButtonsIcons[CurrentPointerButtonIndex].sprite = characterInventory.InventoryManage[i].tSprite;
                        characterInventory.InventoryButtonsIcons[CurrentPointerButtonIndex].transform.GetComponentInChildren<Mask>().showMaskGraphic = true;
                        characterInventory.InventoryButtonRects[CurrentPointerButtonIndex].GetComponentInChildren<Text>().text = PreviousButtonLocation.GetComponentInChildren<Text>().text;

                        // transfer equipped item from old slot to new slot

                        if (WepSwitch.CurrentWeaponItemSlot == int.Parse(PreviousButtonLocation.name))
                        {
                            PreviousButtonLocation.transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.66f);
                            WepSwitch.CurrentWeaponItemSlot = CurrentPointerButtonIndex;
                            characterInventory.InventoryButtonRects[CurrentPointerButtonIndex].transform.GetComponentInChildren<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.66f);

                        }
                        else if (WepSwitch.CurrentLoadedProjectile == int.Parse(PreviousButtonLocation.name))
                            WepSwitch.CurrentLoadedProjectile = CurrentPointerButtonIndex;
                        else if (armorSwitch.CurrentHelmetIteration == int.Parse(PreviousButtonLocation.name))
                        {
                            PreviousButtonLocation.transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.66f);
                            armorSwitch.CurrentHelmetIteration = CurrentPointerButtonIndex;
                            characterInventory.InventoryButtonRects[CurrentPointerButtonIndex].transform.GetComponentInChildren<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.66f);
                        }
                        else if (armorSwitch.CurrentChestplateIteration == int.Parse(PreviousButtonLocation.name))
                        {
                            PreviousButtonLocation.transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.66f);
                            armorSwitch.CurrentChestplateIteration = CurrentPointerButtonIndex;
                            characterInventory.InventoryButtonRects[CurrentPointerButtonIndex].transform.GetComponentInChildren<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.66f);
                        }
                        else if (armorSwitch.CurrentLegsIteration == int.Parse(PreviousButtonLocation.name))
                        {
                            PreviousButtonLocation.transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.66f);
                            armorSwitch.CurrentChestplateIteration = CurrentPointerButtonIndex;
                            characterInventory.InventoryButtonRects[CurrentPointerButtonIndex].transform.GetComponentInChildren<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.66f);
                        }
                    }
                }
            }			

            if (PreviousButtonLocation.GetComponentInChildren<Text>().text != string.Empty &&
                PreviousButtonLocation.transform.Find("ImageScript").GetComponentInChildren<Image>().sprite == characterInventory.DefaultSprite) //empty stack amounts
                PreviousButtonLocation.GetComponentInChildren<Text>().text = string.Empty;

            if (CurrentPointerButtonIndex == -1 && skillbarGUI.CurrentPointerEnterName == string.Empty && UpgradeIndex == -1) // dropping item, skillbargui.currentpointerentername == for potions
            {
                if (UpgradeItems.CurrentUpgradeIndex == characterInventory.CurrentInventoryItemSlot) // update upgrade icons when dropping an item
                {
                    UpgradeItems.PreviousUpgradedIcon.transform.GetComponentInChildren<Image>().sprite = characterInventory.DefaultSprite;
                    UpgradeItems.PreviousUpgradedIcon.transform.GetComponentInChildren<Mask>().showMaskGraphic = true;
                    UpgradeItems.AfterUpgradedIcon.transform.GetComponentInChildren<Image>().sprite = characterInventory.DefaultSprite;
                    UpgradeItems.AfterUpgradedIcon.transform.GetComponentInChildren<Mask>().showMaskGraphic = true;
                }
                skillbarGUI.CurrentSkillPickUpSprite = null; // dropped potion
                characterInventory.dropitem(PreviousButtonLocation);
            }

            terrain.canvas.GetComponentInChildren<MainGUI>().DisableAttackWhenMenuItemPressed = false; // no attacking when holding item
            yield return new WaitForSeconds(0.15f);
            CurrentUpgradeItemSlot = -1;
            skillbarGUI.CurrentSkillPickUpSprite = null; // for potions
        }	
	}

	
	void Start () 
	{
		CurrentPointerButtonIndex = -1;
        CurrentUpgradeItemSlot = -1;
        UpgradeIndex = -1;

		terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
        RectINIT = Instantiate(PickUpWindowRectPrefab, transform.position, transform.rotation) as Button;
        RectINIT.GetComponentInChildren<Image>().enabled = false;
        HoverRectINIT = Instantiate(HoverWindowRectPrefab, transform.position, transform.rotation) as GameObject;
        HoverRectINIT.transform.SetParent(transform);
        HoverRectINIT.transform.localScale = new Vector3(1, 1, 1);
        HoverRectINIT.GetComponentInChildren<Image>().enabled = false;
        HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Level").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponentInChildren<Text>().enabled = false;
        HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponentInChildren<Text>().enabled = false;

        inventory = gameObject.transform.root.GetComponentInChildren<Canvas>();
		characterInventory = gameObject.GetComponentInParent<CharacterInventoryGUI>();
        skillbarGUI = terrain.canvas.GetComponentInChildren<CharacterSkillBarGUI>();
        character = terrain.Player.GetComponentInChildren<CharacterMovement>();
        MiscItems = terrain.Player.GetComponentInChildren<MiscellaneousItemsDatabase>();
		Weapons = terrain.Player.GetComponentInChildren<WeaponsDatabase>();
        WepSwitch = terrain.Player.GetComponentInChildren<WeaponSwitch>();
        Tools = terrain.Player.GetComponentInChildren<ToolDatabase>();
		Potions = terrain.Player.GetComponentInChildren<PotionDatabase>();
		Armors = terrain.Player.GetComponentInChildren<ArmorDatabase>();
        armorSwitch = terrain.Player.GetComponentInChildren<ArmorSwitch>();
        Stats = terrain.Player.GetComponentInChildren<CharacterStats>();
        UpgradeItems = terrain.canvas.GetComponentInChildren<CharacterUpgradeItems>();
	}

	void Update () 
	{

	}
}
