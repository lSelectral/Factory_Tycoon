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
        public double resourceAmount;
    }

    public event EventHandler<OnResourceAmountChangedEventArgs> OnResourceAmountChanged;

    public class OnCurrencyChangedEventArgs: EventArgs
    {
        public double currency;
        public double premiumCurrency;
    }

    public event EventHandler<OnCurrencyChangedEventArgs> OnCurrencyChanged;

    public class OnPricePerProductChangedEventArgs : EventArgs
    {
        public float newPrice;
    }

    public event EventHandler<OnPricePerProductChangedEventArgs> OnPricePerProductChanged;

    #endregion

    public bool isLoadingFromSaveFile;
    
    // Resource related Variables
    public Dictionary<BaseResources, long> resourceValueDict;
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
    double currency,totalResource, premiumCurrency;
    long foodAmount, attackAmount;
    double smoothCurrency,smoothPremiumCurency, smoothTotalResource;
    double smoothVelocity,smoothVelocityPremiumCurrency, smoothVelocityTotalResource;
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

    public double TotalResource
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

    public double Currency
    {
        get
        {
            return currency;
        }
        set
        {
            double newValue;
            newValue = (value * UpgradeSystem.Instance.EarnedCoinMultiplier) - currency;
            currency += newValue;
            if (!isLoadingFromSaveFile)
            {
                SaveSystem.Instance.totalEarnedCurrency += newValue;
                OnCurrencyChanged?.Invoke(this, new OnCurrencyChangedEventArgs() { currency = currency, premiumCurrency = premiumCurrency });
            }
            isLoadingFromSaveFile = false;
        }
    }

    public double PremiumCurrency
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

    public long FoodAmount
    {
        get { return foodAmount; }
        set
        {
            foodAmount = value;
            foodAmountText.text = CurrencyToString(foodAmount);
        }
    }
    public long AttackAmount
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
    private readonly string[] suffix = new string[] { "", "K", "M", "G", "T", "P", "E", "AA", "AB", "BA", "BB", "CA", "CB", "CC" }; // kilo, mega, giga, terra, penta, exa
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

    public string GetValidNameForResource(BaseResources res)
    {
        return char.ToUpper(res.ToString().Substring(3).ToCharArray()[0]) + res.ToString().Substring(4);
    }
    #endregion

    private void Awake()
    {
        scriptableMines = new List<ScriptableMine>();
        scriptableCompounds = new List<ScriptableCompound>();

        foodAmountText.text ="0";
        attackAmountText.text = "0";

        resources = Enum.GetValues(typeof(BaseResources)).Cast<BaseResources>();

        for (int i = 0; i < ProductionManager.Instance.Assets.Length; i++)
        {
            var asset = ProductionManager.Instance.Assets[i];

            if (asset as ScriptableObject != null)
            {
                var sc = asset as ScriptableObject;
                if (sc.GetType() == typeof(ScriptableMine) && (sc as ScriptableMine).ageBelongsTo == Age._0_StoneAge)
                    scriptableMines.Add(sc as ScriptableMine);
                else if (sc.GetType() == typeof(ScriptableCompound) && (sc as ScriptableCompound).ageBelongsTo == Age._0_StoneAge)
                    scriptableCompounds.Add(sc as ScriptableCompound);
            }
        }

        resourceNewPricePerProductDictionary = new Dictionary<BaseResources, float>();
        foreach (var res in resources)
        {
            resourceNewPricePerProductDictionary.Add(res, 0f);
        }

        resourceValueDict = new Dictionary<BaseResources, long>();
        foreach (var res in resources)
        {
            resourceValueDict.Add(res, 0);
        }

        resourceTextDict = new Dictionary<BaseResources, TextMeshProUGUI>();
        foreach (var res in resources)
        {
            resourceTextDict.Add(res, new TextMeshProUGUI());
        }

        #region Add all resources to resource panel
        int[] resourceIncrementArray = { 1, 5, 10, 100, 1000, 10000, 100000 };
        foreach (var resource in resources)
        {
            int arrayCounter = 0;

            var resourceInfo = Instantiate(resourcePrefab, resourcePanel.transform.GetChild(0));

            var text = resourceInfo.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>();
            text.text = "1";

            var resourceName = resourceInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            for (int i = 0; i < scriptableMines.Count; i++)
            {
                if (scriptableMines[i].product == resource)
                    resourceName.text = scriptableMines[i].resourceName;
            }
            for (int i = 0; i < scriptableCompounds.Count; i++)
            {
                if (scriptableCompounds[i].product == resource)
                    resourceName.text = scriptableCompounds[i].resourceName;
            }

            resourceInfo.transform.GetChild(0).GetComponent<Image>().sprite = GetSpriteFromResource(resource);

            resourceInfo.transform.GetChild(3).GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
            { arrayCounter -= 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = CurrencyToString(resourceIncrementArray[arrayCounter]); });
            resourceInfo.transform.GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            { arrayCounter += 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = CurrencyToString(resourceIncrementArray[arrayCounter]); });

            resourceInfo.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => { AddResource(resource, resourceIncrementArray[arrayCounter]); });
            resourceTextDict[resource] = resourceInfo.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            resourceTextDict[resource].text = (GetResourceAmount(resource)).ToString();
        }
        #endregion
    }

    void Update()
    {
        smoothCurrency = SmoothDamp(smoothCurrency, currency, ref smoothVelocity, currencySmoothTime);
        currencyText.text = CurrencyToString(smoothCurrency);

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

    public float AddResource(BaseResources resource, long amount, bool isLoadingFromSave = false)
    {
        resourceValueDict[resource] += amount;
        if (!isLoadingFromSave)
            TotalResource += amount;
        OnResourceAmountChanged?.Invoke(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource] });
        resourceTextDict[resource].text = CurrencyToString(resourceValueDict[resource]);

        Mine_Btn _mine = ProductionManager.Instance.GetMineFromResource(resource);
        if (_mine != null && _mine.ItemTypes.Contains(ItemType.food))
            FoodAmount += _mine.FoodAmount * amount;

        var _compound = ProductionManager.Instance.GetCompoundFromResource(resource);
        if (_compound != null && _compound.ItemTypes.Contains(ItemType.food))
            FoodAmount += _compound.FoodAmount * amount;
        if (_compound != null && _compound.ItemTypes.Contains(ItemType.warItem))
            AttackAmount += _compound.AttackAmount * amount;

        return amount;
    }

    public void ConsumeResource(BaseResources resource, long amount)
    {
        resourceValueDict[resource] -= amount;
        TotalResource -= amount;
        resourceTextDict[resource].text = CurrencyToString(resourceValueDict[resource]);
        OnResourceAmountChanged?.Invoke(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource] });

        Mine_Btn _mine = ProductionManager.Instance.GetMineFromResource(resource);
        if (_mine != null && _mine.ItemTypes.Contains(ItemType.food))
            FoodAmount -= _mine.FoodAmount;

        
        var _compound = ProductionManager.Instance.GetCompoundFromResource(resource);

        if (_compound != null && _compound.ItemTypes.Contains(ItemType.food))
            FoodAmount -= _compound.FoodAmount * amount;
        if (_compound != null && _compound.ItemTypes.Contains(ItemType.warItem))
            AttackAmount -= _compound.AttackAmount * amount;
    }

    public long GetResourceAmount(BaseResources resource)
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

    public static double SmoothDamp(double current, double target, ref double currentVelocity, float smoothTime, double maxSpeed= Mathf.Infinity)
    {
        var deltaTime = Time.deltaTime;
        smoothTime = Mathf.Max(0.0001f, smoothTime);
        double num = 2f / smoothTime;
        double num2 = num * deltaTime;
        double num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
        double num4 = current - target;
        double num5 = target;
        double num6 = maxSpeed * smoothTime;

        num4 = ClampDouble(num4, -num6, num6);
        target = current - num4;
        double num7 = (currentVelocity + num * num4) * deltaTime;
        currentVelocity = (currentVelocity - num * num7) * num3;
        double num8 = target + (num4 + num7) * num3;
        if (num5 - current > 0f == num8 > num5)
        {
            num8 = num5;
            currentVelocity = (num8 - num5) / deltaTime;
        }
        return num8;
    }

    public static double ClampDouble(double value, double min, double max)
    {
        if (value < min)
        {
            value = min;
        }
        else
        {
            if (value > max)
            {
                value = max;
            }
        }
        return value;
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
    _1_bronze_ingot,
    _1_stone_tablet,

    _1_bronze_boot,
    _1_bronze_chestplate,
    _1_bronze_helmet,
    _1_bronze_spear,
    _1_bronze_sword,

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