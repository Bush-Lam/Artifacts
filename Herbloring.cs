using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Herbloring : Photon.MonoBehaviour 
{

	public int GameObjectHerbIndex;
	
	private int[] HerbPatchAmounts = {601,1201,1801,2401,2801,3401};

	public bool HerbloreAnimation;
    public TextMesh ExpPrefab;
    CharacterMovement movement;
    CharacterInventoryGUI characterInventory;
    MiscellaneousItemsDatabase MiscItems;
	GatheringSkillDatabase GatheringSkill;
	CharacterStats Stats;
	PickupObjects ItemPickup;
    MainGUI mainGUI;
    WeaponSwitch ItemID;

	RaycastHit hit;
    public int WaterID; // photonid of raycasted water

    TerrainScript terrain;
	
	void Start () 
	{
        movement = gameObject.GetComponentInChildren<CharacterMovement>();
        MiscItems = gameObject.GetComponentInChildren<MiscellaneousItemsDatabase>();
		GatheringSkill = gameObject.GetComponentInChildren<GatheringSkillDatabase>();
		terrain = GameObject.FindGameObjectWithTag("MainEnvironment").GetComponent<TerrainScript>();
		Stats = gameObject.GetComponentInChildren<CharacterStats>();
		ItemPickup = gameObject.GetComponentInChildren<PickupObjects>();
        characterInventory = terrain.canvas.GetComponentInChildren<CharacterInventoryGUI>();
        mainGUI = terrain.canvas.GetComponentInChildren<MainGUI>();
        ItemID = transform.GetComponentInChildren<WeaponSwitch>();
    }
    public bool CheckFillVialWithWater()
    {
        bool CheckWater = false;

        if (Physics.Raycast(transform.position + new Vector3(0,5,0), -transform.up, out hit, 15, 1 << 0)) // ignore player collider
        {
            if (hit.transform.name == "Water2(Clone)")
            {
                CheckWater = true;
                WaterID = hit.transform.gameObject.GetPhotonView().viewID;
            }
        }
        if (Physics.Raycast(transform.position + new Vector3(0, 5, 0), -transform.up, out hit, 15)) 
        {
            if (hit.transform.name == "MainTerrain")
            {
                CheckWater = false;
            }
        }

        return CheckWater;
    }

    [PunRPC]
    void WaterScale(int ViewID)
    {
        GameObject WaterObject = PhotonView.Find(ViewID).gameObject;
        WaterObject.transform.localScale = new Vector3(WaterObject.transform.localScale.x - 2,
                                                      WaterObject.transform.localScale.y - 2,
                                                      4.3f);
        if (WaterObject.transform.localScale.y <= 2 &&
            WaterObject.transform.localScale.x <= 2)
        {
            WaterObject.transform.localScale = Vector3.zero;
            StartCoroutine("RedoWaterScale", WaterObject);
        }
    }

    IEnumerator RedoWaterScale(GameObject WaterObject)
    {
        yield return new WaitForSeconds(120);
        float RandomScale = Random.Range(35, 40);
        WaterObject.transform.localScale = new Vector3(RandomScale, RandomScale, 4.3f);
    }

    IEnumerator CheckMovement()
    {
        if (movement.PlayerVelocity.magnitude > 3)
        {
            movement.NetworkSetHandAnimations("Gathering", 0);
            StopCoroutine("GatherAnimation");
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

    IEnumerator GatherAnimation()
	{
        Collider[] hColliders = Physics.OverlapSphere(transform.position, 1);

        if (ItemID.CurrentItemId == -1)
        {
            for (int j = 0; j < hColliders.Length; j++)
            {
                for (int i = 0; i < terrain.HerbPatchObjects.Length; i++)
                {
                    if (hColliders[j].transform.name == terrain.HerbPatchObjects[i].transform.name + "(Clone)" &&
                        GatheringSkill.GatheringSkillList[2].CurrentRank >= MiscItems.Miscellaneousitems[i + MiscItems.EndlengthLogs + 1].LevelRank)
                    {
                        movement.NetworkSetHandAnimations("Gathering", 3);
                        Stats.CurrentPlayerStamina -= GatheringSkill.GatheringSkillList[2].StaminaCost * Stats.PlayerStamina;
                        mainGUI.timeElapsedProgressBar = 3 + Time.time;
                        StartCoroutine("CheckMovement"); // do this for all gathering animations, make a load bar for gathering ( 3 seconds)
                        mainGUI.StartCoroutine(mainGUI.ActivateProgressBar(0.075f, terrain.HerbPatchObjects[i].name)); // 1 / 0.075 * 0.2 = 2.66 seconds       
                        yield return new WaitForSeconds(3); // change color of herb leaves to different colors, crafting/herbloring = no weapons/tools in hand, instantiate at start knife/hammer for crafting, make animations for em, put the resource for crafting item on the bench 

                        GatheringSkill.GatheringSkillList[2].CurrentExp += MiscItems.Miscellaneousitems[i].MiscellaneousItemExp;
                        GameObjectHerbIndex = i + MiscItems.EndlengthLogs + 1;
                        GameObject HerbInstantiate = PhotonNetwork.Instantiate(MiscItems.MiscellaneousGameObjects[GameObjectHerbIndex].name, transform.position + transform.forward * 2 + transform.up,
                                                                transform.rotation, 0) as GameObject;
                        TextMesh ExpPrefabText = Instantiate(ExpPrefab, transform.position,
                                        transform.rotation) as TextMesh;
                        ExpPrefabText.text = ("+" + MiscItems.Miscellaneousitems[i + MiscItems.EndlengthBars + 1].MiscellaneousItemExp);
                        ExpPrefabText.characterSize = 0.04f;
                        ExpPrefabText.fontSize = 275;
                        ExpPrefabText.color = new Color32(34, 96, 34, 255);
                        ExpPrefabText.transform.parent = transform;
                        ExpPrefabText.transform.localPosition = Vector3.zero;
                        characterInventory.photonView.RPC("AddandRemoveDropList", PhotonTargets.AllBufferedViaServer, MiscItems.MiscellaneousGameObjects[GameObjectHerbIndex].name, "Common", HerbInstantiate.GetPhotonView().viewID,
                            0f, 0f, 0f, 0f, 0f, 0f, 0f, -1, 1, 1, 1, characterInventory.DroppedItemList.Count);

                        photonView.RPC("HerbPatchScale", PhotonTargets.All, hColliders[j].transform.gameObject.GetPhotonView().viewID);
                        mainGUI.timeElapsedProgressBar = Time.time + 1;
                        movement.NetworkSetHandAnimations("Gathering", 0);
                        mainGUI.CheckMovementProgressBar = false;

                        yield break;
                    }
                }            
            }
        }
    }
	
	[PunRPC]
	void HerbPatchScale(int ViewID) 
	{
        Transform HerbPatchObject = PhotonView.Find(ViewID).transform;
        HerbPatchObject.transform.localScale = Vector3.zero;
        StartCoroutine(RedoHerbScale(HerbPatchObject.gameObject));
    }

	IEnumerator RedoHerbScale(GameObject HerbPatchObject)
	{
        yield return new WaitForSeconds(30);
        HerbPatchObject.transform.localScale = new Vector3(1, 1, 1);
    }
	
	void Update () 
	{

	}
}
