using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;
using System.ComponentModel;

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
        //public BigDouble premiumCurrency;
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

    // Player related variable and smoothing values
    [SerializeField] private TextMeshProUGUI /*totalResourceText,*/ currencyText, premiumCurrencyText, foodAmountText, attackAmountText, populationText;
    [SerializeField] BigDouble currency,totalResource, premiumCurrency, foodAmount, attackAmount = new BigDouble();
    [SerializeField] BigDouble smoothCurrency,smoothPremiumCurency = new BigDouble();
    [SerializeField] BigDouble smoothVelocityCurrency,smoothVelocityPremiumCurrency = new BigDouble();
    float premiumCurrencySmoothTime;
    float currencySmoothTime;

    #region Panels and Prefabs
    [SerializeField] private GameObject resourcePanel;
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] GameObject resourcePanelAgePrefab;
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

    float startTimeCurrency;
    BigDouble oldCurrencyValue;
    public BigDouble Currency
    {
        get
        {
            return currency;
        }
        set
        {
            oldCurrencyValue = currency;
            startTimeCurrency = Time.time;
            BigDouble newValue = value - currency;
            currency = value * UpgradeSystem.Instance.EarnedCoinMultiplier;

            if (newValue > 0)
                SaveSystem.Instance.totalEarnedCurrency += newValue * UpgradeSystem.Instance.EarnedCoinMultiplier;
            else if (newValue < 0)
                SaveSystem.Instance.totalSpendedCurrency += newValue * UpgradeSystem.Instance.EarnedCoinMultiplier;
            OnCurrencyChanged?.Invoke(this, new OnCurrencyChangedEventArgs() { currency = currency/*, premiumCurrency = premiumCurrency*/ });
        }
    }

    float startTimePremiumCurrency;
    BigDouble oldPremiumCurrencyValue;
    public BigDouble PremiumCurrency
    {
        get
        {
            return premiumCurrency;
        }
        set
        {
            oldPremiumCurrencyValue = premiumCurrency;
            startTimePremiumCurrency = Time.time;
            BigDouble newValue = value - premiumCurrency;
            premiumCurrency = value;

            if (newValue > 0)
                SaveSystem.Instance.totalEarnedPremiumCurrency += newValue;
            else if (newValue < 0)
                SaveSystem.Instance.totalSpendedPremiumCurrency += newValue;
            //OnCurrencyChanged?.Invoke(this, new OnCurrencyChangedEventArgs() { currency = currency, premiumCurrency = premiumCurrency });
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
        string r = res.ToString().Substring(3);
        if (r.ToString().ToCharArray().Contains('_'))
        {
            r = r.Replace('_', ' ');
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(r);
        }
        return char.ToUpper(r.ToCharArray()[0]) + r.Substring(1);
    }

    public string GetValidNameForResource(BaseResources res)
    {
        string r = res.ToString().Substring(3);
        if (r.StartsWith("i"))
        {
            r = "I" + r.Remove(0, 1);
        }

        return char.ToUpper(r.ToCharArray()[0]) + r.Substring(1);
    }

    #endregion

    private void Awake()
    {
        foodAmountText.text ="0";
        attackAmountText.text = "0";
        populationText.text = "0";

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
        UpgradeSystem.Instance.OnCombatPowerMultiplierChanged += OnCombatPowerMultiplierChanged;
    }

    private void OnCombatPowerMultiplierChanged(object sender, UpgradeSystem.OnCombatPowerMultiplierChangedEventArgs e)
    {
        AttackAmount *= e.changeAmount;
    }

    List<List<GameObject>> resourceObjects = new List<List<GameObject>>();
    List<GameObject> gameObjects = new List<GameObject>();
    private void Start()
    {
        #region Add all resources to resource panel
        var contentHolder = resourcePanel.transform.GetChild(0).GetChild(0);
        int[] resourceIncrementArray = { 1, 5, 10, 100, 1000, 10000, 100000 };
        int counter = 0;

        foreach (var resource in resources)
        {
            int arrayCounter = 0;
            Age currentAge = ProductionManager.Instance.scriptableProductionUnitList[counter].ageBelongsTo;

            GameObject ageObj = null;
            if (contentHolder.Find(currentAge.ToString()) == null)
            {
                if (gameObjects.Count > 1)
                    resourceObjects.Add(gameObjects);

                gameObjects = new List<GameObject>();

                ageObj = Instantiate(resourcePanelAgePrefab, contentHolder);
                ageObj.name = currentAge.ToString();
                ageObj.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = currentAge.ToString();
                ageObj.transform.Find("Expand").GetComponent<Button>().onClick.AddListener(() => ExpandAllChilds(ageObj.transform, (int)currentAge));
                ageObj.transform.Find("Collapse").GetComponent<Button>().onClick.AddListener(() => CollapseAllChilds(ageObj.transform, (int)currentAge));
            }
            else
                ageObj = contentHolder.Find(currentAge.ToString()).gameObject;

            var resourceInfo = Instantiate(resourcePrefab, contentHolder);

            var text = resourceInfo.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>();
            text.text = "1";

            var resourceName = resourceInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            resourceName.text = ProductionManager.Instance.GetScriptableProductionUnitFromResource(resource).TranslatedName;
            resourceInfo.transform.GetChild(0).GetComponent<Image>().sprite = GetSpriteFromResource(resource);

            resourceInfo.transform.GetChild(3).GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
            { arrayCounter -= 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = 
                CurrencyToString(resourceIncrementArray[arrayCounter]); });
            resourceInfo.transform.GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            { arrayCounter += 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = 
                CurrencyToString(resourceIncrementArray[arrayCounter]); });

            resourceInfo.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => 
            { AddResource(resource, new BigDouble(resourceIncrementArray[arrayCounter], 0)); });
            resourceTextDict[resource] = resourceInfo.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            resourceTextDict[resource].text = (GetResourceAmount(resource)).ToString();

            gameObjects.Add(resourceInfo);
            counter++;
            if (counter == ProductionManager.Instance.scriptableProductionUnitList.Length)
                resourceObjects.Add(gameObjects);
        }
        #endregion
    }

    void Update()
    {
        smoothCurrency = SmoothStep(oldCurrencyValue, currency, Time.time - startTimeCurrency);
        currencyText.text = smoothCurrency.ToString();
        //currencyText.text = string.Format("<mspace=35>{0}</mspace>", smoothCurrency.ToString()); //Constant space between letters. Not pretty result.

        smoothPremiumCurency = SmoothStep(oldPremiumCurrencyValue, premiumCurrency, Time.time - startTimePremiumCurrency);
        premiumCurrencyText.text = smoothPremiumCurency.ToString();
    }

    void CollapseAllChilds(Transform _parent, int index)
    {
        _parent.Find("Expand").gameObject.SetActive(true);
        _parent.Find("Collapse").gameObject.SetActive(false);
        for (int i = 0; i < resourceObjects[index].Count; i++)
        {
            resourceObjects[index][i].SetActive(false);
        }
    }
    void ExpandAllChilds(Transform _parent, int index)
    {
        _parent.Find("Expand").gameObject.SetActive(false);
        _parent.Find("Collapse").gameObject.SetActive(true);
        for (int i = 0; i < resourceObjects[index].Count; i++)
        {
            resourceObjects[index][i].SetActive(true);
        }
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

        ProductionBase productionUnit = ProductionManager.Instance.GetProductionUnitFromResource(resource);

        if (productionUnit.ItemTypes.Contains(ItemType.food))
            FoodAmount += productionUnit.FoodAmount * amount;
        if (productionUnit.ItemTypes.Contains(ItemType.housing))
            UpgradeSystem.Instance.Population += productionUnit.HousingAmount * amount;
        if (productionUnit.ItemTypes.Contains(ItemType.warItem))
            AttackAmount += productionUnit.AttackAmount;

        return amount;
    }

    public void ConsumeResource(BaseResources resource, BigDouble amount)
    {
        resourceValueDict[resource] -= amount;
        TotalResource -= amount;
        resourceTextDict[resource].text = (resourceValueDict[resource]).ToString();
        OnResourceAmountChanged?.Invoke(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource], isConsumed = true });

        ProductionBase productionUnit = ProductionManager.Instance.GetProductionUnitFromResource(resource);

        if (productionUnit.ItemTypes.Contains(ItemType.food))
            FoodAmount -= productionUnit.FoodAmount * amount;
        if (productionUnit.ItemTypes.Contains(ItemType.housing))
            UpgradeSystem.Instance.Population -= productionUnit.HousingAmount * amount;
        if (productionUnit.ItemTypes.Contains(ItemType.warItem))
            AttackAmount -= productionUnit.AttackAmount;
    }

    public BigDouble GetResourceAmount(BaseResources resource)
    {
        return resourceValueDict[resource];
    }

    public Sprite GetSpriteFromResource(BaseResources resource)
    {
        var unit = ProductionManager.Instance.GetScriptableProductionUnitFromResource(resource);
        return unit.icon;
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

    // Interpolates between /min/ and /max/ with smoothing at the limits.
    public BigDouble SmoothStep(BigDouble from, BigDouble to, BigDouble t)
    {
        t = Clamp01(t);
        t = -2.0F * t * t * t + 3.0F * t * t;
        return to * t + from * (1F - t);
    }

    // Clamps value between 0 and 1 and returns value
    BigDouble Clamp01(BigDouble value)
    {
        if (value < 0)
            return 0;
        else if (value > 1)
            return 1;
        else
            return value;
    }
}

/// <summary>
/// Artifact parts that can be placed in specific body parts
/// </summary>
public enum ArtifactPart
{
    Head,
    Chest,
    Arm,
    Leg,
    Weapon,
    Trophy,
    Amulet,
    Ring,
    All,
}

/// <summary>
/// Artifacts have tier between them higher the tier higher the bonus.
/// With 3 same tier artifact sacrificing, 1 higher tier can be acquired.
/// </summary>
public enum ArtifactTier
{
    common,
    rare,
    epic,
    legendary,
    onlyYouHaveIt
}

/// <summary>
/// Artifact sets when combined have a set bonus.
/// </summary>
public enum ArtifactSet
{
    none,
    toughAsStone, // Full stone age set
    dawnOfMetal, // Full bronze age set
    ironFist, // Iron age set
    holyDualWieldSet, // Medieval Age Set
    rangedTeslaScienceSet, // Industrial Age Set
    heavyMachinerySet, // Early Modern Age Set
}

public enum ArtifacPower
{
    attackPower,
    defensePower,
    gatheringSpeed, // Raw resource
    warItemProductionSpeed,
    foodProductionSpeed,
    foodProductionYield,
    tradingGoodProductionSpeed,
    contractRewardMultiplier,
    contractXPMultiplier,
    questRewardMultiplier,
    questXPMultiplier,
    allXPMultiplier,
    allCurrencyMultiplier,

    higherTierArtifactEarnChance, // High tier artifact gain chance
    
    unTouchable, // Low chance money and resources won't reset
}

public enum WorkerType
{
    None, // Production unit will be stopped
    Fill, // Production unit worker capacity will be fulled, if possible, with preffered worker type otherwise standard, if it is not possible to than what comes first.
    Standard, // Has no special talent
    Crafter,
    Hunter,
    Gatherer, // Grant more resource if chosen in gathering jobs
    Cook, // Cooking jobs grant tastier food (More food)
    Warrior, // Trained fearless soldiers have greater defense/attack stat
    Miner, // Miner able to mine more resource than usual workers
    Blacksmith, // Blacksmiths can forge better weapon/armor
    Artist, // Artists can paint and create immense art. (Increase in value)
    Engineer, // Engineers design better products and make the workflow faster.
    Chemist, // Chemists can improve formula and produce much more resource from less.
    Scientist, // Scientist can research new technologies efficiently.
    Robot,
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
    _0_fire,

    _0_rope,
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
    _0_wheat,
    _0_bread,
    _0_log,

    _0_sand,
    _0_leather_armor,
    _0_hut,
    _0_wine,
    _0_wheel,
    #endregion

    #region Bronze Age Resources

    _1_apple,
    _1_copper_ore,
    _1_tin_ore,
    _1_rice,

    _1_bronze_ingot,
    _1_bronze_hammer,
    _1_bronze_pickaxe,
    _1_boat,
    _1_bronze_axe,
    _1_bronze_sword,

    _1_clay_pipe,
    _1_bronze_chestplate,
    _1_bronze_helmet,
    _1_bronze_spear,
    _1_egg,

    _1_bronze_shield,
    _1_silk,
    _1_glass,
    _1_rice_meal,
    _1_Wagon,

    _1_bronze_statue,
    _1_gold_ore,
    _1_chariot,
    _1_apple_pie,
    _1_silk_cloth,
    #endregion

    #region Iron Age Resources

    _2_potato,
    _2_iron_ore,
    _2_boiled_potato,
    _2_iron_ingot,

    _2_sugar,
    _2_steak,
    _2_iron_pickaxe,
    _2_iron_axe,
    _2_iron_spear,
    _2_iron_sword,

    _2_marble,
    _2_iron_boot,
    _2_iron_breastplate,
    _2_iron_helmet,
    _2_iron_shield,

    _2_milk,
    _2_marble_statue,
    _2_crossbow,
    _2_tower,
    _2_medicine,

    _2_golden_jewelery,
    _2_ballista,
    _2_catapult,
    _2_castle,
    _2_vessel,
    #endregion

    #region Middle Age Resources

    _3_bean,
    _3_saltpeter,
    _3_sulfur,
    _3_paper,
    _3_orange,

    _3_gunpowder,
    _3_newspaper,
    _3_liquor,
    _3_book,
    _3_longbow,

    _3_apple_juice,
    _3_orange_juice,
    _3_grenade,
    _3_handgun,
    _3_rifle,

    _3_gastronomy_research,
    _3_mining_research,
    _3_combat_research,
    _3_structure_research,
    _3_economic_research,

    _3_iron_cannon,
    _3_ship,
    _3_telescope,
    _3_prosthetic_limb,
    _3_trebuchet,

    #endregion

    #region Industrial Age Resources

    _4_iron_plate,
    _4_iron_screw,
    _4_iron_rod,
    _4_copper_coils,
    _4_gear,
    _4_wire,
    _4_piston,
    
    _4_pump,
    _4_rail,
    _4_microscope,
    _4_piston_engine,
    _4_steam_engine,
    _4_refrigerator,
    _4_steam_car,
    _4_steam_bot,
    
    _4_sewing_machine,
    _4_vaccine,
    _4_steam_locomotive,
    _4_internal_combustionengine,
    _4_canned_soup,
    _4_canned_bean,
    _4_canned_orangejuice,
    
    _4_electric_motor,
    _4_fertilizer,
    _4_portland_cement,
    _4_automatic_rifle,
    _4_rechargable_battery,
    _4_crude_oil,
    _4_carbon_fiber,
    
    _4_canned_milk,
    _4_steel,
    _4_oil,
    _4_anti_airgun,
    _4_telephone,
    _4_light_bulb,
    _4_machine_guns,
    
    _4_steam_turbine,
    _4_bicycle,
    _4_aluminum,
    _4_petrol_car,
    _4_wind_turbine,
    _4_diesel_engine,
    _4_plastic,

    #endregion

    #region Early Modern Age Resources

    _5_circuit_board,
    _5_advanced_circuit,
    _5_liquid_rocketfuel,
    _5_armor_piercinground,
    _5_heavy_tankammo,
    _5_land_mine,
    _5_submarine,
    
    _5_zeppelin,
    _5_basic_aircraft,
    _5_artillery,
    _5_tank,
    _5_silicon,
    _5_silicon_panel,
    _5_penicilin,
    
    _5_transistor,
    _5_television,
    _5_ram,
    _5_computer,
    _5_ballistic_missile,
    _5_atomic_bomb,
    
    _5_video_game,
    _5_nuclear_power,
    _5_video_recorder,
    _5_solar_battery,
    _5_hovercraft,
    _5_hard_diskdrive,
    
    _5_personal_computer,
    _5_Integrated_circuit,
    _5_robot_arm,
    _5_micro_processor,
    _5_video_game_console,
    
    _5_cd,
    _5_laptop,
    _5_cellphone,
    _5_lithium_battery,
    _5_crypto_currency,
    #endregion
}