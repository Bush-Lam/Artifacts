using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttachWeaponOrArmor : Photon.MonoBehaviour
{
    ArmorSwitch armorswitch; // thisrenderer
    TerrainScript terrain;

    [PunRPC]
    public void AttachWeapon(int viewid, int PlayerObj)
    {
        GameObject obj = PhotonView.Find(viewid).gameObject;
        GameObject PObj = PhotonView.Find(PlayerObj).gameObject;
        
        for (int x = 0; x < armorswitch.thisRenderer.bones.Length; x++)
        {
            for (int i = 0; i < obj.GetComponentInChildren<SkinnedMeshRenderer>().bones.Length; i++)
            {
                if (obj.GetComponentInChildren<SkinnedMeshRenderer>().bones[i].name == PObj.GetComponent<ArmorSwitch>().thisRenderer.bones[x].name)
                {
                    obj.GetComponentInChildren<SkinnedMeshRenderer>().bones[i].parent = PObj.GetComponent<ArmorSwitch>().thisRenderer.bones[x].transform;
                    obj.GetComponentInChildren<SkinnedMeshRenderer>().bones[i].transform.localPosition = new Vector3(0, 0, 0);
                    obj.GetComponentInChildren<SkinnedMeshRenderer>().bones[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }
    }

    void Start()
    {
        terrain = GameObject.FindGameObjectWithTag("MainEnvironment").GetComponent<TerrainScript>();
        armorswitch = terrain.Player.GetComponent<ArmorSwitch>();
    }

    void Update()
    {
    }
}
