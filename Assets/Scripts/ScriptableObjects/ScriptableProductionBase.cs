using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


public abstract class ScriptableProductionBase : ScriptableObject
{
    [TextArea] public string Description;
    [Space(15)]

    //[SerializeField] string Name = "";
    public string TranslatedName;

    public string resourceName;

    public BaseResources product;

    // If product is food, enter how much food it will give
    public long foodAmount;

    public float collectTime;

    public int outputValue;

    public ContractBase[] lockedByContracts;

    [Header("IMAGE SOURCES")]
    [PreviewSprite] public Sprite backgroundImage;

    [PreviewSprite] public Sprite toolImage;

    [PreviewSprite] public Sprite sourceImage;

    [PreviewSprite] public Sprite icon;

    [Header("GAME MECHANIC INFO")]
    public int unlockLevel;

    public int xpAmount = 10;

    public Age ageBelongsTo;

    public Tier tier;

    public ItemType[] itemTypes = new ItemType[1];

    [Header("AUTO CALCULATED VALUES")]
    public string pricePerProductText;
    public float pricePerProduct;

    public string incomePerSecondText;
    public float incomePerSecond;


    /// <summary>
    /// Set some of the values of scriptable object automatically according to hierarchy and naming
    /// </summary>
    public virtual void OnValidate()
    {
        var parentDir = Directory.GetParent(AssetDatabase.GetAssetPath(this));

        // Minimum level threshold
        if (ageBelongsTo != Age._0_StoneAge && unlockLevel < 7)
            unlockLevel = 8;

        // If collect time equal to 0, income per second reach infinite
        if (collectTime < 1)
            collectTime = 1;
        if (outputValue < 1)
            outputValue = 1;

        resourceName = name;

        TranslatedName = ResourceManager.Instance.GetValidNameForResourceGame(product);
        if (TranslatedName == "")
            TranslatedName = name;

        if (itemTypes == null)
            itemTypes = new ItemType[1];

        foreach (Tier tier in Enum.GetValues(typeof(Tier)))
        {
            if (tier.ToString() == parentDir.Name)
                this.tier = tier;
        }

        foreach (Age age in Enum.GetValues(typeof(Age)))
        {
            if (age.ToString() == parentDir.Parent.Name)
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

        
        ///
        /// Remove comment marks if want to activate translation
        /// Cause late play time only in editor.
        /// Run when most of things are done
        ///
        //string _key = name;

        //LocalisationSystem.Init();

        //if (LocalisationSystem.GetLocalisedValue(_key) != string.Empty)
        //{
        //    LocalisationSystem.Replace(_key, name);
        //}
        //else
        //{
        //    LocalisationSystem.Add(_key, name);
        //}

        //TranslatedName = LocalisationSystem.GetLocalisedValue(_key);
    }
}