using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SaveAndLoadGUI : Photon.MonoBehaviour, IPointerDownHandler
{

    CharacterStats Stats;
    GeneralSkillsDatabase GeneralDB;
    GatheringSkillDatabase GatheringDB;
    CraftingSkillDatabase CraftingDB;
    CharacterOptionsGUI optionGUI;
    WeaponSwitch switchWeapons;
    ArmorSwitch switchArmors;

    MonsterSpawn monsterSpawn;
    MonsterFunctions monsterFunction;
    TerrainScript terrainState;
    MainGUI mainGui;
    public MainMenu mainmenuCanvas;

    public TerrainUserData MyTerrainData;
    public CharacterUserData MyCharacterData;

    public string CharacterFileLocation;
    public string TerrainFileLocation;
    public string CharacterFileName;
    public string TerrainFileName;

    string data;

    public Image ListofSavedGamesImage;
    public List<Button> ListofSaveButtons;
    int AutoSaveIndex; 
    public Text CurrentSaveSelectedButton; 
    public string CurrentSaveSelectedButtonText; // for loading into level from menu
    public InputField InputSaveImage;
    public Button ButtonPrefab;

    DirectoryInfo info;
    FileInfo[] AllXMLFiles;
    string[] StringXmlFiles;

    //More Things to Save :
    //All Options Settings, water  
    //Current Weapons, armor, skillbargui // how to save weapon/armor - if slot is darkened, get slot and instantiate it

    public class Vector3SerializationSurrogate : ISerializationSurrogate // serialize/deserialize vector3 and quaternion
    {
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context) // serialize
        {

            Vector3 v3 = (Vector3)obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
        }

        public System.Object SetObjectData(System.Object obj, SerializationInfo info,
                                           StreamingContext context, ISurrogateSelector selector) // deserialize
        {

            Vector3 v3 = (Vector3)obj;
            v3.x = (float)info.GetValue("x", typeof(float));
            v3.y = (float)info.GetValue("y", typeof(float));
            v3.z = (float)info.GetValue("z", typeof(float));
            obj = v3;
            return obj;
        }
    }

    public class QuaternionSerializationSurrogate : ISerializationSurrogate // serialize vector3 and quaternion
    {

        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
        {

            Quaternion q4 = (Quaternion)obj;
            info.AddValue("x", q4.x);
            info.AddValue("y", q4.y);
            info.AddValue("z", q4.z);
            info.AddValue("w", q4.w);
        }

        public System.Object SetObjectData(System.Object obj, SerializationInfo info,
                                           StreamingContext context, ISurrogateSelector selector)
        {

            Quaternion q4 = (Quaternion)obj;
            q4.x = (float)info.GetValue("x", typeof(float));
            q4.y = (float)info.GetValue("y", typeof(float));
            q4.z = (float)info.GetValue("z", typeof(float));
            q4.w = (float)info.GetValue("w", typeof(float));
            obj = q4;
            return obj;
        }
    }

    public void OnPointerDown(PointerEventData data)
    {
        transform.SetAsLastSibling();
    }

    public void LoadThis() // problem == only 1 player has control of this and its set to inactive == no rpc recieved, put saveandloadgui script in canvas, mainmenu, same with options
    {
        CurrentSaveSelectedButtonText = PlayerPrefs.GetString("LoadSave"); // playerprefs setstring in mainmenu

        CharacterFileName = "Default.dat";
        TerrainFileName = "Default.dat";
        TerrainFileLocation = Application.dataPath + "\\" + "TerrainSaveData";
        CharacterFileLocation = Application.dataPath + "\\" + "CharacterSaveData";

        monsterSpawn = GameObject.FindWithTag("MainEnvironment").GetComponent<MonsterSpawn>();
        monsterFunction = GameObject.FindWithTag("MainEnvironment").GetComponent<MonsterFunctions>();
        terrainState = GameObject.FindWithTag("MainEnvironment").GetComponent<TerrainScript>();
        mainGui = terrainState.canvas.GetComponent<MainGUI>();
        switchWeapons = terrainState.Player.GetComponentInChildren<WeaponSwitch>();
        switchArmors = terrainState.Player.GetComponentInChildren<ArmorSwitch>();
        Stats = terrainState.Player.GetComponent<CharacterStats>();
        CraftingDB = terrainState.Player.GetComponent<CraftingSkillDatabase>();
        GatheringDB = terrainState.Player.GetComponent<GatheringSkillDatabase>();
        GeneralDB = terrainState.Player.GetComponent<GeneralSkillsDatabase>();

        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            LoadXML(PlayerPrefs.GetString("PlayerName"), "Character", CharacterFileLocation); // if player is new then currentsavetext == empty

            if (MyCharacterData.ThisCharacterData.PlayerName != string.Empty &&
               MyCharacterData.ThisCharacterData.PlayerName != null)
            {
                Debug.Log(MyCharacterData.ThisCharacterData.PlayerName);
                terrainState.IsThisALoadedCharacterGame = true;
            }
            else
            {
                terrainState.IsThisALoadedCharacterGame = false;
                Stats.PlayerName = PlayerPrefs.GetString("PlayerName").Substring(0, PlayerPrefs.GetString("PlayerName").IndexOf(' '));
            }

            if (CurrentSaveSelectedButtonText != string.Empty)
                terrainState.IsThisALoadedTerrainGame = true;

            StartCoroutine("WaitLoadDataFunction"); // disabled for testing
        }
    }

    IEnumerator WaitLoadDataFunction()
    {
        yield return new WaitForSeconds(0.5f);

        if (terrainState.ListofWaters.Count < 98) // wait until stuff is all loaded
            StartCoroutine("WaitLoadDataFunction");
        else
            LoadDataFunction();
    }

    void SaveDataFunction()
    {
        if (PhotonNetwork.isMasterClient)
        {
            mainGui.photonView.RPC("SaveMultiplayer", PhotonTargets.All, TerrainFileName, "Manual");
        }
    }

    void AutoSaveDataFunction() // only autosave at start, and when a person goes out of range/terrain  
    {
        if (PhotonNetwork.isMasterClient)
        {
            mainGui.photonView.RPC("SaveMultiplayer", PhotonTargets.All, TerrainFileName, "Auto");
        }
    }

    public void LoadDataFunction() // still need to save weaponswitch, inventorymanage, weapons, skill,skillpts, skillbar, 
    {
        if (Application.loadedLevelName == "MainMenu" &&
            PhotonNetwork.playerList.Length >= 2 && PhotonNetwork.isMasterClient) // make list of saves on main menu, and rpc that load to all players, also, when saving a game, rpc, 
        {
            this.transform.SetParent(null);
            mainmenuCanvas.LoadGame(1);
        }
        else if (Application.loadedLevelName == "MainMenu" &&
                    PhotonNetwork.playerList.Length == 1)
        {
            this.transform.SetParent(null);
            mainmenuCanvas.LoadGame(MyTerrainData.ThisTerrainData.CurrentTerrainLevel);
        }

        //MyTerrainData = (UserData)DeserializeObject(data); // reference type userdata for correct types, xml
        //Debug.Log(MyCharacterData.ThisCharacterData.PlayerName);

        if (Application.loadedLevelName != "MainMenu" && terrainState.IsThisALoadedCharacterGame == true) // characters
        {
            Stats.PlayerName = MyCharacterData.ThisCharacterData.PlayerName;          
            //Stats.transform.position = MyCharacterData.ThisCharacterData.PlayerPosition; // only keep master position because if new game then it spawns below level.
            //Stats.transform.rotation = MyCharacterData.ThisCharacterData.PlayerRotation;
            Stats.PlayerLevel = MyCharacterData.ThisCharacterData.PlayerLevel;
            Stats.PlayerHealth = MyCharacterData.ThisCharacterData.PlayerHealth;
            Stats.CurrentPlayerHealth = MyCharacterData.ThisCharacterData.CurrentPlayerHealth;
            Stats.PlayerStamina = MyCharacterData.ThisCharacterData.PlayerStamina;
            Stats.CurrentPlayerStamina = MyCharacterData.ThisCharacterData.CurrentPlayerStamina;
            Stats.PlayerExperience = MyCharacterData.ThisCharacterData.PlayerExperience;
            Stats.ExpRequired = MyCharacterData.ThisCharacterData.ExpRequired;
            Stats.SkillPoints = MyCharacterData.ThisCharacterData.SkillPoints;
            Stats.CurrentHunger = MyCharacterData.ThisCharacterData.CurrentHunger;
            Stats.CurrentThirst = MyCharacterData.ThisCharacterData.CurrentThirst;

            //optionGUI = terrainState.canvas.GetComponentInChildren<CharacterOptionsGUI>();
            //if (optionGUI.SaveAndLoadGameObject != null)
            //    Destroy(optionGUI.SaveAndLoadGameObject);
            //optionGUI.SaveAndLoadGameObject = this.gameObject; // not have 2 save/load objects(dontdestroyonload)

            for (int i = 0; i < GatheringDB.GatheringSkillList.Count; i++)
            {
                GatheringDB.GatheringSkillList[i].CurrentRank = MyCharacterData.ThisCharacterData.GatheringCurrentRank[i];
                GatheringDB.GatheringSkillList[i].CurrentExp = MyCharacterData.ThisCharacterData.GatheringCurrentExp[i];
                GatheringDB.GatheringSkillList[i].MaxExp = MyCharacterData.ThisCharacterData.GatheringMaxExp[i];
            }

            for (int i = 0; i < CraftingDB.CraftingSkillList.Count; i++)
            {
                CraftingDB.CraftingSkillList[i].CurrentRank = MyCharacterData.ThisCharacterData.CraftingCurrentRank[i];
                CraftingDB.CraftingSkillList[i].CurrentExp = MyCharacterData.ThisCharacterData.CraftingCurrentExp[i];
                CraftingDB.CraftingSkillList[i].MaxExp = MyCharacterData.ThisCharacterData.CraftingMaxExp[i];
            }

            for (int i = 0; i < GeneralDB.GeneralSkillList.Count; i++)
            {
                GeneralDB.GeneralSkillList[i].CoolDown = MyCharacterData.ThisCharacterData.GeneralCD[i];
                GeneralDB.GeneralSkillList[i].Duration = MyCharacterData.ThisCharacterData.GeneralDur[i];
                GeneralDB.GeneralSkillList[i].LevelRank = MyCharacterData.ThisCharacterData.GeneralkillLevelRank[i];
                GeneralDB.GeneralSkillList[i].SkillChance = MyCharacterData.ThisCharacterData.SkillChance[i];
                GeneralDB.GeneralSkillList[i].SkillPointsRequired = MyCharacterData.ThisCharacterData.GeneralSkillPointRequired[i];
                GeneralDB.GeneralSkillList[i].SkillValue[0] = MyCharacterData.ThisCharacterData.SkillValue[i];
            }

            terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage = new List<InventoryManager>();

            for (int i = 0; i < terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsIcons.Count; i++)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsIcons[i].sprite = terrainState.canvas.GetComponent<CharacterInventoryGUI>().DefaultSprite;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsIcons[i].GetComponentInChildren<Mask>().showMaskGraphic = false;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsStackText[i].text = string.Empty;
            }

            for (int i = 0; i < MyCharacterData.ThisCharacterData.SlotName.Length; i++) //stacks keeps updating from previous save?
            {
                InventoryManager invManage = new InventoryManager(MyCharacterData.ThisCharacterData.SlotName[i], MyCharacterData.ThisCharacterData.Rarity[i], Resources.Load<Sprite>(MyCharacterData.ThisCharacterData.tSprite[i]), null, MyCharacterData.ThisCharacterData.InvDamageOrValue[i], MyCharacterData.ThisCharacterData.InvWeaponAttackSpeed[i], MyCharacterData.ThisCharacterData.InvCritRate[i], MyCharacterData.ThisCharacterData.InvArmorPen[i], MyCharacterData.ThisCharacterData.InvDefense[i], MyCharacterData.ThisCharacterData.InvHealth[i], MyCharacterData.ThisCharacterData.InvStamina[i], MyCharacterData.ThisCharacterData.CurrentInventorySlot[i], MyCharacterData.ThisCharacterData.StackAmounts[i], MyCharacterData.ThisCharacterData.isASecondary[i]);
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Add(invManage);
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsIcons[terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].CurrentInventorySlot].sprite = invManage.tSprite;
                if (terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].isASecondary == 1)
                    terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsStackText[terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].CurrentInventorySlot].text = invManage.StackAmounts.ToString();
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsIcons[terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].CurrentInventorySlot].GetComponent<Mask>().showMaskGraphic = true;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().FindNextSlot();
            }

            if (MyCharacterData.ThisCharacterData.CurrentSlotMainWeapon != -1)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().CurrentInventoryItemSlot = MyCharacterData.ThisCharacterData.CurrentSlotMainWeapon;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetWeaponValues();
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetToolValues();
            }
            if (MyCharacterData.ThisCharacterData.CurrentSlotSecondaryWeapon != -1)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().CurrentInventoryItemSlot = MyCharacterData.ThisCharacterData.CurrentSlotSecondaryWeapon;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetWeaponValues();
            }
            if (MyCharacterData.ThisCharacterData.CurrentSlotHead != -1)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().CurrentInventoryItemSlot = MyCharacterData.ThisCharacterData.CurrentSlotHead;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetArmorValues();
            }
            if (MyCharacterData.ThisCharacterData.CurrentSlotArmor != -1)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().CurrentInventoryItemSlot = MyCharacterData.ThisCharacterData.CurrentSlotArmor;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetArmorValues();
            }
            if (MyCharacterData.ThisCharacterData.CurrentSlotLegs != -1)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().CurrentInventoryItemSlot = MyCharacterData.ThisCharacterData.CurrentSlotLegs;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetArmorValues();
            }
            for (int i = 0; i < MyCharacterData.ThisCharacterData.TotalMiscStackAmounts.Length; i++)
                terrainState.Player.GetComponent<MiscellaneousItemsDatabase>().TotalMiscellaneousStacks[i] = MyCharacterData.ThisCharacterData.TotalMiscStackAmounts[i];

        }

        if  (Application.loadedLevelName != "MainMenu" && CurrentSaveSelectedButtonText != string.Empty) // Terrain 
        {
            LoadXML(CurrentSaveSelectedButtonText, "Terrain", TerrainFileLocation); // if player is new then currentsavetext == empty
            terrainState.CurrentTerrainLevel = MyTerrainData.ThisTerrainData.CurrentTerrainLevel;

            for (int i = 1; i < terrainState.ListofTrees.Count; i++)
            {
                terrainState.ListofTrees[i].transform.position = MyTerrainData.ThisTerrainData.TreePosition[i];
                terrainState.ListofTrees[i].transform.rotation = MyTerrainData.ThisTerrainData.TreeRotation[i];

                terrainState.ListofTrees[i].transform.localScale = MyTerrainData.ThisTerrainData.TreeScale[i];
                terrainState.ListofTrees[i].transform.GetComponent<Rigidbody>().mass = 3000 * MyTerrainData.ThisTerrainData.TreeScale[i].x;
            }

            for (int i = 1; i < terrainState.ListofRocks.Count; i++)
            {
                terrainState.ListofRocks[i].transform.position = MyTerrainData.ThisTerrainData.RockPosition[i];
                terrainState.ListofRocks[i].transform.rotation = MyTerrainData.ThisTerrainData.RockRotation[i];

                terrainState.ListofRocks[i].transform.localScale = MyTerrainData.ThisTerrainData.RockScale[i];
                terrainState.ListofRocks[i].transform.GetComponent<Rigidbody>().mass = 3000 * MyTerrainData.ThisTerrainData.RockScale[i].x;
            }

            for (int i = 1; i < terrainState.ListofHerbPatches.Count; i++)
            {
                terrainState.ListofHerbPatches[i].transform.position = MyTerrainData.ThisTerrainData.HerbPatchPosition[i];
                terrainState.ListofHerbPatches[i].transform.rotation = MyTerrainData.ThisTerrainData.HerbPatchRotation[i];
                terrainState.ListofHerbPatches[i].transform.localScale = MyTerrainData.ThisTerrainData.HerbPatchScale[i];
            }

            for (int i = 1; i < terrainState.ListofWaters.Count; i++)
            {
                terrainState.ListofWaters[i].transform.position = MyTerrainData.ThisTerrainData.WaterPosition[i];
                terrainState.ListofWaters[i].transform.rotation = MyTerrainData.ThisTerrainData.WaterRotation[i];
                terrainState.ListofWaters[i].transform.localScale = MyTerrainData.ThisTerrainData.WaterScale[i];
            }

            monsterSpawn.BossMonster.transform.position = MyTerrainData.ThisTerrainData.BossMonsterSpawnLocation;
            monsterSpawn.BossMonster.transform.rotation = MyTerrainData.ThisTerrainData.BossMonsterSpawnRotation;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterHealth = MyTerrainData.ThisTerrainData.BossMonsterHealth;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier = MyTerrainData.ThisTerrainData.BossMonsterHealthRandomModifier;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterDefense = MyTerrainData.ThisTerrainData.BossMonsterDefense;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterDamage = MyTerrainData.ThisTerrainData.BossMonsterDamage;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterExp = MyTerrainData.ThisTerrainData.BossMonsterExperience;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterLevel = MyTerrainData.ThisTerrainData.BossMonsterLevel;

            for (int i = 0; i < monsterSpawn.ListofMonsters.Count; i++)
            {
                monsterSpawn.ListofMonsters[i].transform.position = MyTerrainData.ThisTerrainData.MonsterSpawnLocations[i];
                monsterSpawn.ListofMonsters[i].transform.rotation = MyTerrainData.ThisTerrainData.MonsterSpawnRotations[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterMovement>().photonView.RPC("ChangeAppearanceDeath", PhotonTargets.All, MyTerrainData.ThisTerrainData.MonsterMaterials[0 * i], MyTerrainData.ThisTerrainData.MonsterMaterials[1 * i], MyTerrainData.ThisTerrainData.MonsterMaterials[2 * i]);
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterHealth = MyTerrainData.ThisTerrainData.MonsterHealth[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier = MyTerrainData.ThisTerrainData.MonsterHealthRandomModifier[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterDefense = MyTerrainData.ThisTerrainData.MonsterDefense[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterDamage = MyTerrainData.ThisTerrainData.MonsterDamage[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterExp = MyTerrainData.ThisTerrainData.MonsterExperience[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterLevel = MyTerrainData.ThisTerrainData.MonsterLevel[i];
            }

            //terrainState.terrainMap = terrainState.terrain.terrainData.GetHeights(0, 0, terrainState.terrain.terrainData.heightmapWidth, terrainState.terrain.terrainData.heightmapHeight);
            //terrainState.MiddleAlpha = terrainState.terrain.terrainData.GetAlphamaps(0, 0, terrainState.terrain.terrainData.alphamapWidth, terrainState.terrain.terrainData.alphamapHeight);
            //terrainState.detailLayer1 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 0);
            //terrainState.detailLayer2 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 1);
            //terrainState.detailLayer3 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 2);
            //terrainState.detailLayer4 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 3);
            //terrainState.detailLayer5 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 4);
            //terrainState.detailLayer6 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 5);
            //terrainState.detailLayer7 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 6);
            ////terrainState.detailLayer8 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 7);

            //for (int i = 0; i < terrainState.terrain.terrainData.heightmapHeight; i++)
            //{
            //    for (int j = 0; j < terrainState.terrain.terrainData.heightmapWidth; j++)
            //    {
            //        terrainState.terrainMap[i, j] = MyTerrainData.ThisTerrainData.terrainMap[i * terrainState.terrain.terrainData.heightmapWidth + j];
            //    }
            //}

            //for (int i = 0; i < terrainState.terrain.terrainData.detailHeight; i++)
            //{
            //    for (int j = 0; j < terrainState.terrain.terrainData.detailWidth; j++)
            //    {
            //        terrainState.detailLayer1[i, j] = MyTerrainData.ThisTerrainData.detailLayer1[i * terrainState.terrain.terrainData.detailWidth + j];
            //        terrainState.detailLayer2[i, j] = MyTerrainData.ThisTerrainData.detailLayer2[i * terrainState.terrain.terrainData.detailWidth + j];
            //        terrainState.detailLayer3[i, j] = MyTerrainData.ThisTerrainData.detailLayer3[i * terrainState.terrain.terrainData.detailWidth + j];
            //        terrainState.detailLayer4[i, j] = MyTerrainData.ThisTerrainData.detailLayer4[i * terrainState.terrain.terrainData.detailWidth + j];
            //        terrainState.detailLayer5[i, j] = MyTerrainData.ThisTerrainData.detailLayer5[i * terrainState.terrain.terrainData.detailWidth + j];
            //        terrainState.detailLayer6[i, j] = MyTerrainData.ThisTerrainData.detailLayer6[i * terrainState.terrain.terrainData.detailWidth + j];
            //        terrainState.detailLayer7[i, j] = MyTerrainData.ThisTerrainData.detailLayer7[i * terrainState.terrain.terrainData.detailWidth + j];
            //        terrainState.detailLayer8[i, j] = MyTerrainData.ThisTerrainData.detailLayer8[i * terrainState.terrain.terrainData.detailWidth + j];
            //    }
            //}

            //for (int i = 0; i < terrainState.terrain.terrainData.alphamapHeight; i++)
            //{
            //    for (int j = 0; j < terrainState.terrain.terrainData.alphamapWidth; j++)
            //    {
            //        for (int k = 0; k < 2; k++)
            //        {
            //            terrainState.MiddleAlpha[i, j, k] = MyTerrainData.ThisTerrainData.MiddleAlpha[i + terrainState.terrain.terrainData.alphamapHeight * (j + terrainState.terrain.terrainData.alphamapWidth * k)];
            //        }
            //    }
            //}

            //terrainState.terrain.terrainData.SetAlphamaps(0, 0, terrainState.MiddleAlpha);
            //terrainState.terrain.terrainData.SetHeights(0, 0, terrainState.terrainMap);
            //terrainState.terrain.terrainData.SetDetailLayer(0, 0, 0, terrainState.detailLayer1);
            //terrainState.terrain.terrainData.SetDetailLayer(0, 0, 1, terrainState.detailLayer2);
            //terrainState.terrain.terrainData.SetDetailLayer(0, 0, 2, terrainState.detailLayer3);
            //terrainState.terrain.terrainData.SetDetailLayer(0, 0, 3, terrainState.detailLayer4);
            //terrainState.terrain.terrainData.SetDetailLayer(0, 0, 4, terrainState.detailLayer5);
            //terrainState.terrain.terrainData.SetDetailLayer(0, 0, 5, terrainState.detailLayer6);
            //terrainState.terrain.terrainData.SetDetailLayer(0, 0, 6, terrainState.detailLayer7);
            //terrainState.terrain.terrainData.SetDetailLayer(0, 0, 7, terrainState.detailLayer8);

            terrainState.gainX = MyTerrainData.ThisTerrainData.gainX;
            terrainState.gainY = MyTerrainData.ThisTerrainData.gainY;
            terrainState.gainXX = MyTerrainData.ThisTerrainData.gainXX;
            terrainState.gainYY = MyTerrainData.ThisTerrainData.gainYY;

            for (int i = 0; i < MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredIDs.Length; i++)
            {
                GameObject alteredID = PhotonView.Find(MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredIDs[i]).gameObject;
                terrainState.ListofEnvironmentalObjectsAlteredPositions.Add(MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredPositions[i]);
                terrainState.ListofEnvironmentalObjectsAlteredRotations.Add(MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredRotations[i]);
                alteredID.transform.GetComponent<Rigidbody>().isKinematic = false;
                StartCoroutine(monsterFunction.EnvironmentalPhysicsUndo(alteredID, MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredPositions[i], MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredRotations[i]));
            }
            if (PhotonNetwork.isMasterClient)
            {
                mainGui.photonView.RPC("SyncPeople", PhotonTargets.AllBufferedViaServer, Stats.transform.position, terrainState.MasterPosition.GetPhotonView().viewID, terrainState.gainX, terrainState.gainY, terrainState.gainXX, terrainState.gainYY, MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredIDs, MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredPositions, MyTerrainData.ThisTerrainData.ListofEnvironmentalObjectsAlteredRotations,
                    MyTerrainData.ThisTerrainData.TreePosition, MyTerrainData.ThisTerrainData.TreeRotation, MyTerrainData.ThisTerrainData.RockPosition, MyTerrainData.ThisTerrainData.RockRotation, MyTerrainData.ThisTerrainData.HerbPatchPosition, MyTerrainData.ThisTerrainData.HerbPatchRotation, MyTerrainData.ThisTerrainData.WaterPosition, MyTerrainData.ThisTerrainData.WaterRotation,
                    MyTerrainData.ThisTerrainData.BossMonsterHealth, MyTerrainData.ThisTerrainData.BossMonsterHealthRandomModifier, MyTerrainData.ThisTerrainData.BossMonsterDefense, MyTerrainData.ThisTerrainData.BossMonsterDamage, MyTerrainData.ThisTerrainData.BossMonsterExperience, MyTerrainData.ThisTerrainData.BossMonsterLevel,
                    MyTerrainData.ThisTerrainData.MonsterHealth, MyTerrainData.ThisTerrainData.MonsterHealthRandomModifier, MyTerrainData.ThisTerrainData.MonsterDefense, MyTerrainData.ThisTerrainData.MonsterDamage, MyTerrainData.ThisTerrainData.MonsterExperience, MyTerrainData.ThisTerrainData.MonsterLevel);

                mainGui.photonView.RPC("SyncScales", PhotonTargets.AllBufferedViaServer, MyTerrainData.ThisTerrainData.TreeScale, MyTerrainData.ThisTerrainData.RockScale, MyTerrainData.ThisTerrainData.HerbPatchScale, MyTerrainData.ThisTerrainData.WaterScale);
            }

            CurrentSaveSelectedButton = null;
            CurrentSaveSelectedButtonText = string.Empty;
            PlayerPrefs.SetString("LoadSave", string.Empty);
            mainGui.ActivateSaveAndLoadGUI(0, false, false);
            gameObject.transform.localScale = new Vector3(1, 1, 1); // pun rpc environment/alphas/heights...

            terrainState.NotLoading[0] = true;
            terrainState.NotLoading[1] = true;
        }
    }

    string UTF8ByteArrayToString(byte[] characters) // xml
    {
        UTF8Encoding encoding = new UTF8Encoding();
        string constructedString = encoding.GetString(characters);
        return (constructedString);
    }

    byte[] StringToUTF8ByteArray(string pXmlString) // xml
    {
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] byteArray = encoding.GetBytes(pXmlString);
        return byteArray;
    }

    //serialize our UserData
    string SerializeObject(object SerialzedObject, string TypeData)  //xml 
    {
        string XmlizedString = null;
        MemoryStream memoryStream = new MemoryStream();
        XmlSerializer XMLSerializer;
        if (TypeData == "Character")
            XMLSerializer = new XmlSerializer(typeof(CharacterUserData));
        else
            XMLSerializer = new XmlSerializer(typeof(TerrainUserData));

        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        XMLSerializer.Serialize(xmlTextWriter, SerialzedObject);
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
        XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
        return XmlizedString;
    }

    //deserialze our Userdata
    object DeserializeObject(string xmlString, string TypeData)   //xml
    {
        XmlSerializer XMLSerializer;
        if (TypeData == "Character")
            XMLSerializer = new XmlSerializer(typeof(CharacterUserData));
        else
            XMLSerializer = new XmlSerializer(typeof(TerrainUserData));

        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(xmlString));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        return XMLSerializer.Deserialize(memoryStream);
    }

    public void CreateXML(string TypeData, string fileLocation, string TypeofSave)
    {
        BinaryFormatter bf = new BinaryFormatter();
        SurrogateSelector surrogateSelector = new SurrogateSelector();
        Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();
        QuaternionSerializationSurrogate quaternion4SS = new QuaternionSerializationSurrogate();
        FileStream file = null;

        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
        surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternion4SS);
        bf.SurrogateSelector = surrogateSelector;

        AutoSaveIndex++;
        if (AutoSaveIndex > 2)
            AutoSaveIndex = 0;

        if (TypeData == "Character")
        {
            CharacterFileName = PlayerPrefs.GetString("PlayerName");

            if (File.Exists(fileLocation + "\\" + CharacterFileName + ".dat"))
            {
                file = File.Open(fileLocation + "\\" + CharacterFileName + ".dat", FileMode.Open);
                bf.Serialize(new BufferedStream(file), MyCharacterData);
            }
            else
            {
                file = File.Create(fileLocation + "\\" + CharacterFileName);
                bf.Serialize(new BufferedStream(file), MyCharacterData); // write to xml file
            }
            // create new file if it doesnt exist
        }
        else
        {
            if (InputSaveImage.text != string.Empty && TypeofSave == "Manual")
                TerrainFileName = InputSaveImage.text + "_" + Stats.PlayerName + "_Lvl" + Stats.PlayerLevel + " " + System.DateTime.Now.ToString("ddMMMyyyy HH;mm;ss") + ".dat";
            else if (TypeofSave == "Auto" || TypeofSave == "AutoStart")
                TerrainFileName = "Autosave" + "_" + Stats.PlayerName + "_Lvl" + Stats.PlayerLevel + " " + System.DateTime.Now.ToString("ddMMMyyyy HH;mm;ss") + ".dat";

            // dont save environment data in autosave only player data
            if ((TypeofSave == "Auto" || TypeofSave == "AutoStart") && ListofSaveButtons.Count > 2)
            {
                if (File.Exists(fileLocation + "\\" + ListofSaveButtons[AutoSaveIndex].GetComponentInChildren<Text>().text + ".dat")) // saving an autosave into another autosave                     
                {
                    System.IO.File.Move(fileLocation + "\\" + ListofSaveButtons[AutoSaveIndex].GetComponentInChildren<Text>().text + ".dat",
                        fileLocation + "\\" + TerrainFileName);
                
                    ListofSaveButtons[AutoSaveIndex].GetComponentInChildren<Text>().text = TerrainFileName.Remove(TerrainFileName.IndexOf("."), TerrainFileName.Length - TerrainFileName.IndexOf("."));
                    file = File.Open(fileLocation + "\\" + TerrainFileName, FileMode.Open);
                    bf.Serialize(new BufferedStream(file), MyTerrainData); // write to xml file
                }
                else if (ListofSaveButtons.Count < 8)
                {
                    file = File.Create(fileLocation + "\\" + TerrainFileName);
                    bf.Serialize(new BufferedStream(file), MyTerrainData); // write to xml file

                    if (ListofSaveButtons[AutoSaveIndex].GetComponentInChildren<Text>().text == "Empty AutoSave") // autosaving into an empty autosave
                    {
                        ListofSaveButtons[AutoSaveIndex].GetComponentInChildren<Text>().text = TerrainFileName.Remove(TerrainFileName.IndexOf("."), TerrainFileName.Length - TerrainFileName.IndexOf("."));
                    }
                    else // somehow an error occurs
                    {
                        Button SaveGameButton = Instantiate(ButtonPrefab, transform.position, transform.rotation)
                            as Button;
                        ListofSaveButtons.Add(SaveGameButton);
                        SaveGameButton.transform.SetParent(ListofSavedGamesImage.transform);
                        SaveGameButton.transform.localScale = SaveGameButton.transform.parent.localScale;
                        SaveGameButton.GetComponentInChildren<Text>().text = TerrainFileName.Remove(TerrainFileName.IndexOf("."), TerrainFileName.Length - TerrainFileName.IndexOf("."));
                        SaveGameButton.onClick.AddListener(() => CurrentSelectedSaveFunction(SaveGameButton));
                    }
                }
            }
            else if (ListofSaveButtons.Count < 8) // playermade saves
            {
                file = File.Create(fileLocation + "\\" + TerrainFileName);
                bf.Serialize(new BufferedStream(file), MyTerrainData);
                Button SaveGameButton = Instantiate(ButtonPrefab, transform.position, transform.rotation)
                    as Button;
                ListofSaveButtons.Add(SaveGameButton);
                SaveGameButton.transform.SetParent(ListofSavedGamesImage.transform);
                SaveGameButton.transform.localScale = SaveGameButton.transform.parent.localScale;
                SaveGameButton.GetComponentInChildren<Text>().text = TerrainFileName.Remove(TerrainFileName.IndexOf("."), TerrainFileName.Length - TerrainFileName.IndexOf(".")); 
                SaveGameButton.onClick.AddListener(() => CurrentSelectedSaveFunction(SaveGameButton));
            }
            else
            {
                mainGui.StartCoroutine(mainGui.ActivateGeneralText("Delete a save to make more space."));
            }
            
        }

        if (file != null)
            file.Close();

        // create a new save that can be clicked
        //xml
        //writer.Write(data);
        //writer.Close();
    }

    public void LoadXML(string CurrentButtonText, string TypeData, string fileLocation)
    {
        //xml
        //FileName = FileLocation + "\\" + CurrentButtonText.text + ".xml";
        //StreamReader reader = File.OpenText(FileName);
        //string inforeader = reader.ReadToEnd();
        //reader.Close();
        //data = inforeader;
        if (File.Exists(fileLocation + "\\" + CurrentButtonText + ".dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(fileLocation + "\\" + CurrentButtonText + ".dat", FileMode.Open);
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();
            QuaternionSerializationSurrogate quaternion4SS = new QuaternionSerializationSurrogate();

            surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
            surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternion4SS);
            bf.SurrogateSelector = surrogateSelector;
            file.Position = 0;

            if (TypeData == "Character" && file.Length > 0)
                MyCharacterData = (CharacterUserData)bf.Deserialize(file);
            else if (file.Length > 0)
                MyTerrainData = (TerrainUserData)bf.Deserialize(file);
            file.Close();
        }
        else
        {
            if (TypeData != "Character")
            {
                CurrentSaveSelectedButtonText = string.Empty; // new player new save multiplayer 
                PlayerPrefs.SetString("LoadSave", string.Empty);
            }
        }
    }

    public void DestroyButton()
    {
        if ( CurrentSaveSelectedButton == null ||CurrentSaveSelectedButton.text == string.Empty)
            return;

        for (int i = 0; i < ListofSaveButtons.Count; i++)
        {
            if (ListofSaveButtons[i].GetComponentInChildren<Text>().text == CurrentSaveSelectedButton.text && i < 3) // dont delete autosaves
                return;
        }

        FileInfo fileinfo = new FileInfo(TerrainFileLocation + "\\" + CurrentSaveSelectedButton.GetComponentInChildren<Text>().text + ".dat");
        fileinfo.Delete();
        CurrentSaveSelectedButton.text = string.Empty;
        CurrentSaveSelectedButtonText = string.Empty;
        ListofSaveButtons.Remove(CurrentSaveSelectedButton.transform.parent.GetComponent<Button>());
        Destroy(CurrentSaveSelectedButton.transform.parent.gameObject);
    }

    public void CurrentSelectedSaveFunction(Button CurrentButton)
    {
        for (int i = 0; i < ListofSaveButtons.Count; i++)
        {
            ListofSaveButtons[i].GetComponent<Image>().color = new Color(1, 1, 1, 0.66f);
        }
        CurrentSaveSelectedButton = CurrentButton.GetComponentInChildren<Text>();
        CurrentSaveSelectedButtonText = CurrentButton.GetComponentInChildren<Text>().text;
        CurrentButton.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.66f);
    }

    void Start()
    {
        CharacterFileName = "Default.dat";
        TerrainFileName = "Default.dat";
        TerrainFileLocation = Application.dataPath + "\\" + "TerrainSaveData";
        CharacterFileLocation = Application.dataPath + "\\" + "CharacterSaveData";

        if (Application.loadedLevelName != "MainMenu")
        {
            monsterSpawn = GameObject.FindWithTag("MainEnvironment").GetComponent<MonsterSpawn>();
            monsterFunction = GameObject.FindWithTag("MainEnvironment").GetComponent<MonsterFunctions>();
            terrainState = GameObject.FindWithTag("MainEnvironment").GetComponent<TerrainScript>();
            mainGui = terrainState.canvas.GetComponent<MainGUI>();
            switchWeapons = terrainState.Player.GetComponentInChildren<WeaponSwitch>();
            switchArmors = terrainState.Player.GetComponentInChildren<ArmorSwitch>();
            Stats = terrainState.Player.GetComponent<CharacterStats>();
            CraftingDB = terrainState.Player.GetComponent<CraftingSkillDatabase>();
            GatheringDB = terrainState.Player.GetComponent<GatheringSkillDatabase>();
            GeneralDB = terrainState.Player.GetComponent<GeneralSkillsDatabase>();
            terrainState.Player.GetComponent<ArmorSwitch>().thisRenderer = terrainState.Player.GetComponent<ArmorSwitch>().GetComponentInChildren<SkinnedMeshRenderer>(); // get naked bones without armor/weapons equipped
            InvokeRepeating("AutoSaveDataFunction", 15, 25); // only save terrain data at start, when autosaving skip that part for lag purposses
        }


        info = new DirectoryInfo(TerrainFileLocation);
        AllXMLFiles = info.GetFiles("*.dat*");
        StringXmlFiles = new string[AllXMLFiles.Length];
        ListofSaveButtons = new List<Button>();
        int AutoSaveCount = 0;

        for (int j = 0; j < StringXmlFiles.Length; j++)
        {
            StringXmlFiles[j] = AllXMLFiles[j].ToString().Remove(0, TerrainFileLocation.Length + 1);
            if (!StringXmlFiles[j].Contains(".meta") && StringXmlFiles[j].Contains("Autosave")) // for previous autosave
            {
                AutoSaveCount++;
                StringXmlFiles[j] = StringXmlFiles[j].Remove(StringXmlFiles[j].IndexOf("."), StringXmlFiles[j].Length - StringXmlFiles[j].IndexOf("."));

                Button SaveGameButton = Instantiate(ButtonPrefab, transform.position, transform.rotation)
                    as Button;
                ListofSaveButtons.Add(SaveGameButton);
                SaveGameButton.transform.SetParent(ListofSavedGamesImage.transform);
                SaveGameButton.transform.localScale = SaveGameButton.transform.parent.localScale;
                SaveGameButton.GetComponentInChildren<Text>().text = StringXmlFiles[j];
                SaveGameButton.onClick.AddListener(() => CurrentSelectedSaveFunction(SaveGameButton));
            }
        }
        for (int i = AutoSaveCount; i < 3; i++) // empty autosaves
        {
            Button SaveGameButton = Instantiate(ButtonPrefab, transform.position, transform.rotation)
                as Button;
            ListofSaveButtons.Add(SaveGameButton);
            SaveGameButton.transform.SetParent(ListofSavedGamesImage.transform);
            SaveGameButton.transform.localScale = SaveGameButton.transform.parent.localScale;
            SaveGameButton.GetComponentInChildren<Text>().text = "Empty AutoSave";
        }

        for (int j = 0; j < StringXmlFiles.Length; j++) // loads playermade saves
        {
            StringXmlFiles[j] = AllXMLFiles[j].ToString().Remove(0, TerrainFileLocation.Length + 1);

            if (!StringXmlFiles[j].Contains(".meta") && !StringXmlFiles[j].Contains("Autosave"))
            {
                StringXmlFiles[j] = StringXmlFiles[j].Remove(StringXmlFiles[j].IndexOf("."), StringXmlFiles[j].Length - StringXmlFiles[j].IndexOf("."));

                Button SaveGameButton = Instantiate(ButtonPrefab, transform.position, transform.rotation)
                    as Button;
                ListofSaveButtons.Add(SaveGameButton);
                SaveGameButton.transform.SetParent(ListofSavedGamesImage.transform);
                SaveGameButton.transform.localScale = SaveGameButton.transform.parent.localScale;
                SaveGameButton.GetComponentInChildren<Text>().text = StringXmlFiles[j];
                SaveGameButton.onClick.AddListener(() => CurrentSelectedSaveFunction(SaveGameButton));
            }
        }
    }

    void Awake()
    {
        if (Application.loadedLevelName == "MainMenu")
        {
            mainmenuCanvas = transform.root.GetComponent<MainMenu>();
        }

        MyTerrainData = new TerrainUserData();
        MyCharacterData = new CharacterUserData();
    }
}

[System.Serializable]
public class CharacterUserData // and monster data 
{

    public CharacterSerializeData ThisCharacterData;

    [System.Serializable]
    public struct CharacterSerializeData // sending gameobjects - get photonviewid
    { // vector3 and quaternion interface for serializing/deserializing custom classes
        private Vector3 playerPosition;
        public Vector3 PlayerPosition
        {
            get { return playerPosition; }
            set { playerPosition = value; }
        }
        private Quaternion playerRotation;
        public Quaternion PlayerRotation
        {
            get { return playerRotation; }
            set { playerRotation = value; }
        }
        public string PlayerName;
        public float PlayerHealth;
        public float CurrentPlayerHealth;
        public float PlayerStamina;
        public float CurrentPlayerStamina;
        public float PlayerExperience;
        public float PlayerLevel;
        public float ExpRequired;
        public float SkillPoints;
        public float CurrentHunger;
        public float CurrentThirst;
        // stats like damage/crit/atkspeed etc are applied by re-equipping armor/weapon in load section
        public int[] TotalMiscStackAmounts; 

        public int CurrentSlotMainWeapon;
        public int CurrentSlotSecondaryWeapon;
        public int CurrentSlotHead;
        public int CurrentSlotArmor;
        public int CurrentSlotLegs;

        //InventoryManage;
        public string[] SlotName;
        public string[] Rarity;
        public string[] tSprite;
        public float[] InvDamageOrValue;
        public float[] InvWeaponAttackSpeed;
        public float[] InvCritRate;
        public float[] InvArmorPen;
        public float[] InvDefense;
        public float[] InvHealth;
        public float[] InvStamina;
        public int[] CurrentInventorySlot;
        public int[] StackAmounts;
        public int[] isASecondary; //stackable
        public string[] Description; // still need to do

        public int[] GatheringCurrentRank;
        public float[] GatheringCurrentExp;
        public float[] GatheringMaxExp;

        public int[] CraftingCurrentRank;
        public float[] CraftingCurrentExp;
        public float[] CraftingMaxExp;

        // general skills

        public int[] GeneralSkillPointRequired;
        public float[] SkillChance;
        public float[] GeneralDur;
        public float[] GeneralCD;
        public float[] SkillValue;
        public int[] GeneralkillLevelRank;

        public string[] SkillBars;

    }
}

[System.Serializable]
public class TerrainUserData
{

    public TerrainSerializeData ThisTerrainData;

    [System.Serializable]
    public struct TerrainSerializeData // sending gameobjects - get photonviewid
    { // vector3 and quaternion interface for serializing/deserializing custom classes
        public int CurrentTerrainLevel;
        public int MasterPlayerGameObjectPositionViewID;

        public int[] ListofEnvironmentalObjectsAlteredIDs;
        private Vector3[] listofEnvironmentalObjectsAlteredPositions;
        public Vector3[] ListofEnvironmentalObjectsAlteredPositions
        {
            get { return listofEnvironmentalObjectsAlteredPositions; }
            set { listofEnvironmentalObjectsAlteredPositions = value; }
        }
        private Quaternion[] listofEnvironmentalObjectsAlteredRotations;
        public Quaternion[] ListofEnvironmentalObjectsAlteredRotations
        {
            get { return listofEnvironmentalObjectsAlteredRotations; }
            set { listofEnvironmentalObjectsAlteredRotations = value; }
        }
        private Vector3[] rockPosition;
        public Vector3[] RockPosition
        {
            get { return rockPosition; }
            set { rockPosition = value; }
        }
        private Vector3[] treePosition;
        public Vector3[] TreePosition
        {
            get { return treePosition; }
            set { treePosition = value; }
        }
        private Vector3[] herbPatchPosition;
        public Vector3[] HerbPatchPosition
        {
            get { return herbPatchPosition; }
            set { herbPatchPosition = value; }
        }
        private Vector3[] waterPosition;
        public Vector3[] WaterPosition
        {
            get { return waterPosition; }
            set { waterPosition = value; }
        }

        private Quaternion[] rockRotation;
        public Quaternion[] RockRotation
        {
            get { return rockRotation; }
            set { rockRotation = value; }
        }
        private Quaternion[] treeRotation;
        public Quaternion[] TreeRotation
        {
            get { return treeRotation; }
            set { treeRotation = value; }
        }
        private Quaternion[] herbPatchRotation;
        public Quaternion[] HerbPatchRotation
        {
            get { return herbPatchRotation; }
            set { herbPatchRotation = value; }
        }
        private Quaternion[] waterRotation;
        public Quaternion[] WaterRotation
        {
            get { return waterRotation; }
            set { waterRotation = value; }
        }

        private Vector3[] rockScale;
        public Vector3[] RockScale
        {
            get { return rockScale; }
            set { rockScale = value; }
        }
        private Vector3[] treeScale;
        public Vector3[] TreeScale
        {
            get { return treeScale; }
            set { treeScale = value; }
        }
        private Vector3[] herbPatchScale;
        public Vector3[] HerbPatchScale
        {
            get { return herbPatchScale; }
            set { herbPatchScale = value; }
        }
        private Vector3[] waterScale;
        public Vector3[] WaterScale
        {
            get { return waterScale; }
            set { waterScale = value; }
        }

        public int[] detailLayer1;
        public int[] detailLayer2;
        public int[] detailLayer3;
        public int[] detailLayer4;
        public int[] detailLayer5;
        public int[] detailLayer6;
        public int[] detailLayer7;
        public int[] detailLayer8;

        public float[] terrainMap;
        public float gainX;
        public float gainY;
        public float gainXX;
        public float gainYY;
        public float[] MiddleAlpha;

        // monster data - 

        private Vector3[] monsterSpawnLocations;
        public Vector3[] MonsterSpawnLocations
        {
            get { return monsterSpawnLocations; }
            set { monsterSpawnLocations = value; }
        }
        private Quaternion[] monsterSpawnRotations;
        public Quaternion[] MonsterSpawnRotations
        {
            get { return monsterSpawnRotations; }
            set { monsterSpawnRotations = value; }
        }
        private Vector3 bossMonsterSpawnLocation;
        public Vector3 BossMonsterSpawnLocation
        {
            get { return bossMonsterSpawnLocation; }
            set { bossMonsterSpawnLocation = value; }
        }
        private Quaternion bossMonsterSpawnRotation;
        public Quaternion BossMonsterSpawnRotation
        {
            get { return bossMonsterSpawnRotation; }
            set { bossMonsterSpawnRotation = value; }
        }

        public float BossMonsterHealth;
        public float BossMonsterHealthRandomModifier;
        public float BossMonsterDefense;
        public float BossMonsterDamage;
        public float BossMonsterExperience;
        public int BossMonsterLevel;

        public int[] MonsterMaterials;
        public float[] MonsterHealth;
        public float[] MonsterHealthRandomModifier;
        public float[] MonsterDefense;
        public float[] MonsterDamage;
        public float[] MonsterExperience;
        public int[] MonsterLevel;
    }
}