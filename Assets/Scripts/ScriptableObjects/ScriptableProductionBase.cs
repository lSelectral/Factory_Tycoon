using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


public abstract class ScriptableProductionBase : ScriptableObject
{
    [TextArea] public string Description;
    [Space(5)]
    public string TranslatedName;

    public BaseResources product;

    // If product is food, enter how much food it will give
    public long foodAmount;

    // If product is housing, enter how much population it will give
    public long housingAmount;

    // If product is weapon, enter how much attack it will give
    public long attackAmount;

    public float collectTime;

    public int outputValue;

    public Recipe[] recipes;

    public ContractBase[] lockedByContracts;

    [PreviewSprite] public Sprite backgroundImage;

    [PreviewSprite] public Sprite toolImage;

    [PreviewSprite] public Sprite sourceImage;

    [PreviewSprite] public Sprite icon;

    [Header("GAME MECHANIC INFO")]
    public int unlockLevel;

    public int xpAmount = 10;

    [HideInInspector] public Age ageBelongsTo;
    [HideInInspector] public Tier tier;

    public ItemType[] itemTypes = new ItemType[1];

    public string pricePerProductText;
    public BigDouble pricePerProduct;

    public string incomePerSecondText;
    public BigDouble incomePerSecond;

    [SerializeField] List<string> pricePerProductUntilLevel50;
    BigDouble lastPricePerProduct;

    string _name;

    //internal bool isValidate = true;

    /// <summary>
    /// Set some of the values of scriptable object automatically according to hierarchy and naming
    /// </summary>
    public virtual void OnValidate()
    {
        //if (!isValidate) return;
        _name = name;
        if (name.Any(c => char.IsDigit(c)))
            _name = name.Substring(2);


        var parentDir = Directory.GetParent(AssetDatabase.GetAssetPath(this));

        // Minimum level threshold
        if (ageBelongsTo != Age._0_StoneAge && unlockLevel < 7)
            unlockLevel = 8;

        // If collect time equal to 0, income per second reach infinite
        if (collectTime < 1)
            collectTime = 1;
        if (outputValue < 1)
            outputValue = 1;

        TranslatedName = ResourceManager.Instance.GetValidNameForResourceGame(product);
        if (TranslatedName == "")
            TranslatedName = _name;

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
            if (_name != null && ResourceManager.Instance.GetValidNameForResource(res) != null && _name == ResourceManager.Instance.GetValidNameForResource(res))
                product = res;
        }

        pricePerProductText = (pricePerProduct).ToString();

        incomePerSecond = (outputValue * pricePerProduct / collectTime);
        incomePerSecondText = (incomePerSecond).ToString();

        pricePerProductUntilLevel50 = new List<string>();

        lastPricePerProduct = pricePerProduct;
        for (int i = 1; i < 51; i++)
        {
            lastPricePerProduct = UpgradeSystem.Instance.GetNewPricePerProduct(1, lastPricePerProduct, i);
            pricePerProductUntilLevel50.Add(lastPricePerProduct.ToString());
        }

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