using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Map_Part : MonoBehaviour, IPointerClickHandler
{
    string countryName;
    int countryLevel;
    Age currentAgeOfNation;
    long attackPower;
    long foodAmount;
    double moneyAmount;

    [Range(0f, 1f)] public float alphaSlider;
    Image image;

    public string CountryName { get => countryName; set => countryName = value; }
    public int CountryLevel { get => countryLevel; set => countryLevel = value; }
    public Age CurrentAgeOfNation { get => currentAgeOfNation; set => currentAgeOfNation = value; }
    public long AttackPower { get => attackPower; set => attackPower = value; }
    public long FoodAmount { get => foodAmount; set => foodAmount = value; }
    public double MoneyAmount { get => moneyAmount; set => moneyAmount = value; }

    private void Start()
    {
        image = GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = alphaSlider;

        //if (MapManager.Instance.playerCurrentMapPart[0] != this)
        //{
            countryName = "SELECTRA";
            countryLevel = 133;
            currentAgeOfNation = Age._3_warAge;
            attackPower = countryLevel * 15 * UnityEngine.Random.Range(4, 24);
            foodAmount = countryLevel * 9 * UnityEngine.Random.Range(8, 45);
            moneyAmount = Mathf.Pow(countryLevel, UnityEngine.Random.Range(1,9));
        //}
    }

    private void Update()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Mouse on the" + name);
        MapManager.Instance.OnMapClick(this);
        MapManager.Instance.clickedMapPart = this;
        //MapManager.Instance.ConquerArea(MapManager.Instance.playerCurrentMapPart, new Map_Part[] { this }) ;
    }
}