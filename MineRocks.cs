using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineRocks : Photon.MonoBehaviour 
{

	public int GameObjectOreIndex;

	private int[] RockAmounts = {601,1201,1801,2401,3001,3601,4201,4501,4801,5101};
    private int CurrentRockAmount;

	public bool miningAnimation;
    public TextMesh ExpPrefab;
    CharacterInventoryGUI characterInventory;
    CharacterMovement movement;
	MiscellaneousItemsDatabase MiscItems;
	GatheringSkillDatabase GatheringSkill;
	WeaponSwitch ItemID;
	ToolDatabase ToolID;
	CharacterStats Stats;
	PickupObjects ItemPickup;
    MonsterFunctions monsterFunctions;
    MainGUI mainGUI;

	RaycastHit hit;
	Vector3 Forward;

	TerrainScript terrain;

	void Start () 
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
        monsterFunctions = terrain.GetComponent<MonsterFunctions>();
        mainGUI = terrain.canvas.GetComponentInChildren<MainGUI>();
    }

    IEnumerator CheckMovement()
    {
        if (movement.PlayerVelocity.magnitude > 3)
        {
            movement.NetworkSetHandAnimations("Gathering", 0);
            StopCoroutine("MiningAnimation");
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

    IEnumerator MiningAnimation()
    {
        for (int i = 0; i < terrain.RockGameObjects.Length; i++)
        {
            if (Physics.Raycast(new Ray(transform.position + new Vector3(0, 0.2f, 0), transform.forward), out hit, 5, 1 << 0))
            {
                if (hit.transform.name == terrain.RockGameObjects[i].transform.name + "(Clone)" &&
                    GatheringSkill.GatheringSkillList[1].CurrentRank >= MiscItems.Miscellaneousitems[i].LevelRank &&
                    (ItemID.CurrentItemId >= 800 || ItemID.CurrentItemId == -1))
                {
                    Stats.CurrentPlayerStamina -= GatheringSkill.GatheringSkillList[1].StaminaCost * Stats.PlayerStamina;
                        
                    if (ItemID.CurrentItemId == -1) // punching - which can only be used for lvl 1 stuff.
                    {
                        movement.NetworkSetHandAnimations("Gathering", 4);
                        mainGUI.timeElapsedProgressBar = 5.1f + Time.time;
                        StartCoroutine("CheckMovement"); // do this for all gathering animations, make a load bar for gathering ( 3 seconds)
                        mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.067f, terrain.RockGameObjects[i].name));
                        yield return new WaitForSeconds(3.4f);
                        if (Random.Range(0, 100) > 80 - (((MiscItems.Miscellaneousitems[i].LevelRank + 5) / 5) * 30) - (MiscItems.Miscellaneousitems[i].LevelRank / 5)) // base no tool chance == 80%
                        {                            
                            mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("Failed to gather " + MiscItems.Miscellaneousitems[i].MiscellaneousItemName + "."));
                            mainGUI.CheckMovementProgressBar = false;
                            yield return new WaitForSeconds(0.7f);
                            movement.NetworkSetHandAnimations("Gathering", 0);
                            yield break;
                        }
                    }
                    else 
                    {
                        movement.NetworkSetHandAnimations("Gathering", 2);
                        mainGUI.timeElapsedProgressBar = 5.1f + Time.time;
                        StartCoroutine("CheckMovement"); // do this for all gathering animations, make a load bar for gathering ( 3 seconds)
                        mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.067f, terrain.RockGameObjects[i].name)); // 1 / 0.067f * 0.2 = 3 seconds
                        yield return new WaitForSeconds(3.4f);

                        if (Random.Range(0, 100) > ToolID.ToolList[(ItemID.CurrentItemId - 800 + ToolID.EndofAxeList)].ChanceToGather - (((MiscItems.Miscellaneousitems[i].LevelRank + 5) / 5) * 30) - (MiscItems.Miscellaneousitems[i].LevelRank / 5))  // ex : level 0 rock(stone) = -30% , level 1 axe = 115% chance to mine down, level 1 rock(stone) = -60%, -30% every level for trees, +30% for every level axe, -1% as difficulty(mining) increases
                        {
                            mainGUI.StartCoroutine(mainGUI.ActivateGeneralText("Failed to gather " + MiscItems.Miscellaneousitems[i].MiscellaneousItemName + "."));
                            mainGUI.CheckMovementProgressBar = false;
                            yield return new WaitForSeconds(0.7f);
                            movement.NetworkSetHandAnimations("Gathering", 0);
                            yield break;
                        }
                    }

                    GameObjectOreIndex = i;
                    GameObject OreInstantiate = PhotonNetwork.Instantiate(MiscItems.MiscellaneousGameObjects[GameObjectOreIndex].name, new Vector3(transform.position.x, transform.position.y + 2, transform.position.z),
                                                                            MiscItems.MiscellaneousGameObjects[GameObjectOreIndex].transform.rotation, 0) as GameObject;
                    GatheringSkill.GatheringSkillList[1].CurrentExp += MiscItems.Miscellaneousitems[i].MiscellaneousItemExp;
                    TextMesh ExpPrefabText = Instantiate(ExpPrefab, transform.position,
                                    transform.rotation) as TextMesh;
                    ExpPrefabText.text = ("+" + MiscItems.Miscellaneousitems[i].MiscellaneousItemExp.ToString());
                    ExpPrefabText.characterSize = 0.04f;
                    ExpPrefabText.fontSize = 275;
                    ExpPrefabText.color = new Color32(34, 96, 34, 255);
                    ExpPrefabText.transform.parent = transform;
                    ExpPrefabText.transform.localPosition = Vector3.zero;
                    characterInventory.photonView.RPC("AddandRemoveDropList", PhotonTargets.AllBufferedViaServer, MiscItems.MiscellaneousGameObjects[GameObjectOreIndex].name, "Common", OreInstantiate.GetPhotonView().viewID,
                        0f, 0f, 0f, 0f, 0f, 0f, 0f, -1, 1, 1, 1, characterInventory.DroppedItemList.Count);
                    photonView.RPC("rScaleSize", PhotonTargets.AllBuffered, hit.transform.gameObject.GetPhotonView().viewID);

                    mainGUI.CheckMovementProgressBar = false;
                    yield return new WaitForSeconds(0.7f);
                    movement.NetworkSetHandAnimations("Gathering", 0);

                    yield break;
                }
            }
        }  
    }

    [PunRPC]
	void rScaleSize(int ViewID)
	{
        GameObject RockObject = PhotonView.Find(ViewID).gameObject;

        RockObject.transform.localScale = new Vector3(RockObject.transform.localScale.x - 0.38f,
		                                              RockObject.transform.localScale.y - 0.38f, 
		                                              RockObject.transform.localScale.z - 0.38f);
        if (RockObject.transform.localScale.z <= 0.1f)
        {
            RockObject.transform.localScale = Vector3.zero;
            StartCoroutine("RedoScaleSize", RockObject);
        }
	}

    IEnumerator RedoScaleSize(GameObject RockObject)
    {
        yield return new WaitForSeconds(5);
        float RandomScale = Random.Range(0.66f, 1.66f);
        RockObject.transform.localScale = new Vector3(RandomScale, RandomScale, RandomScale);
        RockObject.transform.GetComponent<Rigidbody>().mass = 1500 * RandomScale;
    }

    void Update () 
	{

	}
}
