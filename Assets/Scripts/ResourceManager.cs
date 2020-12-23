using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngineInternal;
using System;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Internal;

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

    #endregion

    public bool isLoadingFromSaveFile;
    
    public Dictionary<BaseResources, long> resourceValueDict;
    public Dictionary<BaseResources, TextMeshProUGUI> resourceTextDict;

    public List<ScriptableMine> scriptableMines;
    public List<ScriptableCompound> scriptableCompounds;

    [SerializeField] private GameObject resourcePanel;
    [SerializeField] private GameObject resourcePrefab;
    double currency,totalResource, premiumCurrency;
    double smoothCurrency,smoothPremiumCurency, smoothTotalResource;
    double smoothVelocity,smoothVelocityPremiumCurrency, smoothVelocityTotalResource;
    public float smoothTime;
    public double currencySmoothTime;

    private long ironOre, copperOre, siliconOre, coal, oil;
    private long reactorComponent, solarCell, powerCell, thrusterComponent, superConductor;
    private long ironIngot, copperIngot, siliconWafer, ironRod, ironPlate, ironScrew, wire, hardenedPlate;
    private long aiChip, circuitBoard, fiberOpticCable, integrationChip, motor, rotor, steelScrew, steelTube, 
        steelPlate, steelIngot, steelBeam, stator, rubber, goldOre, goldIngot, metalGrid;
    [SerializeField] private TextMeshProUGUI /*totalResourceText,*/ currencyText, premiumCurrencyText;
    TextMeshProUGUI ironOreText, wireText, steelTubeTExt, steelIngotText, steelPlateText, aiChipText, circuitBoardText;
    TextMeshProUGUI ironIngotText, integrationChipText, hardenedPlateText, fiberOpticCableText, metalGridText, copperIngotText = new TextMeshProUGUI();
    TextMeshProUGUI steelScrewText, steelBeamText, statorText, rotorText, siliconWaferText, rubberText, motorText, ironScrewText, ironRodText, ironPlateText, goldIngotText, 
        goldOreText, thrusterComponentText, superConductorText, reactorComponentText, solarCellText, powerCellText = new TextMeshProUGUI();

    #region Stone Age Resources

    long stone, berry, stick, leaf, hut, spear, torch, axe, fire, leaf_cloth, pickaxe, rope, bonefire, hammer, animal_trap, pouch, leather_cloth, wheel, arrow, bow;

    TextMeshProUGUI stoneText, berryText, stickText, leafText, hutText, spearText, torchText, axeText, fireText, 
        leaf_clothText, pickaxeText, ropeText, bonefireText, bowText, arrowText, hammerText, animal_trapText, pouchText, leather_clothText, wheelText;

    #endregion

    public GameObject resourceIconPrefab;
    public GameObject resourceAmountTextPrefab;
    public GameObject iconPrefab;

    IEnumerable<BaseResources> resources;

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

    //public long IronOre { get => ironOre; set { ironOre = value; resourceTextDict[BaseResources.ironOre].text = ironOre.ToString(); } }
    //public long CopperOre { get => copperOre; set { copperOre = value; resourceTextDict[BaseResources.copperOre].text = copperOre.ToString(); } }
    //public long SiliconOre { get => siliconOre; set { siliconOre = value; resourceTextDict[BaseResources.siliconOre].text = siliconOre.ToString(); } }
    //public long Coal { get => coal; set { coal = value; resourceTextDict[BaseResources.coal].text = coal.ToString(); } }
    //public long Oil { get => oil; set { oil = value; resourceTextDict[BaseResources.oil].text = oil.ToString(); } }
    //public long IronIngot { get => ironIngot; set { ironIngot = value; resourceTextDict[BaseResources.ironIngot].text = ironIngot.ToString(); } }
    //public long CopperIngot { get => copperIngot; set { copperIngot = value; resourceTextDict[BaseResources.copperIngot].text = copperIngot.ToString(); } }
    //public long SiliconWafer { get => siliconWafer; set { siliconWafer = value; resourceTextDict[BaseResources.siliconWafer].text = siliconWafer.ToString(); } }
    //public long IronRod { get => ironRod; set { ironRod = value; resourceTextDict[BaseResources.ironRod].text = ironRod.ToString(); } }
    //public long IronPlate { get => ironPlate; set { ironPlate = value; resourceTextDict[BaseResources.ironPlate].text = ironPlate.ToString(); } }
    //public long IronScrew { get => ironScrew; set { ironScrew = value; resourceTextDict[BaseResources.ironScrew].text = ironScrew.ToString(); } }
    //public long Wire { get => wire; set { wire = value; resourceTextDict[BaseResources.wire].text = wire.ToString(); } }
    //public long HardenedPlate { get => hardenedPlate; set { hardenedPlate = value; resourceTextDict[BaseResources.hardenedPlate].text = hardenedPlate.ToString(); } }
    //public long AiChip { get => aiChip; set { aiChip = value; resourceTextDict[BaseResources.aiChip].text = aiChip.ToString(); } }
    //public long CircuitBoard { get => circuitBoard; set { circuitBoard = value; resourceTextDict[BaseResources.circuitBoard].text = circuitBoard.ToString(); } }
    //public long FiberOpticCable { get => fiberOpticCable; set { fiberOpticCable = value; resourceTextDict[BaseResources.fiberOpticCable].text = fiberOpticCable.ToString(); } }
    //public long IntegrationChip { get => integrationChip; set { integrationChip = value; resourceTextDict[BaseResources.integrationChip].text = integrationChip.ToString(); } }
    //public long Motor { get => motor; set { motor = value; resourceTextDict[BaseResources.motor].text = motor.ToString(); } }
    //public long Rotor { get => rotor; set { rotor = value; resourceTextDict[BaseResources.rotor].text = rotor.ToString(); } }
    //public long SteelScrew { get => steelScrew; set { steelScrew = value; resourceTextDict[BaseResources.steelScrew].text = steelScrew.ToString(); } }
    //public long SteelTube { get => steelTube; set { steelTube = value; resourceTextDict[BaseResources.steelTube].text = steelTube.ToString(); } }
    //public long SteelPlate { get => steelPlate; set { steelPlate = value; resourceTextDict[BaseResources.steelPlate].text = steelPlate.ToString(); } }
    //public long SteelIngot { get => steelIngot; set { steelIngot = value; resourceTextDict[BaseResources.steelIngot].text = steelIngot.ToString(); } }
    //public long SteelBeam { get => steelBeam; set { steelBeam = value; resourceTextDict[BaseResources.steelBeam].text = steelBeam.ToString(); } }
    //public long Stator { get => stator; set { stator = value; resourceTextDict[BaseResources.stator].text = stator.ToString(); } }
    //public long Rubber { get => rubber; set { rubber = value; resourceTextDict[BaseResources.rubber].text = rubber.ToString(); } }
    //public long GoldOre { get => goldOre; set { goldOre = value; resourceTextDict[BaseResources.goldOre].text = goldOre.ToString(); } }
    //public long GoldIngot { get => goldIngot; set { goldIngot = value; resourceTextDict[BaseResources.goldIngot].text = goldIngot.ToString(); } }
    //public long MetalGrid { get => metalGrid; set { metalGrid = value; resourceTextDict[BaseResources.metalGrid].text = metalGrid.ToString(); } }
    //public long ReactorComponent { get => reactorComponent; set { reactorComponent = value; resourceTextDict[BaseResources.reactorComponent].text = reactorComponent.ToString(); } }
    //public long SolarCell { get => solarCell; set { solarCell = value; resourceTextDict[BaseResources.solarCell].text = solarCell.ToString(); } }
    //public long PowerCell { get => powerCell; set { powerCell = value; resourceTextDict[BaseResources.powerCell].text = powerCell.ToString(); } }
    //public long ThrusterComponent { get => thrusterComponent; set { thrusterComponent = value; resourceTextDict[BaseResources.thrusterComponent].text = thrusterComponent.ToString(); } }
    //public long SuperConductor { get => superConductor; set { superConductor = value; resourceTextDict[BaseResources.superConductor].text = superConductor.ToString(); } }

    #region Stone Age Properties

    public long Stone { get => stone; set { stone = value; resourceTextDict[BaseResources._0_stone].text = stone.ToString(); } }
    public long Berry { get => berry; set { berry = value; resourceTextDict[BaseResources._0_berry].text = berry.ToString(); } }
    public long Stick { get => stick; set { stick = value; resourceTextDict[BaseResources._0_stick].text = stick.ToString(); } }
    public long Leaf { get => leaf; set { leaf = value; resourceTextDict[BaseResources._0_leaf].text = leaf.ToString(); } }
    public long Hut { get => hut; set { hut = value; resourceTextDict[BaseResources._0_hut].text = hut.ToString(); } }
    public long Spear { get => spear; set { spear = value; resourceTextDict[BaseResources._0_spear].text = spear.ToString(); } }
    public long Torch { get => torch; set { torch = value; resourceTextDict[BaseResources._0_torch].text = torch.ToString(); } }
    public long Axe { get => axe; set { axe = value; resourceTextDict[BaseResources._0_axe].text = axe.ToString(); } }
    public long Fire { get => fire; set { fire = value; resourceTextDict[BaseResources._0_fire].text = fire.ToString(); } }
    public long Leaf_cloth { get => leaf_cloth; set { leaf_cloth = value; resourceTextDict[BaseResources._0_leaf_cloth].text = leaf_cloth.ToString(); } }
    public long Pickaxe { get => pickaxe; set { pickaxe = value; resourceTextDict[BaseResources._0_pickaxe].text = pickaxe.ToString(); } }
    public long Rope { get => rope; set { rope = value; resourceTextDict[BaseResources._0_rope].text = rope.ToString(); } }
    public long Bonefire { get => bonefire; set { bonefire = value; resourceTextDict[BaseResources._0_bonefire].text = bonefire.ToString(); } }
    public long Hammer { get => hammer; set { hammer = value; resourceTextDict[BaseResources._0_hammer].text = hammer.ToString(); } }
    public long Animal_trap { get => animal_trap; set { animal_trap = value; resourceTextDict[BaseResources._0_animal_trap].text = animal_trap.ToString(); } }
    public long Pouch { get => pouch; set { pouch = value; resourceTextDict[BaseResources._0_pouch].text = pouch.ToString(); } }
    public long Leather_cloth { get => leather_cloth; set { leather_cloth = value; resourceTextDict[BaseResources._0_leather_cloth].text = leather_cloth.ToString(); } }
    public long Wheel { get => wheel; set { wheel = value; resourceTextDict[BaseResources._0_wheel].text = wheel.ToString(); } }
    public long Arrow { get => arrow; set { arrow = value; resourceTextDict[BaseResources._0_arrow].text = arrow.ToString(); } }
    public long Bow { get => bow; set { bow = value; resourceTextDict[BaseResources._0_bow].text = bow.ToString(); } }

    #endregion

    #endregion

    private readonly string[] suffix = new string[] { "", "K", "M", "G", "T", "P", "E", "AA", "AB", "BA", "BB" , "CA", "CB", "CC"}; // kilo, mega, giga, terra, penta, exa
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

    private void Awake()
    {
        scriptableMines = new List<ScriptableMine>();
        scriptableCompounds = new List<ScriptableCompound>();

        for (int i = 0; i < ProductionManager.Instance.Assets.Length; i++)
        {
            var asset = ProductionManager.Instance.Assets[i];

            if (asset as ScriptableObject != null)
            {
                var sc = asset as ScriptableObject;
                if (sc.GetType() == typeof(ScriptableMine) && (sc as ScriptableMine).ageBelongsTo == Age._0_stoneAge)
                    scriptableMines.Add(sc as ScriptableMine);
                else if (sc.GetType() == typeof(ScriptableCompound) && (sc as ScriptableCompound).ageBelongsTo == Age._0_stoneAge)
                    scriptableCompounds.Add(sc as ScriptableCompound);
            }
        }

        //totalResourceText.text = totalResource.ToString();

        resourceValueDict = new Dictionary<BaseResources, long>
        {
            //{BaseResources.coal, coal },
            //{BaseResources.copperIngot, copperIngot},
            //{BaseResources.ironOre, ironOre },
            //{BaseResources.ironIngot, ironIngot },
            //{BaseResources.ironPlate, ironPlate },
            //{BaseResources.hardenedPlate, hardenedPlate },
            //{BaseResources.copperOre, copperOre },
            //{BaseResources.ironRod, ironRod },
            //{BaseResources.ironScrew, ironScrew },
            //{BaseResources.oil, oil },
            //{BaseResources.siliconOre, siliconOre },
            //{BaseResources.siliconWafer, siliconWafer },
            //{BaseResources.wire, wire },
            //{BaseResources.aiChip, aiChip },
            //{BaseResources.circuitBoard, circuitBoard },
            //{BaseResources.fiberOpticCable, fiberOpticCable },
            //{BaseResources.integrationChip, integrationChip },
            //{BaseResources.motor, motor },
            //{BaseResources.rubber, rubber },
            //{BaseResources.rotor, rotor },
            //{BaseResources.stator, stator },
            //{BaseResources.steelBeam, steelBeam },
            //{BaseResources.steelIngot, steelIngot },
            //{BaseResources.steelPlate, steelPlate },
            //{BaseResources.steelTube, steelTube },
            //{BaseResources.steelScrew, steelScrew },
            //{BaseResources.goldOre, goldOre },
            //{BaseResources.goldIngot, goldIngot },
            //{BaseResources.metalGrid, metalGrid },
            //{BaseResources.reactorComponent, reactorComponent },
            //{BaseResources.solarCell, solarCell },
            //{BaseResources.superConductor, superConductor },
            //{BaseResources.thrusterComponent, thrusterComponent },
            //{BaseResources.powerCell, powerCell },

            {BaseResources._0_berry, berry},    
            {BaseResources._0_stick, stick },
            {BaseResources._0_leaf, leaf },
            {BaseResources._0_hut, hut },
            {BaseResources._0_stone, stone },
            {BaseResources._0_spear, spear },
            {BaseResources._0_torch, torch },
            {BaseResources._0_axe, axe },
            {BaseResources._0_fire, fire },
            {BaseResources._0_leaf_cloth, leaf_cloth },
            {BaseResources._0_pickaxe, pickaxe },
            {BaseResources._0_rope, rope },
            {BaseResources._0_bonefire, bonefire },
            {BaseResources._0_arrow, arrow },
            {BaseResources._0_bow, bow },
            {BaseResources._0_hammer, hammer },
            {BaseResources._0_animal_trap, animal_trap },
            {BaseResources._0_pouch, pouch },
            {BaseResources._0_leather_cloth, leather_cloth },
            {BaseResources._0_wheel, wheel },
        };

        resourceTextDict = new Dictionary<BaseResources, TextMeshProUGUI>
        {
            //{BaseResources.coal, coalText },
            //{BaseResources.copperIngot, copperIngotText},
            //{BaseResources.ironOre, ironOreText },
            //{BaseResources.ironIngot, ironIngotText },
            //{BaseResources.ironPlate, ironPlateText },
            //{BaseResources.hardenedPlate, hardenedPlateText },
            //{BaseResources.copperOre, copperOreText },
            //{BaseResources.ironRod, ironRodText },
            //{BaseResources.ironScrew, ironScrewText },
            //{BaseResources.oil, oilText },
            //{BaseResources.siliconOre, siliconOreText },
            //{BaseResources.siliconWafer, siliconWaferText },
            //{BaseResources.wire, wireText },
            //{BaseResources.aiChip, aiChipText },
            //{BaseResources.circuitBoard, circuitBoardText },
            //{BaseResources.fiberOpticCable, fiberOpticCableText },
            //{BaseResources.integrationChip, integrationChipText },
            //{BaseResources.motor, motorText },
            //{BaseResources.rubber, rubberText },
            //{BaseResources.rotor, rotorText },
            //{BaseResources.stator, statorText },
            //{BaseResources.steelBeam, steelBeamText },
            //{BaseResources.steelIngot, steelIngotText },
            //{BaseResources.steelPlate, steelPlateText },
            //{BaseResources.steelTube, steelTubeTExt },
            //{BaseResources.steelScrew, steelScrewText },
            //{BaseResources.goldIngot, goldIngotText },
            //{BaseResources.goldOre, goldOreText },
            //{BaseResources.metalGrid, metalGridText },
            //{BaseResources.reactorComponent, reactorComponentText },
            //{BaseResources.solarCell, solarCellText },
            //{BaseResources.superConductor, superConductorText },
            //{BaseResources.thrusterComponent, thrusterComponentText },
            //{BaseResources.powerCell, powerCellText },

            #region Stone Age Resources Text
            {BaseResources._0_berry, berryText},
            {BaseResources._0_stick, stickText },
            {BaseResources._0_leaf, leafText },
            {BaseResources._0_stone, stoneText },
            {BaseResources._0_hut, hutText },
            {BaseResources._0_spear, spearText },
            {BaseResources._0_torch, torchText },
            {BaseResources._0_axe, axeText },
            {BaseResources._0_fire, fireText },
            {BaseResources._0_leaf_cloth, leaf_clothText },
            {BaseResources._0_pickaxe, pickaxeText },
            {BaseResources._0_rope, ropeText },
            {BaseResources._0_bonefire, bonefireText },
            {BaseResources._0_bow, bowText },
            {BaseResources._0_arrow, arrowText },
            {BaseResources._0_hammer, hammerText },
            {BaseResources._0_animal_trap, animal_trapText},
            {BaseResources._0_pouch, pouchText },
            {BaseResources._0_leather_cloth, leather_clothText},
            {BaseResources._0_wheel, wheelText },

            #endregion
        };

        resources = Enum.GetValues(typeof(BaseResources)).Cast<BaseResources>();

        int[] resourceIncrementArray = { 1, 5, 10, 100, 1000, 10000, 100000 };

        // Add Resource to panel
        foreach (var resource in resources)
        {
            int arrayCounter = 0;

            var resourceInfo = Instantiate(resourcePrefab, resourcePanel.transform.GetChild(0));

            var text = resourceInfo.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>();
            text.text = "1";

            var resourceName = resourceInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            for (int i = 0; i < scriptableMines.Count; i++)
            {
                if (scriptableMines[i].baseResource == resource)
                    resourceName.text = scriptableMines[i].resourceName;
            }
            for (int i = 0; i < scriptableCompounds.Count; i++)
            {
                if (scriptableCompounds[i].product == resource)
                    resourceName.text = scriptableCompounds[i].partName;
            }

            resourceInfo.transform.GetChild(0).GetComponent<Image>().sprite = GetSpriteFromResource(resource);

            resourceInfo.transform.GetChild(3).GetChild(0).GetComponent<Button>().onClick.AddListener(() => 
            { arrayCounter -= 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = CurrencyToString(resourceIncrementArray[arrayCounter]); });
            resourceInfo.transform.GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() => 
            { arrayCounter += 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = CurrencyToString(resourceIncrementArray[arrayCounter]); });

            resourceInfo.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => { AddResource(resource, resourceIncrementArray[arrayCounter]); });
            resourceTextDict[resource] = resourceInfo.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            resourceTextDict[resource].text =(GetResourceAmount(resource)).ToString();
        }
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

    public float AddResource(BaseResources resource, long amount, bool isLoadingFromSave = false)
    {
        resourceValueDict[resource] += amount;
        if (!isLoadingFromSave)
            TotalResource += amount;
        OnResourceAmountChanged?.Invoke(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource] });
        resourceTextDict[resource].text = CurrencyToString(resourceValueDict[resource]);

        return amount;
    }

    public void ConsumeResource(BaseResources resource, long amount)
    {
        resourceValueDict[resource] -= amount;
        TotalResource -= amount;
        resourceTextDict[resource].text = CurrencyToString(resourceValueDict[resource]);
        OnResourceAmountChanged?.Invoke(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource] });
    }

    public long GetResourceAmount(BaseResources resource)
    {
        return resourceValueDict[resource];
    }

    public Sprite GetSpriteFromResource(BaseResources resource)
    {
        for (int i = 0; i < scriptableMines.Count; i++)
        {
            if (scriptableMines[i].baseResource == resource)
                return scriptableMines[i].icon;
        }
        for (int i = 0; i < scriptableCompounds.Count; i++)
        {
            if (scriptableCompounds[i].product == resource)
                return scriptableCompounds[i].icon;
        }
        return null;
    }

    public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed= Mathf.Infinity)
    {
        var deltaTime = Time.deltaTime;
        //smoothTime = Mathf.Max(0.0001f, smoothTime);
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

public enum BaseResources
{
    #region Stone Age Resources
    _0_berry,
    _0_stone,
    _0_stick,
    _0_leaf,
    _0_hut,
    _0_spear,
    _0_torch,
    _0_axe,
    _0_fire,
    _0_leaf_cloth,
    _0_pickaxe,
    _0_rope,
    _0_bonefire,
    _0_arrow,
    _0_bow,
    _0_hammer,
    _0_animal_trap,
    _0_pouch,
    _0_leather_cloth,
    _0_wheel,
    #endregion


}