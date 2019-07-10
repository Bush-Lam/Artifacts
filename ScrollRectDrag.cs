using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollRectDrag : MonoBehaviour, IDragHandler
{
	//put on Text, then reference to scrollrect
	public Scrollbar scroll;
	public GameObject rectGameobject;
	TerrainScript terrain;
    
    public void HighLightOff()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void DestroyThis()
    {
        
        Destroy(this.gameObject);
    }

    public void OnUp()
    {
        scroll.value += 0.1f;
    }

    public void OnDown()
    {
        scroll.value -= 0.1f;
    }

    void Start()
	{
        if (Application.loadedLevelName != "MainMenu")
		    terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
	}

	public void OnDrag (PointerEventData data) 
	{
		if (rectGameobject != null)
        {
            rectGameobject.transform.GetComponent<RectTransform>().position += new Vector3(data.delta.x, data.delta.y);
            rectGameobject.transform.SetAsLastSibling();
        }
	}

	public void DisableKeybinds() // over any inputfield
	{
        if (Application.loadedLevelName != "MainMenu")
            terrain.canvas.GetComponent<MainGUI>().DisableKeyBindsWhenTyping = true;
	}
	
	public void EnableKeybinds()
	{
        if (Application.loadedLevelName != "MainMenu")
            terrain.canvas.GetComponent<MainGUI>().DisableKeyBindsWhenTyping = false;
	} 

}
