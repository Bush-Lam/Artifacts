using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterSkillPickup : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{

	public Canvas skills;
	
	CharacterSkillsGUI characterSkills;
	MiscellaneousItemsDatabase MiscItems;
	TerrainScript terrain;
	PotionDatabase Potions;
	WeaponsDatabase Weapons;
	ArmorDatabase Armors;
    GeneralSkillsDatabase GeneralDB;
    GatheringSkillDatabase GatheringDB;
    CraftingSkillDatabase CraftingDB;
	CharacterSkillBarGUI skillbarGUI;
    CharacterStats Stats;

	public void OnPointerExit(PointerEventData data)
	{
		if (skillbarGUI.HoverRectINIT != null)
		{
            skillbarGUI.HoverRectINIT.GetComponent<Image>().enabled = false;
            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().enabled = false;
            skillbarGUI.HoverRectINIT.transform.Find("Text").GetComponent<Text>().enabled = false;
        }
	}

    public void OnPointerEnter(PointerEventData data)
    {
        if (data.pointerEnter.transform.parent.name == "IconSkillPickupPrefab" || data.pointerEnter.transform.parent.name == "LevelUp" ||
            (data.pointerEnter.GetComponent<Image>().name == "HoverPickupBar" && data.pointerEnter.GetComponent<Image>().sprite.name != "UISkinTexture"))
        {
            skillbarGUI.HoverRectINIT.GetComponent<Image>().enabled = true;
            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().enabled = true;
            skillbarGUI.HoverRectINIT.transform.Find("Text").GetComponent<Text>().enabled = true;
            if (data.pointerEnter.transform.parent.name == "LevelUp")
                skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text = data.pointerEnter.transform.parent.parent.Find("IconSkillPickupPrefab").Find("ImageScript").GetComponent<Image>().sprite.name; 
            else
                skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text = data.pointerEnter.GetComponent<Image>().sprite.name;

            if (data.pointerEnter.transform.parent.name == "IconSkillPickupPrefab" || data.pointerEnter.transform.parent.name == "LevelUp") // character skills window
            {
                skillbarGUI.HoverRectINIT.transform.SetParent(characterSkills.transform);
                skillbarGUI.HoverRectINIT.transform.localPosition = new Vector2(-144, 300);
            }
            else
            {
                skillbarGUI.HoverRectINIT.transform.SetParent(skillbarGUI.transform);
                float x = (-105 + skillbarGUI.HoverRectINIT.transform.GetComponentInChildren<RectTransform>().rect.width) * terrain.canvas.GetComponentInChildren<Canvas>().scaleFactor;
                float y = (135) * terrain.canvas.GetComponentInChildren<Canvas>().scaleFactor;

                skillbarGUI.HoverRectINIT.transform.position = data.pointerEnter.transform.GetComponentInChildren<RectTransform>().position + new Vector3(x, y, 0);
            }
            for (int i = 0; i < Potions.PotionList.Count; i++)
            {
                if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == Potions.PotionList[i].PotionName)
                {

                    switch (Potions.PotionList[i].PotionType)
                    {
                        case "HP":
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                            "<color=white>Common Usable" + "</color> \n" +
                            "<color=white>Level / Crafting Requirement: " + Potions.PotionList[i].LevelRank + "</color> \n" +
                            "Replenish " + Mathf.Round(Potions.PotionList[i].PotionValue * Stats.PlayerHealth).ToString() + " Health. \r\n";
                            break;
                        case "STAM":
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                            "<color=white>Common Usable" + "</color> \n" +
                            "<color=white>Level / Crafting Requirement: " + Potions.PotionList[i].LevelRank + "</color> \n" +
                            "Replenish " + Mathf.Round(Potions.PotionList[i].PotionValue * Stats.PlayerHealth).ToString() + " Stamina. \r\n";
                            break;
                    }
                }
            }
            for (int i = 0; i < GatheringDB.GatheringSkillList.Count; i++)
            {
                if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GatheringDB.GatheringSkillList[i].GatheringName)
                {
                    switch (GatheringDB.GatheringSkillList[i].GatheringName)
                    {
                        case "Woodcutting":
                            GatheringDB.GatheringSkillList[i].CurrentRank = 40;
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                            "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GatheringDB.GatheringSkillList[0].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                "Using this skill will allow you to gather logs. " + "\r\n" + "<color=white>Ability to gather: </color> " + "\r\n";
                            for (int j = MiscItems.EndlengthBars + 1; j < MiscItems.EndlengthLogs + 1; j++)
                            {
                                if (GatheringDB.GatheringSkillList[i].CurrentRank >= MiscItems.Miscellaneousitems[j].LevelRank)
                                    skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text += MiscItems.Miscellaneousitems[j].MiscellaneousItemName + ", ";
                            }
                            string Text = skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text.Remove(skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text.Length - 2);
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text = Text + ". \r\n";
                            break;
                        case "Mining":
                            GatheringDB.GatheringSkillList[i].CurrentRank = 40;
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                            "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GatheringDB.GatheringSkillList[1].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                "Using this skill will allow you to gather ores. " + "\r\n" + "<color=white>Ability to gather: </color> " + "\r\n";
                            for (int j = 0; j < MiscItems.EndlengthOres + 1; j++)
                            {
                                if (GatheringDB.GatheringSkillList[i].CurrentRank >= MiscItems.Miscellaneousitems[j].LevelRank)
                                    skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text += MiscItems.Miscellaneousitems[j].MiscellaneousItemName + ", ";
                            }
                            Text = skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text.Remove(skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text.Length - 2);
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text = Text + ". \r\n";
                            break;
                        case "Herbalism":
                            GatheringDB.GatheringSkillList[i].CurrentRank = 25;
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                            "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GatheringDB.GatheringSkillList[2].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                "Using this skill will allow you to gather herbs. " + "\r\n" + "<color=white>Ability to gather: </color> " + "\r\n";
                            for (int j = MiscItems.EndlengthLogs + 1; j < MiscItems.EndLengthStamHerbs + 1; j++)
                            {
                                if (GatheringDB.GatheringSkillList[i].CurrentRank >= MiscItems.Miscellaneousitems[j].LevelRank)
                                    skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text += MiscItems.Miscellaneousitems[j].MiscellaneousItemName + ", ";
                            }
                            Text = skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text.Remove(skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text.Length - 2);
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text = Text + ". \r\n";
                            break;
                    }
                }
            }

            for (int i = 0; i < CraftingDB.CraftingSkillList.Count; i++)
            {
                if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == CraftingDB.CraftingSkillList[i].CraftingName)
                {
                    switch (CraftingDB.CraftingSkillList[i].CraftingName)
                    {
                        case "Metalworking":
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                            "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * CraftingDB.CraftingSkillList[0].Stamina).ToString() + " Stamina </color> \r\n" +
                                "Metalworking will allow you to create crafting items or gathering tools. " + "\r\n";                                         
                            break;                                                                                                                     
                        case "Blacksmithing":                                                                                                          
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                            "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * CraftingDB.CraftingSkillList[1].Stamina).ToString() + " Stamina </color> \r\n" +
                                "Blacksmithing will allow you to create weapons from bars. " + "\r\n";                                        
                            break;                                                                                                                     
                        case "Woodworking":                                                                                                            
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                            "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * CraftingDB.CraftingSkillList[2].Stamina).ToString() + " Stamina </color> \r\n" +
                                "Woodworking will allow you to create clothing from wood. " + "\r\n";                                          
                            break;                                                                                                                     
                        case "Potion Crafting":                                                                                                        
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                            "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * CraftingDB.CraftingSkillList[3].Stamina).ToString() + " Stamina </color> \r\n" +
                                "Potion Crafting will allow you to create Potions. " + "\r\n";
                            break;
                    }
                }
            }

            if (data.pointerEnter.transform.parent.name == "LevelUp") // next rank
            {
                for (int i = 0; i < GeneralDB.GeneralSkillList.Count; i++)
                {
                    if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GeneralDB.GeneralSkillList[i].SkillName) 
                    {
                        if (GeneralDB.GeneralSkillList[i].LevelRank >= GeneralDB.GeneralSkillList[i].MaxRank)
                        {
                            skillbarGUI.HoverRectINIT.GetComponent<Image>().enabled = false;
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().enabled = false;
                            skillbarGUI.HoverRectINIT.transform.Find("Text").GetComponent<Text>().enabled = false;
                            return;
                        }

                        switch (GeneralDB.GeneralSkillList[i].SkillName)
                        {
                            case "Stamina Recovery": //0
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[0].LevelRank + 1).ToString() + "/" + (GeneralDB.GeneralSkillList[0].MaxRank).ToString() + "</color> \r\n" +
                                    "Replenish " + Mathf.Round(Stats.PlayerStamina * (GeneralDB.GeneralSkillList[0].SkillValue[0] + 0.05f)).ToString() + " Stamina." + "\r\n";
                                break;
                            case "Health Recovery": //1
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                   "<color=white>Rank " + (GeneralDB.GeneralSkillList[1].LevelRank + 1).ToString() + "/" + (GeneralDB.GeneralSkillList[1].MaxRank).ToString() + "</color> \r\n" +
                                   "Replenish " + Mathf.Round(Stats.tempHealth * (GeneralDB.GeneralSkillList[1].SkillValue[0] + 0.05f)).ToString() + " Health" + "\r\n";
                                break;
                            case "Attack Power": //2
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[2].LevelRank + 1).ToString() + "/" + (GeneralDB.GeneralSkillList[2].MaxRank).ToString() + "</color> \r\n" +
                                    "Passively increase your Attack Power by " + System.Math.Round(GeneralDB.GeneralSkillList[2].SkillValue[0] * 100 + 0.8f, 2).ToString() + "%. " +
                                    " Attack Power increases potency of spells." + "\r\n";
                                break;
                            case "Defense": //3 100 == 100% dmg reduction
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[3].LevelRank + 1).ToString() + "/" + (GeneralDB.GeneralSkillList[3].MaxRank).ToString() + "</color> \r\n" +
                                    "Passively increase your Defense by " + System.Math.Round(GeneralDB.GeneralSkillList[3].SkillValue[0] * 100 + 0.7f, 2).ToString() + "%." + "\r\n";
                                break;
                            case "Critical Strike": //4
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[4].LevelRank + 1).ToString() + "/" + (GeneralDB.GeneralSkillList[4].MaxRank).ToString() + "</color> \r\n" +
                                    "Passively increase your Critical Strike by " + 0.5f.ToString() + "\r\n";
                                break;
                            case "Armor Penetration": //5
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[5].LevelRank + 1).ToString() + "/" + (GeneralDB.GeneralSkillList[5].MaxRank).ToString() + "</color> \r\n" +
                                    "Passively increase your Armor Penetration by " + 1.ToString() + "\r\n";
                                break;
                            case "Attack Speed":
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[6].LevelRank + 1).ToString() + "/" + (GeneralDB.GeneralSkillList[6].MaxRank).ToString() + "</color> \r\n" +
                                    "Passively increase your Attack Speed by " + 1.ToString() +
                                    " Attack speed decreases the cooldown on your basic spells." + "\r\n";
                                break;
                        }
                    }
                }

                for (int i = 0; i < GeneralDB.FireSkillList.Count; i++)
                {
                    if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GeneralDB.FireSkillList[i].SkillName)
                    {
                        if (GeneralDB.FireSkillList[i].LevelRank >= GeneralDB.FireSkillList[i].MaxRank)
                        {
                            skillbarGUI.HoverRectINIT.GetComponent<Image>().enabled = false;
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().enabled = false;
                            skillbarGUI.HoverRectINIT.transform.Find("Text").GetComponent<Text>().enabled = false;
                            return;
                        }
                        switch (GeneralDB.FireSkillList[i].SkillName)
                        {
                            case "Fire Strike":
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.FireSkillList[0].LevelRank + 1).ToString() + "/" + (GeneralDB.FireSkillList[0].MaxRank).ToString() + "</color> \r\n" +
                                    "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GeneralDB.FireSkillList[0].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                    "Conjure a Fire bolt that deals " + System.Math.Round((GeneralDB.FireSkillList[0].SkillValue[0] * 100 + 0.05f) * Stats.TotalWeaponDamage, 2).ToString() + " Damage." + "\r\n";
                                break;
                        }
                    }
                }

                for (int i = 0; i < GeneralDB.IceSkillList.Count; i++)
                {
                    if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GeneralDB.IceSkillList[i].SkillName)
                    {
                        if (GeneralDB.IceSkillList[i].LevelRank >= GeneralDB.IceSkillList[i].MaxRank)
                        {
                            skillbarGUI.HoverRectINIT.GetComponent<Image>().enabled = false;
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().enabled = false;
                            skillbarGUI.HoverRectINIT.transform.Find("Text").GetComponent<Text>().enabled = false;
                            return;
                        }
                        switch (GeneralDB.IceSkillList[i].SkillName)
                        {
                            case "Ice Strike":
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.IceSkillList[0].LevelRank + 1).ToString() + "/" + (GeneralDB.IceSkillList[0].MaxRank).ToString() + "</color> \r\n" +
                                    "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GeneralDB.IceSkillList[0].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                    "Conjure a Ice bolt that deals " + System.Math.Round((GeneralDB.IceSkillList[0].SkillValue[0] * 100 + 0.05f) * Stats.TotalWeaponDamage, 2).ToString() + " Damage." + "\r\n";
                                break;
                        }
                    }
                }

                for (int i = 0; i < GeneralDB.LightningSkillList.Count; i++)
                {
                    if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GeneralDB.LightningSkillList[i].SkillName)
                    {
                        if (GeneralDB.LightningSkillList[i].LevelRank >= GeneralDB.LightningSkillList[i].MaxRank)
                        {
                            skillbarGUI.HoverRectINIT.GetComponent<Image>().enabled = false;
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().enabled = false;
                            skillbarGUI.HoverRectINIT.transform.Find("Text").GetComponent<Text>().enabled = false;
                            return;
                        }
                        switch (GeneralDB.LightningSkillList[i].SkillName)
                        {
                            case "Lightning Strike":
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.LightningSkillList[0].LevelRank + 1).ToString() + "/" + (GeneralDB.LightningSkillList[0].MaxRank).ToString() + "</color> \r\n" +
                                    "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GeneralDB.LightningSkillList[0].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                    "Conjure a Lightning bolt that deals " + System.Math.Round((GeneralDB.LightningSkillList[0].SkillValue[0] * 100 + 0.05f) * Stats.TotalWeaponDamage, 2).ToString() + " Damage." + "\r\n";
                                break;
                        }
                    }
                }

                for (int i = 0; i < GeneralDB.NatureSkillList.Count; i++)
                {
                    if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GeneralDB.NatureSkillList[i].SkillName)
                    {
                        if (GeneralDB.NatureSkillList[i].LevelRank >= GeneralDB.NatureSkillList[i].MaxRank)
                        {
                            skillbarGUI.HoverRectINIT.GetComponent<Image>().enabled = false;
                            skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().enabled = false;
                            skillbarGUI.HoverRectINIT.transform.Find("Text").GetComponent<Text>().enabled = false;
                            return;
                        }
                        switch (GeneralDB.NatureSkillList[i].SkillName)
                        {
                            case "Earth Strike":
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.NatureSkillList[0].LevelRank + 1).ToString() + "/" + (GeneralDB.NatureSkillList[0].MaxRank).ToString() + "</color> \r\n" +
                                    "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GeneralDB.NatureSkillList[0].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                    "Conjure a Earth bolt that deals " + System.Math.Round((GeneralDB.NatureSkillList[0].SkillValue[0] * 100 + 0.05f) * Stats.TotalWeaponDamage, 2).ToString() + " Damage." + "\r\n";
                                break;
                        }
                    }
                }
            }
            else // current rank
            {
                for (int i = 0; i < GeneralDB.GeneralSkillList.Count; i++)
                {
                    if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GeneralDB.GeneralSkillList[i].SkillName)
                    {
                        switch (GeneralDB.GeneralSkillList[i].SkillName)
                        {
                            case "Stamina Recovery": //0
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[0].LevelRank).ToString() + "/" + (GeneralDB.GeneralSkillList[0].MaxRank).ToString() + "</color> \r\n" +
                                    "Replenish " + Mathf.Round(Stats.PlayerStamina * (GeneralDB.GeneralSkillList[0].SkillValue[0])).ToString() + " Stamina." + "\r\n";
                                break;
                            case "Health Recovery": //1
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                   "<color=white>Rank " + (GeneralDB.GeneralSkillList[1].LevelRank).ToString() + "/" + (GeneralDB.GeneralSkillList[1].MaxRank).ToString() + "</color> \r\n" +
                                   "Replenish " + Mathf.Round(Stats.tempHealth * (GeneralDB.GeneralSkillList[1].SkillValue[0])).ToString() + " Health" + "\r\n";
                                break;
                            case "Attack Power": //2
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[2].LevelRank).ToString() + "/" + (GeneralDB.GeneralSkillList[2].MaxRank).ToString() + "</color> \r\n" +
                                    "Passively increase your Attack Power by " + System.Math.Round(GeneralDB.GeneralSkillList[2].SkillValue[0] * 100, 2).ToString() + "%." + "\r\n";
                                break;
                            case "Defense": //3
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[3].LevelRank).ToString() + "/" + (GeneralDB.GeneralSkillList[3].MaxRank).ToString() + "</color> \r\n" +
                                    "Passively increase your Defense by " + System.Math.Round(GeneralDB.GeneralSkillList[3].SkillValue[0] * 100, 2).ToString() + "%." + "\r\n";
                                break;
                            case "Critical Strike": //4
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[4].LevelRank).ToString() + "/" + (GeneralDB.GeneralSkillList[4].MaxRank).ToString() + "</color> \r\n" +
                                    "Passively increase your Critical Strike by " + System.Math.Round(GeneralDB.GeneralSkillList[4].SkillValue[0] * 100, 2).ToString() + "%." + "\r\n";
                                break;
                            case "Armor Penetration": //5
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[5].LevelRank).ToString() + "/" + (GeneralDB.GeneralSkillList[5].MaxRank).ToString() + "</color> \r\n" +
                                    "Passively increase your Armor Penetration by " + System.Math.Round(GeneralDB.GeneralSkillList[5].SkillValue[0] * 100, 2).ToString() + "%." + "\r\n";
                                break;
                            case "Attack Speed":
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.GeneralSkillList[6].LevelRank).ToString() + "/" + (GeneralDB.GeneralSkillList[6].MaxRank).ToString() + "</color> \r\n" +
                                    "Passively increase your Attack Speed by " + System.Math.Round(GeneralDB.GeneralSkillList[6].SkillValue[0] * 100, 2).ToString() + "%. \r\n";
                                break;
                        }
                    }
                }

                for (int i = 0; i < GeneralDB.FireSkillList.Count; i++)
                {
                    if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GeneralDB.FireSkillList[i].SkillName)
                    {
                        switch (GeneralDB.FireSkillList[i].SkillName)
                        {
                            case "Fire Strike":
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.FireSkillList[0].LevelRank).ToString() + "/" + (GeneralDB.FireSkillList[0].MaxRank).ToString() + "</color> \r\n" +
                                    "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GeneralDB.FireSkillList[0].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                    "Conjure a Fire bolt that deals " + System.Math.Round(GeneralDB.FireSkillList[0].SkillValue[0] * 100 * Stats.TotalWeaponDamage, 2).ToString() + " Damage." + "\r\n";
                                break;
                        }
                    }
                }

                for (int i = 0; i < GeneralDB.IceSkillList.Count; i++)
                {
                    if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GeneralDB.IceSkillList[i].SkillName)
                    {
                        switch (GeneralDB.IceSkillList[i].SkillName)
                        {
                            case "Ice Strike":
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.IceSkillList[0].LevelRank).ToString() + "/" + (GeneralDB.IceSkillList[0].MaxRank).ToString() + "</color> \r\n" +
                                    "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GeneralDB.IceSkillList[0].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                    "Conjure a Ice bolt that deals " + System.Math.Round(GeneralDB.IceSkillList[0].SkillValue[0] * 100 * Stats.TotalWeaponDamage, 2).ToString() + " Damage." + "\r\n";
                                break;
                        }
                    }
                }

                for (int i = 0; i < GeneralDB.LightningSkillList.Count; i++)
                {
                    if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GeneralDB.LightningSkillList[i].SkillName)
                    {
                        switch (GeneralDB.LightningSkillList[i].SkillName)
                        {
                            case "Lightning Strike":
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.LightningSkillList[0].LevelRank).ToString() + "/" + (GeneralDB.LightningSkillList[0].MaxRank).ToString() + "</color> \r\n" +
                                    "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GeneralDB.LightningSkillList[0].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                    "Conjure a Lightning bolt that deals " + System.Math.Round(GeneralDB.LightningSkillList[0].SkillValue[0] * 100 * Stats.TotalWeaponDamage, 2).ToString() + " Damage." + "\r\n";
                                break;
                        }
                    }
                }

                for (int i = 0; i < GeneralDB.NatureSkillList.Count; i++)
                {
                    if (skillbarGUI.HoverRectINIT.GetComponentInChildren<Text>().text == GeneralDB.NatureSkillList[i].SkillName)
                    {
                        switch (GeneralDB.NatureSkillList[i].SkillName)
                        {
                            case "Earth Strike":
                                skillbarGUI.HoverRectINIT.transform.Find("Description").GetComponent<Text>().text =
                                    "<color=white>Rank " + (GeneralDB.NatureSkillList[0].LevelRank).ToString() + "/" + (GeneralDB.NatureSkillList[0].MaxRank).ToString() + "</color> \r\n" +
                                    "<color=white>Uses " + Mathf.Round(Stats.PlayerStamina * GeneralDB.NatureSkillList[0].StaminaCost).ToString() + " Stamina </color> \r\n" +
                                    "Conjure a Earth bolt that deals " + System.Math.Round(GeneralDB.NatureSkillList[0].SkillValue[0] * 100 * Stats.TotalWeaponDamage, 2).ToString() + " Damage." + "\r\n";
                                break;
                        }
                    }
                }
            }
        }
    }

    public void OnPointerDown (PointerEventData data)
	{
        if (data.pointerCurrentRaycast.gameObject.transform.parent.name == "LevelUp")
            return;

        if (data.pointerCurrentRaycast.gameObject.transform.parent.name == "IconSkillPickupPrefab" ||
            (data.pointerEnter.GetComponent<Image>().name == "HoverPickupBar" && data.pointerEnter.GetComponent<Image>().sprite.name != "UISkinTexture")
            && data.pointerEnter.transform.parent.Find(data.pointerEnter.transform.parent.name + "CoolDownSkill").GetComponent<Image>().fillAmount == 0)
        {
            terrain.canvas.GetComponentInChildren<MainGUI>().SetActiveWindow(skillbarGUI.RectINIT.gameObject, 1, true, true);
            skillbarGUI.RectINIT.transform.GetComponentInChildren<Image>().sprite = data.pointerEnter.GetComponentInChildren<Image>().sprite;
            if (data.pointerEnter.GetComponent<Image>().name == "HoverPickupBar") 
            {
                data.pointerEnter.GetComponentInChildren<Image>().sprite = skills.GetComponent<CharacterInventoryGUI>().DefaultSprite;
                data.pointerEnter.GetComponentInChildren<Mask>().showMaskGraphic = false;
            }
            skillbarGUI.RectINIT.transform.SetAsLastSibling();
            skillbarGUI.RectINIT.GetComponent<RectTransform>().position = Input.mousePosition;
            StartCoroutine("PointingDown", skillbarGUI.RectINIT);
        }
	}

	IEnumerator PointingDown(Image RECTINIT)
	{
		if (Input.GetMouseButton(0))
		{
			yield return null;
			RECTINIT.GetComponent<RectTransform>().position = Input.mousePosition;

			StartCoroutine("PointingDown", RECTINIT);
		}

		else
		{
			StopCoroutine("PointingDown");
			skillbarGUI.CurrentSkillPickUpSprite = RECTINIT.transform.GetComponentInChildren<Image>().sprite; // in skillbargui
            terrain.canvas.GetComponentInChildren<MainGUI>().SetActiveWindow(skillbarGUI.RectINIT.gameObject, 0, false, false);

            yield return new WaitForSeconds(0.15f);

			skillbarGUI.CurrentSkillPickUpSprite = null;
        }
	}

	void Start () 
	{
		terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();

        MiscItems =  terrain.Player.GetComponentInChildren<MiscellaneousItemsDatabase>();
		Potions = terrain.Player.GetComponentInChildren<PotionDatabase>();
		Weapons = terrain.Player.GetComponentInChildren<WeaponsDatabase>();
		Armors = terrain.Player.GetComponentInChildren<ArmorDatabase>();
        GeneralDB = terrain.Player.GetComponentInChildren<GeneralSkillsDatabase>();
        GatheringDB = terrain.Player.GetComponentInChildren<GatheringSkillDatabase>();
        CraftingDB = terrain.Player.GetComponentInChildren<CraftingSkillDatabase>();
        Stats = terrain.Player.GetComponentInChildren<CharacterStats>();

        skills = GameObject.Find("Canvas").GetComponentInChildren<Canvas>();
		characterSkills = skills.GetComponentInChildren<CharacterSkillsGUI>();
        skillbarGUI = terrain.canvas.GetComponentInChildren<CharacterSkillBarGUI>();
    }
	
	void Update () 
	{
		
	}
}
