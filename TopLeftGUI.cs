using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TopLeftGUI : MonoBehaviour 
{

	TerrainScript terrain;
	CharacterStats Stats;

	public Image CurrentPlayerHealth;
	public Image CurrentPlayerStam;
    public Image CurrentHunger;
    public Image CurrentThirst;
	public Image CurrentPlayerExp;

	public Text PlayerHpText;
	public Text PlayerStamText;
    public Text PlayerHungerText;
    public Text PlayerThirstText;
    public Text PlayerExpText;


	float PlayerHealthNormals;
	float PlayerStamNormals;
    float PlayerHungerNormals;
    float PlayerThirstNormals;
	float PlayerExpNormals;

	void TopGUI()
	{
		PlayerHpText.text = "Health: " + Mathf.Round(Stats.CurrentPlayerHealth) + "/" + Mathf.Round(Stats.PlayerHealth);
		PlayerStamText.text = "Stamina: " + Mathf.Round(Stats.CurrentPlayerStamina) + "/" + Mathf.Round(Stats.PlayerStamina);
        PlayerHungerText.text = "Hunger: " + Mathf.Round(Stats.CurrentHunger) + "/" + 100;
        PlayerThirstText.text = "Thirst: " + Mathf.Round(Stats.CurrentThirst) + "/" + 100;
        PlayerExpText.text = "Exp: " + Mathf.Round(Stats.PlayerExperience) + "/" +Mathf.Round( Stats.ExpRequired);
		
		PlayerHealthNormals = Stats.CurrentPlayerHealth / Stats.PlayerHealth;
		PlayerStamNormals = Stats.CurrentPlayerStamina / Stats.PlayerStamina;
        PlayerHungerNormals = Stats.CurrentHunger / 100;
        PlayerThirstNormals = Stats.CurrentThirst / 100;
        PlayerExpNormals = Stats.PlayerExperience / Stats.ExpRequired;
		
		CurrentPlayerHealth.fillAmount = PlayerHealthNormals;
		CurrentPlayerStam.fillAmount = PlayerStamNormals;
        CurrentHunger.fillAmount = PlayerHungerNormals;
        CurrentThirst.fillAmount = PlayerThirstNormals;
        CurrentPlayerExp.fillAmount = PlayerExpNormals;
	}

	void Start()
	{
		terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
		Stats = terrain.Player.GetComponentInChildren<CharacterStats>();
	}

	void Update()
	{
		TopGUI();
	}

}
