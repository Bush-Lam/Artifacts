using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChopTrees : Photon.MonoBehaviour
{

    int GameObjectLogIndex;

    private int[] TreeAmounts = { 601, 1201, 1801, 2401, 3001, 3601, 4201, 4501, 4801 };

    public bool ChopAnimation;
    public TextMesh ExpPrefab;
    CharacterMovement movement;
    CharacterInventoryGUI characterInventory;
    MiscellaneousItemsDatabase MiscItems;
    GatheringSkillDatabase GatheringSkill;
    WeaponSwitch ItemID;
    ToolDatabase ToolID;
    CharacterStats Stats;
    PickupObjects ItemPickup;
    MainGUI mainGUI;

    RaycastHit hit;
    Vector3 Forward;

    TerrainScript terrain;

    void Start()
    {
        movement = gameObject.GetComponentInChildren<CharacterMovement>();
        GatheringSkill = gameObject.GetComponentInChildren<GatheringSkillDatabase>();
        ItemID = gameObject.GetComponentInChildren<WeaponSwitch>();
        ToolID = gameObject.GetComponentInChildren<ToolDatabase>();
        MiscItems = gameObject.GetComponentInChildren<MiscellaneousItemsDatabase>();
        terrain = GameObject.FindGameObjectWithTag("MainEnvironment").GetComponent<TerrainScript>();
        Stats = gameObject.GetComponentInChildren<CharacterStats>();
        ItemPickup = gameObject.GetComponentInChildren<PickupObjects>();
        characterInventory = terrain.canvas.GetComponentInChildren<CharacterInventoryGUI>();
        mainGUI = terrain.canvas.GetComponentInChildren<MainGUI>();
    }

    IEnumerator CheckMovement()
    {
        if (movement.PlayerVelocity.magnitude > 3)
        {
            movement.NetworkSetHandAnimations("Gathering", 0);
            StopCoroutine("ChoppingAnimation");
            mainGUI.timeElapsedProgressBar = Time.time + 1;
            mainGUI.CheckMovementProgressBar = true;
            yield return new WaitForSeconds(1);
            mainGUI.CheckMovementProgressBar = false;
        }
        else if (mainGUI.timeElapsedProgressBar > Time.time)
        {
            yield return new WaitForSeconds(0.02f); 
            StartCoroutine("CheckMovement");
        }
    }

    public IEnumerator ChoppingAnimation() // only chop down trees that are not environmental physics
    {
        for (int i = 0; i < terrain.TreeGameObjects.Length; i++)
        {
            Forward = transform.TransformDirection(Vector3.forward);

            if (Physics.Raycast(transform.position + new Vector3(0, 3, 0), Forward, out hit, 11))
            {
                if (hit.transform.name == terrain.TreeGameObjects[i].transform.name + "(Clone)"  &&
                    GatheringSkill.GatheringSkillList[0].CurrentRank >= MiscItems.Miscellaneousitems[i + MiscItems.EndlengthBars + 1].LevelRank &&
                    ((ItemID.CurrentItemId >= 750 && ItemID.CurrentItemId < 800) || ItemID.CurrentItemId == -1)) // correct woodcutting level and axe needs to be considered
                {
                    Stats.CurrentPlayerStamina -= GatheringSkill.GatheringSkillList[0].StaminaCost * Stats.PlayerStamina;

                    if (ItemID.CurrentItemId == -1) // punch
                    {
                        movement.NetworkSetHandAnimations("Gathering", 4);
                        mainGUI.timeElapsedProgressBar = 5.1f + Time.time;
                        StartCoroutine("CheckMovement"); // do this for all gathering animations, make a load bar for gathering ( 3 seconds)
                        mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.067f, terrain.TreeGameObjects[i].name)); 
                        yield return new WaitForSeconds(3.4f);
                        if (Random.Range(0, 100) > 80 - (((MiscItems.Miscellaneousitems[i + MiscItems.EndlengthBars + 1].LevelRank + 5) / 5) * 30) - (MiscItems.Miscellaneousitems[i + MiscItems.EndlengthBars + 1].LevelRank / 5)) // base no tool chance == 80%
                        {
                            mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("Failed to gather " + MiscItems.Miscellaneousitems[i + MiscItems.EndlengthBars + 1].MiscellaneousItemName + "."));
                            mainGUI.CheckMovementProgressBar = false;
                            yield return new WaitForSeconds(0.7f);
                            movement.NetworkSetHandAnimations("Gathering", 0);
                            yield break;
                        }
                    }
                    else 
                    {
                        movement.NetworkSetHandAnimations("Gathering", 1);
                        mainGUI.timeElapsedProgressBar = 5.1f + Time.time;
                        StartCoroutine("CheckMovement"); // do this for all gathering animations, make a load bar for gathering ( 3 seconds)
                        mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.067f, terrain.TreeGameObjects[i].name)); // 1 / 0.067f * 0.2 = 3 seconds
                        yield return new WaitForSeconds(3.4f);
                        if (Random.Range(0, 100) > ToolID.ToolList[(ItemID.CurrentItemId - 750)].ChanceToGather - (((MiscItems.Miscellaneousitems[i + MiscItems.EndlengthBars + 1].LevelRank + 5) / 5) * 30) - (MiscItems.Miscellaneousitems[i + MiscItems.EndlengthBars + 1].LevelRank / 5)) // ex : level 0 tree(maple) = -30% , level 1 axe = 115% chance to cut down, level 1 tree(oak) = -60%, -30% every level for trees, +30% for every level axe, -1% as difficulty(woodcutting) increases
                        {                         
                            mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("Failed to gather " + MiscItems.Miscellaneousitems[i + MiscItems.EndlengthBars + 1].MiscellaneousItemName + "."));
                            mainGUI.CheckMovementProgressBar = false;
                            yield return new WaitForSeconds(0.7f);
                            movement.NetworkSetHandAnimations("Gathering", 0);
                            yield break;
                        }
                    }

                    GatheringSkill.GatheringSkillList[0].CurrentExp += MiscItems.Miscellaneousitems[i + MiscItems.EndlengthBars + 1].MiscellaneousItemExp;
                    GameObjectLogIndex = i + MiscItems.EndlengthBars + 1;
                    GameObject LogInstantiate = PhotonNetwork.Instantiate(MiscItems.MiscellaneousGameObjects[GameObjectLogIndex].name, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z),
                                                                            MiscItems.MiscellaneousGameObjects[GameObjectLogIndex].transform.rotation, 0) as GameObject;
                    TextMesh ExpPrefabText = Instantiate(ExpPrefab, transform.position,
                                    transform.rotation) as TextMesh;
                    ExpPrefabText.text = ("+" + MiscItems.Miscellaneousitems[i + MiscItems.EndlengthBars + 1].MiscellaneousItemExp);
                    ExpPrefabText.characterSize = 0.04f;
                    ExpPrefabText.fontSize = 275;
                    ExpPrefabText.color = new Color32(34, 96, 34, 255);
                    ExpPrefabText.transform.parent = transform;
                    ExpPrefabText.transform.localPosition = Vector3.zero;
                    characterInventory.photonView.RPC("AddandRemoveDropList", PhotonTargets.AllBufferedViaServer, MiscItems.MiscellaneousGameObjects[GameObjectLogIndex].name, "Common", LogInstantiate.GetPhotonView().viewID,
                        0f, 0f, 0f, 0f, 0f, 0f, 0f, -1, 1, 1, 1, characterInventory.DroppedItemList.Count);

                    photonView.RPC("tScaleSize", PhotonTargets.All, hit.transform.gameObject.GetPhotonView().viewID, i);
                        
                    mainGUI.CheckMovementProgressBar = false;
                    yield return new WaitForSeconds(0.7f);
                    movement.NetworkSetHandAnimations("Gathering", 0);

                    yield break;
                }
            }
        }
        
    }

    [PunRPC]
    void tScaleSize(int ViewID, int Index) // make the intersection of log and upper part closed/filled so no white part shows
    {
        GameObject TreeObject = PhotonView.Find(ViewID).gameObject;
        //scale lower half of tree by 0.2 / scale of tree, subtract top part proportionally
        TreeObject.transform.Find("Cylinder").transform.localPosition = new Vector3(TreeObject.transform.Find("Cylinder").transform.localPosition.x,
                                                      TreeObject.transform.Find("Cylinder").transform.localPosition.y - (terrain.TreeGameObjects[Index].transform.Find("Cylinder_001").GetComponentInChildren<MeshRenderer>().bounds.size.y / (4.92f * TreeObject.transform.localScale.y)),
                                                      TreeObject.transform.Find("Cylinder").transform.localPosition.z);

        TreeObject.transform.Find("Cylinder_001").transform.localScale = new Vector3(TreeObject.transform.Find("Cylinder_001").transform.localScale.x,
                                                      TreeObject.transform.Find("Cylinder_001").transform.localScale.y,
                                                      TreeObject.transform.Find("Cylinder_001").transform.localScale.z - (0.2f / TreeObject.transform.localScale.y));

        if (TreeObject.transform.Find("Cylinder_001").transform.localScale.z <= 0.3f)
        {
            TreeObject.transform.localScale = Vector3.zero;
            StartCoroutine(RedoScaleSize(TreeObject, Index));
        }
    }

    IEnumerator RedoScaleSize(GameObject TreeObject, int i)
    {
        yield return new WaitForSeconds(5);
        float RandomScale = Random.Range(0.66f, 1.66f);
        TreeObject.transform.GetComponent<Rigidbody>().mass = 3000 * RandomScale;
        TreeObject.transform.localScale = new Vector3(RandomScale, RandomScale, RandomScale);
        TreeObject.transform.Find("Cylinder_001").transform.localScale = new Vector3(1, 1, 1);
        TreeObject.transform.Find("Cylinder").transform.localPosition = new Vector3(terrain.TreeGameObjects[i].transform.Find("Cylinder").localPosition.x,
            terrain.TreeGameObjects[i].transform.Find("Cylinder").localPosition.y, terrain.TreeGameObjects[i].transform.Find("Cylinder").localPosition.z);
        for (int j = 0; j < terrain.ListofEnvironmentalObjectsAltered.Count; i++)
        {
            if (TreeObject == terrain.ListofEnvironmentalObjectsAltered[j].gameObject) // wait until environmentalobjsundo happens
                yield return new WaitForSeconds(120);
        }
        TreeObject.transform.localScale = new Vector3(1, 1, 1);
    }

    void Update()
    {

    }
}
