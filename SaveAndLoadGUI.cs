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

    public UserData MyData;

    string FileLocation;
    public string FileName;

    string data;

    public Image ListofSavedGamesImage;
    public List<Button> ListofSaveButtons;
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
        CurrentSaveSelectedButtonText = PlayerPrefs.GetString("LoadSave");

        if (SceneManager.GetActiveScene().buildIndex != 0 && CurrentSaveSelectedButtonText != string.Empty)
        {
            FileName = "Default.dat";
            FileLocation = Application.dataPath + "\\" + "SaveData";
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
            terrainState.IsThisALoadedGame = true;

            StartCoroutine("WaitLoadDataFunction");
        }
    }

    IEnumerator WaitLoadDataFunction()
    {
        yield return new WaitForSeconds(0.5f);

        LoadDataFunction();
    }

    void SaveDataFunction()
    {
        if (ListofSaveButtons.Count >= 8) // out of space
            return;

        if (PhotonNetwork.isMasterClient)
            mainGui.photonView.RPC("SaveMultiplayer", PhotonTargets.All, FileName);
        else
            mainGui.SaveMultiplayer(FileName);
    }

    

    public void LoadDataFunction() // still need to save weaponswitch, inventorymanage, weapons, skill,skillpts, skillbar, 
    {
        if (CurrentSaveSelectedButtonText == string.Empty)
            return;

        LoadXML(CurrentSaveSelectedButtonText); // if player is new then currentsavetext == empty

        if (Application.loadedLevelName == "MainMenu" &&
            PhotonNetwork.playerList.Length >= 2 && PhotonNetwork.isMasterClient) // make list of saves on main menu, and rpc that load to all players, also, when saving a game, rpc, 
        {
            this.transform.SetParent(null);
            mainmenuCanvas.MultiPlayerLoad();
        }
        else if (Application.loadedLevelName == "MainMenu" &&
                    PhotonNetwork.playerList.Length == 1)
        {
            this.transform.SetParent(null);
            mainmenuCanvas.SinglePlayerLoad(MyData.ThisPlayer.CurrentTerrainLevel);
        }

        //MyData = (UserData)DeserializeObject(data); // reference type userdata for correct types, xml
        if  (Application.loadedLevelName != "MainMenu" && CurrentSaveSelectedButtonText != string.Empty) // if new player and new save, skip this
        {
            Stats.transform.position = MyData.ThisPlayer.PlayerPosition;
            terrainState.MasterPosition = MyData.ThisPlayer.MasterPlayerPosition;
            Stats.transform.rotation = MyData.ThisPlayer.PlayerRotation;
            Stats.PlayerLevel = MyData.ThisPlayer.PlayerLevel;
            Stats.PlayerHealth = MyData.ThisPlayer.PlayerHealth;
            Stats.CurrentPlayerHealth = MyData.ThisPlayer.CurrentPlayerHealth;
            Stats.PlayerStamina = MyData.ThisPlayer.PlayerStamina;
            Stats.CurrentPlayerStamina = MyData.ThisPlayer.CurrentPlayerStamina;
            Stats.PlayerExperience = MyData.ThisPlayer.PlayerExperience;
            Stats.ExpRequired = MyData.ThisPlayer.ExpRequired;
            Stats.SkillPoints = MyData.ThisPlayer.SkillPoints;
            terrainState.CurrentTerrainLevel = MyData.ThisPlayer.CurrentTerrainLevel;
            //optionGUI = terrainState.canvas.GetComponentInChildren<CharacterOptionsGUI>();
            //if (optionGUI.SaveAndLoadGameObject != null)
            //    Destroy(optionGUI.SaveAndLoadGameObject);
            //optionGUI.SaveAndLoadGameObject = this.gameObject; // not have 2 save/load objects(dontdestroyonload)

            for (int i = 0; i < GatheringDB.GatheringSkillList.Count; i++)
            {
                GatheringDB.GatheringSkillList[i].CurrentRank = MyData.ThisPlayer.GatheringCurrentRank[i];
                GatheringDB.GatheringSkillList[i].CurrentExp = MyData.ThisPlayer.GatheringCurrentExp[i];
                GatheringDB.GatheringSkillList[i].MaxExp = MyData.ThisPlayer.GatheringMaxExp[i];
            }

            for (int i = 0; i < CraftingDB.CraftingSkillList.Count; i++)
            {
                CraftingDB.CraftingSkillList[i].CurrentRank = MyData.ThisPlayer.CraftingCurrentRank[i];
                CraftingDB.CraftingSkillList[i].CurrentExp = MyData.ThisPlayer.CraftingCurrentExp[i];
                CraftingDB.CraftingSkillList[i].MaxExp = MyData.ThisPlayer.CraftingMaxExp[i];
            }

            for (int i = 0; i < GeneralDB.GeneralSkillList.Count; i++)
            {
                GeneralDB.GeneralSkillList[i].CoolDown = MyData.ThisPlayer.GeneralCD[i];
                GeneralDB.GeneralSkillList[i].Duration = MyData.ThisPlayer.GeneralDur[i]; 
                GeneralDB.GeneralSkillList[i].LevelRank = MyData.ThisPlayer.GeneralkillLevelRank[i];
                GeneralDB.GeneralSkillList[i].SkillChance = MyData.ThisPlayer.SkillChance[i];
                GeneralDB.GeneralSkillList[i].SkillPointsRequired = MyData.ThisPlayer.GeneralSkillPointRequired[i]; 
                GeneralDB.GeneralSkillList[i].SkillValue[0] = MyData.ThisPlayer.SkillValue[i];          
            }

            terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage = new List<InventoryManager>();

            for (int i = 0; i < terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsIcons.Count; i++)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsIcons[i].sprite = terrainState.canvas.GetComponent<CharacterInventoryGUI>().DefaultSprite;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsIcons[i].GetComponentInChildren<Mask>().showMaskGraphic = false;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsStackText[i].text = string.Empty;
            }

            for (int i = 0; i < MyData.ThisPlayer.SlotName.Length; i++) //stacks keeps updating from previous save?
            {
                InventoryManager invManage = new InventoryManager(MyData.ThisPlayer.SlotName[i], MyData.ThisPlayer.Rarity[i], Resources.Load<Sprite>(MyData.ThisPlayer.tSprite[i]), null, MyData.ThisPlayer.InvDamageOrValue[i], MyData.ThisPlayer.InvWeaponAttackSpeed[i], MyData.ThisPlayer.InvCritRate[i], MyData.ThisPlayer.InvArmorPen[i], MyData.ThisPlayer.InvDefense[i], MyData.ThisPlayer.InvHealth[i], MyData.ThisPlayer.InvStamina[i], MyData.ThisPlayer.CurrentInventorySlot[i], MyData.ThisPlayer.StackAmounts[i], MyData.ThisPlayer.isASecondary[i]);
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage.Add(invManage);
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsIcons[terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].CurrentInventorySlot].sprite = terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].tSprite;
                if (terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].isASecondary == 1)
                    terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsStackText[terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].CurrentInventorySlot].text = terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].StackAmounts.ToString();
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryButtonsIcons[terrainState.canvas.GetComponent<CharacterInventoryGUI>().InventoryManage[i].CurrentInventorySlot].GetComponent<Mask>().showMaskGraphic = true;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().FindNextSlot();
            }

            if (MyData.ThisPlayer.CurrentSlotMainWeapon != -1)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().CurrentInventoryItemSlot = MyData.ThisPlayer.CurrentSlotMainWeapon;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetWeaponValues();
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetToolValues();
            }
            if (MyData.ThisPlayer.CurrentSlotSecondaryWeapon != -1)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().CurrentInventoryItemSlot = MyData.ThisPlayer.CurrentSlotSecondaryWeapon;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetWeaponValues();
            }
            if (MyData.ThisPlayer.CurrentSlotHead != -1)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().CurrentInventoryItemSlot = MyData.ThisPlayer.CurrentSlotHead;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetArmorValues();
            }
            if (MyData.ThisPlayer.CurrentSlotArmor != -1)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().CurrentInventoryItemSlot = MyData.ThisPlayer.CurrentSlotArmor;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetArmorValues();
            }
            if (MyData.ThisPlayer.CurrentSlotLegs != -1)
            {
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().CurrentInventoryItemSlot = MyData.ThisPlayer.CurrentSlotLegs;
                terrainState.canvas.GetComponent<CharacterInventoryGUI>().SetArmorValues();
            }

            for (int i = 1; i < terrainState.ListofTrees.Count; i++)
            {
                terrainState.ListofTrees[i].transform.position = MyData.ThisPlayer.TreePosition[i];
                terrainState.ListofTrees[i].transform.rotation = MyData.ThisPlayer.TreeRotation[i];

                terrainState.ListofTrees[i].transform.localScale = MyData.ThisPlayer.TreeScale[i];
                terrainState.ListofTrees[i].transform.GetComponent<Rigidbody>().mass = 3000 * MyData.ThisPlayer.TreeScale[i].x;
            }

            for (int i = 1; i < terrainState.ListofRocks.Count; i++)
            {
                terrainState.ListofRocks[i].transform.position = MyData.ThisPlayer.RockPosition[i];
                terrainState.ListofRocks[i].transform.rotation = MyData.ThisPlayer.RockRotation[i];

                terrainState.ListofRocks[i].transform.localScale = MyData.ThisPlayer.RockScale[i];
                terrainState.ListofRocks[i].transform.GetComponent<Rigidbody>().mass = 3000 * MyData.ThisPlayer.RockScale[i].x;
            }

            for (int i = 1; i < terrainState.ListofHerbPatches.Count; i++)
            {
                terrainState.ListofHerbPatches[i].transform.position = MyData.ThisPlayer.HerbPatchPosition[i];
                terrainState.ListofHerbPatches[i].transform.rotation = MyData.ThisPlayer.HerbPatchRotation[i];
                terrainState.ListofHerbPatches[i].transform.localScale = MyData.ThisPlayer.HerbPatchScale[i];
            }

            for (int i = 1; i < terrainState.ListofWaters.Count; i++)
            {
                terrainState.ListofWaters[i].transform.position = MyData.ThisPlayer.WaterPosition[i];
                terrainState.ListofWaters[i].transform.rotation = MyData.ThisPlayer.WaterRotation[i];
                terrainState.ListofWaters[i].transform.localScale = MyData.ThisPlayer.WaterScale[i];
            }

            monsterSpawn.BossMonster.transform.position = MyData.ThisPlayer.BossMonsterSpawnLocation;
            monsterSpawn.BossMonster.transform.rotation = MyData.ThisPlayer.BossMonsterSpawnRotation;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterHealth = MyData.ThisPlayer.BossMonsterHealth;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier = MyData.ThisPlayer.BossMonsterHealthRandomModifier;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterDefense = MyData.ThisPlayer.BossMonsterDefense;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterDamage = MyData.ThisPlayer.BossMonsterDamage;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterExp = MyData.ThisPlayer.BossMonsterExperience;
            monsterSpawn.BossMonster.GetComponent<MonsterOnCollision>().MonsterLevel = MyData.ThisPlayer.BossMonsterLevel;

            for (int i = 0; i < monsterSpawn.ListofMonsters.Count; i++)
            {
                monsterSpawn.ListofMonsters[i].transform.position = MyData.ThisPlayer.MonsterSpawnLocations[i];
                monsterSpawn.ListofMonsters[i].transform.rotation = MyData.ThisPlayer.MonsterSpawnRotations[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterMovement>().photonView.RPC("ChangeAppearanceDeath", PhotonTargets.All, MyData.ThisPlayer.MonsterMaterials[0 * i], MyData.ThisPlayer.MonsterMaterials[1 * i], MyData.ThisPlayer.MonsterMaterials[2 * i]);
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterHealth = MyData.ThisPlayer.MonsterHealth[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterHealthRandomModifier = MyData.ThisPlayer.MonsterHealthRandomModifier[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterDefense = MyData.ThisPlayer.MonsterDefense[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterDamage = MyData.ThisPlayer.MonsterDamage[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterExp = MyData.ThisPlayer.MonsterExperience[i];
                monsterSpawn.ListofMonsters[i].GetComponent<MonsterOnCollision>().MonsterLevel = MyData.ThisPlayer.MonsterLevel[i];
            }

            terrainState.terrainMap = terrainState.terrain.terrainData.GetHeights(0, 0, terrainState.terrain.terrainData.heightmapWidth, terrainState.terrain.terrainData.heightmapHeight);
            terrainState.MiddleAlpha = terrainState.terrain.terrainData.GetAlphamaps(0, 0, terrainState.terrain.terrainData.alphamapWidth, terrainState.terrain.terrainData.alphamapHeight);
            terrainState.detailLayer1 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 0);
            terrainState.detailLayer2 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 1);
            terrainState.detailLayer3 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 2);
            terrainState.detailLayer4 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 3);
            terrainState.detailLayer5 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 4);
            terrainState.detailLayer6 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 5);
            terrainState.detailLayer7 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 6);
            terrainState.detailLayer8 = terrainState.terrain.terrainData.GetDetailLayer(0, 0, terrainState.terrain.terrainData.detailWidth, terrainState.terrain.terrainData.detailHeight, 7);

            for (int i = 0; i < terrainState.terrain.terrainData.heightmapHeight; i++)
            {
                for (int j = 0; j < terrainState.terrain.terrainData.heightmapWidth; j++)
                {
                    terrainState.terrainMap[i, j] = MyData.ThisPlayer.terrainMap[i * terrainState.terrain.terrainData.heightmapWidth + j];
                }
            }

            for (int i = 0; i < terrainState.terrain.terrainData.detailHeight; i++)
            {
                for (int j = 0; j < terrainState.terrain.terrainData.detailWidth; j++)
                {
                    terrainState.detailLayer1[i, j] = MyData.ThisPlayer.detailLayer1[i * terrainState.terrain.terrainData.detailWidth + j];
                    terrainState.detailLayer2[i, j] = MyData.ThisPlayer.detailLayer2[i * terrainState.terrain.terrainData.detailWidth + j];
                    terrainState.detailLayer3[i, j] = MyData.ThisPlayer.detailLayer3[i * terrainState.terrain.terrainData.detailWidth + j];
                    terrainState.detailLayer4[i, j] = MyData.ThisPlayer.detailLayer4[i * terrainState.terrain.terrainData.detailWidth + j];
                    terrainState.detailLayer5[i, j] = MyData.ThisPlayer.detailLayer5[i * terrainState.terrain.terrainData.detailWidth + j];
                    terrainState.detailLayer6[i, j] = MyData.ThisPlayer.detailLayer6[i * terrainState.terrain.terrainData.detailWidth + j];
                    terrainState.detailLayer7[i, j] = MyData.ThisPlayer.detailLayer7[i * terrainState.terrain.terrainData.detailWidth + j];
                    terrainState.detailLayer8[i, j] = MyData.ThisPlayer.detailLayer8[i * terrainState.terrain.terrainData.detailWidth + j];
                }
            }

            for (int i = 0; i < terrainState.terrain.terrainData.alphamapHeight; i++)
            {
                for (int j = 0; j < terrainState.terrain.terrainData.alphamapWidth; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        terrainState.MiddleAlpha[i, j, k] = MyData.ThisPlayer.MiddleAlpha[i + terrainState.terrain.terrainData.alphamapHeight * (j + terrainState.terrain.terrainData.alphamapWidth * k)];
                    }
                }
            }

            terrainState.terrain.terrainData.SetAlphamaps(0, 0, terrainState.MiddleAlpha);
            terrainState.terrain.terrainData.SetHeights(0, 0, terrainState.terrainMap);
            terrainState.terrain.terrainData.SetDetailLayer(0, 0, 0, terrainState.detailLayer1);
            terrainState.terrain.terrainData.SetDetailLayer(0, 0, 1, terrainState.detailLayer2);
            terrainState.terrain.terrainData.SetDetailLayer(0, 0, 2, terrainState.detailLayer3);
            terrainState.terrain.terrainData.SetDetailLayer(0, 0, 3, terrainState.detailLayer4);
            terrainState.terrain.terrainData.SetDetailLayer(0, 0, 4, terrainState.detailLayer5);
            terrainState.terrain.terrainData.SetDetailLayer(0, 0, 5, terrainState.detailLayer6);
            terrainState.terrain.terrainData.SetDetailLayer(0, 0, 6, terrainState.detailLayer7);
            terrainState.terrain.terrainData.SetDetailLayer(0, 0, 7, terrainState.detailLayer8);

            terrainState.gainX = MyData.ThisPlayer.gainX;
            terrainState.gainY = MyData.ThisPlayer.gainY;

            for (int i = 0; i < MyData.ThisPlayer.ListofEnvironmentalObjectsAlteredIDs.Length; i++)
            {
                GameObject alteredID = PhotonView.Find(MyData.ThisPlayer.ListofEnvironmentalObjectsAlteredIDs[i]).gameObject;
                terrainState.ListofEnvironmentalObjectsAlteredPositions.Add(MyData.ThisPlayer.ListofEnvironmentalObjectsAlteredPositions[i]);
                terrainState.ListofEnvironmentalObjectsAlteredRotations.Add(MyData.ThisPlayer.ListofEnvironmentalObjectsAlteredRotations[i]);
                alteredID.transform.GetComponent<Rigidbody>().isKinematic = false;
                StartCoroutine(monsterFunction.EnvironmentalPhysicsUndo(alteredID, MyData.ThisPlayer.ListofEnvironmentalObjectsAlteredPositions[i], MyData.ThisPlayer.ListofEnvironmentalObjectsAlteredRotations[i]));
            }
            if (PhotonNetwork.isMasterClient)
            {
                mainGui.photonView.RPC("SyncPeople", PhotonTargets.OthersBuffered, Stats.transform.position, terrainState.MasterPosition, terrainState.gainX, terrainState.gainY, MyData.ThisPlayer.ListofEnvironmentalObjectsAlteredIDs, MyData.ThisPlayer.ListofEnvironmentalObjectsAlteredPositions, MyData.ThisPlayer.ListofEnvironmentalObjectsAlteredRotations,
                    MyData.ThisPlayer.TreePosition, MyData.ThisPlayer.TreeRotation, MyData.ThisPlayer.RockPosition, MyData.ThisPlayer.RockRotation, MyData.ThisPlayer.HerbPatchPosition, MyData.ThisPlayer.HerbPatchRotation, MyData.ThisPlayer.WaterPosition, MyData.ThisPlayer.WaterRotation,
                    MyData.ThisPlayer.BossMonsterHealth, MyData.ThisPlayer.BossMonsterHealthRandomModifier, MyData.ThisPlayer.BossMonsterDefense, MyData.ThisPlayer.BossMonsterDamage, MyData.ThisPlayer.BossMonsterExperience, MyData.ThisPlayer.BossMonsterLevel,
                    MyData.ThisPlayer.MonsterHealth, MyData.ThisPlayer.MonsterHealthRandomModifier, MyData.ThisPlayer.MonsterDefense, MyData.ThisPlayer.MonsterDamage, MyData.ThisPlayer.MonsterExperience, MyData.ThisPlayer.MonsterLevel);

                mainGui.photonView.RPC("SyncScales", PhotonTargets.OthersBuffered, MyData.ThisPlayer.TreeScale, MyData.ThisPlayer.RockScale, MyData.ThisPlayer.HerbPatchScale, MyData.ThisPlayer.WaterScale);
            }

            CurrentSaveSelectedButton = null;
            CurrentSaveSelectedButtonText = string.Empty;
            PlayerPrefs.SetString("LoadSave", string.Empty);
            mainGui.ActivateSaveAndLoadGUI(0, false, false);
            gameObject.transform.localScale = new Vector3(1, 1, 1); // pun rpc environment/alphas/heights...

            terrainState.NotLoading[0] = true;
            terrainState.NotLoading[1] = true;

            Debug.Log(terrainState.NotLoading[0]);
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
    string SerializeObject(object SerialzedObject)  //xml 
    {
        string XmlizedString = null;
        MemoryStream memoryStream = new MemoryStream();
        XmlSerializer XMLSerializer = new XmlSerializer(typeof(UserData));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        XMLSerializer.Serialize(xmlTextWriter, SerialzedObject);
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
        XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
        return XmlizedString;
    }

    //deserialze our Userdata
    object DeserializeObject(string xmlString)   //xml
    {
        XmlSerializer XMLSerializer = new XmlSerializer(typeof(UserData));
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(xmlString));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        return XMLSerializer.Deserialize(memoryStream);
    }

    public void CreateXML()
    {
        FileName = InputSaveImage.text + ".dat";
        //xml
        //StreamWriter writer;
        //FileInfo fileinfo = new FileInfo(FileLocation + "\\" + FileName);

        //if (!fileinfo.Exists)
        //{
        //    writer = fileinfo.CreateText();
        //}
        //else
        //{
        //    fileinfo.Delete();
        //    writer = fileinfo.CreateText();
        //}

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(FileLocation + "\\" + FileName);
        SurrogateSelector surrogateSelector = new SurrogateSelector();
        Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();
        QuaternionSerializationSurrogate quaternion4SS = new QuaternionSerializationSurrogate();

        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
        surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternion4SS);
        bf.SurrogateSelector = surrogateSelector;

        bf.Serialize(file, MyData);
        file.Close();

        // create a new save that can be clicked

        Button SaveGameButton = Instantiate(ButtonPrefab, transform.position, transform.rotation)
            as Button;
        ListofSaveButtons.Add(SaveGameButton);
        SaveGameButton.transform.SetParent(ListofSavedGamesImage.transform);
        SaveGameButton.transform.localScale = SaveGameButton.transform.parent.localScale;
        SaveGameButton.GetComponentInChildren<Text>().text = FileName.Remove(FileName.IndexOf("."), FileName.Length - FileName.IndexOf(".")); ;
        SaveGameButton.onClick.AddListener(() => CurrentSelectedSaveFunction(SaveGameButton));

        //xml
        //writer.Write(data);
        //writer.Close();
    }

    public void LoadXML(string CurrentButtonText)
    {
        //xml
        //FileName = FileLocation + "\\" + CurrentButtonText.text + ".xml";
        //StreamReader reader = File.OpenText(FileName);
        //string inforeader = reader.ReadToEnd();
        //reader.Close();
        //data = inforeader;
        if (File.Exists(FileLocation + "\\" + CurrentButtonText + ".dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(FileLocation + "\\" + CurrentButtonText + ".dat", FileMode.Open);
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();
            QuaternionSerializationSurrogate quaternion4SS = new QuaternionSerializationSurrogate();

            surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
            surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternion4SS);
            bf.SurrogateSelector = surrogateSelector;

            MyData = (UserData)bf.Deserialize(file);
            file.Close();
        }
        else
        {
            CurrentSaveSelectedButtonText = string.Empty; // new player new save multiplayer 
            PlayerPrefs.SetString("LoadSave", string.Empty);
        }
    }

    public void DestroyButton()
    {
        if ( CurrentSaveSelectedButton == null ||CurrentSaveSelectedButton.text == string.Empty)
            return;

        FileInfo fileinfo = new FileInfo(FileLocation + "\\" + CurrentSaveSelectedButton.GetComponentInChildren<Text>().text + ".dat");
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
        FileName = "Default.dat";
        FileLocation = Application.dataPath + "\\" + "SaveData";

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
        }

        MyData = new UserData();
        info = new DirectoryInfo(FileLocation);
        AllXMLFiles = info.GetFiles("*.dat*");
        StringXmlFiles = new string[AllXMLFiles.Length];
        ListofSaveButtons = new List<Button>();

        for (int j = 0; j < StringXmlFiles.Length; j++) // load all the saves when first loaded
        {
            StringXmlFiles[j] = AllXMLFiles[j].ToString().Remove(0, FileLocation.Length + 1);

            if (!StringXmlFiles[j].Contains(".meta"))
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
    }
}

[System.Serializable]
public class UserData
{

    public UserDataDatabase ThisPlayer;

    [System.Serializable]
    public struct UserDataDatabase // sending gameobjects - get photonviewid
    { // vector3 and quaternion interface for serializing/deserializing custom classes
        private Vector3 playerPosition;
        public Vector3 PlayerPosition
        {
            get { return playerPosition; }
            set { playerPosition = value; }
        }
        private Vector3 MasterplayerPosition; // terrain purposes
        public Vector3 MasterPlayerPosition
        {
            get { return MasterplayerPosition; }
            set { MasterplayerPosition = value; }
        }
        private Quaternion playerRotation;
        public Quaternion PlayerRotation
        {
            get { return playerRotation; }
            set { playerRotation = value; }
        }
        public float PlayerHealth;
        public float CurrentPlayerHealth;
        public float PlayerStamina;
        public float CurrentPlayerStamina;
        public float PlayerExperience;
        public float PlayerLevel;
        public float ExpRequired;
        public float SkillPoints;

        public int CurrentTerrainLevel;

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
        public float[] MiddleAlpha;

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

        // monster data

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
        public float BossMonsterLevel;

        public int[] MonsterMaterials;
        public float[] MonsterHealth;
        public float[] MonsterHealthRandomModifier;
        public float[] MonsterDefense;
        public float[] MonsterDamage;
        public float[] MonsterExperience;
        public float[] MonsterLevel;
    }
}