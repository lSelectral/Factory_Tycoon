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
        public BigDouble resourceAmount;
        public bool isConsumed;
    }

    public event EventHandler<OnResourceAmountChangedEventArgs> OnResourceAmountChanged;

    public class OnCurrencyChangedEventArgs: EventArgs
    {
        public BigDouble currency;
        public BigDouble premiumCurrency;
    }

    public event EventHandler<OnCurrencyChangedEventArgs> OnCurrencyChanged;

    public class OnPricePerProductChangedEventArgs : EventArgs
    {
        public BigDouble newPrice;
    }

    public event EventHandler<OnPricePerProductChangedEventArgs> OnPricePerProductChanged;

    #endregion

    [Header("Item Type Images")]
    [SerializeField] Sprite tradeIcon, warItemIcon, foodIcon, craftingIcon, gatheringIcon, housingIcon;

    public bool isLoadingFromSaveFile;
    
    // Resource related Variables
    public Dictionary<BaseResources, BigDouble> resourceValueDict;
    public Dictionary<BaseResources, TextMeshProUGUI> resourceTextDict;
    /// <summary>
    /// Stores all resources current price per product values for send information about new prices of products.
    /// When a components sub product price change, its price will change too
    /// </summary>
    public Dictionary<BaseResources, BigDouble> resourceNewPricePerProductDictionary;
    IEnumerable<BaseResources> resources;

    public List<ScriptableProductionBase> scriptableProductionUnits;

    // Player related variable and smoothing values
    [SerializeField] private TextMeshProUGUI /*totalResourceText,*/ currencyText, premiumCurrencyText, foodAmountText, attackAmountText;
    [SerializeField] BigDouble currency,totalResource, premiumCurrency, foodAmount, attackAmount = new BigDouble();
    [SerializeField] BigDouble smoothCurrency,smoothPremiumCurency = new BigDouble();
    [SerializeField] BigDouble smoothVelocityCurrency,smoothVelocityPremiumCurrency = new BigDouble();
    public float premiumCurrencySmoothTime;
    public float currencySmoothTime;

    #region Panels and Prefabs
    [SerializeField] private GameObject resourcePanel;
    [SerializeField] private GameObject resourcePrefab;
    public GameObject resourceIconPrefab;
    public GameObject resourceAmountTextPrefab;
    public GameObject iconPrefab;
    #endregion

    #region Properties

    public BigDouble TotalResource
    {
        get
        {
            return totalResource;
        }
        set
        {
            totalResource = value;
            //if (!isLoadingFromSaveFile)
            //{
                SaveSystem.Instance.totalProducedResource += totalResource;
            //}
        }
    }

    public BigDouble Currency
    {
        get
        {
            return currency;
        }
        set
        {
            BigDouble newValue;
            newValue = (value * UpgradeSystem.Instance.EarnedCoinMultiplier) - currency;
            currency = value;
            //if (!isLoadingFromSaveFile)
            //{
                SaveSystem.Instance.totalEarnedCurrency += newValue;
                OnCurrencyChanged?.Invoke(this, new OnCurrencyChangedEventArgs() { currency = currency, premiumCurrency = premiumCurrency });
            //}
        }
    }

    public BigDouble PremiumCurrency
    {
        get
        {
            return premiumCurrency;
        }
        set
        {
            premiumCurrency = value;
            //if (!isLoadingFromSaveFile)
            //{
                SaveSystem.Instance.totalEarnedPremiumCurrency += value - premiumCurrency;
                OnCurrencyChanged?.Invoke(this, new OnCurrencyChangedEventArgs() { currency = currency, premiumCurrency = premiumCurrency });
            //}
        }
    }

    public BigDouble FoodAmount
    {
        get { return foodAmount; }
        set
        {
            foodAmount = value;
            foodAmountText.text = foodAmount.ToString();
        }
    }
    public BigDouble AttackAmount
    {
        get { return attackAmount; }
        set
        {
            attackAmount = value;
            attackAmountText.text = attackAmount.ToString();
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

    //public string CurrencyToString(BigDouble valueToConvert)
    //{
    //    return valueToConvert.ToString();
    //}

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

        resourceNewPricePerProductDictionary = new Dictionary<BaseResources, BigDouble>();
        foreach (var res in resources)
        {
            resourceNewPricePerProductDictionary.Add(res, 0);
        }

        resourceValueDict = new Dictionary<BaseResources, BigDouble>();
        foreach (var res in resources)
        {
            resourceValueDict.Add(res, new BigDouble());
        }

        resourceTextDict = new Dictionary<BaseResources, TextMeshProUGUI>();
        foreach (var res in resources)
        {
            resourceTextDict.Add(res, new TextMeshProUGUI());
        }
    }

    private void Start()
    {
        scriptableProductionUnits = ProductionManager.Instance.scriptableProductionUnitList;

        #region Add all resources to resource panel
        int[] resourceIncrementArray = { 1, 5, 10, 100, 1000, 10000, 100000 };
        foreach (var resource in resources)
        {
            int arrayCounter = 0;

            var resourceInfo = Instantiate(resourcePrefab, resourcePanel.transform.GetChild(0));

            var text = resourceInfo.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>();
            text.text = "1";

            var resourceName = resourceInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            //resourceName.text = ProductionManager.Instance.GetScriptableProductionUnitFromResource(resource).TranslatedName;

            resourceInfo.transform.GetChild(0).GetComponent<Image>().sprite = GetSpriteFromResource(resource);

            resourceInfo.transform.GetChild(3).GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
            { arrayCounter -= 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = CurrencyToString(resourceIncrementArray[arrayCounter]); });
            resourceInfo.transform.GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            { arrayCounter += 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = CurrencyToString(resourceIncrementArray[arrayCounter]); });

            resourceInfo.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => { AddResource(resource, new BigDouble(resourceIncrementArray[arrayCounter], 0)); });
            resourceTextDict[resource] = resourceInfo.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            resourceTextDict[resource].text = (GetResourceAmount(resource)).ToString();
        }
        #endregion
    }

    void Update()
    {
        smoothCurrency = SmoothDamp(smoothCurrency, currency, ref smoothVelocityCurrency, currencySmoothTime);
        currencyText.text = smoothCurrency.ToString();

        smoothPremiumCurency = SmoothDamp(smoothPremiumCurency, premiumCurrency, ref smoothVelocityPremiumCurrency, premiumCurrencySmoothTime);
        premiumCurrencyText.text = smoothPremiumCurency.ToString();

        //smoothTotalResource = SmoothDamp(smoothTotalResource, totalResource, ref smoothVelocityTotalResource, smoothTime);
        //totalResourceText.text = "Total Resource\n" + CurrencyToString((smoothTotalResource), 0);
    }

    public void SetNewPricePerProduct(BaseResources res, BigDouble newPrice)
    {
        resourceNewPricePerProductDictionary[res] = newPrice;
        OnPricePerProductChanged(this, new OnPricePerProductChangedEventArgs() { newPrice = newPrice });
    }

    public BigDouble AddResource(BaseResources resource, BigDouble amount)
    {
        resourceValueDict[resource] += amount;
        TotalResource += amount;
        OnResourceAmountChanged?.Invoke(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource], isConsumed = false });
        resourceTextDict[resource].text = resourceValueDict[resource].ToString();

        Mine_Btn _mine = ProductionManager.Instance.GetProductionUnitFromResource(resource).GetComponent<Mine_Btn>();
        if (_mine != null && _mine.ItemTypes.Contains(ItemType.food))
            FoodAmount += amount * _mine.FoodAmount;

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
        resourceTextDict[resource].text = (resourceValueDict[resource]).ToString();
        OnResourceAmountChanged?.Invoke(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource], isConsumed = true });

        Mine_Btn _mine = ProductionManager.Instance.GetProductionUnitFromResource(resource).GetComponent<Mine_Btn>();
        if (_mine != null && _mine.ItemTypes.Contains(ItemType.food))
            FoodAmount -= _mine.FoodAmount * amount;

        
        var _compound = ProductionManager.Instance.GetProductionUnitFromResource(resource).GetComponent<Compounds>();

        if (_compound != null && _compound.ItemTypes.Contains(ItemType.food))
            FoodAmount -= _compound.FoodAmount * amount;
        if (_compound != null && _compound.ItemTypes.Contains(ItemType.warItem))
            AttackAmount -= _compound.AttackAmount * amount;
    }

    public BigDouble GetResourceAmount(BaseResources resource)
    {
        return resourceValueDict[resource];
    }

    public Sprite GetSpriteFromResource(BaseResources resource)
    {
        return ProductionManager.Instance.GetScriptableProductionUnitFromResource(resource).icon;
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
            default:
                return tradeIcon;
        }
    }

    public static BigDouble SmoothDamp(BigDouble current, BigDouble target, ref BigDouble currentVelocity, float smoothTime)
    {
        var deltaTime = Time.deltaTime;
        smoothTime = Mathf.Max(0.0001f, smoothTime);
        BigDouble num = new BigDouble(2f / smoothTime, 0);

        BigDouble num2 = num * deltaTime;
        BigDouble num3 = 1f / (num2 + 1f + num2 * 0.48f * num2 + num2 * num2 * 0.235f * 0.235f);
        BigDouble num4 = current - target;
        BigDouble num5 = target;
        BigDouble num6 = BigDouble.PositiveInfinity * smoothTime;

        num4 = ClampBigDouble(num4, -num6, num6);
        target = current - num4;
        BigDouble num7 = (currentVelocity + num * num4) * deltaTime;
        currentVelocity = (currentVelocity - num * num7) * num3;
        BigDouble num8 = target + (num4 + num7) * num3;
        if (num5 - current > 0f == num8 > num5)
        {
            num8 = num5;
            currentVelocity = (num8 - num5) / deltaTime;
        }
        return num8;
    }

    public static BigDouble ClampBigDouble(BigDouble value, BigDouble min, BigDouble max)
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
    _0_fire,
    _0_stone,

    _0_spear,
    _0_meat,
    _0_axe,
    _0_grape,
    _0_clay,

    _0_arrow,
    _0_bow,
    _0_brick,
    _0_clay_vase,
    _0_leather,

    _0_fish,
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