using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class ScriptableProductionBase : ScriptableObject
{
    [TextArea] public string Description;
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

    public Recipe[] recipes = new Recipe[] { };

    [HideInInspector] public Age ageBelongsTo;
    [HideInInspector] public Tier tier;

    public ItemType[] itemTypes = new ItemType[1];

    string _previousProductPricePerProduct;
    string _nextProductPricePerProduct;

    public BigDouble basePricePerProduct;
    BigDouble _basePricePerProduct;

    [ReadOnly] public string pricePerProductExpText;
    [ReadOnly] public string pricePerProductText;
    [ReadOnly] public BigDouble pricePerProduct;

    [ReadOnly] public string incomePerSecondText;
    [ReadOnly] public BigDouble incomePerSecond;

    [ReadOnly] public string optimalPricePerProductText;
    [ReadOnly] public BigDouble optimalPricePerProduct;

    [ReadOnly] public string optimalIncomePerSecondText;
    [Tooltip("Minimum income per second calculated by ingredients")]
    [ReadOnly] public BigDouble optimalPricePerSecond;

    [Tooltip("Check if real income per second is greater than minimum value")]
    [ReadOnly] [SerializeField] protected bool isOptimal;

    public ContractBase[] lockedByContracts;

    [Header("GAME MECHANIC INFO")]
    public int unlockLevel;

    public int xpAmount = 10;

    [SerializeField] List<string> pricePerProductUntilLevel50;
    BigDouble lastPricePerProduct;

    [PreviewSprite] public Sprite backgroundImage;

    [PreviewSprite] public Sprite toolImage;

    [PreviewSprite] public Sprite sourceImage;

    [PreviewSprite] public Sprite icon;

    string _name;

    internal bool isValidate = true;

#if UNITY_EDITOR
    /// <summary>
    /// Set some of the values of scriptable object automatically according to hierarchy and naming
    /// </summary>
    public virtual void OnValidate()
    {
        if (ResourceManager.Instance == null) return;
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

        foreach (BaseResources res in Enum.GetValues(typeof(BaseResources)))
        {
            if (_name != null && ResourceManager.Instance.GetValidNameForResource(res) != null && _name == ResourceManager.Instance.GetValidNameForResource(res))
                product = res;
        }

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

        pricePerProductUntilLevel50 = new List<string>();

        // If set to true, Input Resource and Amount array size will set to biggest one's size
        if (recipes == null || recipes.Length == 0)
            recipes = new Recipe[] { new Recipe { } };

        //if (recipes[0].collectTime <= 0 && collectTime > 0)
        recipes[0].collectTime = collectTime;

        //if (recipes[0].outputAmount <= 0)
        recipes[0].outputAmount = outputValue;


        _basePricePerProduct = new BigDouble(basePricePerProduct.Mantissa, basePricePerProduct.Exponent);
        //Set Price and optimal price values
        if (collectTime > 0 && outputValue > 0)
        {
            optimalPricePerProduct = ProductionManager.Instance.GET_OPTIMAL_PRICE_PER_PRODUCT_EDITOR(this);
            optimalPricePerProductText = (optimalPricePerProduct).ToString();

            pricePerProduct = (optimalPricePerProduct + _basePricePerProduct);
            pricePerProductText = pricePerProduct.ToString();
            pricePerProductExpText = pricePerProduct.ToString("E");

            optimalPricePerSecond = ProductionManager.Instance.GetIncomePerSecondForEDITOR(recipes[0].inputResources, recipes[0].inputAmounts);
            optimalIncomePerSecondText = (optimalPricePerSecond).ToString();

            incomePerSecond = (pricePerProduct) * outputValue / collectTime;
            incomePerSecondText = (incomePerSecond).ToString();
            if (incomePerSecond >= optimalPricePerSecond)
                isOptimal = true;
            else
                isOptimal = false;
        }

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
#endif

}