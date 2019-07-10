using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterSkillsGUI : MonoBehaviour, IPointerDownHandler
{
	TerrainScript terrain;
    public GameObject RankCraftGatherSkillsGroupReference; // contains icon/current rank/name/exp
    public GameObject RankGeneralSkillsGroupReference; // contains icon/current rank/name/exp

    public List<GameObject> GatheringRankSkillsGroupList;
	public List<GameObject> CraftingRankSkillsGroupList;
	public List<GameObject> GeneralRankSkillsGroupList;
    public List<GameObject> FireRankSkillsGroupList;
    public List<GameObject> IceRankSkillsGroupList;
    public List<GameObject> LightningRankSkillsGroupList;
    public List<GameObject> NatureRankSkillsGroupList;

    public GameObject GatheringSkillImage;
	public GameObject CraftingSkillImage;
	public GameObject GeneralSkillImage;
    public GameObject FireSkillImage;
    public GameObject IceSkillImage;
    public GameObject LightningSkillImage;
    public GameObject NatureSkillImage;

    public GameObject GatheringSkillIconImage;
	public GameObject CraftingSkillIconImage;
	public GameObject GeneralSkillIconImage;
    public GameObject FireSkillIConImage;
    public GameObject IceSkillIconImage;
    public GameObject LightningSkillIconImage;
    public GameObject NatureSkillIconImage;

    public GameObject GatheringRankSkillImage;
	public GameObject CraftingRankSkillImage;
	public GameObject GeneralRankSkillImage;
    public GameObject FireRankSkillImage;
    public GameObject IceRankSkillImage;
    public GameObject LightningRankSkillImage;
    public GameObject NatureRankSkillImage;

    private Vector3 MousePosition;
	private RectTransform TransformPosition;

	public int CurrentTab = 0;

	public List<Button> SkillTablList;

    GatheringSkillDatabase gatheringDatabase;
    CraftingSkillDatabase craftingDatabase;
    CharacterSkillBarGUI skillbarGUI;
    MiscellaneousItemsDatabase MiscItems;
	PotionDatabase Potions;
	WeaponsDatabase Weapons;
	ArmorDatabase Armors;
	GeneralSkillsDatabase GeneralsDB;
    CharacterStats Stats;
    CharacterMovement charMovement;

	public void OnPointerDown(PointerEventData data)
	{
		transform.SetAsLastSibling();
	}

	public void LevelupCraftingAndGathering()
	{	
        for (int i = 0; i < craftingDatabase.CraftingSkillList.Count; i++)
        {
            if (craftingDatabase.CraftingSkillList[i].CurrentExp >= craftingDatabase.CraftingSkillList[i].MaxExp)
            {
                if (craftingDatabase.CraftingSkillList[i].CurrentExp > craftingDatabase.CraftingSkillList[i].MaxExp)
                    craftingDatabase.CraftingSkillList[i].CurrentExp = craftingDatabase.CraftingSkillList[i].CurrentExp
                        - craftingDatabase.CraftingSkillList[i].MaxExp;
                else
                    craftingDatabase.CraftingSkillList[i].CurrentExp = 0;
                craftingDatabase.CraftingSkillList[i].MaxExp += 4 + Mathf.Round(craftingDatabase.CraftingSkillList[i].CurrentRank * 0.2f);
                craftingDatabase.CraftingSkillList[i].CurrentRank += 1;

                skillbarGUI.NewCraftingItemsFromLevelup(craftingDatabase.CraftingSkillList[i].CraftingName, craftingDatabase.CraftingSkillList[i].CurrentRank);
            }

            CraftingRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
            "Rank " + craftingDatabase.CraftingSkillList[i].CurrentRank;
            CraftingRankSkillsGroupList[i].transform.Find("RankExp").Find("Exp").GetComponentInChildren<Text>().text =
                Mathf.Round(craftingDatabase.CraftingSkillList[i].CurrentExp) + "/" + Mathf.Round(craftingDatabase.CraftingSkillList[i].MaxExp);
            CraftingRankSkillsGroupList[i].transform.Find("RankExp").Find("Exp").GetComponent<Image>().fillAmount =
                Mathf.Round(craftingDatabase.CraftingSkillList[i].CurrentExp) / Mathf.Round(craftingDatabase.CraftingSkillList[i].MaxExp);

        }
        for (int i = 0; i < gatheringDatabase.GatheringSkillList.Count; i++)
        {
            if (gatheringDatabase.GatheringSkillList[i].CurrentExp >= gatheringDatabase.GatheringSkillList[i].MaxExp)
            {
                if (gatheringDatabase.GatheringSkillList[i].CurrentExp > gatheringDatabase.GatheringSkillList[i].MaxExp)
                    gatheringDatabase.GatheringSkillList[i].CurrentExp = gatheringDatabase.GatheringSkillList[i].CurrentExp
                        - gatheringDatabase.GatheringSkillList[i].MaxExp;
                else
                    gatheringDatabase.GatheringSkillList[i].CurrentExp = 0;
                gatheringDatabase.GatheringSkillList[i].MaxExp += 6 + Mathf.Round(gatheringDatabase.GatheringSkillList[i].CurrentRank * 0.2f);
                gatheringDatabase.GatheringSkillList[i].CurrentRank += 1;
            }

            GatheringRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
            "Rank " + gatheringDatabase.GatheringSkillList[i].CurrentRank;
            GatheringRankSkillsGroupList[i].transform.Find("RankExp").Find("Exp").GetComponentInChildren<Text>().text =
                Mathf.Round(gatheringDatabase.GatheringSkillList[i].CurrentExp) + "/" + Mathf.Round(gatheringDatabase.GatheringSkillList[i].MaxExp);
            GatheringRankSkillsGroupList[i].transform.Find("RankExp").Find("Exp").GetComponent<Image>().fillAmount =
                Mathf.Round(gatheringDatabase.GatheringSkillList[i].CurrentExp) / Mathf.Round(gatheringDatabase.GatheringSkillList[i].MaxExp);
        }
    }

	public void LevelupGeneralSkills(int i)
	{
        if (GeneralsDB.GeneralSkillList[i].LevelRank < GeneralsDB.GeneralSkillList[i].MaxRank)
        {
            GeneralsDB.GeneralSkillList[i].LevelRank += 1;

            switch (GeneralRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name)
            {
                case "Stamina Recovery":
                    GeneralsDB.GeneralSkillList[i].SkillValue[0] += 0.05f;
                    break;
                case "Health Recovery":
                    GeneralsDB.GeneralSkillList[i].SkillValue[0] += 0.05f;
                    break;
                case "Attack Power":
                    GeneralsDB.GeneralSkillList[i].SkillValue[0] += 0.008f;
                    break;
                case "Defense":
                    GeneralsDB.GeneralSkillList[i].SkillValue[0] += 0.007f;
                    break;
                case "Critical Strike":
                    GeneralsDB.GeneralSkillList[i].SkillValue[0] += 0.5f;
                    break;
                case "Armor Penetration":
                    GeneralsDB.GeneralSkillList[i].SkillValue[0] += 1;
                    break;
                case "Attack Speed":
                    GeneralsDB.GeneralSkillList[i].SkillValue[0] += 5;
                    //GeneralsDB.GeneralSkillList[i].SkillValue[0] += 40; // testing purposes
                    break;
            }

            GeneralRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + (GeneralsDB.GeneralSkillList[i].LevelRank).ToString();
        }
	}

    public void LevelupElementalSkills(int type,int i) // type 1 = fire, 2 = ice, 3 = lightning
    {
        if (type == 1)
        {
            GeneralsDB.FireSkillList[i].LevelRank += 1;

            switch (FireRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name)
            {
                case "Fire Strike":
                    GeneralsDB.FireSkillList[i].SkillValue[0] += 0.005f; // level up all 4 attributes
                    break;
            }

            FireRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + (GeneralsDB.FireSkillList[i].LevelRank).ToString();
        }
        else if (type == 2)
        {
            GeneralsDB.IceSkillList[i].LevelRank += 1;

            switch (IceRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name)
            {
                case "Ice Strike":
                    GeneralsDB.IceSkillList[i].SkillValue[0] += 0.005f; // level up all 4 attributes
                    break;
            }

            IceRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + (GeneralsDB.IceSkillList[i].LevelRank).ToString();
        }
        else if (type == 3)
        {
            GeneralsDB.LightningSkillList[i].LevelRank += 1;

            switch (LightningRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name)
            {
                case "Lightning Strike":
                    GeneralsDB.LightningSkillList[i].SkillValue[0] += 0.005f; // level up all 4 attributes
                    break;
            }

            LightningRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + (GeneralsDB.LightningSkillList[i].LevelRank).ToString();
        }
        else if (type == 4)
        {
            GeneralsDB.NatureSkillList[i].LevelRank += 1;

            switch (NatureRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").transform.Find("ImageScript").GetComponentInChildren<Image>().sprite.name)
            {
                case "Earth Strike":
                    GeneralsDB.NatureSkillList[i].SkillValue[0] += 0.005f; // level up all 4 attributes
                    break;
            }

            NatureRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + (GeneralsDB.NatureSkillList[i].LevelRank).ToString();
        }
    }

    void ShowButtons(int currentTab)
	{

		if (currentTab == 1)
		{
			GatheringSkillIconImage.SetActive(true);
			GatheringSkillImage.SetActive(true);
			GatheringRankSkillImage.SetActive(true);
		}
		else
		{
			GatheringSkillIconImage.SetActive(false);
			GatheringSkillImage.SetActive(false);
			GatheringRankSkillImage.SetActive(false);
		}

		if (currentTab == 2)
		{
			CraftingSkillIconImage.SetActive(true);
			CraftingSkillImage.SetActive(true);
			CraftingRankSkillImage.SetActive(true);
		}
		else
		{
			CraftingSkillIconImage.SetActive(false);
			CraftingSkillImage.SetActive(false);
			CraftingRankSkillImage.SetActive(false);
		}

		if (currentTab == 3)
		{
			GeneralSkillIconImage.SetActive(true);
			GeneralSkillImage.SetActive(true);
			GeneralRankSkillImage.SetActive(true);
		}
		else
		{
			GeneralSkillIconImage.SetActive(false);
			GeneralSkillImage.SetActive(false);
			GeneralRankSkillImage.SetActive(false);
		}

        if (currentTab == 4) // fire
        {
            FireSkillIConImage.SetActive(true);
            FireSkillImage.SetActive(true);
            FireRankSkillImage.SetActive(true);
            charMovement.CurrentElementalStrike = "Fire";
            skillbarGUI.ElementalStrikeButton.transform.Find("HoverPickupBar").GetComponent<Image>().sprite = GeneralsDB.FireSkillsSprites[0];
        }
        else
        {
            FireSkillIConImage.SetActive(false);
            FireSkillImage.SetActive(false);
            FireRankSkillImage.SetActive(false);
        }

        if (currentTab == 5) // ice 
        {
            IceSkillIconImage.SetActive(true);
            IceSkillImage.SetActive(true);
            IceRankSkillImage.SetActive(true);
            charMovement.CurrentElementalStrike = "Ice";
            skillbarGUI.ElementalStrikeButton.transform.Find("HoverPickupBar").GetComponent<Image>().sprite = GeneralsDB.IceSkillsSprites[0];
        }
        else
        {
            IceSkillIconImage.SetActive(false);
            IceSkillImage.SetActive(false);
            IceRankSkillImage.SetActive(false);
        }

        if (currentTab == 6) // lightning
        {
            LightningSkillIconImage.SetActive(true);
            LightningSkillImage.SetActive(true);
            LightningRankSkillImage.SetActive(true);
            charMovement.CurrentElementalStrike = "Lightning";
            skillbarGUI.ElementalStrikeButton.transform.Find("HoverPickupBar").GetComponent<Image>().sprite = GeneralsDB.LightningSkillsSprites[0];
        }
        else
        {
            LightningSkillIconImage.SetActive(false);
            LightningSkillImage.SetActive(false);
            LightningRankSkillImage.SetActive(false);
        }

        if (currentTab == 7) // nature
        {
            NatureSkillIconImage.SetActive(true);
            NatureSkillImage.SetActive(true);
            NatureRankSkillImage.SetActive(true);
            charMovement.CurrentElementalStrike = "Earth";
            skillbarGUI.ElementalStrikeButton.transform.Find("HoverPickupBar").GetComponent<Image>().sprite = GeneralsDB.NatureSkillsSprites[0];
        }
        else
        {
            NatureSkillIconImage.SetActive(false);
            NatureSkillImage.SetActive(false);
            NatureRankSkillImage.SetActive(false);
        }
    }

	public void OpenGathering()
	{
		ShowButtons(1);
	}
	public void OpenCrafting()
	{
		ShowButtons(2);
	}
	public void OpenGeneral()
	{
		ShowButtons(3);
	}
    public void OpenFire()
    {
        ShowButtons(4);
    }
    public void OpenIce()
    {
        ShowButtons(5);
    }
    public void OpenLightning()
    {
        ShowButtons(6);
    }
    public void OpenNature()
    {
        ShowButtons(7);
    }

    void Start () 
	{

		terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
		SkillTablList = new List<Button>();

		gatheringDatabase = terrain.Player.GetComponentInChildren<GatheringSkillDatabase>();
		craftingDatabase = terrain.Player.GetComponentInChildren<CraftingSkillDatabase>();
        MiscItems = terrain.Player.GetComponentInChildren<MiscellaneousItemsDatabase>();
		Potions = terrain.Player.GetComponentInChildren<PotionDatabase>();
		Weapons = terrain.Player.GetComponentInChildren<WeaponsDatabase>();
		Armors = terrain.Player.GetComponentInChildren<ArmorDatabase>();
		GeneralsDB = terrain.Player.GetComponentInChildren<GeneralSkillsDatabase>();
        Stats = terrain.Player.GetComponentInChildren<CharacterStats>();
        skillbarGUI = terrain.canvas.GetComponent<MainGUI>().characterSkillsBarGUI;
        charMovement = terrain.Player.GetComponentInChildren<CharacterMovement>();

        for (int i = 0; i < gatheringDatabase.GatheringSkillList.Count; i++)
		{
			GameObject RankGroup = Instantiate(RankCraftGatherSkillsGroupReference, transform.position,transform.rotation) as GameObject;
			
			GatheringRankSkillsGroupList.Add(RankGroup);

			GatheringRankSkillsGroupList[i].transform.SetParent(GatheringRankSkillImage.transform);
			GatheringRankSkillsGroupList[i].transform.localScale = GatheringSkillImage.transform.localScale;
            GatheringRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").Find("ImageScript").GetComponentInChildren<Image>().sprite = gatheringDatabase.GatheringSkillSprites[i];
            GatheringRankSkillsGroupList[i].transform.Find("RankNames").GetComponentInChildren<Text>().text =
                gatheringDatabase.GatheringSkillList[i].GatheringName;
            GatheringRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + gatheringDatabase.GatheringSkillList[i].CurrentRank.ToString();
            GatheringRankSkillsGroupList[i].transform.Find("RankExp").Find("Exp").GetComponentInChildren<Text>().text =
                "Exp: " + gatheringDatabase.GatheringSkillList[i].CurrentExp + "/" + gatheringDatabase.GatheringSkillList[i].MaxExp;
            GatheringRankSkillsGroupList[i].transform.Find("RankExp").Find("Exp").GetComponent<Image>().fillAmount =
                gatheringDatabase.GatheringSkillList[i].CurrentExp / gatheringDatabase.GatheringSkillList[i].MaxExp;
        }
		
		for (int i = 0; i < craftingDatabase.CraftingSkillList.Count; i++)
		{						
			GameObject RankGroup = Instantiate(RankCraftGatherSkillsGroupReference, transform.position,transform.rotation) as GameObject;

			CraftingRankSkillsGroupList.Add(RankGroup);
			CraftingRankSkillsGroupList[i].transform.SetParent(CraftingRankSkillImage.transform);
			CraftingRankSkillsGroupList[i].transform.localScale = CraftingRankSkillImage.transform.localScale;
            CraftingRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").Find("ImageScript").GetComponentInChildren<Image>().sprite = craftingDatabase.CraftingSkillSprites[i];
            CraftingRankSkillsGroupList[i].transform.Find("RankNames").GetComponentInChildren<Text>().text =
                    craftingDatabase.CraftingSkillList[i].CraftingName;
            CraftingRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + craftingDatabase.CraftingSkillList[i].CurrentRank;
            CraftingRankSkillsGroupList[i].transform.Find("RankExp").Find("Exp").GetComponentInChildren<Text>().text =
                "Exp: " + craftingDatabase.CraftingSkillList[i].CurrentExp + "/" + craftingDatabase.CraftingSkillList[i].MaxExp;
            CraftingRankSkillsGroupList[i].transform.Find("RankExp").Find("Exp").GetComponent<Image>().fillAmount =
                craftingDatabase.CraftingSkillList[i].CurrentExp / craftingDatabase.CraftingSkillList[i].MaxExp;
        }

		for (int i = 0; i < GeneralsDB.GeneralSkillList.Count; i++) 
		{
			GameObject RankGroup = Instantiate(RankGeneralSkillsGroupReference, transform.position,transform.rotation) as GameObject;
			
			GeneralRankSkillsGroupList.Add(RankGroup);
			GeneralRankSkillsGroupList[i].transform.SetParent(GeneralRankSkillImage.transform);
			GeneralRankSkillsGroupList[i].transform.localScale = GeneralRankSkillImage.transform.localScale;
            GeneralRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").Find("ImageScript").GetComponentInChildren<Image>().sprite = GeneralsDB.GeneralSkillsSprites[i];
            GeneralRankSkillsGroupList[i].transform.Find("RankNames").GetComponentInChildren<Text>().text =
                 GeneralsDB.GeneralSkillList[i].SkillName;
            GeneralRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " +  GeneralsDB.GeneralSkillList[i].LevelRank.ToString();
			int x = i;
			GeneralRankSkillsGroupList[x].transform.Find("LevelUp").GetComponentInChildren<Button>().onClick.AddListener(() => LevelupGeneralSkills(x));
		}

        for (int i = 0; i < GeneralsDB.FireSkillList.Count; i++)
        {
            GameObject RankGroup = Instantiate(RankGeneralSkillsGroupReference, transform.position, transform.rotation) as GameObject;

            FireRankSkillsGroupList.Add(RankGroup);
            FireRankSkillsGroupList[i].transform.SetParent(FireRankSkillImage.transform);
            FireRankSkillsGroupList[i].transform.localScale = FireRankSkillImage.transform.localScale;
            FireRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").Find("ImageScript").GetComponentInChildren<Image>().sprite = GeneralsDB.FireSkillsSprites[i];
            FireRankSkillsGroupList[i].transform.Find("RankNames").GetComponentInChildren<Text>().text =
                 GeneralsDB.FireSkillList[i].SkillName;
            FireRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + GeneralsDB.GeneralSkillList[i].LevelRank.ToString();
            int x = i;
            FireRankSkillsGroupList[x].transform.Find("LevelUp").GetComponentInChildren<Button>().onClick.AddListener(() => LevelupElementalSkills(1,x));
        }

        for (int i = 0; i < GeneralsDB.IceSkillList.Count; i++)
        {
            GameObject RankGroup = Instantiate(RankGeneralSkillsGroupReference, transform.position, transform.rotation) as GameObject;

            IceRankSkillsGroupList.Add(RankGroup);
            IceRankSkillsGroupList[i].transform.SetParent(IceRankSkillImage.transform);
            IceRankSkillsGroupList[i].transform.localScale = IceRankSkillImage.transform.localScale;
            IceRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").Find("ImageScript").GetComponentInChildren<Image>().sprite = GeneralsDB.IceSkillsSprites[i];
            IceRankSkillsGroupList[i].transform.Find("RankNames").GetComponentInChildren<Text>().text =
                 GeneralsDB.IceSkillList[i].SkillName;
            IceRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + GeneralsDB.IceSkillList[i].LevelRank.ToString();
            int x = i;
            IceRankSkillsGroupList[x].transform.Find("LevelUp").GetComponentInChildren<Button>().onClick.AddListener(() => LevelupElementalSkills(2,x));
        }

        for (int i = 0; i < GeneralsDB.LightningSkillList.Count; i++)
        {
            GameObject RankGroup = Instantiate(RankGeneralSkillsGroupReference, transform.position, transform.rotation) as GameObject;

            LightningRankSkillsGroupList.Add(RankGroup);
            LightningRankSkillsGroupList[i].transform.SetParent(LightningRankSkillImage.transform);
            LightningRankSkillsGroupList[i].transform.localScale = LightningRankSkillImage.transform.localScale;
            LightningRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").Find("ImageScript").GetComponentInChildren<Image>().sprite = GeneralsDB.LightningSkillsSprites[i];
            LightningRankSkillsGroupList[i].transform.Find("RankNames").GetComponentInChildren<Text>().text =
                 GeneralsDB.LightningSkillList[i].SkillName;
            LightningRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + GeneralsDB.LightningSkillList[i].LevelRank.ToString();
            int x = i;
            LightningRankSkillsGroupList[x].transform.Find("LevelUp").GetComponentInChildren<Button>().onClick.AddListener(() => LevelupElementalSkills(3,x));
        }

        for (int i = 0; i < GeneralsDB.NatureSkillList.Count; i++)
        {
            GameObject RankGroup = Instantiate(RankGeneralSkillsGroupReference, transform.position, transform.rotation) as GameObject;

            NatureRankSkillsGroupList.Add(RankGroup);
            NatureRankSkillsGroupList[i].transform.SetParent(NatureRankSkillImage.transform);
            NatureRankSkillsGroupList[i].transform.localScale = NatureRankSkillImage.transform.localScale;
            NatureRankSkillsGroupList[i].transform.Find("IconSkillPickupPrefab").Find("ImageScript").GetComponentInChildren<Image>().sprite = GeneralsDB.NatureSkillsSprites[i];
            NatureRankSkillsGroupList[i].transform.Find("RankNames").GetComponentInChildren<Text>().text =
                 GeneralsDB.NatureSkillList[i].SkillName;
            NatureRankSkillsGroupList[i].transform.Find("CurrentRanks").GetComponentInChildren<Text>().text =
                "Rank " + GeneralsDB.NatureSkillList[i].LevelRank.ToString();
            int x = i;
            NatureRankSkillsGroupList[x].transform.Find("LevelUp").GetComponentInChildren<Button>().onClick.AddListener(() => LevelupElementalSkills(4, x));
        }

        TransformPosition = gameObject.GetComponent<RectTransform>();

		ShowButtons(1);
        InvokeRepeating("LevelupCraftingAndGathering", 0, 1); // we want to show only the gathering tab first when game starts, thats why it is disabled in the gameobject
	}
	
	void Update () 
	{

	}
}
