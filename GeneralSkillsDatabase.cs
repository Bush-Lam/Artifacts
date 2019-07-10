using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneralSkillsDatabase : MonoBehaviour
{
	public GameObject[] GeneralSkillsPrefab;
	public GameObject[] GeneralSkills;

    public GameObject[] FireSkillsPrefab;
    public GameObject[] FireSkills;

    public GameObject[] IceSkillsPrefab;
    public GameObject[] IceSkills;

    public GameObject[] LightningSkillsPrefab;
    public GameObject[] LightningSkills;

    public GameObject[] NatureSkillsPrefab;
    public GameObject[] NatureSkills;

    public GameObject[] WeaponParticles;

    public GameObject CurrentSkillParticle; // particles for the skill

	TerrainScript terrain;
	CharacterStats Stats;
    WeaponSwitch wepswitch;

	public List<Sprite> GeneralSkillsSprites;
    public List<Sprite> FireSkillsSprites;
    public List<Sprite> IceSkillsSprites;
    public List<Sprite> LightningSkillsSprites;
    public List<Sprite> NatureSkillsSprites;

    // 60 seconds - 0.017f 30 seconds - 0.033f 15 seconds - 0.067 45 - 0.02f
    //1 / 60                    1 / 30              1/15
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////buff	        //procchance	//value1         //value2	     //value3	  //value4      //value5    //hp            //stam	        //dur		        //cd
    GeneralSkillCreation    GSkill1 		= new GeneralSkillCreation		("Stamina Recovery",				1,		false,		false,		    0,			    0, 		         0,              0,           0,            0,           0,              0,		    	0,				    0.2f,			0,	        15,    	1);
	GeneralSkillCreation 	GSkill2 		= new GeneralSkillCreation		("Health Recovery",					2, 		false,		false,		    0,			    0,		         0,              0,           0,            0,           0,              0,		    	0,				    0.017f,			0,	        15,    	1);
    GeneralSkillCreation 	GSkill3 		= new GeneralSkillCreation		("Attack Power",				    3, 		false,		false,		    0,			    0,		         0,              0,           0,            0,           0,              0,             0,				    0,			    0,	        40,    	1);    
    GeneralSkillCreation 	GSkill4 		= new GeneralSkillCreation		("Defense",				            4, 		false,      false,		    0,			    0,               0,              0,           0,            0,           0,              0,			    0,				    0,			    0,	        30,    	1);
    GeneralSkillCreation 	GSkill5 		= new GeneralSkillCreation		("Critical Strike",					5, 		false,		false,		    0,			    0,  	         0,              0,           0,            0,           0,              0,			    0,				    0,			    0,	        30,    	1);
    GeneralSkillCreation 	GSkill6 		= new GeneralSkillCreation		("Armor Penetration",				6, 		false,		false,		    0,			    0,  	         0,              0,           0,            0,           0,              0,			    0,				    0,			    0,	        25,    	1);
    GeneralSkillCreation    GSkill7 		= new GeneralSkillCreation	    ("Attack Speed",		            7, 		false,      false,		    0,			    0,               0,              0,           0,            0,           0,              0,			    0,				    0,			    0,	        40,    	1);

    GeneralSkillCreation    FSkill1 		= new GeneralSkillCreation	    ("Fire Strike",		                8, 		false,      false,          0,  	        0.05f,           0,              0,           0,            0,           0,              0.1f,          0,                  0,			    1,	        35,    	1);

    GeneralSkillCreation    ISkill1 		= new GeneralSkillCreation	    ("Ice Strike",		                9, 		false,      false,		    0,	            1f,              0,              0,           0,            0,           0,              0.1f,          0,                  0,			    1,	        35,    	1);

    GeneralSkillCreation    LSkill1         = new GeneralSkillCreation	    ("Lightning Strike",		        10, 	false,      false,		    0,	            0.05f,           0,              0,           0,            0,           0,              0.1f,          0,                  0,			    1,	        35,    	1);
    
    GeneralSkillCreation    NSkill1         = new GeneralSkillCreation	    ("Earth Strike",		            11, 	false,      false,		    0,	            0.05f,           0,              0,           0,            0,           0,              0.1f,          0,                  0,			    1,	        35,    	1);

    //charged strike - can also crit;
    //Weapon Masteries?
    //Fire Enhancement = Chance to burn the target 
    //Lightning Enhancement = Increased movement --- make this faster
    //Ice Enhancement = Blocks a Percent of Damage

    public List<GeneralSkillCreation> GeneralSkillList = new List<GeneralSkillCreation>();
    public List<GeneralSkillCreation> FireSkillList = new List<GeneralSkillCreation>();
    public List<GeneralSkillCreation> IceSkillList = new List<GeneralSkillCreation>();
    public List<GeneralSkillCreation> LightningSkillList = new List<GeneralSkillCreation>();
    public List<GeneralSkillCreation> NatureSkillList = new List<GeneralSkillCreation>();

    void Awake() 
	{

		Stats = gameObject.GetComponentInChildren<CharacterStats>();
        wepswitch = gameObject.GetComponentInChildren<WeaponSwitch>();

		GeneralSkillList.Add(GSkill1);
		GeneralSkillList.Add(GSkill2);
        GeneralSkillList.Add(GSkill3);
        GeneralSkillList.Add(GSkill4);
        GeneralSkillList.Add(GSkill5);
        GeneralSkillList.Add(GSkill6);
        GeneralSkillList.Add(GSkill7);
        FireSkillList.Add(FSkill1);
        IceSkillList.Add(ISkill1);
        LightningSkillList.Add(LSkill1);
        NatureSkillList.Add(NSkill1);

        GeneralSkills = new GameObject[GeneralSkillsPrefab.Length];
        FireSkills = new GameObject[FireSkillsPrefab.Length];
        IceSkills = new GameObject[IceSkillsPrefab.Length];
        LightningSkills = new GameObject[LightningSkillsPrefab.Length];
        NatureSkills = new GameObject[NatureSkillsPrefab.Length];
    }

	public IEnumerator StaminaRecovery()
	{
		GeneralSkills[0] = PhotonNetwork.Instantiate(GeneralSkillsPrefab[0].name, transform.position, GeneralSkillsPrefab[0].transform.rotation,0)
			as GameObject;
		GeneralSkills[0].GetComponent<ParticleSystem>().Play();
		GeneralSkills[0].transform.parent = this.transform;
		Stats.CurrentPlayerStamina += Stats.PlayerStamina *
			GeneralSkillList[0].SkillValue[0];
		GeneralSkillList[0].IsSkillOn = false;
		yield return new WaitForSeconds(3);
		PhotonNetwork.Destroy(GeneralSkills[0].gameObject);
	}

	public IEnumerator HealthRecovery()
	{
		GeneralSkills[1] = PhotonNetwork.Instantiate(GeneralSkillsPrefab[1].name, transform.position, GeneralSkillsPrefab[1].transform.rotation,0)
			as GameObject;
		GeneralSkills[1].GetComponent<ParticleSystem>().Play();
		GeneralSkills[1].transform.parent = this.transform;
		Stats.CurrentPlayerHealth += Stats.PlayerHealth *
			GeneralSkillList[1].SkillValue[0];
		GeneralSkillList[1].IsSkillOn = false;
		yield return new WaitForSeconds(3);
		PhotonNetwork.Destroy(GeneralSkills[1].gameObject);
	}

    void Update()
    {

    }
}

public class GeneralSkillCreation
{
	
	public string SkillName;
	public int SkillID;
	public bool IsSkillOn;
	public bool IsABuff;
	public float SkillChance;
	public float[] SkillValue = new float[5];
    public float HealthCost;
    public float StaminaCost;
	public float Duration;
	public float CoolDown;
	public int LevelRank;
    public int MaxRank;
	public int SkillPointsRequired;
	
	public GeneralSkillCreation(string name, int id, bool on,  bool buff, float chance, float value1, float value2, float value3, float value4, float value5, float hp, float stam, float dur, float cd, int levelrank, int maxrank, int sp)
	{
		SkillName = name;
		SkillID = id;
		IsSkillOn = on;
		IsABuff = buff;
		SkillChance = chance;
        //values are not set in stone - could be anything
		SkillValue[0] = value1; // dmg/main value
        SkillValue[1] = value2; // atkspeed
        SkillValue[2] = value3; // crit
        SkillValue[3] = value4; // arpen
        SkillValue[4] = value5; // other

        HealthCost = hp;
        StaminaCost = stam;
		Duration = dur;
		CoolDown = cd;
		LevelRank = levelrank;
        MaxRank = maxrank;
		SkillPointsRequired = sp;
	}
}