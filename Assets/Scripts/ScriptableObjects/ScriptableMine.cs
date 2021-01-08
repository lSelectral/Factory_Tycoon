using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName ="New Mine", menuName = "Factory/Mine")]
public class ScriptableMine : ScriptableObject
{
    [SerializeField] string Name = "";
    public string TranslatedName;

    public string resourceName;

    public BaseResources baseResource;

    public float collectTime;

    public int outputValue;

    public float pricePerProduct;

    public Sprite backgroundImage;

    public Sprite icon;

    public int unlockLevel;

    public int xpAmount = 10;

    public bool isLockedByContract;

    public Age ageBelongsTo;

    public Tier tier;

    public ItemType[] itemType;

    public bool isUnlocked;

    public string incomePerSecond;

    public float _incomePerSecond;

    /// <summary>
    /// Set some of the values of scriptable object automatically according to hierarchy and naming
    /// </summary>
    private void OnValidate()
    {
        //if (!Application.isPlaying)
        //{
            Name = name;
            resourceName = name;

            foreach (Tier tier in Enum.GetValues(typeof(Tier)))
            {
                if (tier.ToString() == Directory.GetParent(AssetDatabase.GetAssetPath(this)).Name)
                    this.tier = tier;
            }

            foreach (Age age in Enum.GetValues(typeof(Age)))
            {
                if (age.ToString() == Directory.GetParent(AssetDatabase.GetAssetPath(this)).Parent.Name)
                    this.ageBelongsTo = age;
            }

            foreach (BaseResources res in Enum.GetValues(typeof(BaseResources)))
            {
                if (name == ResourceManager.Instance.GetValidNameForResource(res))
                    baseResource = res;
            }
            _incomePerSecond = (outputValue * pricePerProduct / collectTime);
            incomePerSecond = ResourceManager.Instance.CurrencyToString(_incomePerSecond);

            // Auto text localization
            //string _key = Name;

        //if (LocalisationSystem.localisedEn.ContainsKey(_key) && LocalisationSystem.localisedTR.ContainsKey(_key))
        //{
        //    LocalisationSystem.Replace(_key, Name);
        //}
        //else
        //{
        //    LocalisationSystem.Add(_key, Name);
        //}

        //if (LocalisationSystem.GetLocalisedValue(_key) != String.Empty)
        //{
        //    LocalisationSystem.Replace(_key, Name);
        //}
        //else
        //    LocalisationSystem.Add(_key, Name);
        //TranslatedName = LocalisationSystem.GetLocalisedValue(_key);
        //}
    }
}