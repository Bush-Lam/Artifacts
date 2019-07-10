using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PickupObjects : Photon.MonoBehaviour 
{
	TerrainScript terrain;
	MiscellaneousItemsDatabase MiscDB;
	MineRocks RockDB;
	ChopTrees TreeDB;
	WeaponsDatabase WeaponDB;
	ToolDatabase ToolsDB;
	Herbloring HerbPatchDB;
	PotionDatabase PotionDB;
	CharacterInventoryGUI inventory;

	float PickUpItemCoolDown;

	void PickupStuff() // call this function in inventory; also destroys the picked up items in inventory
	{
		if (Time.time > PickUpItemCoolDown)
		{
			for (int y = 0; y < inventory.DroppedItemList.Count; y++)
			{
                if (Vector3.Distance(transform.position, inventory.DroppedItemList[y].tObject.transform.position) < 5)
                {
                    if (inventory.InventoryManage.Count > 24)
                    {
                        for (int i = 0; i < inventory.InventoryManage.Count; i++)
                        {
                            if (inventory.DroppedItemList[y].SlotName == inventory.InventoryManage[i].SlotName &&
                                inventory.DroppedItemList[y].Rarity == inventory.InventoryManage[i].Rarity &&
                               inventory.DroppedItemList[y].isASecondary == 1)
                            {
                                PickUpItemCoolDown = Time.time + 1;
                                inventory.AddtoInventoryDropped(inventory.DroppedItemList[y]);
                                terrain.canvas.GetPhotonView().RPC("AddandRemoveDropList", PhotonTargets.AllBufferedViaServer, inventory.DroppedItemList[y].SlotName, inventory.DroppedItemList[y].Rarity, inventory.DroppedItemList[y].tObject.GetPhotonView().viewID, inventory.DroppedItemList[y].DamageOrValue, inventory.DroppedItemList[y].WeaponAttackSpeed, inventory.DroppedItemList[y].CritRate, inventory.DroppedItemList[y].ArmorPenetration, inventory.DroppedItemList[y].Defense, inventory.DroppedItemList[y].Health, inventory.DroppedItemList[y].Stamina, inventory.DroppedItemList[y].CurrentInventorySlot, inventory.DroppedItemList[y].StackAmounts, inventory.DroppedItemList[y].isASecondary, 0, y);
                                return;
                            }
                        }
                    }
                    else
                    {
                        PickUpItemCoolDown = Time.time + 1;
                        inventory.AddtoInventoryDropped(inventory.DroppedItemList[y]);
                        terrain.canvas.GetPhotonView().RPC("AddandRemoveDropList", PhotonTargets.AllBufferedViaServer, inventory.DroppedItemList[y].SlotName, inventory.DroppedItemList[y].Rarity, inventory.DroppedItemList[y].tObject.GetPhotonView().viewID, inventory.DroppedItemList[y].DamageOrValue, inventory.DroppedItemList[y].WeaponAttackSpeed, inventory.DroppedItemList[y].CritRate, inventory.DroppedItemList[y].ArmorPenetration, inventory.DroppedItemList[y].Defense, inventory.DroppedItemList[y].Health, inventory.DroppedItemList[y].Stamina, inventory.DroppedItemList[y].CurrentInventorySlot, inventory.DroppedItemList[y].StackAmounts, inventory.DroppedItemList[y].isASecondary, 0, y);
                        return;
                    }              
                }                    
			}
			
		}
	}

	void Start ()
	{
		PickUpItemCoolDown = Time.time;

		RockDB = gameObject.GetComponentInChildren<MineRocks>();
		TreeDB = gameObject.GetComponentInChildren<ChopTrees>();
		MiscDB = gameObject.GetComponentInChildren<MiscellaneousItemsDatabase>();
		WeaponDB = gameObject.GetComponentInChildren<WeaponsDatabase>();
		ToolsDB = gameObject.GetComponentInChildren<ToolDatabase>();
		HerbPatchDB = gameObject.GetComponentInChildren<Herbloring>();
		PotionDB = gameObject.GetComponentInChildren<PotionDatabase>();
		terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
		inventory = terrain.canvas.GetComponentInChildren<CharacterInventoryGUI>();
	}
	
	void Update ()
	{
		if (Input.GetKey(terrain.canvas.GetComponentInChildren<MainGUI>().KeyBinds[14]) && photonView.isMine == true)
		{
            PickupStuff();
		}
	}
}

