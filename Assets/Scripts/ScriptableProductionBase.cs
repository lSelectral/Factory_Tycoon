using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

//[CreateAssetMenu(fileName = "New Mine", menuName = "Factory/Base")]
public abstract class ScriptableProductionBase : ScriptableObject
{
    public string Description;
    //[SerializeField] string Name = "";
    public string TranslatedName;

    public string resourceName;

    public BaseResources product;

    // If product is food, enter how much food it will give
    public long foodAmount;

    public float collectTime;

    public int outputValue;

    public ContractBase[] lockedByContracts;

    public Sprite backgroundImage;

    public Sprite toolImage;

    public Sprite sourceImage;

    public Sprite icon;

    public int unlockLevel;

    public int xpAmount = 10;

    public Age ageBelongsTo;

    public Tier tier;

    public ItemType[] itemTypes = new ItemType[1];

    public string pricePerProductText;
    public float pricePerProduct;

    public string incomePerSecondText;
    public float incomePerSecond;


    /// <summary>
    /// Set some of the values of scriptable object automatically according to hierarchy and naming
    /// </summary>
    public virtual void OnValidate()
    {
        // If these value equal to 0, income per second reach infinite
        if (collectTime < 1)
            collectTime = 1;
        if (outputValue < 1)
            outputValue = 1;

        resourceName = name;

        if (itemTypes == null)
            itemTypes = new ItemType[1];

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
            if (name != null && ResourceManager.Instance.GetValidNameForResource(res) != null && name == ResourceManager.Instance.GetValidNameForResource(res))
                product = res;
        }

        pricePerProductText = ResourceManager.Instance.CurrencyToString(pricePerProduct);
        
        incomePerSecond = (outputValue * pricePerProduct / collectTime);
        incomePerSecondText = ResourceManager.Instance.CurrencyToString(incomePerSecond);


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