using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Map_Part : MonoBehaviour, IPointerClickHandler
{
    string countryName;
    int countryLevel;
    float countryPower;
    

    [Range(0f, 1f)] public float alphaSlider;
    Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = alphaSlider;
    }

    private void Update()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MapManager.Instance.clickedMapPart = this;
        Debug.Log("Mouse on the" + name);
        //MapManager.Instance.MapClickEffet(this);
        MapManager.Instance.MergeArea(MapManager.Instance.playerStartMapPart, MapManager.Instance.clickedMapPart);
    }
}