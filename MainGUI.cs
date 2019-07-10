using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class MainGUI : Photon.MonoBehaviour
{
	public Canvas canvas;
    public GameObject EnvironmentDesc;
    public GameObject ProgressBar;
    public GameObject GeneralText;
	public GameObject characterSkillsPrefab;
	public GameObject inventoryPrefab;
	public GameObject characterStatsPrefab;
	public GameObject OptionsPrefab;
    public GameObject UpgradesPrefab;
    public GameObject SaveAndLoadGamePrefab;

    GameObject characterSkills;
    TerrainScript terrain;
    MiscellaneousItemsDatabase MiscItems;
    SaveAndLoadGUI SaveLoadGUI;
    MonsterSpawn monsterSpawn;
    MonsterFunctions monsterFunction;
    CharacterStats Stats;
    CharacterMovement charactermovement;
    GeneralSkillsDatabase GeneralDB;
    GatheringSkillDatabase GatheringDB;
    CraftingSkillDatabase CraftingDB;
    CharacterOptionsGUI optionGUI;
    WeaponSwitch switchWeapons;
    ArmorSwitch switchArmors;
    ChopTrees choptrees;
    MineRocks minerocks;
    Herbloring herbloring;

    public GameObject inventory;
	GameObject characterStats;
	GameObject options;
    GameObject Upgrades;

	public CharacterSkillBarGUI characterSkillsBarGUI;
	CharacterInventoryGUI characterinventory;
    public GameObject SaveAndLoadGameObject;

    string ForwardKey = "w";
	string LeftKey = "a";
	string RightKey = "d";
	string Backward = "s";
	string Jump = "space";
	string Teleport = "tab";

	string FirstKey = "1";
	string SecondKey = "2";
	string ThirdKey = "3";
	string FourthKey = "4";
	string FifthKey = "5";
	string SixthKey = "6";
	string SeventhKey = "7";
	string EighthKey = "8";
	string PickupKey = "e"; //14

	string InventoryKey = "i"; //15
	string CharacterKey = "c"; //16
	string OptionKey = "o"; //17
	string SkillKey = "k"; //18
	string UpgradeKey = "u"; //19
    string AutoRunKey = "numlock"; //20

    public List<string> KeyBinds;
    public float Sensitivity = 1;
    public float SoundLevel = 1;
    public float MusicLevel = 1;
	public bool DisableKeyBindsWhenTyping;
    public bool DisableAttackWhenMenuItemPressed;
    float timeElapsedExamine;
    public float timeElapsedProgressBar;
    public bool CheckMovementProgressBar; // restrict movement when gathering or crafting 

    public bool AutoRun;

    public void SetActiveWindow(GameObject gameobject, int Alpha, bool IsInteractable, bool BlockRaycasts)
	{
        gameobject.GetComponent<CanvasGroup>().alpha = Alpha;
        gameobject.GetComponent<CanvasGroup>().interactable = IsInteractable;
        gameobject.GetComponent<CanvasGroup>().blocksRaycasts = BlockRaycasts;
    }

	public void DestroyWindow(GameObject gameObject)
	{
		Destroy(gameObject);
	}

    [PunRPC]
    public void SaveMultiplayer(string MultiplayerFileName, string TypeofSave) // from saveandLoad -- cant rpc from local client
    {
        SaveLoadGUI.TerrainFileName = MultiplayerFileName; // sync the filename.xml to everyone
        SaveLoadGUI.CharacterFileName = PlayerPrefs.GetString("PlayerName");

        if (Application.loadedLevelName != "MainMenu")
        {
            SaveLoadGUI.MyCharacterData.ThisCharacterData.PlayerName = PlayerPrefs.GetString("PlayerName").Substring(0, PlayerPrefs.GetString("PlayerName").IndexOf(' '));
            SaveLoadGUI.MyCharacterData.ThisCharacterData.PlayerPosition = Stats.transform.position;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.PlayerRotation = Stats.transform.rotation;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.PlayerLevel = Stats.PlayerLevel;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.PlayerHealth = Stats.PlayerHealth;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentPlayerHealth = Stats.CurrentPlayerHealth;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.PlayerStamina = Stats.PlayerStamina;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentPlayerStamina = Stats.CurrentPlayerStamina;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.PlayerExperience = Stats.PlayerExperience;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.ExpRequired = Stats.ExpRequired;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.SkillPoints = Stats.SkillPoints;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentHunger = Stats.CurrentHunger;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentThirst = Stats.CurrentThirst;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.TotalMiscStackAmounts = new int[MiscItems.TotalMiscellaneousStacks.Length];
            PlayerPrefs.SetString("PlayerName", PlayerPrefs.GetString("PlayerName").Substring(0, PlayerPrefs.GetString("PlayerName").IndexOf(' ')) + " Lvl" + Stats.PlayerLevel);

            SaveLoadGUI.MyTerrainData.ThisTerrainData.CurrentTerrainLevel = terrain.CurrentTerrainLevel;

            SaveLoadGUI.MyCharacterData.ThisCharacterData.SlotName = new string[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.Rarity = new string[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.tSprite = new string[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.InvDamageOrValue = new float[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.InvWeaponAttackSpeed = new float[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.InvCritRate = new float[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.InvArmorPen = new float[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.InvDefense = new float[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.InvHealth = new float[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.InvStamina = new float[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentInventorySlot = new int[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.StackAmounts = new int[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.isASecondary = new int[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.Description = new string[terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count];

            SaveLoadGUI.MyCharacterData.ThisCharacterData.GatheringCurrentRank = new int[GatheringDB.GatheringSkillList.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.GatheringCurrentExp = new float[GatheringDB.GatheringSkillList.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.GatheringMaxExp = new float[GatheringDB.GatheringSkillList.Count];

            SaveLoadGUI.MyCharacterData.ThisCharacterData.CraftingCurrentRank = new int[CraftingDB.CraftingSkillList.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CraftingCurrentExp = new float[CraftingDB.CraftingSkillList.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CraftingMaxExp = new float[CraftingDB.CraftingSkillList.Count];

            SaveLoadGUI.MyCharacterData.ThisCharacterData.GeneralCD = new float[GeneralDB.GeneralSkillList.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.GeneralDur = new float[GeneralDB.GeneralSkillList.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.SkillChance = new float[GeneralDB.GeneralSkillList.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.GeneralSkillPointRequired = new int[GeneralDB.GeneralSkillList.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.SkillValue = new float[GeneralDB.GeneralSkillList.Count];
            SaveLoadGUI.MyCharacterData.ThisCharacterData.GeneralkillLevelRank = new int[GeneralDB.GeneralSkillList.Count];

            for (int i = 0; i < GatheringDB.GatheringSkillList.Count; i++)
            {
                SaveLoadGUI.MyCharacterData.ThisCharacterData.GatheringCurrentRank[i] = GatheringDB.GatheringSkillList[i].CurrentRank;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.GatheringCurrentExp[i] = GatheringDB.GatheringSkillList[i].CurrentExp;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.GatheringMaxExp[i] = GatheringDB.GatheringSkillList[i].MaxExp;
            }

            for (int i = 0; i < CraftingDB.CraftingSkillList.Count; i++)
            {
                SaveLoadGUI.MyCharacterData.ThisCharacterData.CraftingCurrentRank[i] = CraftingDB.CraftingSkillList[i].CurrentRank;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.CraftingCurrentExp[i] = CraftingDB.CraftingSkillList[i].CurrentExp;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.CraftingMaxExp[i] = CraftingDB.CraftingSkillList[i].MaxExp;
            }

            for (int i = 0; i < GeneralDB.GeneralSkillList.Count; i++)
            {
                SaveLoadGUI.MyCharacterData.ThisCharacterData.GeneralCD[i] = GeneralDB.GeneralSkillList[i].CoolDown;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.GeneralDur[i] = GeneralDB.GeneralSkillList[i].Duration;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.GeneralkillLevelRank[i] = GeneralDB.GeneralSkillList[i].LevelRank;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.SkillChance[i] = GeneralDB.GeneralSkillList[i].SkillChance;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.GeneralSkillPointRequired[i] = GeneralDB.GeneralSkillList[i].SkillPointsRequired;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.SkillValue[i] = GeneralDB.GeneralSkillList[i].SkillValue[0];
            }

            SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentSlotMainWeapon = switchWeapons.CurrentWeaponItemSlot;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentSlotSecondaryWeapon = switchWeapons.CurrentLoadedProjectile;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentSlotHead = switchArmors.CurrentHelmetIteration;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentSlotArmor = switchArmors.CurrentChestplateIteration;
            SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentSlotLegs = switchArmors.CurrentLegsIteration;

            for (int i = 0; i < terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Count; i++)
            {
                SaveLoadGUI.MyCharacterData.ThisCharacterData.SlotName[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].SlotName;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.Rarity[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].Rarity;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.tSprite[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].tSprite.name;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.InvDamageOrValue[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].DamageOrValue;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.InvWeaponAttackSpeed[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].WeaponAttackSpeed;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.InvCritRate[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].CritRate;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.InvArmorPen[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].ArmorPenetration;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.InvDefense[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].Defense;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.InvHealth[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].Health;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.InvStamina[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].Stamina;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.CurrentInventorySlot[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].CurrentInventorySlot;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.StackAmounts[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].StackAmounts;
                SaveLoadGUI.MyCharacterData.ThisCharacterData.isASecondary[i] = terrain.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].isASecondary;
            }

            for (int i = 0; i < MiscItems.TotalMiscellaneousStacks.Length; i++)
                SaveLoadGUI.MyCharacterData.ThisCharacterData.TotalMiscStackAmounts[i] = MiscItems.TotalMiscellaneousStacks[i];

            SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterHealth = new float[monsterSpawn.ListofMonsters.Count];
            SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterHealthRandomModifier = new float[monsterSpawn.ListofMonsters.Count];
            SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterDamage = new float[monsterSpawn.ListofMonsters.Count];
            SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterDefense = new float[monsterSpawn.ListofMonsters.Count];
            SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterExperience = new float[monsterSpawn.ListofMonsters.Count];
            SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterLevel = new int[monsterSpawn.ListofMonsters.Count];
            SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterMaterials = new int[monsterSpawn.ListofMonsters.Count * 3];
 
            SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterSpawnLocations = new Vector3[monsterSpawn.ListofMonsters.Count];
            SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterSpawnRotations = new Quaternion[monsterSpawn.ListofMonsters.Count];

            if (TypeofSave != "Auto")
            {
                SaveLoadGUI.MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredIDs = new int[terrain.ListofEnvironmentalObjectsAltered.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredPositions = new Vector3[terrain.ListofEnvironmentalObjectsAlteredPositions.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredRotations = new Quaternion[terrain.ListofEnvironmentalObjectsAlteredRotations.Count];

                SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer1 = new int[terrain.detailLayer1.Length];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer2 = new int[terrain.detailLayer2.Length];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer3 = new int[terrain.detailLayer3.Length];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer4 = new int[terrain.detailLayer4.Length];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer5 = new int[terrain.detailLayer5.Length];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer6 = new int[terrain.detailLayer6.Length];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer7 = new int[terrain.detailLayer7.Length];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer8 = new int[terrain.detailLayer8.Length];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.terrainMap = new float[terrain.terrainMap.Length];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.MiddleAlpha = new float[terrain.MiddleAlpha.Length];
            }

            SaveLoadGUI.MyTerrainData.ThisTerrainData.BossMonsterSpawnLocation = monsterSpawn.BossMonster.transform.position;
            SaveLoadGUI.MyTerrainData.ThisTerrainData.BossMonsterSpawnRotation = monsterSpawn.BossMonster.transform.rotation;
            SaveLoadGUI.MyTerrainData.ThisTerrainData.BossMonsterHealth = monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterHealth;
            SaveLoadGUI.MyTerrainData.ThisTerrainData.BossMonsterHealthRandomModifier = monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier;
            SaveLoadGUI.MyTerrainData.ThisTerrainData.BossMonsterDamage = monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterDamage;
            SaveLoadGUI.MyTerrainData.ThisTerrainData.BossMonsterDefense = monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterDefense;
            SaveLoadGUI.MyTerrainData.ThisTerrainData.BossMonsterExperience = monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterExp;
            SaveLoadGUI.MyTerrainData.ThisTerrainData.BossMonsterLevel = monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterLevel;

            for (int i = 0; i < monsterSpawn.ListofMonsters.Count; i++)
            {
                SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterSpawnLocations[i] = monsterSpawn.ListofMonsters[i].transform.position;
                SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterSpawnRotations[i] = monsterSpawn.ListofMonsters[i].transform.rotation;
                for (int j = 0; j < 3; j++)
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterMaterials[i * j] = monsterSpawn.ListofMonsters[i].GetComponent<MonsterMovement>().SavedMaterials[j];

                if (monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterHealth > 0)
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterHealth[i] = monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterHealth;
                else
                {
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterHealth[i] = monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier;
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterSpawnLocations[i] = new Vector3(monsterSpawn.pos.x, (terrain.GetComponent<Terrain>().SampleHeight(monsterSpawn.pos) + terrain.transform.position.y) + 1, monsterSpawn.pos.z);
                }
                SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterHealthRandomModifier[i] = monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier;
                SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterDamage[i] = monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterDamage;
                SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterDefense[i] = monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterDefense;
                SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterExperience[i] = monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterExp;
                SaveLoadGUI.MyTerrainData.ThisTerrainData.MonsterLevel[i] = monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterLevel;
            }

            if (TypeofSave != "Auto")
            {
                terrain.terrainMap = terrain.terrain.terrainData.GetHeights(0, 0, terrain.terrain.terrainData.heightmapWidth, terrain.terrain.terrainData.heightmapHeight);
                terrain.MiddleAlpha = terrain.terrain.terrainData.GetAlphamaps(0, 0, terrain.terrain.terrainData.alphamapWidth, terrain.terrain.terrainData.alphamapHeight);
                terrain.detailLayer1 = terrain.terrain.terrainData.GetDetailLayer(0, 0, terrain.terrain.terrainData.detailWidth, terrain.terrain.terrainData.detailHeight, 0);
                terrain.detailLayer2 = terrain.terrain.terrainData.GetDetailLayer(0, 0, terrain.terrain.terrainData.detailWidth, terrain.terrain.terrainData.detailHeight, 1);
                terrain.detailLayer3 = terrain.terrain.terrainData.GetDetailLayer(0, 0, terrain.terrain.terrainData.detailWidth, terrain.terrain.terrainData.detailHeight, 2);
                terrain.detailLayer4 = terrain.terrain.terrainData.GetDetailLayer(0, 0, terrain.terrain.terrainData.detailWidth, terrain.terrain.terrainData.detailHeight, 3);
                terrain.detailLayer5 = terrain.terrain.terrainData.GetDetailLayer(0, 0, terrain.terrain.terrainData.detailWidth, terrain.terrain.terrainData.detailHeight, 4);
                terrain.detailLayer6 = terrain.terrain.terrainData.GetDetailLayer(0, 0, terrain.terrain.terrainData.detailWidth, terrain.terrain.terrainData.detailHeight, 5);
                terrain.detailLayer7 = terrain.terrain.terrainData.GetDetailLayer(0, 0, terrain.terrain.terrainData.detailWidth, terrain.terrain.terrainData.detailHeight, 6);
                terrain.detailLayer8 = terrain.terrain.terrainData.GetDetailLayer(0, 0, terrain.terrain.terrainData.detailWidth, terrain.terrain.terrainData.detailHeight, 7);

                SaveLoadGUI.MyTerrainData.ThisTerrainData.TreePosition = new Vector3[terrain.ListofTrees.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.RockPosition = new Vector3[terrain.ListofRocks.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.HerbPatchPosition = new Vector3[terrain.ListofHerbPatches.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.WaterPosition = new Vector3[terrain.ListofWaters.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.TreeRotation = new Quaternion[terrain.ListofTrees.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.RockRotation = new Quaternion[terrain.ListofRocks.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.HerbPatchRotation = new Quaternion[terrain.ListofHerbPatches.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.WaterRotation = new Quaternion[terrain.ListofWaters.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.TreeScale = new Vector3[terrain.ListofTrees.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.RockScale = new Vector3[terrain.ListofRocks.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.HerbPatchScale = new Vector3[terrain.ListofHerbPatches.Count];
                SaveLoadGUI.MyTerrainData.ThisTerrainData.WaterScale = new Vector3[terrain.ListofWaters.Count];

                // flatten 2d/3d array to 1d array

                for (int i = 0; i < terrain.terrain.terrainData.heightmapHeight; i++)
                {
                    for (int j = 0; j < terrain.terrain.terrainData.heightmapWidth; j++)
                    {
                        SaveLoadGUI.MyTerrainData.ThisTerrainData.terrainMap[i * terrain.terrain.terrainData.heightmapWidth + j] = terrain.terrainMap[i, j];
                    }
                }

                for (int i = 0; i < terrain.terrain.terrainData.detailHeight; i++)
                {
                    for (int j = 0; j < terrain.terrain.terrainData.detailWidth; j++)
                    {
                        SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer1[i * terrain.terrain.terrainData.detailWidth + j] = terrain.detailLayer1[i, j];
                        SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer2[i * terrain.terrain.terrainData.detailWidth + j] = terrain.detailLayer2[i, j];
                        SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer3[i * terrain.terrain.terrainData.detailWidth + j] = terrain.detailLayer3[i, j];
                        SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer4[i * terrain.terrain.terrainData.detailWidth + j] = terrain.detailLayer4[i, j];
                        SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer5[i * terrain.terrain.terrainData.detailWidth + j] = terrain.detailLayer5[i, j];
                        SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer6[i * terrain.terrain.terrainData.detailWidth + j] = terrain.detailLayer6[i, j];
                        SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer7[i * terrain.terrain.terrainData.detailWidth + j] = terrain.detailLayer7[i, j];
                        SaveLoadGUI.MyTerrainData.ThisTerrainData.detailLayer8[i * terrain.terrain.terrainData.detailWidth + j] = terrain.detailLayer8[i, j];
                    }
                }

                for (int i = 0; i < terrain.terrain.terrainData.alphamapHeight; i++)
                {
                    for (int j = 0; j < terrain.terrain.terrainData.alphamapWidth; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            SaveLoadGUI.MyTerrainData.ThisTerrainData.MiddleAlpha[i + terrain.terrain.terrainData.alphamapHeight * (j + terrain.terrain.terrainData.alphamapWidth * k)] = terrain.MiddleAlpha[i, j, k];
                        }
                    }
                }

                for (int i = 0; i < terrain.ListofEnvironmentalObjectsAltered.Count; i++)
                {
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredIDs[i] = terrain.ListofEnvironmentalObjectsAltered[i].GetPhotonView().viewID; // add photonviews to trees/rocks
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredPositions[i] = terrain.ListofEnvironmentalObjectsAlteredPositions[i];
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredRotations[i] = terrain.ListofEnvironmentalObjectsAlteredRotations[i];
                }

                for (int i = 1; i < terrain.ListofTrees.Count; i++)
                {
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.TreePosition[i] = terrain.ListofTrees[i].transform.position;
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.TreeRotation[i] = terrain.ListofTrees[i].transform.rotation;
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.TreeScale[i] = terrain.ListofTrees[i].transform.localScale;
                }
                for (int i = 1; i < terrain.ListofRocks.Count; i++)
                {
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.RockPosition[i] = terrain.ListofRocks[i].transform.position;
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.RockRotation[i] = terrain.ListofRocks[i].transform.rotation;
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.RockScale[i] = terrain.ListofRocks[i].transform.localScale;
                }

                for (int i = 1; i < terrain.ListofHerbPatches.Count; i++)
                {
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.HerbPatchPosition[i] = terrain.ListofHerbPatches[i].transform.position;
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.HerbPatchRotation[i] = terrain.ListofHerbPatches[i].transform.rotation;
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.HerbPatchScale[i] = terrain.ListofHerbPatches[i].transform.localScale;
                }
                for (int i = 1; i < terrain.ListofWaters.Count; i++)
                {
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.WaterPosition[i] = terrain.ListofWaters[i].transform.position;
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.WaterRotation[i] = terrain.ListofWaters[i].transform.rotation;
                    SaveLoadGUI.MyTerrainData.ThisTerrainData.WaterScale[i] = terrain.ListofWaters[i].transform.localScale;
                }
            }

            SaveLoadGUI.MyTerrainData.ThisTerrainData.gainX = terrain.gainX;
            SaveLoadGUI.MyTerrainData.ThisTerrainData.gainY = terrain.gainY;
            SaveLoadGUI.MyTerrainData.ThisTerrainData.gainXX = terrain.gainXX;
            SaveLoadGUI.MyTerrainData.ThisTerrainData.gainYY = terrain.gainYY;

            //data = SerializeObject(MyData); // xml
            SaveLoadGUI.CreateXML("Character", SaveLoadGUI.CharacterFileLocation, TypeofSave);
            //SaveLoadGUI.CreateXML("Terrain", SaveLoadGUI.TerrainFileLocation, TypeofSave);
        }
    }

    [PunRPC]
    public void SyncScales(Vector3[] TreeScales, Vector3[] RockScales, Vector3[] HerbScales, Vector3[] WaterScales) // from saveandLoad -- cant rpc from local client
    {
        //if (SaveLoadGUI.CurrentSaveSelectedButtonText != string.Empty) // only for people that are new ??
        //    return;

        for (int i = 1; i < terrain.ListofTrees.Count; i++)
        {
            terrain.ListofTrees[i].transform.localScale = TreeScales[i];
            terrain.ListofTrees[i].transform.GetComponent<Rigidbody>().mass = 3000 * TreeScales[i].x;
        }

        for (int i = 1; i < terrain.ListofRocks.Count; i++)
        {
            terrain.ListofRocks[i].transform.localScale = RockScales[i];
            terrain.ListofRocks[i].transform.GetComponent<Rigidbody>().mass = 3000 * RockScales[i].x;
        }

        for (int i = 1; i < terrain.ListofHerbPatches.Count; i++)
            terrain.ListofHerbPatches[i].transform.localScale = HerbScales[i];

        for (int i = 1; i < terrain.ListofWaters.Count; i++)
            terrain.ListofWaters[i].transform.localScale = WaterScales[i];
    }

    [PunRPC]
    public void SyncPeople(Vector3 PlayerPosition, int MasterPositionGameObjectviewID, float Gainx, float Gainy, float Gainxx, float Gainyy, int[] AlteredEnvironmentIDs, Vector3[] AlteredEnvironmentPositions, Quaternion[] AlteredEnvironmentRotations,
        Vector3[] TreePositions, Quaternion[] TreeRotations, Vector3[] RockPositions, Quaternion[] RockRotations, Vector3[] HerbPositions, Quaternion[] HerbRotations, Vector3[] WaterPositions, Quaternion[] WaterRotations,
        float bMonsterHealth, float bMonsterHealthMod, float bMonsterDefense, float bMonsterDamage, float bMonsterExp, int bMonsterLvl,
        float[] MonsterHealth, float[] MonsterHealthMod, float[] MonsterDefense, float[] MonsterDamage, float[] MonsterExp, int[] MonsterLvl) // from saveandLoad -- cant rpc from local client
    {
        //if (SaveLoadGUI.CurrentSaveSelectedButtonText != string.Empty) // only for people that are new????
        //    return;

        SaveLoadGUI.CurrentSaveSelectedButton = null;
        SaveLoadGUI.CurrentSaveSelectedButtonText = string.Empty;
        PlayerPrefs.SetString("LoadSave", string.Empty);
        ActivateSaveAndLoadGUI(0, false, false);
        gameObject.transform.localScale = new Vector3(1, 1, 1); // pun rpc environment/alphas/heights...
        PhotonView thisPlayer = PhotonView.Find(MasterPositionGameObjectviewID); // this is for when other players get too far from the server player
        terrain.MasterPosition = thisPlayer.gameObject;

        terrain.Player.transform.position = PlayerPosition;

        monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterHealth = bMonsterHealth;
        monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier = bMonsterHealthMod;
        monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterDefense = bMonsterDefense;
        monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterDamage = bMonsterDamage;
        monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterExp = bMonsterExp;
        monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterLevel = bMonsterLvl;

        for (int i = 0; i < monsterSpawn.ListofMonsters.Count; i++)
        {
            monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterHealth = MonsterHealth[i];
            monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier = MonsterHealthMod[i];
            monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterDefense = MonsterDefense[i];
            monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterDamage = MonsterDamage[i];
            monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterExp = MonsterExp[i];
            monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterLevel = MonsterLvl[i];
        }

        terrain.gainX = Gainx;
        terrain.gainY = Gainy;
        terrain.gainXX = Gainxx;
        terrain.gainYY = Gainyy;

        terrain.NewHeightsAndAlphamaps(PlayerPosition, true);

        for (int i = 0; i < AlteredEnvironmentIDs.Length; i++)
        {
            GameObject alteredID = PhotonView.Find(AlteredEnvironmentIDs[i]).gameObject;
            terrain.ListofEnvironmentalObjectsAlteredPositions.Add(AlteredEnvironmentPositions[i]);
            terrain.ListofEnvironmentalObjectsAlteredRotations.Add(AlteredEnvironmentRotations[i]);
            alteredID.transform.GetComponent<Rigidbody>().isKinematic = false;
            StartCoroutine(monsterFunction.EnvironmentalPhysicsUndo(alteredID, AlteredEnvironmentPositions[i], AlteredEnvironmentRotations[i]));
        }

        for (int i = 1; i < terrain.ListofTrees.Count; i++)
        {
            terrain.ListofTrees[i].transform.position = TreePositions[i];
            terrain.ListofTrees[i].transform.rotation = TreeRotations[i];
        }

        for (int i = 1; i < terrain.ListofRocks.Count; i++)
        {
            terrain.ListofRocks[i].transform.position = RockPositions[i];
            terrain.ListofRocks[i].transform.rotation = RockRotations[i];
        }

        for (int i = 1; i < terrain.ListofHerbPatches.Count; i++)
        {
            terrain.ListofHerbPatches[i].transform.position = HerbPositions[i];
            terrain.ListofHerbPatches[i].transform.rotation = HerbRotations[i];
        }

        for (int i = 1; i < terrain.ListofWaters.Count; i++)
        {
            terrain.ListofWaters[i].transform.position = WaterPositions[i];
            terrain.ListofWaters[i].transform.rotation = WaterRotations[i];
        }

        terrain.NotLoading[0] = true;
        terrain.NotLoading[1] = true;
    }

    public void ActivateInventory(int Alpha, bool IsInteractable, bool BlockRaycasts)
	{
        if (inventory == null)
		{
			inventory = Instantiate(inventoryPrefab, new Vector2(0,0),transform.rotation) as GameObject;
            inventory.GetComponent<CharacterInventoryPickupWindowGUI>().enabled = true;
			inventory.transform.SetParent(canvas.transform);
            inventory.transform.localScale = new Vector3(1,1,1);
            inventory.GetPhotonView().viewID = PhotonNetwork.AllocateViewID();

            characterSkillsBarGUI.characterInventory = canvas.GetComponentInChildren<CharacterInventoryGUI>();
            characterSkillsBarGUI.characterInventoryPickup = inventory.GetComponent<CharacterSkillPickup>();
			characterinventory.InventoryButtonRects = inventory.transform.Find("VerticalLayout").Find("GridLayout").GetComponentsInChildren<Button>();
			inventory.GetComponentInChildren<Button>().onClick.AddListener(()	=>	SetActiveWindow(inventory, 0, false, false));

            for (int i = 0; i < characterinventory.InventoryButtonRects.Length; i++)
			{
				characterinventory.InventoryButtonsIcons[i] = characterinventory.InventoryButtonRects[i].transform.Find("ImageScript").GetComponentInChildren<Image>();
				characterinventory.InventoryButtonsIcons[i].GetComponent<Mask>().showMaskGraphic = false;

				characterinventory.InventoryButtonsStackText.Add(characterinventory.InventoryButtonRects[i].GetComponentInChildren<Text>());
				characterinventory.InventoryButtonsStackText[i].GetComponentInChildren<Text>().text = string.Empty;
			}
		}
        //make sit down healing/recover icons redo other icons?
        inventory.GetComponent<RectTransform>().localPosition = new Vector2(Screen.width / 5, Screen.height / 10);
        inventory.GetComponent<CanvasGroup>().alpha = Alpha;
        inventory.GetComponent<CanvasGroup>().interactable = IsInteractable;
        inventory.GetComponent<CanvasGroup>().blocksRaycasts = BlockRaycasts;
    }
	public void ActivateStats(int Alpha, bool IsInteractable, bool BlockRaycasts)
	{
		if (characterStats == null)
		{
			characterStats = Instantiate(characterStatsPrefab, new Vector2(0, 0), transform.rotation) as GameObject;
            characterStats.transform.SetParent(canvas.transform);
            characterStats.transform.localScale = new Vector3(1,1,1);
			characterStats.GetComponentInChildren<Button>().onClick.AddListener(()	=>	SetActiveWindow(characterStats, 0, false, false));
        }

        characterStats.GetComponent<RectTransform>().localPosition = new Vector2(Screen.width / 5, Screen.height / 10);
        characterStats.GetComponent<CanvasGroup>().alpha = Alpha;
        characterStats.GetComponent<CanvasGroup>().interactable = IsInteractable;
        characterStats.GetComponent<CanvasGroup>().blocksRaycasts = BlockRaycasts;
    }
	public void ActivateOptions(int Alpha, bool IsInteractable, bool BlockRaycasts)
	{
		if (options == null)
		{
			options = Instantiate(OptionsPrefab, new Vector2(0, 0), transform.rotation) as GameObject;
            options.transform.SetParent(canvas.transform);
            options.transform.localScale = new Vector3(1,1,1);
			options.GetComponentInChildren<Button>().onClick.AddListener(()	=>	SetActiveWindow(options, 0, false, false));
		}

        options.GetComponent<RectTransform>().localPosition = new Vector2(-Screen.width / 5, -Screen.height / 10);
        options.GetComponent<CanvasGroup>().alpha = Alpha;
        options.GetComponent<CanvasGroup>().interactable = IsInteractable;
        options.GetComponent<CanvasGroup>().blocksRaycasts = BlockRaycasts;
    }
	public void ActivateSkills(int Alpha, bool IsInteractable, bool BlockRaycasts)
	{
		if (characterSkills == null)
		{
			characterSkills = Instantiate(characterSkillsPrefab, new Vector2(0, 0), transform.rotation) as GameObject;
            characterSkills.transform.SetParent(canvas.transform);
            characterSkillsBarGUI.characterSkills = characterSkills.GetComponentInChildren<CharacterSkillsGUI>();
			characterSkills.transform.localScale = new Vector3(1,1,1);
			characterSkills.GetComponentInChildren<Button>().onClick.AddListener(()	=>	SetActiveWindow(characterSkills, 0, false, false));
		}

        characterSkills.GetComponent<RectTransform>().localPosition = new Vector2(-Screen.width / 5, -Screen.height / 5);
        characterSkills.GetComponent<CanvasGroup>().alpha = Alpha;
        characterSkills.GetComponent<CanvasGroup>().interactable = IsInteractable;
        characterSkills.GetComponent<CanvasGroup>().blocksRaycasts = BlockRaycasts;
    }
    public void ActivateUpgrade(int Alpha, bool IsInteractable, bool BlockRaycasts)
    {
        if (Upgrades == null)
        {
            Upgrades = Instantiate(UpgradesPrefab, new Vector2(0, 0), transform.rotation) as GameObject;
            Upgrades.transform.SetParent(canvas.transform);
            Upgrades.transform.localScale = new Vector3(1, 1, 1);
            Upgrades.GetComponentInChildren<Button>().onClick.AddListener(() => SetActiveWindow(Upgrades, 0, false, false));
        }

        Upgrades.GetComponent<RectTransform>().localPosition = new Vector2(Screen.width / 5, Screen.height / 10);
        Upgrades.GetComponent<CanvasGroup>().alpha = Alpha;
        Upgrades.GetComponent<CanvasGroup>().interactable = IsInteractable;
        Upgrades.GetComponent<CanvasGroup>().blocksRaycasts = BlockRaycasts;
    }

    public void ActivateSaveAndLoadGUI(int Alpha, bool IsInteractable, bool BlockRaycasts)
    {
        if (SaveAndLoadGameObject == null)
        {
            SaveAndLoadGameObject = Instantiate(SaveAndLoadGamePrefab, new Vector2(0, 0), transform.rotation) as GameObject;
            SaveAndLoadGameObject.transform.SetParent(canvas.transform);
            SaveAndLoadGameObject.transform.localScale = new Vector3(1, 1, 1);
            SaveAndLoadGameObject.GetComponentInChildren<Button>().onClick.AddListener(() => SetActiveWindow(SaveAndLoadGameObject, 0, false, false));
            SaveAndLoadGameObject.GetPhotonView().viewID = PhotonNetwork.AllocateViewID();
            SaveAndLoadGameObject.GetComponent<SaveAndLoadGUI>().LoadThis();
            SaveLoadGUI = SaveAndLoadGameObject.GetComponent<SaveAndLoadGUI>();
            terrain.saveLoadGUI = SaveLoadGUI;
        }

        SaveAndLoadGameObject.GetComponent<RectTransform>().localPosition = new Vector2(Screen.width / 5, Screen.height / 10);
        SaveAndLoadGameObject.GetComponent<CanvasGroup>().alpha = Alpha;
        SaveAndLoadGameObject.GetComponent<CanvasGroup>().interactable = IsInteractable;
        SaveAndLoadGameObject.GetComponent<CanvasGroup>().blocksRaycasts = BlockRaycasts;
    }

    public IEnumerator FadeTextToFullAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }
    }

    public IEnumerator FadeTextToZeroAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }

    public IEnumerator ActivateGeneralText(string GenText)
    {
        GeneralText.GetComponent<Text>().color = new Color(GeneralText.GetComponent<Text>().color.r, GeneralText.GetComponent<Text>().color.g, GeneralText.GetComponent<Text>().color.b, 0);
        GeneralText.GetComponent<Text>().text = GenText;
        StartCoroutine(FadeTextToFullAlpha(1f, GeneralText.GetComponent<Text>()));
        yield return new WaitForSeconds(2);
        GeneralText.GetComponent<Text>().text = string.Empty;
    }

    public IEnumerator ActivateProgressBar(float skillDuration, string Name)
    {
        yield return new WaitForSeconds(0.2f);

        ProgressBar.transform.Find("CurrentProgressBar").GetComponent<Image>().fillAmount += skillDuration;
        ProgressBar.transform.GetComponent<Mask>().showMaskGraphic = true;
        ProgressBar.transform.GetComponentInChildren<Text>().text = Name;

        if ((ProgressBar.transform.Find("CurrentProgressBar").GetComponent<Image>().fillAmount != 1 && CheckMovementProgressBar == false) ||
            charactermovement.Attacking == true)
            StartCoroutine(ActivateProgressBar(skillDuration, Name));
        else
        {
            ProgressBar.transform.Find("CurrentProgressBar").GetComponent<Image>().fillAmount = 0;
            ProgressBar.transform.GetComponent<Mask>().showMaskGraphic = false;
            ProgressBar.transform.GetComponentInChildren<Text>().text = string.Empty;
        }
    }

    public IEnumerator ActivateDesc()
    {
        yield return new WaitForSeconds(1.2f);
        StartCoroutine(ActivateDesc());

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 250))
        {
            if (hit.transform.CompareTag("Environment"))
            {                
                timeElapsedExamine = Time.time + 3;
                EnvironmentDesc.GetComponent<Mask>().showMaskGraphic = true;
                if (hit.transform.name == "Cylinder_001") // tree
                    EnvironmentDesc.GetComponentInChildren<Text>().text = hit.transform.parent.name.Remove(hit.transform.parent.name.IndexOf('('), hit.transform.parent.name.Length - hit.transform.parent.name.IndexOf('('));
                else // everything else
                    EnvironmentDesc.GetComponentInChildren<Text>().text = hit.transform.name.Remove(hit.transform.name.IndexOf('('), hit.transform.name.Length - hit.transform.name.IndexOf('('));
            }
            else if (hit.transform.CompareTag("Monster"))
            {
                timeElapsedExamine = Time.time + 1;
                EnvironmentDesc.GetComponent<Mask>().showMaskGraphic = true;
                EnvironmentDesc.GetComponentInChildren<Text>().text = "Threat: " + hit.transform.GetComponent<MonsterOnCollision>().MonsterLevel.ToString();
                transform.Find("EnemyHealth").Find("EnemyCurrentHealth").GetComponent<Image>().fillAmount = hit.transform.GetComponent<MonsterOnCollision>().MonsterHealth / hit.transform.GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier;
                transform.Find("EnemyHealth").GetComponentInChildren<Text>().text = Mathf.Round(hit.transform.GetComponent<MonsterOnCollision>().MonsterHealth) + "/" + Mathf.Round(hit.transform.GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier);
            }
        }

        if (Time.time >= timeElapsedExamine)
        {
            EnvironmentDesc.GetComponent<Mask>().showMaskGraphic = false;
            EnvironmentDesc.GetComponentInChildren<Text>().text = string.Empty;
        }
    }

    public void AutoRunning()
    {
        if (AutoRun == false)
            AutoRun = true;
        else
            AutoRun = false;
    }
    void Awake()
	{
        terrain = GameObject.FindGameObjectWithTag("MainEnvironment").GetComponent<TerrainScript>();
        monsterSpawn = GameObject.FindWithTag("MainEnvironment").GetComponent<MonsterSpawn>();
        monsterFunction = GameObject.FindWithTag("MainEnvironment").GetComponent<MonsterFunctions>();
        MiscItems = terrain.Player.GetComponent<MiscellaneousItemsDatabase>();
        switchWeapons = terrain.Player.GetComponentInChildren<WeaponSwitch>();
        switchArmors = terrain.Player.GetComponentInChildren<ArmorSwitch>();
        Stats = terrain.Player.GetComponent<CharacterStats>();
        CraftingDB = terrain.Player.GetComponent<CraftingSkillDatabase>();
        GatheringDB = terrain.Player.GetComponent<GatheringSkillDatabase>();
        GeneralDB = terrain.Player.GetComponent<GeneralSkillsDatabase>();
        characterSkillsBarGUI = canvas.GetComponentInChildren<CharacterSkillBarGUI>();
        characterinventory = canvas.GetComponentInChildren<CharacterInventoryGUI>();
        choptrees = terrain.Player.GetComponentInChildren<ChopTrees>();
        minerocks = terrain.Player.GetComponentInChildren<MineRocks>();
        herbloring = terrain.Player.GetComponentInChildren<Herbloring>();

        Sensitivity = 0.5f;
        SoundLevel = 1;
        MusicLevel = 1;

        KeyBinds = new List<string>();

		KeyBinds.Add(ForwardKey);
		KeyBinds.Add(LeftKey);
		KeyBinds.Add(RightKey);
		KeyBinds.Add(Backward);
		KeyBinds.Add(Jump);
		KeyBinds.Add(Teleport);
		KeyBinds.Add(FirstKey);
		KeyBinds.Add(SecondKey);
		KeyBinds.Add(ThirdKey);
		KeyBinds.Add(FourthKey);
		KeyBinds.Add(FifthKey);
		KeyBinds.Add(SixthKey);
		KeyBinds.Add(SeventhKey);
		KeyBinds.Add(EighthKey);
		KeyBinds.Add(PickupKey);
		KeyBinds.Add(InventoryKey);
		KeyBinds.Add(CharacterKey);
		KeyBinds.Add(OptionKey);
		KeyBinds.Add(SkillKey);
		KeyBinds.Add(UpgradeKey);
        KeyBinds.Add(AutoRunKey);

        StartCoroutine(ActivateDesc());

        ActivateOptions(1, true, true);
        ActivateOptions(0, false, false);

        ActivateInventory(1, true, true);
        ActivateInventory(0, false, false);

        ActivateSaveAndLoadGUI(1, true, true);
        ActivateSaveAndLoadGUI(0, false, false);

        ActivateUpgrade(1, true, true);
        ActivateUpgrade(0, false, false);
    }

    private void Start()
    {
        charactermovement = terrain.Player.GetComponentInChildren<CharacterMovement>();
        //call again to get data again;
        terrain = GameObject.FindGameObjectWithTag("MainEnvironment").GetComponent<TerrainScript>();
        monsterSpawn = GameObject.FindWithTag("MainEnvironment").GetComponent<MonsterSpawn>();
        monsterFunction = GameObject.FindWithTag("MainEnvironment").GetComponent<MonsterFunctions>();
        MiscItems = terrain.Player.GetComponent<MiscellaneousItemsDatabase>();
        switchWeapons = terrain.Player.GetComponentInChildren<WeaponSwitch>();
        switchArmors = terrain.Player.GetComponentInChildren<ArmorSwitch>();
        Stats = terrain.Player.GetComponent<CharacterStats>();
        CraftingDB = terrain.Player.GetComponent<CraftingSkillDatabase>();
        GatheringDB = terrain.Player.GetComponent<GatheringSkillDatabase>();
        GeneralDB = terrain.Player.GetComponent<GeneralSkillsDatabase>();
        characterSkillsBarGUI = canvas.GetComponentInChildren<CharacterSkillBarGUI>();
        characterinventory = canvas.GetComponentInChildren<CharacterInventoryGUI>();
        choptrees = terrain.Player.GetComponentInChildren<ChopTrees>();
        minerocks = terrain.Player.GetComponentInChildren<MineRocks>();
        herbloring = terrain.Player.GetComponentInChildren<Herbloring>();
    }

    void Update()
	{
		if (Input.GetKey(KeyBinds[15]) && DisableKeyBindsWhenTyping == false)
		{
            ActivateInventory(1, true, true);
        }
		if (Input.GetKey(KeyBinds[16]) && DisableKeyBindsWhenTyping == false)
		{
			ActivateStats(1, true, true);
		}
		if (Input.GetKey(KeyBinds[17]) && DisableKeyBindsWhenTyping == false)
		{
			ActivateOptions(1, true, true);
		}
		if (Input.GetKey(KeyBinds[18]) && DisableKeyBindsWhenTyping == false)
		{
			ActivateSkills(1, true, true);
		}
        if (Input.GetKey(KeyBinds[19]) && DisableKeyBindsWhenTyping == false)
        {
            ActivateUpgrade(1, true, true);
        }
        if (Input.GetKey(KeyBinds[20]) && DisableKeyBindsWhenTyping == false)
            AutoRunning();

        if (EventSystem.current.IsPointerOverGameObject())
        {
            DisableAttackWhenMenuItemPressed = true;
        }
        else
        {
            DisableAttackWhenMenuItemPressed = false;
        }
    }
}
