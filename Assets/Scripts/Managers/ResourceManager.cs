using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;

public class ResourceManager : Singleton<ResourceManager>
{
    #region Events

    public class OnResourceAmountChangedEventArgs: EventArgs
    {
        public BaseResources resource;
        public BNum resourceAmount;
        public bool isConsumed;
    }

    public event EventHandler<OnResourceAmountChangedEventArgs> OnResourceAmountChanged;

    public class OnCurrencyChangedEventArgs: EventArgs
    {
        public BNum currency;
        public BNum premiumCurrency;
    }

    public event EventHandler<OnCurrencyChangedEventArgs> OnCurrencyChanged;

    public class OnPricePerProductChangedEventArgs : EventArgs
    {
        public float newPrice;
    }

    public event EventHandler<OnPricePerProductChangedEventArgs> OnPricePerProductChanged;

    #endregion

    [Header("Item Type Images")]
    [SerializeField] Sprite tradeIcon, warItemIcon, foodIcon, craftingIcon, gatheringIcon, housingIcon;

    public bool isLoadingFromSaveFile;
    
    // Resource related Variables
    public Dictionary<BaseResources, BNum> resourceValueDict;
    public Dictionary<BaseResources, TextMeshProUGUI> resourceTextDict;
    /// <summary>
    /// Stores all resources current price per product values for send information about new prices of products.
    /// When a components sub product price change, its price will change too
    /// TODO MAKE THIS DICTIONARY. Currently null.
    /// </summary>
    public Dictionary<BaseResources, float> resourceNewPricePerProductDictionary;
    IEnumerable<BaseResources> resources;

    public List<ScriptableMine> scriptableMines;
    public List<ScriptableCompound> scriptableCompounds;

    // Player related variable and smoothing values
    [SerializeField] private TextMeshProUGUI /*totalResourceText,*/ currencyText, premiumCurrencyText, foodAmountText, attackAmountText;
    [SerializeField] BNum currency,totalResource, premiumCurrency, foodAmount, attackAmount = new BNum();
    [SerializeField] BNum smoothCurrency,smoothPremiumCurency, smoothTotalResource = new BNum();
    [SerializeField] BNum smoothVelocity,smoothVelocityPremiumCurrency, smoothVelocityTotalResource = new BNum();
    public float smoothTime;
    public float currencySmoothTime;

    #region Panels and Prefabs
    [SerializeField] private GameObject resourcePanel;
    [SerializeField] private GameObject resourcePrefab;
    public GameObject resourceIconPrefab;
    public GameObject resourceAmountTextPrefab;
    public GameObject iconPrefab;
    #endregion

    #region Properties

    public BNum TotalResource
    {
        get
        {
            return totalResource;
        }
        set
        {
            totalResource = value;
            if (!isLoadingFromSaveFile)
            {
                SaveSystem.Instance.totalProducedResource += totalResource;
            }
        }
    }

    public BNum Currency
    {
        get
        {
            return currency;
        }
        set
        {
            BNum newValue;
            newValue = (value * UpgradeSystem.Instance.EarnedCoinMultiplier) - currency;
            currency = value;
            if (!isLoadingFromSaveFile)
            {
                SaveSystem.Instance.totalEarnedCurrency += newValue;
                OnCurrencyChanged?.Invoke(this, new OnCurrencyChangedEventArgs() { currency = currency, premiumCurrency = premiumCurrency });
            }
            isLoadingFromSaveFile = false;
        }
    }

    public BNum PremiumCurrency
    {
        get
        {
            return premiumCurrency;
        }
        set
        {
            premiumCurrency = value;
            if (!isLoadingFromSaveFile)
            {
                SaveSystem.Instance.totalEarnedPremiumCurrency += value - premiumCurrency;
                OnCurrencyChanged?.Invoke(this, new OnCurrencyChangedEventArgs() { currency = currency, premiumCurrency = premiumCurrency });
            }
            isLoadingFromSaveFile = false;
        }
    }

    public BNum FoodAmount
    {
        get { return foodAmount; }
        set
        {
            foodAmount = value;
            foodAmountText.text = CurrencyToString(foodAmount);
        }
    }
    public BNum AttackAmount
    {
        get { return attackAmount; }
        set
        {
            attackAmount = value;
            attackAmountText.text = CurrencyToString(attackAmount);
        }
    }
    #endregion

    #region Helper Functions
    private readonly string[] suffix = new string[] { "", "K", "M", "A", "B", "C", "D", "E", "F", "G", "H", "AA", "AB", "AC", "AD", "AE" }; // kilo, mega, giga, terra, penta, exa
    public string CurrencyToString(float valueToConvert, int decimalAmount = 2)
    {
        int scale = 0;
        float v = valueToConvert;
        while (v >= 1000f)
        {
            v /= 1000f;
            scale++;
            if (scale >= suffix.Length)
                return valueToConvert.ToString("e2"); // overflow, can't display number, fallback to exponential
        }
        if (decimalAmount == 2)
            return v.ToString("0.##") + suffix[scale];
        else if (decimalAmount == 0)
            return v.ToString("0");
        return "";
    }

    public string CurrencyToString(BNum valueToConvert, int decimalAmount = 2)
    {
        return valueToConvert.ToString("sym");
    }

    // TODO add engineering notation. Let users to choose which format he wants.
    // Also show user possible formats at the start of game, for later add it to option menu.
    public string CurrencyToString(double valueToConvert, int decimalAmount = 2)
    {
        int scale = 0;
        double v = valueToConvert;
        while (v >= 1000f)
        {
            v /= 1000f;
            scale++;
            if (scale >= suffix.Length)
                return valueToConvert.ToString("e2"); // overflow, can't display number, fallback to exponential
        }
        if (decimalAmount == 2)
            return v.ToString("0.##") + suffix[scale];
        else if (decimalAmount == 0)
            return v.ToString("0");
        return "";
    }

    public string GetValidName(string oldString)
    {
        bool q = oldString.Any(char.IsLower);

        int index = 0;

        var returnString = "";

        foreach (char ch in oldString.ToCharArray())
        {
            if (char.IsUpper(ch))
            {
                var firstLetter = 'a';
                if (oldString[0] != 'i')
                    firstLetter = oldString[0];
                else
                    firstLetter = 'ı';

                returnString = char.ToUpper(firstLetter) + oldString.Substring(1, index - 1) + " " + oldString.Substring(index);

                return returnString;
            }
            index += 1;
        }
        if (returnString == "")
        {
            return char.ToUpper(oldString[0]) + oldString.Substring(1, oldString.Length - 1);
        }

        return "";
    }

    public string GetValidNameForResourceGame(BaseResources res)
    {
        string r = res.ToString();
        r = r.Substring(3);
        if (r.ToString().ToCharArray().Contains('_'))
        {
            r = r.Replace('_', ' ');
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(r);
        }
        return char.ToUpper(r.ToCharArray()[0]) + r.Substring(1);
    }

    public string GetValidNameForResource(BaseResources res)
    {
        string r = res.ToString();
        return char.ToUpper(r.Substring(3).ToCharArray()[0]) + r.Substring(4);
    }

    #endregion

    private void Awake()
    {

        foodAmountText.text ="0";
        attackAmountText.text = "0";

        resources = Enum.GetValues(typeof(BaseResources)).Cast<BaseResources>();

        resourceNewPricePerProductDictionary = new Dictionary<BaseResources, float>();
        foreach (var res in resources)
        {
            resourceNewPricePerProductDictionary.Add(res, 0f);
        }

        resourceValueDict = new Dictionary<BaseResources, BNum>();
        foreach (var res in resources)
        {
            resourceValueDict.Add(res, new BNum());
        }

        resourceTextDict = new Dictionary<BaseResources, TextMeshProUGUI>();
        foreach (var res in resources)
        {
            resourceTextDict.Add(res, new TextMeshProUGUI());
        }

        #region Add all resources to resource panel
        //int[] resourceIncrementArray = { 1, 5, 10, 100, 1000, 10000, 100000 };
        //foreach (var resource in resources)
        //{
        //    int arrayCounter = 0;

        //    var resourceInfo = Instantiate(resourcePrefab, resourcePanel.transform.GetChild(0));

        //    var text = resourceInfo.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>();
        //    text.text = "1";

        //    var resourceName = resourceInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        //    for (int i = 0; i < scriptableMines.Count; i++)
        //    {
        //        if (scriptableMines[i].product == resource)
        //            resourceName.text = scriptableMines[i].resourceName;
        //    }
        //    for (int i = 0; i < scriptableCompounds.Count; i++)
        //    {
        //        if (scriptableCompounds[i].product == resource)
        //            resourceName.text = scriptableCompounds[i].resourceName;
        //    }

        //    resourceInfo.transform.GetChild(0).GetComponent<Image>().sprite = GetSpriteFromResource(resource);

        //    resourceInfo.transform.GetChild(3).GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
        //    { arrayCounter -= 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = CurrencyToString(resourceIncrementArray[arrayCounter]); });
        //    resourceInfo.transform.GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
        //    { arrayCounter += 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = CurrencyToString(resourceIncrementArray[arrayCounter]); });

        //    resourceInfo.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => { AddResource(resource, new BNum(resourceIncrementArray[arrayCounter],0)); });
        //    resourceTextDict[resource] = resourceInfo.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        //    resourceTextDict[resource].text = (GetResourceAmount(resource)).ToString();
        //}
        #endregion
    }

    void Update()
    {
        //smoothCurrency = SmoothDamp(smoothCurrency, currency, ref smoothVelocity, currencySmoothTime);
        currencyText.text = currency.ToString("sym");

        smoothPremiumCurency = SmoothDamp(smoothPremiumCurency, premiumCurrency, ref smoothVelocityPremiumCurrency, smoothTime);
        premiumCurrencyText.text = CurrencyToString(smoothPremiumCurency);

        //smoothTotalResource = SmoothDamp(smoothTotalResource, totalResource, ref smoothVelocityTotalResource, smoothTime);
        //totalResourceText.text = "Total Resource\n" + CurrencyToString((smoothTotalResource), 0);
    }

    public void SetNewPricePerProduct(BaseResources res, float newPrice)
    {
        resourceNewPricePerProductDictionary[res] = newPrice;
        OnPricePerProductChanged(this, new OnPricePerProductChangedEventArgs() { newPrice = newPrice });
    }

    public BNum AddResource(BaseResources resource, BNum amount, bool isLoadingFromSave = false)
    {
        resourceValueDict[resource] += amount;
        if (!isLoadingFromSave)
            TotalResource += amount;
        OnResourceAmountChanged?.Invoke(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource], isConsumed = false });
        resourceTextDict[resource].text = CurrencyToString(resourceValueDict[resource]);

        Mine_Btn _mine = ProductionManager.Instance.GetProductionUnitFromResource(resource).GetComponent<Mine_Btn>();
        if (_mine != null && _mine.ItemTypes.Contains(ItemType.food))
            FoodAmount += amount * _mine.foodAmount;

        var _compound = ProductionManager.Instance.GetProductionUnitFromResource(resource).GetComponent<Compounds>();
        if (_compound != null && _compound.ItemTypes.Contains(ItemType.food))
            FoodAmount += amount * _compound.FoodAmount;
        if (_compound != null && _compound.ItemTypes.Contains(ItemType.warItem))
            AttackAmount += amount * _compound.AttackAmount;

        return amount;
    }

    public void ConsumeResource(BaseResources resource, long amount)
    {
        resourceValueDict[resource] -= amount;
        TotalResource -= amount;
        resourceTextDict[resource].text = CurrencyToString(resourceValueDict[resource]);
        OnResourceAmountChanged?.Invoke(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource], isConsumed = true });

        Mine_Btn _mine = ProductionManager.Instance.GetProductionUnitFromResource(resource).GetComponent<Mine_Btn>();
        if (_mine != null && _mine.ItemTypes.Contains(ItemType.food))
            FoodAmount -= _mine.FoodAmount;

        
        var _compound = ProductionManager.Instance.GetProductionUnitFromResource(resource).GetComponent<Compounds>();

        if (_compound != null && _compound.ItemTypes.Contains(ItemType.food))
            FoodAmount -= _compound.FoodAmount * amount;
        if (_compound != null && _compound.ItemTypes.Contains(ItemType.warItem))
            AttackAmount -= _compound.AttackAmount * amount;
    }

    public BNum GetResourceAmount(BaseResources resource)
    {
        return resourceValueDict[resource];
    }

    public Sprite GetSpriteFromResource(BaseResources resource)
    {
        for (int i = 0; i < scriptableMines.Count; i++)
        {
            if (scriptableMines[i].product == resource)
                return scriptableMines[i].icon;
        }
        for (int i = 0; i < scriptableCompounds.Count; i++)
        {
            if (scriptableCompounds[i].product == resource)
                return scriptableCompounds[i].icon;
        }
        return null;
    }

    public Sprite GetSpriteFromItemType(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.craftingItem:
                return craftingIcon;
            case ItemType.tradeGoods:
                return tradeIcon;
            case ItemType.warItem:
                return warItemIcon;
            case ItemType.food:
                return foodIcon;
            case ItemType.gathering:
                return gatheringIcon;
            case ItemType.housing:
                return housingIcon;
        }
        return tradeIcon;
    }

    public static BNum SmoothDamp(BNum current, BNum target, ref BNum currentVelocity, float smoothTime)
    {
        var deltaTime = Time.deltaTime;
        smoothTime = Mathf.Max(0.0001f, smoothTime);
        BNum num = new BNum(2f / smoothTime, 0);

        BNum num2 = num * deltaTime;
        BNum num3 = 1f / (num2 + 1f + num2 * 0.48f * num2 + num2 * num2 * 0.235f * 0.235f);
        BNum num4 = current - target;
        BNum num5 = target;
        BNum num6 = new BNum(999, 10000) * smoothTime;

        num4 = ClampBNum(num4, -num6, num6);
        target = current - num4;
        BNum num7 = (currentVelocity + num * num4) * deltaTime;
        currentVelocity = (currentVelocity - num * num7) * num3;
        BNum num8 = target + (num4 + num7) * num3;
        if (num5 - current > 0f == num8 > num5)
        {
            num8 = num5;
            currentVelocity = (num8 - num5) / deltaTime;
        }
        return num8;
    }

    public static BNum ClampBNum(BNum value, BNum min, BNum max)
    {
        return (value < min) ? min : (value > max) ? max : value;
    }
}

public enum ItemType
{
    craftingItem,
    tradeGoods,
    warItem,
    food,
    gathering,
    housing,
}

public enum BaseResources
{
    #region Stone Age Resources
    _0_berry,
    _0_leaf,
    _0_stick,
    _0_stone,

    _0_axe,
    _0_clay,
    _0_grape,
    _0_pickaxe,
    _0_spear,

    _0_arrow,
    _0_bow,
    _0_clay_vase,
    _0_leather,
    _0_meat,

    _0_fire,
    _0_hut,
    _0_leather_armor,
    _0_wheel,
    _0_wine,
    #endregion

    #region Bronze Age Resources
    _1_copper_ore,
    _1_rope,
    _1_tin_ore,

    _1_bronze_axe,
    _1_bronze_pickaxe,
    _1_bronze_ingot,
    _1_bronze_sword,
    _1_stone_tablet,

    _1_bronze_boot,
    _1_bronze_chestplate,
    _1_bronze_hammer,
    _1_bronze_helmet,
    _1_bronze_spear,

    _1_bronze_gloves,
    _1_bronze_shield,
    _1_chariot,
    _1_silk,
    _1_wheeled_wagon,

    _1_bronze_statue,
    _1_gold_ore,
    _1_painted_vase,
    _1_silk_cloth,
    #endregion

    #region Iron Age Resources

    #endregion

    #region Middle Age Resources

    #endregion
}