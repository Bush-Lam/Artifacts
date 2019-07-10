using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterSkillBarGUI : MonoBehaviour
{
    public Canvas canvas;
    public Image SkillBar;
    public Image ImagePrefab;
    public Image CraftingIconPrefab;

    public CharacterSkillsGUI characterSkills;
    public CharacterInventoryGUI characterInventory;
    public CharacterSkillPickup characterInventoryPickup;
    MiscellaneousItemsDatabase MiscItems;
    GatheringSkillDatabase gatheringDatabase;
    CraftingSkillDatabase craftingDatabase;
    TerrainScript terrain;
    PotionDatabase Potions;
    WeaponsDatabase Weapons;
    ToolDatabase Tools;
    WeaponSwitch WepSwitch;
    ArmorDatabase Armors;
    CharacterStats Stats;
    ChopTrees choptree;
    MineRocks minerocks;
    Herbloring pickingherbs;
    GeneralSkillsDatabase GeneralSkillsDB;
    ElementalSkills EleSkills;

    PickupObjects Pickup;
    MainGUI mainGUI;
    CharacterMovement movement;

    public Button PickUpWindowRectPrefab;
    public GameObject MetalworkingCraftingPrefab;
    public GameObject WoodworkingCraftingPrefab;
    public GameObject BlacksmithingCraftingPrefab;
    public GameObject PotionsCraftingPrefab;
    Coroutine CraftingCoroutine;
    public GameObject MetalworkingWindow;
    public GameObject WoodworkingWindow;
    public GameObject BlacksmithingWindow;
    public GameObject PotionsWindow;
   List<GameObject> CraftingCompletedObject; //put the crafting item onto the crafting table/furnace for animation
    public GameObject CurrentCraftingStation;

    public List<Button> KeyBindRects;
    public List<Image> CoolDownRects;
    public Button ElementalStrikeButton;
    public Image ElementalStrikeCoolDown;

    public List<Image> BuffBarRects;
    public List<Image> DurationBarRects;
    int CurrentBuffBarCount;

    public string CurrentPointerEnterName;
    public Sprite CurrentSkillPickUpSprite;

    float TimeElapsed;

    public Image HoverRectINIT; // referenced in skillpickup, same w/ rectinit
    public Image RectINIT;
    public Image PickUpWindowSkillPrefab;
    public Image HoverGeneralSkillPrefab;

    void DestroyThis(GameObject gameobject)
    {
        Destroy(gameobject);
    }

    public void CraftingSwitch(int CraftingCount, GameObject CraftingWindow)
    {
        if (CraftingWindow.transform.name == MetalworkingCraftingPrefab.name + "(Clone)")
        {
            for (int i = 0; i < CraftingWindow.transform.Find("ScrollRect").Find("Content").transform.childCount; i++)
            {
                CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft" + i).gameObject.SetActive(false);
            }
            switch (CraftingCount)
            {
                case 0:
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft0").gameObject.SetActive(true);
                    break;
                case 1:
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft1").gameObject.SetActive(true);
                    break;
                case 2:
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft2").gameObject.SetActive(true);
                    break;
                case 3:
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft3").gameObject.SetActive(true);
                    break;
            }
        }
        else if (CraftingWindow.transform.name == BlacksmithingCraftingPrefab.name + "(Clone)" ||
            CraftingWindow.transform.name == WoodworkingCraftingPrefab.name + "(Clone)")
        {
            for (int i = 0; i < CraftingWindow.transform.Find("ScrollRect").Find("Content").transform.childCount; i++)
            {
                CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft" + i).gameObject.SetActive(false); // woodworking and blacksmithing have different tabs
            }
            switch (CraftingCount)
            {
                case 0:
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft0").gameObject.SetActive(true);
                    break;
                case 1:
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft1").gameObject.SetActive(true);
                    break;
                case 2:
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft2").gameObject.SetActive(true);
                    break;
            }
        }
        else if (CraftingWindow.transform.name == PotionsCraftingPrefab.name + "(Clone)")
        {
            switch (CraftingCount)
            {
                case 0:
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft0").gameObject.SetActive(true);
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft1").gameObject.SetActive(false);
                    break;
                case 1:
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft0").gameObject.SetActive(false);
                    CraftingWindow.transform.Find("ScrollRect").Find("Content").Find("Craft1").gameObject.SetActive(true);
                    break;
            }
        }
    }

    public bool EquippedCraftingTool(string Craftingname)
    {
        bool IsEquipped = true;

        if (Craftingname == "Nothing" && WepSwitch.CurrentItemId != -1)
        {
            IsEquipped = false;
            mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("You Must equip nothing."));
        }
        else if (Craftingname == "Woodworking" && WepSwitch.CurrentItemId != 851)
        {
            IsEquipped = false;
            mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("You Must equip a Knife."));
        }
        else if ((Craftingname == "Blacksmithing" || Craftingname == "PotionCrafting") && WepSwitch.CurrentItemId != 850)
        {
            IsEquipped = false;
            mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("You Must equip a Hammer."));
        }

        return IsEquipped;
    }

    public bool NearACraftingStation(string CraftableName)
    {
        bool NearCraftingStation = false;

        if (CraftableName.Contains("Water")) // vial of water
        {
            NearCraftingStation = pickingherbs.CheckFillVialWithWater();
        }

        for (int i = 0; i < WepSwitch.CraftingStations.Count; i++)
        {
            if (Vector3.Distance(WepSwitch.CraftingStations[i].transform.position, terrain.Player.transform.position) < 3)
            {
                if ((CraftableName.Contains("Bar") || CraftableName.Contains("Empty")) && WepSwitch.CraftingStations[i].name == "Stone Furnace(Clone)") // bars or vials
                {
                    NearCraftingStation = true;
                    CurrentCraftingStation = WepSwitch.CraftingStations[i];
                }
                else if (!CraftableName.Contains("Bar") && !CraftableName.Contains("Water") && WepSwitch.CraftingStations[i].name == "Stone Workbench(Clone)") // weps/armors/potions
                {
                    NearCraftingStation = true;
                    CurrentCraftingStation = WepSwitch.CraftingStations[i];
                }
            }
        }

        if (NearCraftingStation == false)
        {
            if (CraftableName.Contains("Bar") || CraftableName.Contains("Empty"))
                mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("You Must be near a Furnace."));
            else if (CraftableName.Contains("Water"))
                mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("You Must be near Water."));
            else
                mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("You Must be near a Workbench."));
        }

        return NearCraftingStation;
    }

    public IEnumerator CheckMovement() // dont move when crafting.
    {
        if (movement.PlayerVelocity.magnitude > 3)
        {
            movement.NetworkSetHandAnimations("Gathering", 0); // crafting animation 0
            movement.NetworkSetHandAnimations("Crafting", 0); // crafting animation 0
            StopCoroutine(CraftingCoroutine);
            mainGUI.timeElapsedProgressBar = Time.time + 1;
            mainGUI.CheckMovementProgressBar = true;
            for (int y = 0; y < CraftingCompletedObject.Count; y++)
            {
                PhotonNetwork.Destroy(CraftingCompletedObject[y]);
            }
            CraftingCompletedObject.Clear();
            yield return new WaitForSeconds(1);
            mainGUI.CheckMovementProgressBar = false;
        }
        else if (mainGUI.timeElapsedProgressBar > Time.time)
        {
            yield return new WaitForSeconds(0.02f);
            StartCoroutine("CheckMovement");
        }
    }

    public void CheckAllUpdateStacksFromCraftingWindow(string MatName, int Stacks)
    {
        CheckUpdateStacksFromCraftingWindow(PotionsWindow, MatName, Stacks);
        CheckUpdateStacksFromCraftingWindow(MetalworkingWindow, MatName, Stacks);
        CheckUpdateStacksFromCraftingWindow(WoodworkingWindow, MatName, Stacks);
        CheckUpdateStacksFromCraftingWindow(BlacksmithingWindow, MatName, Stacks);
    }

    IEnumerator CraftingComponents(string CraftingType, int a, int MatsReqInventoryManageIndex)
    {
        GameObject tCraftingCompletedObject = null;
        int NewMatHeight = 0;// for every new mat shift up
        float HeightMod = 0; // modifier for when new mats, reset x then shift up 
        float WidthMod = 0;

        if (CraftingType == "Miscs")
        {
            if (MiscItems.Miscellaneousitems[a].MiscellaneousItemName != "Vial of Water")
            {
                yield return new WaitForSeconds(1);
                for (int y = 0; y < MatsReqInventoryManageIndex; y++)
                {
                    for (int x = 0; x < MiscItems.Miscellaneousitems[a].MatsAmounts[y]; x++)
                    {
                        tCraftingCompletedObject = PhotonNetwork.Instantiate(MiscItems.MiscellaneousSprites[MiscItems.Miscellaneousitems[a].ReferenceIndexToMat[y]].name, CurrentCraftingStation.transform.position + new Vector3(0, 2, 0), Quaternion.identity, 0);
                        tCraftingCompletedObject.GetComponent<Collider>().isTrigger = true;
                        tCraftingCompletedObject.GetComponent<Rigidbody>().isKinematic = true;
                        tCraftingCompletedObject.transform.SetParent(CurrentCraftingStation.transform);
                        tCraftingCompletedObject.transform.localPosition = Vector3.zero;

                        if (a > MiscItems.EndlengthOres && a <= MiscItems.EndlengthBars)
                        {
                            tCraftingCompletedObject.transform.localPosition = new Vector3(-0.209f, 0.069f - (x * 0.275f), 0.088f);
                        }
                        else
                        {
                            if (NewMatHeight != y) // new mats are shifted to right when y changes
                            {
                                NewMatHeight = y;
                                HeightMod = (y * -0.363f); // shift up by -0.363
                                WidthMod = 0;
                                tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f - (WidthMod * 0.275f), 0.448f);
                            }
                            else
                            {
                                if (x % 2 == 0)
                                    tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f - (WidthMod * 0.275f), 0.448f);
                                else
                                {
                                    WidthMod++;
                                    tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f + (WidthMod * 0.275f), 0.448f);
                                }
                            }
                        }
                        tCraftingCompletedObject.transform.localRotation = Quaternion.Euler(0, 0, 45);
                        CraftingCompletedObject.Add(tCraftingCompletedObject);
                    }
                }
            }
            else if (MiscItems.Miscellaneousitems[a].MiscellaneousItemName == "Vial of Water")
            {
                yield return new WaitForSeconds(0.65f);
                for (int y = 0; y < MatsReqInventoryManageIndex; y++)
                {
                    for (int x = 0; x < MiscItems.Miscellaneousitems[a].MatsAmounts[y]; x++)
                    {
                        tCraftingCompletedObject = PhotonNetwork.Instantiate(MiscItems.MiscellaneousSprites[MiscItems.Miscellaneousitems[a].ReferenceIndexToMat[y]].name, transform.position, Quaternion.identity, 0);
                        tCraftingCompletedObject.GetComponent<Collider>().isTrigger = true;
                        tCraftingCompletedObject.GetComponent<Rigidbody>().isKinematic = true;
                        tCraftingCompletedObject.transform.SetParent(WepSwitch.transform);
                        tCraftingCompletedObject.transform.localPosition = Vector3.zero;
                        tCraftingCompletedObject.transform.localPosition = new Vector3(0.181f, 0.098f, 0);
                        tCraftingCompletedObject.transform.localRotation = Quaternion.Euler(0, 0, 45);
                        CraftingCompletedObject.Add(tCraftingCompletedObject);
                    }
                }
            }
        }
        else if (CraftingType == "Weapons")
        {
            yield return new WaitForSeconds(0.5f);
            for (int y = 0; y < MatsReqInventoryManageIndex; y++)
            {
                for (int x = 0; x < Weapons.WeaponList[a].MatsAmounts[y]; x++)
                {
                    tCraftingCompletedObject = PhotonNetwork.Instantiate(MiscItems.MiscellaneousSprites[Weapons.WeaponList[a].ReferenceIndexToMat[y]].name, CurrentCraftingStation.transform.position + new Vector3(0, 2, 0), Quaternion.identity, 0);
                    tCraftingCompletedObject.GetComponent<Collider>().isTrigger = true;
                    tCraftingCompletedObject.GetComponent<Rigidbody>().isKinematic = true;
                    tCraftingCompletedObject.transform.SetParent(CurrentCraftingStation.transform);
                    tCraftingCompletedObject.transform.localPosition = Vector3.zero;
                    if (NewMatHeight != y) // new mats are shifted to right when y changes
                    {
                        NewMatHeight = y;
                        HeightMod = (y * -0.363f); // shift up by -0.363
                        WidthMod = 0;
                        tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f - (WidthMod * 0.275f), 0.448f);
                    }
                    else
                    {
                        if (x % 2 == 0)
                            tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f - (WidthMod * 0.275f), 0.448f);
                        else
                        {
                            WidthMod++;
                            tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f + (WidthMod * 0.275f), 0.448f);
                        }
                    }
                    tCraftingCompletedObject.transform.localRotation = Quaternion.Euler(0, 0, 45);
                    CraftingCompletedObject.Add(tCraftingCompletedObject);
                }
            }
        }
        else if (CraftingType == "Armors")
        {
            yield return new WaitForSeconds(0.5f);
            for (int y = 0; y < MatsReqInventoryManageIndex; y++)
            {
                for (int x = 0; x < Armors.ArmorList[a].MatsAmounts[y]; x++)
                { 
                    tCraftingCompletedObject = PhotonNetwork.Instantiate(MiscItems.MiscellaneousSprites[Armors.ArmorList[a].ReferenceIndexToMat[y]].name, CurrentCraftingStation.transform.position + new Vector3(0, 2, 0), Quaternion.identity, 0);
                    tCraftingCompletedObject.GetComponent<Collider>().isTrigger = true;
                    tCraftingCompletedObject.GetComponent<Rigidbody>().isKinematic = true;
                    tCraftingCompletedObject.transform.SetParent(CurrentCraftingStation.transform);
                    tCraftingCompletedObject.transform.localPosition = Vector3.zero;
                    if (NewMatHeight != y) // new mats are shifted to right when y changes
                    {
                        NewMatHeight = y;
                        HeightMod = (y * -0.363f); // shift up by -0.363
                        WidthMod = 0;
                        tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f - (WidthMod * 0.275f), 0.448f);
                    }
                    else
                    {
                        if (x % 2 == 0)
                            tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f - (WidthMod * 0.275f), 0.448f);
                        else
                        {
                            WidthMod++;
                            tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f + (WidthMod * 0.275f), 0.448f);
                        }
                    }
                    tCraftingCompletedObject.transform.localRotation = Quaternion.Euler(0, 0, 45);
                    CraftingCompletedObject.Add(tCraftingCompletedObject);
                }
            }
        }
        else if (CraftingType == "Tools")
        {
            yield return new WaitForSeconds(0.5f);
            if (Tools.ToolList[a].ToolId != 1)
            {
                for (int y = 0; y < MatsReqInventoryManageIndex; y++)
                {
                    for (int x = 0; x < Tools.ToolList[a].MatsAmounts[y]; x++)
                    { // make incomplete crafting objects, create gameobject ref in the misc/weapon/tool classes and instantiate that.
                        tCraftingCompletedObject = PhotonNetwork.Instantiate(MiscItems.MiscellaneousSprites[Tools.ToolList[a].ReferenceIndexToMat[y]].name, CurrentCraftingStation.transform.position + new Vector3(0, 2, 0), Quaternion.identity, 0);
                        tCraftingCompletedObject.GetComponent<Collider>().isTrigger = true;
                        tCraftingCompletedObject.GetComponent<Rigidbody>().isKinematic = true;
                        Vector3 craftScale = tCraftingCompletedObject.transform.localScale;
                        tCraftingCompletedObject.transform.SetParent(CurrentCraftingStation.transform);
                        tCraftingCompletedObject.transform.localScale = craftScale;
                        tCraftingCompletedObject.transform.localPosition = Vector3.zero;
                        if (NewMatHeight != y) // new mats are shifted to right when y changes
                        {
                            NewMatHeight = y;
                            HeightMod = (y * -0.363f); // shift up by -0.363
                            WidthMod = 0;
                            tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f - (WidthMod * 0.275f), 0.448f);
                        }
                        else
                        {
                            if (x % 2 == 0)
                                tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f - (WidthMod * 0.275f), 0.448f);
                            else
                            {
                                WidthMod++;
                                tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f + (WidthMod * 0.275f), 0.448f);
                            }
                        }
                        tCraftingCompletedObject.transform.localRotation = Quaternion.Euler(0, 0, 45);
                        CraftingCompletedObject.Add(tCraftingCompletedObject);
                    }
                }
            }
        }
        else if (CraftingType == "Pots")
        {
            yield return new WaitForSeconds(0.5f);
            for (int y = 0; y < MatsReqInventoryManageIndex; y++)
            {
                for (int x = 0; x < Potions.PotionList[a].MatsAmounts[y]; x++)
                { // make incomplete crafting objects, create gameobject ref in the misc/weapon/tool classes and instantiate that.
                    tCraftingCompletedObject = PhotonNetwork.Instantiate(MiscItems.MiscellaneousSprites[Potions.PotionList[a].ReferenceIndexToMat[y]].name, CurrentCraftingStation.transform.position + new Vector3(0, 2, 0), Quaternion.identity, 0);
                    tCraftingCompletedObject.GetComponent<Collider>().isTrigger = true;
                    tCraftingCompletedObject.GetComponent<Rigidbody>().isKinematic = true;
                    Vector3 craftScale = tCraftingCompletedObject.transform.localScale;
                    tCraftingCompletedObject.transform.SetParent(CurrentCraftingStation.transform);
                    tCraftingCompletedObject.transform.localScale = craftScale;
                    tCraftingCompletedObject.transform.localPosition = Vector3.zero;
                    if (NewMatHeight != y) // new mats are shifted to right when y changes
                    {
                        NewMatHeight = y;
                        HeightMod = (y * -0.363f); // shift up by -0.363
                        WidthMod = 0;
                        tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f - (WidthMod * 0.275f), 0.448f);
                    }
                    else
                    {
                        if (x % 2 == 0)
                            tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f - (WidthMod * 0.275f), 0.448f);
                        else
                        {
                            WidthMod++;
                            tCraftingCompletedObject.transform.localPosition = new Vector3(-0.629f + (HeightMod), 0.618f + (WidthMod * 0.275f), 0.448f);
                        }
                    }
                    tCraftingCompletedObject.transform.localRotation = Quaternion.Euler(0, 0, 45);
                    CraftingCompletedObject.Add(tCraftingCompletedObject);
                }
            }
        }
    }

    IEnumerator CreateMiscellaneous(int a, string CraftingName, int CraftingAmount) // CraftingAmount - create 1,5,10 
    {
        if (mainGUI.timeElapsedProgressBar >= Time.time || EquippedCraftingTool("Nothing") == false)
            yield break;

        for (int craftAmount = 0; craftAmount < CraftingAmount; craftAmount++)
        {
            if (NearACraftingStation(MiscItems.Miscellaneousitems[a].MiscellaneousItemName) == false || EquippedCraftingTool("Nothing") == false)
            {
                mainGUI.CheckMovementProgressBar = false;
                mainGUI.timeElapsedProgressBar = Time.time + 1;
                for (int y = 0; y < CraftingCompletedObject.Count; y++)
                {
                    PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                }
                CraftingCompletedObject.Clear();
                yield break;
            }

            List<int> MatsReqInventoryManageIndex = new List<int>(); // get the amount of mats for a given craftable
            int CraftingSkill = 0;
            if (CraftingName == "Metalworking")
                CraftingSkill = 0;
            else
                CraftingSkill = 1; // make water scale bigger, make Currentcraftingamount -> break if in progress

            for (int i = 0; i < MiscItems.Miscellaneousitems[a].ReferenceIndexToMat.Length; i++)
            {
                if (MiscItems.Miscellaneousitems[a].ReferenceIndexToMat[i] != -1)
                    MatsReqInventoryManageIndex.Add(-1);
            }

            for (int c = 0; c < 5; c++)
            {
                if (MiscItems.Miscellaneousitems[a].ReferenceIndexToMat[c] != -1)
                {
                    for (int b = 0; b < characterInventory.InventoryManage.Count; b++)
                    {
                        if (characterInventory.InventoryManage[b].SlotName == MiscItems.MiscellaneousSprites[MiscItems.Miscellaneousitems[a].ReferenceIndexToMat[c]].name) //MiscItems.Miscellaneousitems[a].ReferenceIndexToMat[c] == index for material in database
                        {
                            if (characterInventory.InventoryManage[b].StackAmounts >= MiscItems.Miscellaneousitems[a].MatsAmounts[c] * CraftingAmount
                                && craftingDatabase.CraftingSkillList[CraftingSkill].CurrentRank >= MiscItems.Miscellaneousitems[a].LevelRank)
                            {
                                MatsReqInventoryManageIndex[c] = b; // get the inventorymanage index for crafting materials
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
            {
                if (MatsReqInventoryManageIndex[i] == -1) // this makes sure that we have the neccessary crefting items and its amounts
                {
                    mainGUI.timeElapsedProgressBar = 0;
                    for (int y = 0; y < CraftingCompletedObject.Count; y++)
                    {
                        PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                    }
                    CraftingCompletedObject.Clear();
                    yield break;
                }
            }

            for (int j = 0; j < characterInventory.InventoryManage.Count; j++)
            {

                if (characterInventory.InventoryManage[j].SlotName == MiscItems.Miscellaneousitems[a].MiscellaneousItemName &&
                    characterInventory.InventoryManage[j].isASecondary == 1 && characterInventory.InventoryManage[j].StackAmounts < 50) // stacks that are already in inventory
                {
                    // make crafting animation
                    mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.055f, MiscItems.Miscellaneousitems[a].MiscellaneousItemName));
                    mainGUI.timeElapsedProgressBar = 4.9f + Time.time;
                    StartCoroutine("CheckMovement");
                    if (a > MiscItems.EndlengthOres && a <= MiscItems.EndlengthBars)
                        movement.NetworkSetHandAnimations("Crafting", 1); // smelting
                    else if (a == MiscItems.EndLengthVials)
                        movement.NetworkSetHandAnimations("Gathering", 5); // vials of water
                    else
                        movement.NetworkSetHandAnimations("Crafting", 3);
  
                    StartCoroutine(CraftingComponents("Miscs", a, MatsReqInventoryManageIndex.Count)); // crafting components shown on table/furnace
                    yield return new WaitForSeconds(3.925f);

                    movement.NetworkSetHandAnimations("Crafting", 0); // crafting animation 0
                    movement.NetworkSetHandAnimations("Gathering", 0);
                    mainGUI.CheckMovementProgressBar = false;

                    characterInventory.InventoryManage[j].StackAmounts += 1;

                    if (MiscItems.Miscellaneousitems[a].MiscellaneousItemName == "Vial of Water") // makes sure we have enough mats and near water
                        pickingherbs.photonView.RPC("WaterScale", PhotonTargets.All, pickingherbs.WaterID);

                    characterInventory.RecalculateStackAmounts(j, -1); // update add 1 stack being crafted, updates total stacks from function also updates CheckAllUpdateStacksFromCraftingWindow

                    for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
                    {
                        characterInventory.RecalculateStackAmounts(MatsReqInventoryManageIndex[i], MiscItems.Miscellaneousitems[a].MatsAmounts[i]); // updates total stacks(mats) from function also updates CheckAllUpdateStacksFromCraftingWindow
                    }

                    craftingDatabase.CraftingSkillList[CraftingSkill].CurrentExp += MiscItems.Miscellaneousitems[a].MiscellaneousItemExp;
                    Stats.CurrentPlayerStamina -= craftingDatabase.CraftingSkillList[CraftingSkill].Stamina * Stats.PlayerStamina;
                    break;
                }
                else if (characterInventory.InventoryManage.Count <= 24 && j >= characterInventory.InventoryManage.Count - 1)
                {
                    InventoryManager misc1 = new InventoryManager(MiscItems.Miscellaneousitems[a].MiscellaneousItemName, "Common", MiscItems.MiscellaneousSprites[a], null,
                        0, 0, 0, 0, 0, 0, 0, -1, 1, 1);
                    mainGUI.timeElapsedProgressBar = 4.9f + Time.time;
                    StartCoroutine("CheckMovement");
                    mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.055f, MiscItems.Miscellaneousitems[a].MiscellaneousItemName));
                    if (a > MiscItems.EndlengthOres && a <= MiscItems.EndlengthBars)
                        movement.NetworkSetHandAnimations("Crafting", 1); // smelting
                    else if (a == MiscItems.EndLengthVials)
                        movement.NetworkSetHandAnimations("Gathering", 5); // vials of water
                    else
                        movement.NetworkSetHandAnimations("Crafting", 3);

                    StartCoroutine(CraftingComponents("Miscs", a, MatsReqInventoryManageIndex.Count)); // crafting components shown on table/furnace
                    yield return new WaitForSeconds(3.925f);

                    movement.NetworkSetHandAnimations("Gathering", 0); // crafting animation 0
                    movement.NetworkSetHandAnimations("Crafting", 0);
                    mainGUI.CheckMovementProgressBar = false;

                    characterInventory.AddToInventory(misc1);

                    if (MiscItems.Miscellaneousitems[a].MiscellaneousItemName == "Vial of Water") // makes sure we have enough mats and near water
                        pickingherbs.photonView.RPC("WaterScale", PhotonTargets.All, pickingherbs.WaterID);

                    for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
                    {
                        characterInventory.RecalculateStackAmounts(MatsReqInventoryManageIndex[i], MiscItems.Miscellaneousitems[a].MatsAmounts[i]);
                    }
                    //this updates the bar stacks in other windows when making bars
                    CheckAllUpdateStacksFromCraftingWindow(characterInventory.InventoryManage[j].SlotName, MiscItems.TotalMiscellaneousStacks[a]); // updates total stacks(mats) from function also updates CheckAllUpdateStacksFromCraftingWindow

                    craftingDatabase.CraftingSkillList[CraftingSkill].CurrentExp += MiscItems.Miscellaneousitems[a].MiscellaneousItemExp;
                    Stats.CurrentPlayerStamina -= craftingDatabase.CraftingSkillList[CraftingSkill].Stamina * Stats.PlayerStamina;
                    break;
                }
            }

            for (int y = 0; y < CraftingCompletedObject.Count; y++)
            {
                PhotonNetwork.Destroy(CraftingCompletedObject[y]);
            }
            CraftingCompletedObject.Clear();
            
        }
    }

    IEnumerator CreateWeapon(int a, string CraftingName, int CraftingAmount)
    {
        if (mainGUI.timeElapsedProgressBar >= Time.time || EquippedCraftingTool(CraftingName) == false)
            yield break;

        for (int craftAmount = 0; craftAmount < CraftingAmount; craftAmount++)
        {
            if (NearACraftingStation(Weapons.WeaponList[a].WeaponName) == false || EquippedCraftingTool(CraftingName) == false)
            {
                mainGUI.CheckMovementProgressBar = false;
                mainGUI.timeElapsedProgressBar = Time.time + 1;
                for (int y = 0; y < CraftingCompletedObject.Count; y++)
                {
                    PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                }
                CraftingCompletedObject.Clear();
                yield break;
            }

            List<int> MatsReqInventoryManageIndex = new List<int>(); // get the amount of mats for a given craftable, and make sure it is there first/and stacks are there
            int CraftingSkill = 0;
            if (CraftingName == "Woodworking")
                CraftingSkill = 1;
            else
                CraftingSkill = 2;

            for (int i = 0; i < Weapons.WeaponList[a].ReferenceIndexToMat.Length; i++)
            {
                if (Weapons.WeaponList[a].ReferenceIndexToMat[i] != -1)
                    MatsReqInventoryManageIndex.Add(-1);
            }

            for (int c = 0; c < 5; c++)
            {
                if (Weapons.WeaponList[a].ReferenceIndexToMat[c] != -1)
                {
                    for (int b = 0; b < characterInventory.InventoryManage.Count; b++)
                    {
                        if (characterInventory.InventoryManage[b].SlotName == MiscItems.MiscellaneousSprites[Weapons.WeaponList[a].ReferenceIndexToMat[c]].name)
                        {
                            if (characterInventory.InventoryManage[b].StackAmounts >= Weapons.WeaponList[a].MatsAmounts[c]
                                && craftingDatabase.CraftingSkillList[CraftingSkill].CurrentRank >= Weapons.WeaponList[a].LevelRank)
                            {
                                MatsReqInventoryManageIndex[c] = b;  // get the inventorymanage index for crafting materials
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
            {
                if (MatsReqInventoryManageIndex[i] == -1)
                {
                    mainGUI.timeElapsedProgressBar = 0;
                    for (int y = 0; y < CraftingCompletedObject.Count; y++)
                    {
                        PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                    }
                    CraftingCompletedObject.Clear();
                    yield break;
                }
            }

            if (Weapons.WeaponList[a].IsASecondaryWeapon == 1)
            {
                for (int j = 0; j < characterInventory.InventoryManage.Count; j++)
                {
                    if (characterInventory.InventoryManage[j].SlotName == Weapons.WeaponList[a].WeaponName &&
                        characterInventory.InventoryManage[j].isASecondary == 1 && characterInventory.InventoryManage[j].StackAmounts < 50) // stacks that are already in inventory
                    {
                        mainGUI.timeElapsedProgressBar = 4.9f + Time.time;
                        mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.055f, Weapons.WeaponList[a].WeaponName));
                        StartCoroutine("CheckMovement");
                        if (CraftingName == "Woodworking")
                            movement.NetworkSetHandAnimations("Crafting", 2);
                        else
                            movement.NetworkSetHandAnimations("Crafting", 3);

                        StartCoroutine(CraftingComponents("Weapons", a, MatsReqInventoryManageIndex.Count)); // crafting components shown on table/furnace
                        yield return new WaitForSeconds(3.925f);

                        movement.NetworkSetHandAnimations("Crafting", 0); // crafting animation 0
                        mainGUI.CheckMovementProgressBar = false;

                        characterInventory.InventoryManage[j].StackAmounts += 1; // update dropitem(stacks)
                        characterInventory.RecalculateStackAmounts(j, -1); // update add 2 stack

                        for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
                        {
                            characterInventory.RecalculateStackAmounts(MatsReqInventoryManageIndex[i], Weapons.WeaponList[a].MatsAmounts[i]);
                        }

                        craftingDatabase.CraftingSkillList[CraftingSkill].CurrentExp += Weapons.WeaponList[a].WeaponExp;
                        Stats.CurrentPlayerStamina -= craftingDatabase.CraftingSkillList[CraftingSkill].Stamina * Stats.PlayerStamina;
                        break;
                    }
                    else if (characterInventory.InventoryManage.Count <= 24 && j >= characterInventory.InventoryManage.Count - 1) // j at end of loop to ensure theres no stacks < 50 
                    {
                        InventoryManager wep1 = new InventoryManager(Weapons.WeaponList[a].WeaponName, "Common", Weapons.WeaponSprites[a], null,
                        Weapons.WeaponList[a].WeaponDamage, Weapons.WeaponList[a].WeaponAttackSpeed,
                        Weapons.WeaponList[a].CritRate, Weapons.WeaponList[a].ArmorPenetration, 0, 0, 0, -1, 1, 1);
                        mainGUI.timeElapsedProgressBar = 4.9f + Time.time;
                        mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.055f, Weapons.WeaponList[a].WeaponName));
                        if (CraftingName == "Woodworking")
                            movement.NetworkSetHandAnimations("Crafting", 2);
                        else
                            movement.NetworkSetHandAnimations("Crafting", 3);

                        StartCoroutine("CheckMovement");
                        StartCoroutine(CraftingComponents("Weapons", a, MatsReqInventoryManageIndex.Count)); // crafting components shown on table/furnace
                        yield return new WaitForSeconds(3.925f);

                        movement.NetworkSetHandAnimations("Crafting", 0); // crafting animation 0
                        mainGUI.CheckMovementProgressBar = false;

                        characterInventory.AddToInventory(wep1);
                        for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
                        {
                            characterInventory.RecalculateStackAmounts(MatsReqInventoryManageIndex[i], Weapons.WeaponList[a].MatsAmounts[i]);
                        }

                        craftingDatabase.CraftingSkillList[CraftingSkill].CurrentExp += Weapons.WeaponList[a].WeaponExp;
                        Stats.CurrentPlayerStamina -= craftingDatabase.CraftingSkillList[CraftingSkill].Stamina * Stats.PlayerStamina;
                        break;
                    }
                }

            } // make all crafting/gathering animations connected to rotationidle instead
            else if (Weapons.WeaponList[a].IsASecondaryWeapon == 0)
            {
                if (characterInventory.InventoryManage.Count <= 24)
                {
                    InventoryManager wep1 = new InventoryManager(Weapons.WeaponList[a].WeaponName, "Common", Weapons.WeaponSprites[a], null,
                    Weapons.WeaponList[a].WeaponDamage, Weapons.WeaponList[a].WeaponAttackSpeed,
                    Weapons.WeaponList[a].CritRate, Weapons.WeaponList[a].ArmorPenetration, 0, 0, 0, -1, -1, 0);

                    mainGUI.timeElapsedProgressBar = 4.9f + Time.time;
                    mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.055f, Weapons.WeaponList[a].WeaponName));
                    StartCoroutine("CheckMovement");
                    if (CraftingName == "Woodworking")
                        movement.NetworkSetHandAnimations("Crafting", 2);
                    else
                        movement.NetworkSetHandAnimations("Crafting", 3);

                    StartCoroutine(CraftingComponents("Weapons", a, MatsReqInventoryManageIndex.Count)); // crafting components shown on table/furnace
                    yield return new WaitForSeconds(3.925f);

                    movement.NetworkSetHandAnimations("Crafting", 0); // crafting animation 0
                    mainGUI.CheckMovementProgressBar = false;

                    characterInventory.AddToInventory(wep1);
                    for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
                    {
                        characterInventory.RecalculateStackAmounts(MatsReqInventoryManageIndex[i], Weapons.WeaponList[a].MatsAmounts[i]);
                    }

                    craftingDatabase.CraftingSkillList[CraftingSkill].CurrentExp += Weapons.WeaponList[a].WeaponExp;
                    Stats.CurrentPlayerStamina -= craftingDatabase.CraftingSkillList[CraftingSkill].Stamina * Stats.PlayerStamina;
                }
            }

            for (int y = 0; y < CraftingCompletedObject.Count; y++)
            {
                PhotonNetwork.Destroy(CraftingCompletedObject[y]);
            }
            CraftingCompletedObject.Clear();          
        }
    }

    IEnumerator CreateTools(int a, string CraftingName, int CraftingAmount)
    {
        if (mainGUI.timeElapsedProgressBar >= Time.time)
        {
            if ((Tools.ToolList[a].ToolId != 850 && Tools.ToolList[a].ToolId >= 750) ||
                Tools.ToolList[a].ToolId == 1)
            {
                if (EquippedCraftingTool("Blacksmithing") == false)
                    yield break;
            }
        }

        for (int craftAmount = 0; craftAmount < CraftingAmount; craftAmount++)
        {
            if (a < Tools.ToolList.Count)
            {
                if (Tools.ToolList[a].ToolId != 1) // only axes/pickaxes/hammers/knives
                { 
                    if (NearACraftingStation(Tools.ToolList[a].ToolName) == false)
                    {
                        mainGUI.CheckMovementProgressBar = false;
                        mainGUI.timeElapsedProgressBar = Time.time + 1;
                        for (int y = 0; y < CraftingCompletedObject.Count; y++)
                        {
                            PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                        }
                        CraftingCompletedObject.Clear();
                        yield break;
                    }
                    if ((Tools.ToolList[a].ToolId != 850 && Tools.ToolList[a].ToolId >= 750) ||
                        Tools.ToolList[a].ToolId == 2)
                    {
                        if (EquippedCraftingTool("Blacksmithing") == false)
                        {
                            mainGUI.CheckMovementProgressBar = false;
                            mainGUI.timeElapsedProgressBar = Time.time + 1;
                            for (int y = 0; y < CraftingCompletedObject.Count; y++)
                            {
                                PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                            }
                            CraftingCompletedObject.Clear();
                            yield break;
                        }
                    }
                }
            }

            List<int> MatsReqInventoryManageIndex = new List<int>(); // get the amount of mats for a given craftable, and make sure it is there first/and stacks are there
            for (int i = 0; i < Tools.ToolList[a].ReferenceIndexToMat.Length; i++)
            {
                if (Tools.ToolList[a].ReferenceIndexToMat[i] != -1)
                    MatsReqInventoryManageIndex.Add(-1);
            }

            for (int c = 0; c < 5; c++)
            {
                if (Tools.ToolList[a].ReferenceIndexToMat[c] != -1)
                {
                    for (int b = 0; b < characterInventory.InventoryManage.Count; b++)
                    {
                        if (characterInventory.InventoryManage[b].SlotName == MiscItems.MiscellaneousSprites[Tools.ToolList[a].ReferenceIndexToMat[c]].name)
                        {
                            if (characterInventory.InventoryManage[b].StackAmounts >= Tools.ToolList[a].MatsAmounts[c]
                                && craftingDatabase.CraftingSkillList[0].CurrentRank >= Tools.ToolList[a].RequiredLevel)
                            {
                                MatsReqInventoryManageIndex[c] = b;  // get the inventorymanage index for crafting materials 
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
            {
                if (MatsReqInventoryManageIndex[i] == -1)
                {
                    mainGUI.timeElapsedProgressBar = Time.time + 1;
                    for (int y = 0; y < CraftingCompletedObject.Count; y++)
                    {
                        PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                    }
                    CraftingCompletedObject.Clear();

                    yield break;
                }
            }

            if (characterInventory.InventoryManage.Count <= 24)
            {
                InventoryManager tools = new InventoryManager(Tools.ToolList[a].ToolName, "Common",
                Tools.ToolSprites[a], null, 0, 0, 0, 0, 0, 0, 0,
                -1, 1, 0);
                mainGUI.timeElapsedProgressBar = 4.9f + Time.time;
                mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.055f, Tools.ToolList[a].ToolName));
                StartCoroutine("CheckMovement");
                if (Tools.ToolList[a].ToolId != 1 && Tools.ToolList[a].ToolId != 850)
                    movement.NetworkSetHandAnimations("Crafting", 3);

                StartCoroutine(CraftingComponents("Tools", a, MatsReqInventoryManageIndex.Count)); // crafting components shown on table/furnace
                yield return new WaitForSeconds(3.925f);

                movement.NetworkSetHandAnimations("Crafting", 0); // crafting animation 0
                mainGUI.CheckMovementProgressBar = false;

                characterInventory.AddToInventory(tools);
                for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
                {
                    characterInventory.RecalculateStackAmounts(MatsReqInventoryManageIndex[i], Tools.ToolList[a].MatsAmounts[i]);
                }

                craftingDatabase.CraftingSkillList[0].CurrentExp += Tools.ToolList[a].ToolExp;
                Stats.CurrentPlayerStamina -= craftingDatabase.CraftingSkillList[0].Stamina * Stats.PlayerStamina;
            }

            for (int y = 0; y < CraftingCompletedObject.Count; y++)
            {
                PhotonNetwork.Destroy(CraftingCompletedObject[y]);
            }
            CraftingCompletedObject.Clear();        
        }
    }

    IEnumerator CreateArmors(int a, string CraftingName, int CraftingAmount)
    {
        if (mainGUI.timeElapsedProgressBar >= Time.time || EquippedCraftingTool(CraftingName) == false)
            yield break;

        for (int craftAmount = 0; craftAmount < CraftingAmount; craftAmount++)
        {
            if (NearACraftingStation(Armors.ArmorList[a].ArmorName) == false || EquippedCraftingTool(CraftingName) == false)
            {
                mainGUI.CheckMovementProgressBar = false;
                mainGUI.timeElapsedProgressBar = Time.time + 1;
                for (int y = 0; y < CraftingCompletedObject.Count; y++)
                {
                    PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                }
                CraftingCompletedObject.Clear();
                yield break;
            }

            List<int> MatsReqInventoryManageIndex = new List<int>(); // get the amount of mats for a given craftable, and make sure it is there first/and stacks are there
            int CraftingSkill = 0;
            if (CraftingName == "Woodworking")
                CraftingSkill = 1;
            else
                CraftingSkill = 2;

            for (int i = 0; i < Armors.ArmorList[a].ReferenceIndexToMat.Length; i++)
            {
                if (Armors.ArmorList[a].ReferenceIndexToMat[i] != -1)
                    MatsReqInventoryManageIndex.Add(-1);
            }

            for (int c = 0; c < 5; c++)
            {
                if (Armors.ArmorList[a].ReferenceIndexToMat[c] != -1)
                {
                    for (int b = 0; b < characterInventory.InventoryManage.Count; b++)
                    {
                        if (characterInventory.InventoryManage[b].SlotName == MiscItems.MiscellaneousSprites[Armors.ArmorList[a].ReferenceIndexToMat[c]].name)
                        {
                            if (characterInventory.InventoryManage[b].StackAmounts >= Armors.ArmorList[a].MatsAmounts[c]
                                && craftingDatabase.CraftingSkillList[CraftingSkill].CurrentRank >= Armors.ArmorList[a].LevelRank)
                            {
                                MatsReqInventoryManageIndex[c] = b; // get the inventorymanage index for crafting materials
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
            {
                if (MatsReqInventoryManageIndex[i] == -1)
                {
                    mainGUI.timeElapsedProgressBar = 0;
                    for (int y = 0; y < CraftingCompletedObject.Count; y++)
                    {
                        PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                    }
                    CraftingCompletedObject.Clear();
                    yield break;
                }
            }

            if (characterInventory.InventoryManage.Count <= 24)
            {
                InventoryManager armors = new InventoryManager(Armors.ArmorList[a].ArmorName, "Common",
                Armors.ArmorSprites[a], null, 0, 0, 0, 0, Armors.ArmorList[a].DefenseValues, Armors.ArmorList[a].BonusHealth, Armors.ArmorList[a].BonusStamina,
                -1, 1, 0);
                mainGUI.timeElapsedProgressBar = 4.9f + Time.time;
                mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.055f, Armors.ArmorList[a].ArmorName));
                StartCoroutine("CheckMovement");
                if (CraftingName == "Woodworking")
                    movement.NetworkSetHandAnimations("Crafting", 2);
                else
                    movement.NetworkSetHandAnimations("Crafting", 3);

                StartCoroutine(CraftingComponents("Armors", a, MatsReqInventoryManageIndex.Count)); // crafting components shown on table/furnace

                yield return new WaitForSeconds(3.925f);
                movement.NetworkSetHandAnimations("Crafting", 0);
                characterInventory.AddToInventory(armors);
                for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
                {
                    characterInventory.RecalculateStackAmounts(MatsReqInventoryManageIndex[i], Armors.ArmorList[a].MatsAmounts[i]);
                }

                craftingDatabase.CraftingSkillList[CraftingSkill].CurrentExp += Armors.ArmorList[a].ArmorExp;
                Stats.CurrentPlayerStamina -= craftingDatabase.CraftingSkillList[CraftingSkill].Stamina * Stats.PlayerStamina;
            }

            for (int y = 0; y < CraftingCompletedObject.Count; y++)
            {
                PhotonNetwork.Destroy(CraftingCompletedObject[y]);
            }
            CraftingCompletedObject.Clear();        
        }
    }

    IEnumerator CreatePotions(int a, string CraftingName, int CraftingAmount)
    {
        if (mainGUI.timeElapsedProgressBar >= Time.time || EquippedCraftingTool(CraftingName) == false)
            yield break;

        for (int craftAmount = 0; craftAmount < CraftingAmount; craftAmount++)
        {
            if (NearACraftingStation(Potions.PotionList[a].PotionName) == false || EquippedCraftingTool(CraftingName) == false)
            {
                mainGUI.CheckMovementProgressBar = false;
                mainGUI.timeElapsedProgressBar = Time.time + 1;
                for (int y = 0; y < CraftingCompletedObject.Count; y++)
                {
                    PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                }
                CraftingCompletedObject.Clear();
                yield break;
            }

            List<int> MatsReqInventoryManageIndex = new List<int>(); // get the amount of mats for a given craftable, and make sure it is there first/and stacks are there

            for (int i = 0; i < Potions.PotionList[a].ReferenceIndexToMat.Length; i++)
            {
                if (Potions.PotionList[a].ReferenceIndexToMat[i] != -1)
                    MatsReqInventoryManageIndex.Add(-1);
            }

            for (int c = 0; c < 5; c++)
            {
                if (Potions.PotionList[a].ReferenceIndexToMat[c] != -1)
                {
                    for (int b = 0; b < characterInventory.InventoryManage.Count; b++)
                    {
                        if (characterInventory.InventoryManage[b].SlotName == MiscItems.MiscellaneousSprites[Potions.PotionList[a].ReferenceIndexToMat[c]].name)
                        {
                            if (characterInventory.InventoryManage[b].StackAmounts >= Potions.PotionList[a].MatsAmounts[c]
                                && craftingDatabase.CraftingSkillList[3].CurrentRank >= Potions.PotionList[a].LevelRank)
                            {
                                MatsReqInventoryManageIndex[c] = b; // get the inventorymanage index for crafting materials
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
            {
                if (MatsReqInventoryManageIndex[i] == -1)
                {
                    mainGUI.timeElapsedProgressBar = Time.time + 1;
                    for (int y = 0; y < CraftingCompletedObject.Count; y++)
                    {
                        PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                    }
                    CraftingCompletedObject.Clear();
                    yield break;
                }
            }

            for (int j = 0; j < characterInventory.InventoryManage.Count; j++)
            {
                if (characterInventory.InventoryManage[j].SlotName == Potions.PotionList[a].PotionName &&
                    characterInventory.InventoryManage[j].isASecondary == 1 && characterInventory.InventoryManage[j].StackAmounts < 50) // stacks that are already in inventory
                {
                    mainGUI.timeElapsedProgressBar = 4.9f + Time.time;
                    mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.055f, Potions.PotionList[a].PotionName));
                    StartCoroutine("CheckMovement");
                    movement.NetworkSetHandAnimations("Crafting", 3);

                    StartCoroutine(CraftingComponents("Pots", a, MatsReqInventoryManageIndex.Count)); // crafting components shown on table/furnace
                    yield return new WaitForSeconds(3.925f);

                    movement.NetworkSetHandAnimations("Crafting", 0);
                    characterInventory.InventoryManage[j].StackAmounts += 1;
                    characterInventory.RecalculateStackAmounts(j, -1); // update add 1 stack

                    for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
                    {
                        characterInventory.RecalculateStackAmounts(MatsReqInventoryManageIndex[i], Potions.PotionList[a].MatsAmounts[i]);
                    }

                    craftingDatabase.CraftingSkillList[3].CurrentExp += Potions.PotionList[a].PotionExp;
                    Stats.CurrentPlayerStamina -= craftingDatabase.CraftingSkillList[3].Stamina * Stats.PlayerStamina;
                    break;
                }
                else if (characterInventory.InventoryManage.Count <= 24 && j >= characterInventory.InventoryManage.Count - 1)
                {
                    InventoryManager pots = new InventoryManager(Potions.PotionList[a].PotionName, "Common", Potions.PotionSprites[a], null,
                        0, 0, 0, 0, 0, 0, 0, -1, 1, 1);
                    mainGUI.timeElapsedProgressBar = 4.9f + Time.time;
                    mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.055f, Potions.PotionList[a].PotionName));
                    StartCoroutine("CheckMovement");
                    movement.NetworkSetHandAnimations("Crafting", 3);

                    StartCoroutine(CraftingComponents("Pots", a, MatsReqInventoryManageIndex.Count)); // crafting components shown on table/furnace
                    yield return new WaitForSeconds(3.925f);

                    movement.NetworkSetHandAnimations("Crafting", 0);
                    characterInventory.AddToInventory(pots);

                    for (int i = 0; i < MatsReqInventoryManageIndex.Count; i++)
                    {
                        characterInventory.RecalculateStackAmounts(MatsReqInventoryManageIndex[i], Potions.PotionList[a].MatsAmounts[i]);
                    }

                    craftingDatabase.CraftingSkillList[3].CurrentExp += Potions.PotionList[a].PotionExp;
                    Stats.CurrentPlayerStamina -= craftingDatabase.CraftingSkillList[3].Stamina * Stats.PlayerStamina;
                    break;
                }

                for (int y = 0; y < CraftingCompletedObject.Count; y++)
                {
                    PhotonNetwork.Destroy(CraftingCompletedObject[y]);
                }
                CraftingCompletedObject.Clear();              
            }
        }
    }

    public void UsePotions(string PotionName) // for drinking water/hp/stam potions
    {
        for (int i = 0; i < characterInventory.InventoryManage.Count; i++)
        {
            if (PotionName == characterInventory.InventoryManage[i].SlotName)
            {
                for (int j = 0; j < Potions.PotionList.Count; j++)
                {
                    if (Potions.PotionList[j].PotionName == characterInventory.InventoryManage[i].SlotName &&
                        Stats.PlayerLevel >= Potions.PotionList[j].LevelRank)
                    {
                        for (int x = 0; x < KeyBindRects.Count; x++) // errror
                        {
                            if (KeyBindRects[x].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name == Potions.PotionList[j].PotionName &&
                                characterInventory.InventoryManage[i].StackAmounts == 1)
                            {
                                KeyBindRects[x].transform.Find("HoverPickupBar").GetComponent<Image>().sprite = characterInventory.DefaultSprite;
                                KeyBindRects[x].transform.Find("HoverPickupBar").GetComponent<Image>().GetComponentInChildren<Mask>().showMaskGraphic = false;
                            }
                        }

                        switch (Potions.PotionList[j].PotionType)
                        {
                            case "HP":
                                if (Stats.CurrentPlayerHealth == Stats.PlayerHealth)
                                    break;
                                if (Stats.CurrentPlayerHealth + (Stats.tempHealth * Potions.PotionList[j].PotionValue) > Stats.PlayerHealth)
                                    Stats.CurrentPlayerHealth = Stats.PlayerHealth;
                                else
                                    Stats.CurrentPlayerHealth += Stats.tempHealth * Potions.PotionList[j].PotionValue;
                                characterInventory.RecalculateStackAmounts(i, 1);

                                break;
                            case "STAM":
                                if (Stats.CurrentPlayerStamina == Stats.PlayerStamina)
                                    break;
                                if (Stats.CurrentPlayerStamina + (Stats.PlayerStamina * Potions.PotionList[j].PotionValue) > Stats.PlayerStamina)
                                    Stats.CurrentPlayerStamina = Stats.PlayerStamina;
                                else
                                    Stats.CurrentPlayerStamina += Stats.PlayerStamina * Potions.PotionList[j].PotionValue;
                                characterInventory.RecalculateStackAmounts(i, 1);

                                break;
                            case "Water":
                                //drinking water
                                break;
                        }

                        return;
                    }
                }
            }
        }
    }

    public delegate IEnumerator CreationDelegate(int a, string Name, int CraftingAmount);

    public void NewCraftingItemsFromLevelup(string WindowName, int CurrentCraftingLevel) // used in skillsgui 
    {
        GameObject Window = null;
        if (WindowName == MetalworkingWindow.transform.Find("Image").GetComponentInChildren<Text>().text)
            Window = MetalworkingWindow;
        else if (WindowName == BlacksmithingWindow.transform.Find("Image").GetComponentInChildren<Text>().text)
            Window = BlacksmithingWindow;
        else if (WindowName == WoodworkingWindow.transform.Find("Image").GetComponentInChildren<Text>().text)
            Window = WoodworkingWindow;
        else if (WindowName == PotionsWindow.transform.Find("Image").GetComponentInChildren<Text>().text)
            Window = PotionsWindow;

        if (Window != null)
        {
            Image[] TotalMats = Window.transform.Find("ScrollRect/Content").GetComponentsInChildren<Image>();

            for (int i = 0; i < MiscItems.Miscellaneousitems.Count; i++)
            {
                for (int j = 0; j < TotalMats.Length; j++)
                {
                    if (MiscItems.Miscellaneousitems[i].MiscellaneousItemName == TotalMats[j].GetComponentInChildren<Text>().text)
                    {
                        if (CurrentCraftingLevel >= MiscItems.Miscellaneousitems[i].LevelRank)
                            TotalMats[j].GetComponentInChildren<Text>().color = Color.black;
                        else
                            TotalMats[j].GetComponentInChildren<Text>().color = Color.gray;
                    }
                }
            }

            for (int i = 0; i < Weapons.WeaponList.Count; i++)
            {
                for (int j = 0; j < TotalMats.Length; j++)
                {
                    if (Weapons.WeaponList[i].WeaponName == TotalMats[j].GetComponentInChildren<Text>().text)
                    {
                        if (CurrentCraftingLevel >= Weapons.WeaponList[i].LevelRank)
                            TotalMats[j].GetComponentInChildren<Text>().color = Color.black;
                        else
                            TotalMats[j].GetComponentInChildren<Text>().color = Color.gray;
                    }
                }
            }

            for (int i = 0; i < Potions.PotionList.Count; i++)
            {
                for (int j = 0; j < TotalMats.Length; j++)
                {
                    if (Potions.PotionList[i].PotionName == TotalMats[j].GetComponentInChildren<Text>().text)
                    {
                        if (CurrentCraftingLevel >= Potions.PotionList[i].LevelRank)
                            TotalMats[j].GetComponentInChildren<Text>().color = Color.black;
                        else
                            TotalMats[j].GetComponentInChildren<Text>().color = Color.gray;
                    }
                }
            }

            for (int i = 0; i < Armors.ArmorList.Count; i++)
            {
                for (int j = 0; j < TotalMats.Length; j++)
                {
                    if (Armors.ArmorList[i].ArmorName == TotalMats[j].GetComponentInChildren<Text>().text)
                    {
                        if (CurrentCraftingLevel >= Armors.ArmorList[i].LevelRank)
                            TotalMats[j].GetComponentInChildren<Text>().color = Color.black;
                        else
                            TotalMats[j].GetComponentInChildren<Text>().color = Color.gray;
                    }
                }
            }
        }
    }

    public void CheckUpdateStacksFromCraftingWindow(GameObject Window, string MatName, int Stacks)
    {
        if (Window != null)
        {
            Text[] TotalMatsRequired = Window.transform.Find("Mats/MatsRequiredNames").GetComponentsInChildren<Text>();
            Text[] TotalMatsRequiredQuantity = Window.transform.Find("Mats/MatsRequiredStacks").GetComponentsInChildren<Text>();

            UpdateStacksFromCraftingWindow(TotalMatsRequired, TotalMatsRequiredQuantity, MatName, Stacks);
        }
    }

    public void UpdateStacksFromCraftingWindow(Text[] TMatsRequired, Text[] TMatsRequiredQuantity, string MatName, int Stacks)
    {
        for (int c = 0; c < TMatsRequired.Length; c++)
        {
            if (TMatsRequired[c].text == MatName)
            {
                TMatsRequiredQuantity[c].GetComponentInChildren<Text>().text =
                Stacks + TMatsRequiredQuantity[c].GetComponentInChildren<Text>().text.Substring(TMatsRequiredQuantity[c].GetComponentInChildren<Text>().text.IndexOf("/"), TMatsRequiredQuantity[c].GetComponentInChildren<Text>().text.Length - TMatsRequiredQuantity[c].GetComponentInChildren<Text>().text.IndexOf("/"));
            }
        }
    }

    public void UpdateMatsRequired(GameObject CraftingWindow, string CraftingName, int CraftingCount, int a, string Name, float CraftingRank, int LevelRank, int[] MatIndex, int[] MatsAmounts, Sprite ItemCreatedIcon, CreationDelegate CreateObject)
    {
        Image[] RequiredStackTexts = CraftingWindow.transform.Find("Mats/MatsRequiredStacks").GetComponentsInChildren<Image>();
        Image[] RequiredName = CraftingWindow.transform.Find("Mats/MatsRequiredNames").GetComponentsInChildren<Image>();
        Image Icons = CraftingWindow.transform.Find("Mats/ItemIcons/Image").GetComponent<Image>();
        Image Craft1 = CraftingWindow.transform.Find("Mats/MatsCraft/Craft1").GetComponent<Image>();
        Image Craft5 = CraftingWindow.transform.Find("Mats/MatsCraft/Craft5").GetComponent<Image>();
        Image Craft10 = CraftingWindow.transform.Find("Mats/MatsCraft/Craft10").GetComponent<Image>();
        Craft1.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        Craft5.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        Craft10.GetComponentInChildren<Button>().onClick.RemoveAllListeners();

        Icons.transform.Find("ImageScript").GetComponent<Image>().sprite = ItemCreatedIcon;
        Icons.transform.GetComponent<Mask>().showMaskGraphic = true;
        Icons.transform.Find("ImageScript").GetComponent<Mask>().showMaskGraphic = true;

        for (int c = 0; c < MatsAmounts.Length; c++)
        {
            if (MatsAmounts[c] != -1)
            {
                RequiredStackTexts[c].GetComponent<Mask>().showMaskGraphic = true;
                RequiredName[c].GetComponent<Mask>().showMaskGraphic = true;
                RequiredName[c].GetComponentInChildren<Text>().text = MiscItems.MiscellaneousSprites[MatIndex[c]].name;
                RequiredStackTexts[c].GetComponentInChildren<Text>().fontSize = 17;
                RequiredStackTexts[c].GetComponentInChildren<Text>().text = "0/" + MatsAmounts[c];
                CheckUpdateStacksFromCraftingWindow(CraftingWindow, MiscItems.MiscellaneousSprites[MatIndex[c]].name, MiscItems.TotalMiscellaneousStacks[MatIndex[c]]);
            }
            else
            {
                RequiredStackTexts[c].GetComponentInChildren<Text>().text = string.Empty;
                RequiredStackTexts[c].GetComponent<Mask>().showMaskGraphic = false;
                RequiredName[c].GetComponentInChildren<Text>().text = string.Empty;
                RequiredName[c].GetComponent<Mask>().showMaskGraphic = false;
            }
        }

        int ReferenceA = a;

        Craft1.GetComponentInChildren<Button>().onClick.AddListener(() => CoroutineForCrafting(CreateObject, ReferenceA, CraftingName, 1));
        Craft5.GetComponentInChildren<Button>().onClick.AddListener(() => CoroutineForCrafting(CreateObject, ReferenceA, CraftingName, 5));
        Craft10.GetComponentInChildren<Button>().onClick.AddListener(() => CoroutineForCrafting(CreateObject, ReferenceA, CraftingName, 10));
    }

    public void CoroutineForCrafting(CreationDelegate CreateObject, int a, string CraftingName, int CraftingCount) // dont want multiple coroutines 
    {
        if (Time.time >= mainGUI.timeElapsedProgressBar)
        {
            CraftingCoroutine = StartCoroutine(CreateObject(a, CraftingName, CraftingCount));
        }
    }

    public void CreateCraftingwindow(GameObject CraftingWindow, string CraftingName, int CraftingCount, int a, string Name, float CraftingRank, int LevelRank, int[] MatIndex, int[] MatsAmounts, Sprite ItemCreatedIcon, CreationDelegate CreateObject) // created once at load
    {
        CraftingWindow.transform.SetParent(canvas.transform);
        CraftingWindow.GetComponentInChildren<Button>().onClick.AddListener(() => mainGUI.SetActiveWindow(CraftingWindow.gameObject, 0, false, false));
        CraftingWindow.GetComponentInChildren<RectTransform>().localScale = new Vector3(1, 1, 0);

        CraftingWindow.GetComponentInChildren<Text>().text = CraftingName;

        Image Texts = Instantiate(CraftingIconPrefab, transform.position, transform.rotation) as Image;

        Texts.transform.SetParent(CraftingWindow.transform.Find("ScrollRect/Content/Craft" + CraftingCount.ToString() + "/Items").transform);
        Texts.transform.localScale = new Vector3(1, 1, 0);
        Texts.GetComponentInChildren<Text>().text = Name;

        if (CraftingRank >= LevelRank)
        {
            Texts.GetComponentInChildren<Text>().color = Color.black;
        }
        else
        {
            Texts.GetComponentInChildren<Text>().color = Color.gray;
        }

        Texts.GetComponentInChildren<Button>().onClick.AddListener(() => UpdateMatsRequired(CraftingWindow, CraftingName, CraftingCount, a, Name, CraftingRank, LevelRank, MatIndex, MatsAmounts, ItemCreatedIcon, CreateObject));
        CraftingSwitch(0, CraftingWindow); // set default crafting screen to first one.
    }

    public void ButtonPressed()
    {
        if (Time.time >= TimeElapsed)
        {
            for (int i = 0; i < KeyBindRects.Count; i++) // the 8 buttons in skillbar
            {
                if (Input.GetKey(mainGUI.KeyBinds[i + 6])) // 1,2,3,4,5,6,7,8 - default
                {
                    // very unoptimized == create a dictionary for keybinds[i] and have the key be i , and value be a string that contains which skill type(ex:gathering,crafting, for the weapon skills have a switch/case for fire/lightning/general/etc) is being used 
                    for (int x = 0; x < Potions.PotionList.Count; x++)
                    {
                        if (KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name == Potions.PotionList[x].PotionName && mainGUI.DisableKeyBindsWhenTyping == false)
                        {
                            TimeElapsed = Time.time + 1;
                            UsePotions(KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name);
                        }
                    }

                    if (KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name == "Teleport")
                    {
                        movement.StartCoroutine("Teleport");
                    }

                    for (int x = 0; x < gatheringDatabase.GatheringSkillList.Count; x++)
                    {
                        if (KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name == gatheringDatabase.GatheringSkillList[x].GatheringName &&
                            Time.time >= mainGUI.timeElapsedProgressBar && mainGUI.DisableKeyBindsWhenTyping == false && movement.PlayerVelocity.magnitude < 1)
                        {
                            switch (gatheringDatabase.GatheringSkillList[x].GatheringName)
                            {
                                case "Woodcutting":
                                    if (Stats.PlayerStamina >= Stats.PlayerStamina * gatheringDatabase.GatheringSkillList[x].StaminaCost)
                                        choptree.StartCoroutine("ChoppingAnimation");
                                    break;
                                case "Mining":
                                    if (Stats.PlayerStamina >= Stats.PlayerStamina * gatheringDatabase.GatheringSkillList[x].StaminaCost)
                                        minerocks.StartCoroutine("MiningAnimation");
                                    break;
                                case "Herbalism":
                                    if (Stats.PlayerStamina >= Stats.PlayerStamina * gatheringDatabase.GatheringSkillList[x].StaminaCost)
                                        pickingherbs.StartCoroutine("GatherAnimation");
                                    break;
                            }
                        }
                    }

                    for (int x = 0; x < craftingDatabase.CraftingSkillList.Count; x++) // finish woodworking, armor, potionmaking
                    {
                        if ((KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name == craftingDatabase.CraftingSkillList[x].CraftingName
                             && mainGUI.DisableKeyBindsWhenTyping == false))
                        {
                            switch (craftingDatabase.CraftingSkillList[x].CraftingName)
                            {
                                case "Metalworking":
                                    mainGUI.SetActiveWindow(MetalworkingWindow.gameObject, 1, true, true);
                                    break;
                                case "Woodworking":
                                    mainGUI.SetActiveWindow(WoodworkingWindow.gameObject, 1, true, true);
                                    break;
                                case "Blacksmithing":
                                    mainGUI.SetActiveWindow(BlacksmithingWindow.gameObject, 1, true, true);
                                    break;
                                case "Potion Crafting":
                                    mainGUI.SetActiveWindow(PotionsWindow.gameObject, 1, true, true);
                                    break;
                            }
                        }
                    }
                    if (movement.Attacking == false &&
            movement.TmeAttackElapsed < Time.time && WepSwitch.CurrentItemId > 0 && movement.CurrentElementalStrike != string.Empty && movement.JumpBool == false)
                    {
                        GeneralSkillCreation SkillCreation = null;

                        for (int x = 0; x < GeneralSkillsDB.GeneralSkillList.Count; x++)
                        {
                            if (KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name == GeneralSkillsDB.GeneralSkillList[x].SkillName && mainGUI.DisableKeyBindsWhenTyping == false
                                && CoolDownRects[i].fillAmount == 0)
                            {
                                SkillCreation = GeneralSkillsDB.GeneralSkillList[x];
                            }
                        }

                        for (int x = 0; x < GeneralSkillsDB.FireSkillList.Count; x++)
                        {
                            if (KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name == GeneralSkillsDB.FireSkillList[x].SkillName && mainGUI.DisableKeyBindsWhenTyping == false
                                && CoolDownRects[i].fillAmount == 0)
                            {
                                SkillCreation = GeneralSkillsDB.FireSkillList[x];
                            }
                        }

                        for (int x = 0; x < GeneralSkillsDB.IceSkillList.Count; x++)
                        {
                            if (KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name == GeneralSkillsDB.IceSkillList[x].SkillName && mainGUI.DisableKeyBindsWhenTyping == false
                                && CoolDownRects[i].fillAmount == 0)
                            {
                                SkillCreation = GeneralSkillsDB.IceSkillList[x];
                            }
                        }

                        for (int x = 0; x < GeneralSkillsDB.LightningSkillList.Count; x++)
                        {
                            if (KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name == GeneralSkillsDB.LightningSkillList[x].SkillName && mainGUI.DisableKeyBindsWhenTyping == false
                                && CoolDownRects[i].fillAmount == 0)
                            {
                                SkillCreation = GeneralSkillsDB.LightningSkillList[x];
                            }
                        }

                        for (int x = 0; x < GeneralSkillsDB.NatureSkillList.Count; x++)
                        {
                            if (KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite.name == GeneralSkillsDB.NatureSkillList[x].SkillName && mainGUI.DisableKeyBindsWhenTyping == false
                                && CoolDownRects[i].fillAmount == 0)
                            {
                                SkillCreation = GeneralSkillsDB.NatureSkillList[x];
                            }
                        }

                        if (SkillCreation != null)
                        {
                            if (WepSwitch.CurrentObject == null) // make sure weapon skills have weapon equipped;
                            {
                                return;
                            }

                            TimeElapsed = Time.time + 1;
                            SkillCreation.IsSkillOn = true;
                            CallSkillFunctions(SkillCreation.SkillName);

                            if (SkillCreation.IsABuff == true) // buff cd
                            {
                                BuffBarRects[CurrentBuffBarCount].gameObject.SetActive(true);
                                BuffBarRects[CurrentBuffBarCount].transform.Find("ImageScript").GetComponent<Image>().sprite = KeyBindRects[i].transform.Find("HoverPickupBar").GetComponent<Image>().sprite;
                                BuffBarRects[CurrentBuffBarCount].transform.Find("ImageScript").GetComponent<Mask>().showMaskGraphic = true;
                                StartCoroutine(BuffBarDuration(BuffBarRects[CurrentBuffBarCount], DurationBarRects[CurrentBuffBarCount], SkillCreation.Duration));
                                CurrentBuffBarCount++;

                                if (CurrentBuffBarCount == BuffBarRects.Count)
                                    CurrentBuffBarCount = 0;
                            }

                            StartCoroutine(SkillBarCoolDown(CoolDownRects[i], SkillCreation.CoolDown));
                        }
                    }
                }
            }
        }
    }

    public void CallSkillFunctions(string SkillName)
    {
        switch (SkillName)
        {
            case "Stamina Recovery":
                GeneralSkillsDB.StartCoroutine("StaminaRecovery");
                break;
            case "Health Recovery":
                GeneralSkillsDB.StartCoroutine("HealthRecovery");
                break;

            //elemental strikes can be keybinded to 1-8 and lmb

            case "Fire Strike":
                movement.StrikeFromSkillBar = true;
                break;
            case "Ice Strike":
                movement.StrikeFromSkillBar = true;
                break;
            case "Lightning Strike":
                movement.StrikeFromSkillBar = true;
                break;
            case "Earth Strike":
                movement.StrikeFromSkillBar = true;
                break;

        }
    }

    public IEnumerator BuffBarDuration(Image SkillBarRect, Image SkillBuffRect, float dur)
    {
        SkillBuffRect.fillAmount += dur;

        yield return new WaitForSeconds(1);
        if (SkillBuffRect.fillAmount == 1)
        {
            SkillBuffRect.fillAmount = 0;
            SkillBarRect.transform.Find("ImageScript").GetComponent<Mask>().showMaskGraphic = false;
            SkillBarRect.gameObject.SetActive(false);
            StopCoroutine("BuffBarDuration");
        }
        else
            StartCoroutine(BuffBarDuration(SkillBarRect, SkillBuffRect, dur));

    }

    public IEnumerator SkillBarCoolDown(Image keybindCD, float CD)
    {
        keybindCD.fillAmount += CD;
        keybindCD.transform.parent.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);

        yield return new WaitForSeconds(1);
        if (keybindCD.fillAmount == 1)
        {
            keybindCD.fillAmount = 0;
            keybindCD.transform.parent.GetComponent<Image>().color = new Color(1, 1, 1);
            StopCoroutine("SkillBarCoolDown");
        }
        else
            StartCoroutine(SkillBarCoolDown(keybindCD, CD));
    }

    //These pointer functions are referenced in the canvas in hierarchy (skillbars)

    void PointerExit(Button ActionButton)
    {
        CurrentPointerEnterName = string.Empty;
    }

    IEnumerator PointerStay(Button ActionButton)
    {
        yield return new WaitForSeconds(0.1f);

        if (CurrentPointerEnterName == ActionButton.name && CurrentSkillPickUpSprite != null)
        {
            ActionButton.transform.Find("HoverPickupBar").GetComponent<Image>().sprite = CurrentSkillPickUpSprite;
            ActionButton.transform.Find("HoverPickupBar").GetComponent<Mask>().showMaskGraphic = true;
            CurrentPointerEnterName = string.Empty;
            CurrentSkillPickUpSprite = null;
            StopCoroutine(PointerStay(ActionButton));
        }
        else if (CurrentPointerEnterName == ActionButton.name)
            StartCoroutine(PointerStay(ActionButton));
    }

    public void PointerEnter(Button ActionButton)
    {

        CurrentPointerEnterName = ActionButton.name;
        for (int i = 0; i < CoolDownRects.Count; i++)
        {
            if (CoolDownRects[i].name == ActionButton.name + "CoolDownSkill" &&
                CoolDownRects[i].fillAmount == 0)
            {
                StartCoroutine(PointerStay(ActionButton));
            }
        }
    }

    void Start()
    {
        canvas = GameObject.Find("Canvas").GetComponentInChildren<Canvas>();
        terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
        CraftingCompletedObject = new List<GameObject>();

        for (int i = 0; i < BuffBarRects.Count; i++)
            BuffBarRects[i].gameObject.SetActive(false);

        gatheringDatabase = terrain.Player.GetComponentInChildren<GatheringSkillDatabase>();
        craftingDatabase = terrain.Player.GetComponentInChildren<CraftingSkillDatabase>();
        MiscItems = terrain.Player.GetComponentInChildren<MiscellaneousItemsDatabase>();
        Potions = terrain.Player.GetComponentInChildren<PotionDatabase>();
        Weapons = terrain.Player.GetComponentInChildren<WeaponsDatabase>();
        Tools = terrain.Player.GetComponentInChildren<ToolDatabase>();
        WepSwitch = terrain.Player.GetComponentInChildren<WeaponSwitch>();
        Armors = terrain.Player.GetComponentInChildren<ArmorDatabase>();
        Stats = terrain.Player.GetComponentInChildren<CharacterStats>();
        choptree = terrain.Player.GetComponentInChildren<ChopTrees>();
        minerocks = terrain.Player.GetComponentInChildren<MineRocks>();
        pickingherbs = terrain.Player.GetComponentInChildren<Herbloring>();
        GeneralSkillsDB = terrain.Player.GetComponentInChildren<GeneralSkillsDatabase>();
        Pickup = terrain.Player.GetComponentInChildren<PickupObjects>();
        mainGUI = canvas.GetComponent<MainGUI>();
        movement = terrain.Player.GetComponentInChildren<CharacterMovement>();
        EleSkills = terrain.Player.GetComponentInChildren<ElementalSkills>();

        MetalworkingWindow = Instantiate(MetalworkingCraftingPrefab, new Vector2(Screen.width / 2, Screen.height / 4), transform.rotation) as GameObject;
        MetalworkingWindow.GetComponent<RectTransform>().localPosition = new Vector2(Screen.width / 3, Screen.height / 15);

        WoodworkingWindow = Instantiate(WoodworkingCraftingPrefab, new Vector2(Screen.width / 2, Screen.height / 4), transform.rotation) as GameObject;
        WoodworkingWindow.GetComponent<RectTransform>().localPosition = new Vector2(Screen.width / 3, Screen.height / 15);

        BlacksmithingWindow = Instantiate(BlacksmithingCraftingPrefab, new Vector2(Screen.width / 2, Screen.height / 4), transform.rotation) as GameObject;
        BlacksmithingWindow.GetComponent<RectTransform>().localPosition = new Vector2(Screen.width / 3, Screen.height / 15);

        PotionsWindow = Instantiate(PotionsCraftingPrefab, new Vector2(Screen.width / 2, Screen.height / 4), transform.rotation) as GameObject;
        PotionsWindow.GetComponent<RectTransform>().localPosition = new Vector2(Screen.width / 3, Screen.height / 15);

        mainGUI.SetActiveWindow(MetalworkingWindow.gameObject, 0, false, false);
        mainGUI.SetActiveWindow(WoodworkingWindow.gameObject, 0, false, false);
        mainGUI.SetActiveWindow(BlacksmithingWindow.gameObject, 0, false, false);
        mainGUI.SetActiveWindow(PotionsWindow.gameObject, 0, false, false);

        for (int x = 0; x < craftingDatabase.CraftingSkillList.Count; x++) // finish woodworking, armor, potionmaking
        {
            switch (craftingDatabase.CraftingSkillList[x].CraftingName)
            {
                case "Metalworking":

                    for (int t = 0; t < 4; t++)
                    {
                        int CraftingCount = t;
                        MetalworkingWindow.transform.Find("CraftButton" + CraftingCount.ToString()).GetComponentInChildren<Button>().onClick.AddListener(() => CraftingSwitch(CraftingCount, MetalworkingWindow));
                    }
                    for (int a = Tools.EndofPickAxeList; a < Tools.ToolList.Count; a++) // craftbench/furnace
                    {
                        int[] MatIndexes = new int[Tools.ToolList[a].ReferenceIndexToMat.Length];
                        for (int y = 0; y < MatIndexes.Length; y++)
                            MatIndexes[y] = Tools.ToolList[a].ReferenceIndexToMat[y]; // required material names
                        CreateCraftingwindow(MetalworkingWindow, "Metalworking", 0, a, Tools.ToolList[a].ToolName, craftingDatabase.CraftingSkillList[0].CurrentRank, Tools.ToolList[a].RequiredLevel, MatIndexes, Tools.ToolList[a].MatsAmounts, Tools.ToolSprites[a], new CreationDelegate(CreateTools));
                    }
                    for (int a = 0; a < Tools.EndofAxeList; a++)
                    {
                        int[] MatIndexes = new int[Tools.ToolList[a].ReferenceIndexToMat.Length];
                        for (int y = 0; y < MatIndexes.Length; y++)
                            MatIndexes[y] = Tools.ToolList[a].ReferenceIndexToMat[y]; // required material names
                        CreateCraftingwindow(MetalworkingWindow, "Metalworking", 1, a, Tools.ToolList[a].ToolName, craftingDatabase.CraftingSkillList[0].CurrentRank, Tools.ToolList[a].RequiredLevel, MatIndexes, Tools.ToolList[a].MatsAmounts, Tools.ToolSprites[a], new CreationDelegate(CreateTools));
                    }
                    for (int a = Tools.EndofAxeList; a < Tools.EndofPickAxeList; a++)
                    {
                        int[] MatIndexes = new int[Tools.ToolList[a].ReferenceIndexToMat.Length];
                        for (int y = 0; y < MatIndexes.Length; y++)
                            MatIndexes[y] = Tools.ToolList[a].ReferenceIndexToMat[y]; // required material names
                        CreateCraftingwindow(MetalworkingWindow, "Metalworking", 2, a, Tools.ToolList[a].ToolName, craftingDatabase.CraftingSkillList[0].CurrentRank, Tools.ToolList[a].RequiredLevel, MatIndexes, Tools.ToolList[a].MatsAmounts, Tools.ToolSprites[a], new CreationDelegate(CreateTools));
                    }
                    for (int a = MiscItems.EndlengthOres + 1; a < MiscItems.EndlengthBars + 1; a++) // bars
                    {
                        int[] MatIndexes = new int[MiscItems.Miscellaneousitems[a].ReferenceIndexToMat.Length];
                        for (int y = 0; y < MatIndexes.Length; y++)
                            MatIndexes[y] = MiscItems.Miscellaneousitems[a].ReferenceIndexToMat[y];
                        CreateCraftingwindow(MetalworkingWindow, "Metalworking", 3, a, MiscItems.Miscellaneousitems[a].MiscellaneousItemName, craftingDatabase.CraftingSkillList[0].CurrentRank, MiscItems.Miscellaneousitems[a].LevelRank, MatIndexes, MiscItems.Miscellaneousitems[a].MatsAmounts, MiscItems.MiscellaneousSprites[a], new CreationDelegate(CreateMiscellaneous));
                    }
                    break;
                case "Woodworking":

                    for (int t = 0; t < 3; t++)
                    {
                        int CraftingCount = t;
                        WoodworkingWindow.transform.Find("CraftButton" + CraftingCount.ToString()).GetComponentInChildren<Button>().onClick.AddListener(() => CraftingSwitch(CraftingCount, WoodworkingWindow));
                    }
                    for (int a = 0; a < Armors.WoodHoodEndLength; a++) // for all crafting involving wood
                    {
                        int[] MatIndexes = new int[Armors.ArmorList[a].ReferenceIndexToMat.Length];
                        for (int y = 0; y < MatIndexes.Length; y++)
                            MatIndexes[y] = Armors.ArmorList[a].ReferenceIndexToMat[y];
                        CreateCraftingwindow(WoodworkingWindow, "Woodworking", 2, a, Armors.ArmorList[a].ArmorName, craftingDatabase.CraftingSkillList[1].CurrentRank, Armors.ArmorList[a].LevelRank, MatIndexes, Armors.ArmorList[a].MatsAmounts, Armors.ArmorSprites[a], new CreationDelegate(CreateArmors));
                    }
                    for (int a = Armors.WoodHoodEndLength; a < Armors.WoodRobeEndLength; a++)
                    {
                        int[] MatIndexes = new int[Armors.ArmorList[a].ReferenceIndexToMat.Length];
                        for (int y = 0; y < MatIndexes.Length; y++)
                            MatIndexes[y] = Armors.ArmorList[a].ReferenceIndexToMat[y];
                        CreateCraftingwindow(WoodworkingWindow, "Woodworking", 1, a, Armors.ArmorList[a].ArmorName, craftingDatabase.CraftingSkillList[1].CurrentRank, Armors.ArmorList[a].LevelRank, MatIndexes, Armors.ArmorList[a].MatsAmounts, Armors.ArmorSprites[a], new CreationDelegate(CreateArmors));
                    }
                    break;
                case "Blacksmithing":

                    for (int t = 0; t < 1; t++)
                    {
                        int CraftingCount = t;
                        BlacksmithingWindow.transform.Find("CraftButton" + CraftingCount.ToString()).GetComponentInChildren<Button>().onClick.AddListener(() => CraftingSwitch(CraftingCount, BlacksmithingWindow));
                    }

                    //for (int a = 0; a < Weapons.EndOfChakramList; a++)
                    //{
                    //    int[] MatIndexes = new int[Weapons.WeaponList[a].ReferenceIndexToMat.Length];
                    //    for (int y = 0; y < MatIndexes.Length; y++)
                    //        MatIndexes[y] = Weapons.WeaponList[a].ReferenceIndexToMat[y];
                    //    CreateCraftingwindow(BlacksmithingWindow, "Blacksmithing", 1, a, Weapons.WeaponList[a].WeaponName, craftingDatabase.CraftingSkillList[2].CurrentRank, Weapons.WeaponList[a].LevelRank, MatIndexes, Weapons.WeaponList[a].MatsAmounts, Weapons.WeaponSprites[a], new CreationDelegate(CreateWeapon));
                    //}
                    for (int a = 0; a < Weapons.EndofMetalStaff; a++)
                    {
                        int[] MatIndexes = new int[Weapons.WeaponList[a].ReferenceIndexToMat.Length];
                        for (int y = 0; y < MatIndexes.Length; y++)
                            MatIndexes[y] = Weapons.WeaponList[a].ReferenceIndexToMat[y];
                        CreateCraftingwindow(BlacksmithingWindow, "Blacksmithing", 0, a, Weapons.WeaponList[a].WeaponName, craftingDatabase.CraftingSkillList[2].CurrentRank, Weapons.WeaponList[a].LevelRank, MatIndexes, Weapons.WeaponList[a].MatsAmounts, Weapons.WeaponSprites[a], new CreationDelegate(CreateWeapon));
                    }
                    break;
                case "Potion Crafting":

                    for (int t = 0; t < 2; t++)
                    {
                        int CraftingCount = t;
                        PotionsWindow.transform.Find("CraftButton" + CraftingCount.ToString()).GetComponentInChildren<Button>().onClick.AddListener(() => CraftingSwitch(CraftingCount, PotionsWindow));
                    }

                    for (int a = MiscItems.EndLengthStamHerbs + 1; a < MiscItems.EndLengthVials + 1; a++) // vials/vial of water
                    {
                        int[] MatIndexes = new int[MiscItems.Miscellaneousitems[a].ReferenceIndexToMat.Length];
                        for (int y = 0; y < MatIndexes.Length; y++)
                            MatIndexes[y] = MiscItems.Miscellaneousitems[a].ReferenceIndexToMat[y];
                        CreateCraftingwindow(PotionsWindow, "Potion Crafting", 0, a, MiscItems.Miscellaneousitems[a].MiscellaneousItemName, craftingDatabase.CraftingSkillList[3].CurrentRank, MiscItems.Miscellaneousitems[a].LevelRank, MatIndexes, MiscItems.Miscellaneousitems[a].MatsAmounts, MiscItems.MiscellaneousSprites[a], new CreationDelegate(CreateMiscellaneous));
                        CreateCraftingwindow(PotionsWindow, "Potion Crafting", 1, a, MiscItems.Miscellaneousitems[a].MiscellaneousItemName, craftingDatabase.CraftingSkillList[3].CurrentRank, MiscItems.Miscellaneousitems[a].LevelRank, MatIndexes, MiscItems.Miscellaneousitems[a].MatsAmounts, MiscItems.MiscellaneousSprites[a], new CreationDelegate(CreateMiscellaneous));
                    }
                    for (int a = 0; a < Potions.EndLengthHealthPotion; a++)
                    {
                        int[] MatIndexes = new int[Potions.PotionList[a].ReferenceIndexToMat.Length];
                        for (int y = 0; y < MatIndexes.Length; y++)
                            MatIndexes[y] = Potions.PotionList[a].ReferenceIndexToMat[y];
                        CreateCraftingwindow(PotionsWindow, "Potion Crafting", 0, a, Potions.PotionList[a].PotionName, craftingDatabase.CraftingSkillList[3].CurrentRank, Potions.PotionList[a].LevelRank, MatIndexes, Potions.PotionList[a].MatsAmounts, Potions.PotionSprites[a], new CreationDelegate(CreatePotions));
                    }
                    for (int a = Potions.EndLengthHealthPotion; a < Potions.EndLengthStaminaPotion; a++)
                    {
                        int[] MatIndexes = new int[Potions.PotionList[a].ReferenceIndexToMat.Length];
                        for (int y = 0; y < MatIndexes.Length; y++)
                            MatIndexes[y] = Potions.PotionList[a].ReferenceIndexToMat[y];
                        CreateCraftingwindow(PotionsWindow, "Potion Crafting", 1, a, Potions.PotionList[a].PotionName, craftingDatabase.CraftingSkillList[3].CurrentRank, Potions.PotionList[a].LevelRank, MatIndexes, Potions.PotionList[a].MatsAmounts, Potions.PotionSprites[a], new CreationDelegate(CreatePotions));
                    }
                    break;
            }
        }

        RectINIT = Instantiate(PickUpWindowSkillPrefab, transform.position, transform.rotation) as Image;
        RectINIT.transform.SetParent(terrain.canvas.transform);
        terrain.canvas.GetComponentInChildren<MainGUI>().SetActiveWindow(RectINIT.gameObject, 0, false, false);
        RectINIT.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
        RectINIT.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        HoverRectINIT = Instantiate(HoverGeneralSkillPrefab, transform.position, transform.rotation) as Image;
        HoverRectINIT.transform.SetParent(terrain.canvas.transform);
        HoverRectINIT.transform.localScale = new Vector3(1, 1, 1);
        HoverRectINIT.GetComponent<Image>().enabled = false;
        HoverRectINIT.transform.Find("Description").GetComponent<Text>().enabled = false;
        HoverRectINIT.transform.Find("Text").GetComponent<Text>().enabled = false;
    }

    void Update()
    {
        ButtonPressed();
    }
}
