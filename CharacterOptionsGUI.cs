using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterOptionsGUI : MonoBehaviour, IPointerDownHandler
{
	TerrainScript terrain;
    MainGUI mainGUI;
	public GameObject KeybindsPrefab;
	GameObject Keybinds;

	public InputField ResolutionX;
	public InputField ResolutionY;
	public Dropdown FullScreen;
    public Dropdown Quality;
    public Slider SoundLevel;
    public Slider MusicLevel;
    public Slider SensitivityLevel;

    public void OnPointerDown(PointerEventData data)
    {
        transform.SetAsLastSibling();
    }

	void SetActiveWindow(GameObject gameobject, bool SetFalse)
	{
		gameobject.SetActive(SetFalse);
	}

	public void OpenKeybinds()
	{
		Keybinds.SetActive(true);
	}

    public void OpenSaveAndLoadGame()
    {
        mainGUI.ActivateSaveAndLoadGUI(1, true, true);
    }

    public void SetSoundLevel()
    {
        mainGUI.SoundLevel = SoundLevel.value;
        PlayerPrefs.SetFloat("SoundLevel", SoundLevel.value);
        PlayerPrefs.Save();
        for (int i = 0; i < terrain.SoundEffects.Count; i++)
            terrain.SoundEffects[i].volume = mainGUI.SoundLevel;
    }

    public void SetMusicLevel()
    {
        mainGUI.MusicLevel = MusicLevel.value;
        PlayerPrefs.SetFloat("MusicLevel", MusicLevel.value);
        PlayerPrefs.Save();
        for (int i = 0; i < terrain.Musics.Count; i++)
            terrain.Musics[i].volume = mainGUI.MusicLevel;
    }

    public void SetSensitivity()
    {
        mainGUI.Sensitivity = SensitivityLevel.value / 6;
        PlayerPrefs.SetFloat("Sensitivity", SensitivityLevel.value / 6);
        PlayerPrefs.Save();
    }

    public void ApplyQuality()
    {
        QualitySettings.SetQualityLevel(Quality.value);
        PlayerPrefs.SetInt("Quality", Quality.value);
        PlayerPrefs.Save();
    }

	public void ApplyResolution()
	{
        bool fullScreen;
        if (FullScreen.value == 0)
            fullScreen = false;
        else
            fullScreen = true;
        if (ResolutionX.text == string.Empty)
            ResolutionX.text = Screen.width.ToString();
        else if (ResolutionY.text == string.Empty)
            ResolutionY.text = Screen.height.ToString();

        Screen.SetResolution(int.Parse(ResolutionX.text), int.Parse(ResolutionY.text), fullScreen);
        PlayerPrefs.SetInt("ResolutionX", int.Parse(ResolutionX.text));
        PlayerPrefs.SetInt("ResolutionY", int.Parse(ResolutionY.text));
        PlayerPrefs.SetInt("FullScreen", FullScreen.value);
    }

	public void MainMenu()
	{
		Application.LoadLevel("MainMenu");
	}

    void Start()
	{
        // set all options to playerprefs at main menu/level 1
        terrain = GameObject.FindGameObjectWithTag("MainEnvironment").GetComponent<TerrainScript>();
        mainGUI = terrain.canvas.GetComponent<MainGUI>();

        Quality.value = QualitySettings.GetQualityLevel();
        if (Screen.fullScreen == true)
            FullScreen.value = 1;
        else
            FullScreen.value = 0;
        ResolutionX.text= Screen.width.ToString();
        ResolutionY.text = Screen.width.ToString();
        SensitivityLevel.value = mainGUI.Sensitivity;

        Keybinds = Instantiate(KeybindsPrefab, new Vector2(0, 0), transform.rotation) as GameObject;
		Keybinds.transform.SetParent(terrain.canvas.transform);
        Keybinds.GetComponent<RectTransform>().localPosition = new Vector2(-Screen.width / 5, -Screen.height / 10);
        Keybinds.transform.localScale = new Vector3(1,1,1);
		Keybinds.GetComponentInChildren<Button>().onClick.AddListener(()	=>	SetActiveWindow(Keybinds, false));
		Keybinds.SetActive(false);
    }
	
	void Update()
	{

	}

}
