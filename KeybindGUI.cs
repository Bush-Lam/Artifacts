using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeybindGUI : MonoBehaviour, IPointerDownHandler
{
	TerrainScript terrain;
	public RectTransform KeyBindValues;
    public Text[] KeyBindNames;
	public InputField[] KeyBindInputField;
    public Text Unbounded;
	CharacterSkillBarGUI skillbarGUI;

	public void OnPointerDown(PointerEventData data)
	{
		transform.SetAsLastSibling();
	}
	
	public void Apply()
	{
        Unbounded.text = "Same Keys: ";
        for (int i = 0; i < terrain.canvas.GetComponent<MainGUI>().KeyBinds.Count;i++)
		{
            for (int j = 0; j < KeyBindInputField.Length; j++)
            {
                if (KeyBindInputField[i].text == KeyBindInputField[j].text && 
                    i != j && !Unbounded.text.Contains(KeyBindNames[i].text)) // if multiple keybinds same
                { 
                    Unbounded.text += KeyBindNames[i].text + ", ";
                }
            }

            terrain.canvas.GetComponentInChildren<MainGUI>().KeyBinds[i] = KeyBindInputField[i].text;
            
		}
	}
	
	void Start()
	{
		terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
		KeyBindInputField = KeyBindValues.GetComponentsInChildren<InputField>();
        KeyBindNames = transform.Find("ScrollRect").Find("Content").Find("KeybindName").GetComponentsInChildren<Text>();
        skillbarGUI = terrain.canvas.GetComponentInChildren<CharacterSkillBarGUI>();

		for (int i = 0; i < terrain.canvas.GetComponent<MainGUI>().KeyBinds.Count; i++)
		{
            KeyBindNames[i].fontSize = 16;
            KeyBindInputField[i].textComponent.fontSize = 16;
            KeyBindInputField[i].text = terrain.canvas.GetComponent<MainGUI>().KeyBinds[i];
            KeyBindInputField[i].placeholder.GetComponentInChildren<Text>().text = KeyBindInputField[i].textComponent.text;
            PlayerPrefs.SetString(KeyBindInputField[i].text, terrain.canvas.GetComponent<MainGUI>().KeyBinds[i]);
            PlayerPrefs.Save();
		}
	}
	
	void Update()
	{
		for (int i = 0; i < KeyBindInputField.Length; i++)
		{
			if (KeyBindInputField[i].isFocused == true)
			{
                if (KeyBindInputField[i].text == string.Empty)
                {
                    KeyBindInputField[i].text = terrain.canvas.GetComponentInChildren<MainGUI>().KeyBinds[i];
                }
                // character limits to limit unwanted input
                if (Input.GetKey(KeyCode.LeftShift))
				{	
					KeyBindInputField[i].text = string.Empty;
					KeyBindInputField[i].characterLimit = 10;
					KeyBindInputField[i].text = "left shift";
				}		
				else if (Input.GetKey(KeyCode.LeftAlt))
				{	
					KeyBindInputField[i].text = string.Empty;
					KeyBindInputField[i].characterLimit = 8;
					KeyBindInputField[i].text = "left alt";
				}
				else if (Input.GetKey(KeyCode.RightShift))
				{	
					KeyBindInputField[i].text = string.Empty;
					KeyBindInputField[i].characterLimit = 11;
					KeyBindInputField[i].text = "right shift";
				}
				else if (Input.GetKey(KeyCode.RightAlt))
				{	
					KeyBindInputField[i].text = string.Empty;
					KeyBindInputField[i].characterLimit = 9;
					KeyBindInputField[i].text = "right alt";
				}
				else if (Input.GetKey(KeyCode.UpArrow))
				{	
					KeyBindInputField[i].text = string.Empty;
					KeyBindInputField[i].characterLimit = 7;
					KeyBindInputField[i].text = "up";
				}
				else if (Input.GetKey(KeyCode.LeftArrow))
				{	
					KeyBindInputField[i].text = string.Empty;
					KeyBindInputField[i].characterLimit = 8;
					KeyBindInputField[i].text = "left";
				}
				else if (Input.GetKey(KeyCode.RightArrow))
				{	
					KeyBindInputField[i].text = string.Empty;
					KeyBindInputField[i].characterLimit = 10;
					KeyBindInputField[i].text = "right";
				}
				else if (Input.GetKey(KeyCode.DownArrow))
				{	
					KeyBindInputField[i].text = string.Empty;
					KeyBindInputField[i].characterLimit = 9;
					KeyBindInputField[i].text = "down";
				}
				else if (Input.GetKey(KeyCode.LeftControl))
				{	
					KeyBindInputField[i].text = string.Empty;
					KeyBindInputField[i].characterLimit = 9;
					KeyBindInputField[i].text = "left ctrl";
				}
				else if (Input.GetKey(KeyCode.RightControl))
				{	
					KeyBindInputField[i].text = string.Empty;
					KeyBindInputField[i].characterLimit = 9;
					KeyBindInputField[i].text = "right ctrl";
                }
                else if (Input.GetKey(KeyCode.Space))
                {
                    KeyBindInputField[i].text = string.Empty;
                    KeyBindInputField[i].characterLimit = 5;
                    KeyBindInputField[i].text = "space";
                }
                else
					KeyBindInputField[i].characterLimit = 1;
			}
		}

	}
	
}