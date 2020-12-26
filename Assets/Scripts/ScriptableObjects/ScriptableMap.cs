using UnityEngine;

public class ScriptableMap : ScriptableObject
{
    public string countryName;
    public Sprite icon;
    [Tooltip("If country made of multiple region choose others")]
    public Map_Part[] countriesMadeOf;
    public int startCountryLevel;
    public Age startAgeOfNation;
    public long startAttackPower;
    public long startDefensePower;
    public long startFoodAmount;
    public double startMoneyAmount;
}