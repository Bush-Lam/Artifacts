using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectFunction : MonoBehaviour, IScrollHandler
{
    RectTransform TransRef;
    ScrollRect ScrollRef;
    RectTransform ContentRef;
    TerrainScript terrain;
    MainGUI maingui;

    [SerializeField] float ScrollSpeed = 100;

    float MinScroll = 0;
    float MaxScroll;

    // Use this for initialization
    void Start()
    {
        TransRef = GetComponent<RectTransform>();
        ScrollRef = GetComponent<ScrollRect>();
        ContentRef = ScrollRef.content;
        terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
        maingui = terrain.canvas.GetComponent<MainGUI>();
        MaxScroll = ContentRef.rect.height - TransRef.rect.height;
    }

    public void Up()
    {
        ScrollRef.verticalScrollbar.value += 0.1f;
    }
    public void Down()
    {
        ScrollRef.verticalScrollbar.value -= 0.1f;
    }

    void ReactivateCharacterControl()
    {
        maingui.DisableAttackWhenMenuItemPressed = false;
    }

    public void OnScroll(PointerEventData eventData)
    {
        Vector2 ScrollDelta = eventData.scrollDelta;

        maingui.DisableAttackWhenMenuItemPressed = true;

        Invoke("ReactivateCharacterControl", 0.2f);

        if (ScrollDelta.y < 0)
        {
            ScrollRef.verticalScrollbar.value -= 0.1f;
        }
        else if (ScrollDelta.y > 0)
        {
            ScrollRef.verticalScrollbar.value += 0.1f;
        }
    }
}
