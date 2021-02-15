using UnityEngine;

[CreateAssetMenu(fileName ="New Region", menuName ="Region")]
public class ScriptableMap : ScriptableObject
{
    public string countryName;
    public Sprite icon;
    [Tooltip("If country made of multiple region choose others")]
    public Map_Part[] countriesMadeOf;
    public int startCountryLevel;
    [SearchableEnum] public Age startAgeOfNation;
    public BigDouble startAttackPower;
    public BigDouble startDefensePower;
    public BigDouble startFoodAmount;
    public BigDouble startMoneyAmount;
    
    /// <summary>
    /// Every country has one specific resource
    /// This resource granted at start to country as much as its size
    /// Also when producing this resource production speed is x1.5 faster
    /// </summary>
    [Tooltip("Country specific resources has production bonuses")]
    [SearchableEnum] public BaseResources specificResource;
}