using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterStats : Photon.MonoBehaviour {

	public string PlayerName;
	public float PlayerHealth; // total health when adding up all bonuses
    public float tempHealth; // our base health
    public float tempStamina; // our base stam
	public float CurrentPlayerHealth;
	public float PlayerStamina; // total stam when adding up all bonuses
	public float CurrentPlayerStamina;
    // wearing heavy armor increases hunger? which is metal armor 
    public float CurrentHunger; // when hunger is == 0 == decrease health
    // wearing heavy armor increases thirst? which is metal armor  
    public float CurrentThirst; // when thirst is == 0 == decrease stam

    public float Defense;
    public float DefenseBuff;
    public float CritRate;
    public float CritDamage;
    public float MovementSpeed;
    public float MovementSpeedBuff;
    public float AttackSpeed;
    public float ArmorPenetration; // 1 armor pen = 1% armor reduction capped at 100% // might be a lil overpowered

	public float MainWeaponDamage;
    public float MainWeaponAttackSpeed;
    public float MainWeaponCrit;
    public float MainWeaponArmorPen;
    public float SecondaryWeaponDamage;

    public float HelmetDefense;
    public float ChestDefense;
    public float LegDefense;
    public float HelmetHeath;
    public float ChestHealth;
    public float LegHealth;
    public float HelmetStamina;
    public float ChestStamina;
    public float LegStamina;

    public float TotalWeaponDamage;
	public float RNGWeaponDamage;

	public float PlayerExperience;
	public float PlayerLevel; // max level - 70 - 10 levels per Game level

	public float SkillPoints; // 2 sp per level
	
	public float ExpRequired;

    GeneralSkillsDatabase GeneralDB;

    [PunRPC]
    void SetpLevel(int playerID, float Level)
    {
        GameObject obj = PhotonView.Find(playerID).gameObject;
        obj.GetComponent<CharacterStats>().PlayerLevel = Level;
    }

    IEnumerator RegenPerFive()
	{
        yield return new WaitForSeconds(1);
        if (CurrentPlayerStamina < PlayerStamina)
		{
			CurrentPlayerStamina += tempStamina * 0.01f;
			StopCoroutine("RegenPerFive");
		}
		if (CurrentPlayerStamina > PlayerStamina)
		{
			CurrentPlayerStamina = PlayerStamina;
		}
        if (CurrentPlayerHealth < PlayerHealth)
        {
            CurrentPlayerHealth += tempHealth * 0.001f;
            StopCoroutine("RegenPerFive");
        }
        if (CurrentPlayerHealth > PlayerHealth)
        {
            CurrentPlayerHealth = PlayerHealth;
        }
        if (CurrentHunger >= 1)
        {
            CurrentHunger -= 0.1f;
        }
        if (CurrentThirst >= 1)
        {
            CurrentThirst -= 0.2f;
        }

        StartCoroutine("RegenPerFive");
    }

	void SetStats()
	{
        TotalWeaponDamage = 0;
        TotalWeaponDamage = ((MainWeaponDamage + SecondaryWeaponDamage) + ((MainWeaponDamage + SecondaryWeaponDamage) * GeneralDB.GeneralSkillList[2].SkillValue[0]));
        CritRate = MainWeaponCrit + GeneralDB.GeneralSkillList[4].SkillValue[0];
        ArmorPenetration = MainWeaponArmorPen + GeneralDB.GeneralSkillList[5].SkillValue[0];
        AttackSpeed = MainWeaponAttackSpeed + GeneralDB.GeneralSkillList[6].SkillValue[0];
        Defense = Mathf.Round(HelmetDefense + ChestDefense + LegDefense + GeneralDB.GeneralSkillList[3].SkillValue[0] + DefenseBuff * (HelmetDefense + ChestDefense + LegDefense + GeneralDB.GeneralSkillList[3].SkillValue[0]));         
        PlayerHealth = tempHealth + Mathf.Round(HelmetHeath * tempHealth) + Mathf.Round(ChestHealth * tempHealth) + Mathf.Round(LegHealth * tempHealth);
        PlayerStamina = tempStamina + Mathf.Round(HelmetStamina * tempStamina) + Mathf.Round(ChestStamina * tempStamina) + Mathf.Round(LegStamina * tempStamina);
        MovementSpeed = 4 + (4 * MovementSpeedBuff);

        //RNGWeaponDamage = Mathf.Round(Random.Range(TotalWeaponDamage - TotalWeaponDamage / 15, TotalWeaponDamage + TotalWeaponDamage / 15)) + 700;
        RNGWeaponDamage = Mathf.Round(Random.Range(TotalWeaponDamage-TotalWeaponDamage/15, TotalWeaponDamage+TotalWeaponDamage/15)); // testing purposes
	}

	void LevelUp()
	{
        if (PlayerExperience >= ExpRequired)
		{
            if (PlayerExperience > ExpRequired)
                PlayerExperience = PlayerExperience - ExpRequired;

            PlayerLevel++;
            photonView.RPC("SetpLevel", PhotonTargets.OthersBuffered, gameObject.GetPhotonView().viewID, PlayerLevel);
            ExpRequired = Mathf.Pow(PlayerLevel, 2) + 30;
            tempHealth = 500 + (100 * (PlayerLevel / 2) + 250 * (PlayerLevel / 5));
            tempStamina = 300 + (50 * (PlayerLevel / 2) + 100 * (PlayerLevel / 5));
			SkillPoints += 1;

			CurrentPlayerHealth = tempHealth; 
			CurrentPlayerStamina = tempStamina;
		}
	}

	// Use this for initialization
	void Start () 
	{
        GeneralDB = gameObject.GetComponentInChildren<GeneralSkillsDatabase>();
        PlayerLevel = 1;
        ExpRequired = Mathf.Pow(PlayerLevel, 2.1f) + 25;
        tempHealth = 500 + (100 * (PlayerLevel / 2) + 250 * (PlayerLevel / 5));
        tempStamina = 300 + (50 * (PlayerLevel / 2) + 100 * (PlayerLevel / 5));
        CurrentPlayerHealth = tempHealth;
		CurrentPlayerStamina = tempStamina;
        CurrentHunger = 100;
        CurrentThirst = 100;
        MovementSpeed = 1;
        CritDamage = 1;
        ArmorPenetration = 1;
        Defense = 1;
        MovementSpeedBuff = 0;
        DefenseBuff = 0;
        //PlayerExperience += 10000; // testing
        StartCoroutine("RegenPerFive");
    }

    // Update is called once per frame
    void Update () 
	{
		LevelUp();
		SetStats();
    }
}
