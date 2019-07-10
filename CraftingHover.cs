using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftingHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // this script is for viewing information on the crafting page
    public Canvas skills;

    CharacterSkillsGUI characterSkills;
    MiscellaneousItemsDatabase MiscItems;
    TerrainScript terrain;
    PotionDatabase Potions;
    ToolDatabase ToolsDB;
    WeaponsDatabase Weapons;
    ArmorDatabase Armors;
    CharacterStats Stats;
    CharacterSkillBarGUI skillbarGUI;

    public GameObject HoverWindowRectPrefab;

    GameObject HoverRectINIT;

    public void OnPointerEnter(PointerEventData data) // thisobject = which button is entered
    {
        if (data.pointerEnter.name == "IconHoverCraftingSkills(Clone)")
        {
            float x = (100 + HoverRectINIT.transform.GetComponent<RectTransform>().rect.width) * terrain.canvas.GetComponent<Canvas>().scaleFactor;
            float y = (50 + HoverRectINIT.transform.GetComponent<RectTransform>().rect.height / 2) * terrain.canvas.GetComponent<Canvas>().scaleFactor;
            HoverRectINIT.transform.position = data.pointerEnter.GetComponent<RectTransform>().position + new Vector3(x, y, 0);
            HoverRectINIT.GetComponentInChildren<Image>().enabled = true;
            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().enabled = true;
            HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().enabled = true;
            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().enabled = true;

            for (int i = 0; i < MiscItems.Miscellaneousitems.Count; i++)
            {
                if (data.pointerEnter.GetComponentInChildren<Text>().text == MiscItems.Miscellaneousitems[i].MiscellaneousItemName)
                {
                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = MiscItems.Miscellaneousitems[i].MiscellaneousItemName;
                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.white;
                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = "Common Craftable";
                    HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().text = "Crafting Requirement: " + MiscItems.Miscellaneousitems[i].LevelRank;
                    HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().text = MiscItems.Miscellaneousitems[i].Description;
                    HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = true;
                    HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = false;
                    HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = false;
                    HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
                    HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
                }
            }

            for (int i = 0; i < Weapons.WeaponList.Count; i++)
            {
                if (data.pointerEnter.GetComponentInChildren<Text>().text == Weapons.WeaponList[i].WeaponName)
                {
                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = Weapons.WeaponList[i].WeaponName;
                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.white;
                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = "Common Weapon";
                    HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().text = "Level/Crafting Requirement: " + Weapons.WeaponList[i].LevelRank;
                    HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = true;
                    HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = true;
                    HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = true;
                    HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = true;
                    HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().text = "+ " + Weapons.WeaponList[i].WeaponDamage.ToString() + " Weapon Damage";
                    HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
                    if (Weapons.WeaponList[i].IsASecondaryWeapon == 0)
                    {
                        HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().text = "+" + Mathf.Round(Weapons.WeaponList[i].ArmorPenetration) + " Armor Penetration" + "\n";
                        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().text = "+" + Mathf.Round(Weapons.WeaponList[i].WeaponAttackSpeed) + " Attack Speed";
                        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().text = "+" + Mathf.Round(Weapons.WeaponList[i].CritRate) + " Critical Chance";
                    }
                    else
                    {
                        HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
                        HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = false;
                        HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = false;
                    }
                }
            }

            for (int i = 0; i < Potions.PotionList.Count; i++)
            {
                if (data.pointerEnter.GetComponentInChildren<Text>().text == Potions.PotionList[i].PotionName)
                {
                    for (int j = 0; j < Potions.EndLengthHealthPotion; j++)
                    {
                        if (data.pointerEnter.GetComponentInChildren<Text>().text == Potions.PotionList[j].PotionName)
                        {
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = Potions.PotionList[j].PotionName;
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.white;
                            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = "Common Usable";
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().text = "Level/Crafting Requirement: " + Potions.PotionList[j].LevelRank;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().text = "Replenish " + Stats.tempHealth * Potions.PotionList[j].PotionValue + " Health.";
                        }
                    }
                    for (int j = Potions.EndLengthHealthPotion; j < Potions.EndLengthStaminaPotion; j++)
                    {
                        if (data.pointerEnter.GetComponentInChildren<Text>().text == Potions.PotionList[j].PotionName)
                        {
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = Potions.PotionList[j].PotionName;
                            HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.white;
                            HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = "Common Usable";
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = true;
                            HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
                            HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().text = "Level/Crafting Requirement: " + Potions.PotionList[j].LevelRank;
                            HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().text = "Replenish " + Stats.PlayerStamina * Potions.PotionList[j].PotionValue + "Stamina.";
                        }
                    }
                }
            }

            for (int i = 0; i < Armors.ArmorList.Count; i++)
            {
                if (data.pointerEnter.GetComponentInChildren<Text>().text == Armors.ArmorList[i].ArmorName)
                {
                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = Armors.ArmorList[i].ArmorName;
                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.white;
                    HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = "Common Armor";
                    HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = true;
                    HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
                    HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = true;
                    HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
                    HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = true;
                    HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().text = "Level/Crafting Requirement: " + Armors.ArmorList[i].LevelRank;
                    HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().text = "+ " + Armors.ArmorList[i].DefenseValues + " Defense";
                    HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().text = "+ " + Mathf.Round(Stats.tempHealth * Armors.ArmorList[i].BonusHealth) + " Health";
                    HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().text = "+ " + Mathf.Round(Stats.tempStamina * Armors.ArmorList[i].BonusStamina) + " Stamina";
                }
            }

            for (int i = 0; i < ToolsDB.ToolList.Count; i++)
            {
                if (data.pointerEnter.GetComponentInChildren<Text>().text == ToolsDB.ToolList[i].ToolName)
                {
                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().text = ToolsDB.ToolList[i].ToolName;
                    HoverRectINIT.transform.Find("Image").GetComponentInChildren<Text>().color = Color.white;

                    if (i < ToolsDB.EndofPickAxeList)
                    {
                        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = "Common Gathering Tool";
                    }
                    else
                    {
                        HoverRectINIT.transform.Find("Image").Find("Rarity").GetComponent<Text>().text = "Common Crafting Station";
                    }
            

                    HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().enabled = true;
                    HoverRectINIT.transform.Find("Image").Find("Rate2").GetComponent<Text>().enabled = false;
                    HoverRectINIT.transform.Find("Image").Find("Rate3").GetComponent<Text>().enabled = false;
                    HoverRectINIT.transform.Find("Image").Find("Rate4").GetComponent<Text>().enabled = false;
                    HoverRectINIT.transform.Find("Image").Find("Rate5").GetComponent<Text>().enabled = false;
                    HoverRectINIT.transform.Find("Image").Find("Level").GetComponent<Text>().text = "Level/Crafting Requirement: " + ToolsDB.ToolList[i].RequiredLevel;
                    HoverRectINIT.transform.Find("Image").Find("Rate1").GetComponent<Text>().text = ToolsDB.ToolList[i].Description;
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (HoverRectINIT != null)
        {
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
    }

    void Start()
    {
        terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();

        MiscItems = terrain.Player.GetComponentInChildren<MiscellaneousItemsDatabase>();
        Stats = terrain.Player.GetComponentInChildren<CharacterStats>();
        ToolsDB = terrain.Player.GetComponentInChildren<ToolDatabase>();
        Potions = terrain.Player.GetComponentInChildren<PotionDatabase>();
        Weapons = terrain.Player.GetComponentInChildren<WeaponsDatabase>();
        Armors = terrain.Player.GetComponentInChildren<ArmorDatabase>();

        HoverRectINIT = Instantiate(HoverWindowRectPrefab, transform.position, transform.rotation) as GameObject;
        HoverRectINIT.transform.SetParent(transform.parent.parent.parent.parent.parent.transform);
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

        skills = GameObject.Find("Canvas").GetComponentInChildren<Canvas>();
        characterSkills = skills.GetComponentInChildren<CharacterSkillsGUI>();
        skillbarGUI = terrain.canvas.GetComponentInChildren<CharacterSkillBarGUI>();
    }

    void Update()
    {

    }
}

