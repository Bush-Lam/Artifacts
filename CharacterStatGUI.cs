using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CharacterStatGUI : MonoBehaviour, IPointerDownHandler
{
	TerrainScript terrain;
	CharacterStats Stats;
	public RectTransform CharacterStatsValues;

	Transform[] ArrayofStats;

	public void OnPointerDown(PointerEventData data)
	{
		transform.SetAsLastSibling();
	}

	void SetStats()
	{
		for (int i =0; i < ArrayofStats.Length;i++)
		{
			ArrayofStats[0].GetComponentInChildren<Text>().text = Stats.PlayerName;
			ArrayofStats[1].GetComponentInChildren<Text>().text = Stats.PlayerLevel.ToString();
			ArrayofStats[2].GetComponentInChildren<Text>().text = (Stats.PlayerExperience + "/" + Stats.ExpRequired).ToString();
			ArrayofStats[3].GetComponentInChildren<Text>().text = (Mathf.Round(Stats.CurrentPlayerHealth) + "/" + Mathf.Round(Stats.PlayerHealth)).ToString();
			ArrayofStats[4].GetComponentInChildren<Text>().text = (Mathf.Round(Stats.CurrentPlayerStamina) + "/" + Mathf.Round(Stats.PlayerStamina)).ToString();
			ArrayofStats[5].GetComponentInChildren<Text>().text = Stats.TotalWeaponDamage.ToString();
			ArrayofStats[6].GetComponentInChildren<Text>().text = Stats.AttackSpeed.ToString();
            ArrayofStats[7].GetComponentInChildren<Text>().text = Stats.ArmorPenetration.ToString();
            ArrayofStats[8].GetComponentInChildren<Text>().text = Stats.CritRate.ToString() + "%";
			ArrayofStats[9].GetComponentInChildren<Text>().text = (Stats.Defense).ToString();
            ArrayofStats[10].GetComponentInChildren<Text>().text = Stats.MovementSpeed.ToString();
        }
	}

	void Start () 
	{
		terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
		Stats = terrain.Player.GetComponentInChildren<CharacterStats>();
		ArrayofStats = new Transform[11];

		for (int i =0; i < ArrayofStats.Length; i++)
		{
			ArrayofStats[i] = CharacterStatsValues.Find("Button" + i.ToString());
		}
	
	}

	void Update () 
	{
		SetStats();
	}
}
