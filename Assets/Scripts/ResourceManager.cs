using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngineInternal;
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
        public float currency;
        public float premiumCurrency;
    }

    public event EventHandler<OnCurrencyChangedEventArgs> OnCurrencyChanged;

    #endregion

    public bool isLoadingFromSaveFile;
    
    public Dictionary<BaseResources, long> resourceValueDict;
    public Dictionary<BaseResources, TextMeshProUGUI> resourceTextDict;

    [SerializeField] private GameObject resourcePanel;
    [SerializeField] private GameObject resourcePrefab;
    private float totalResource, currency, premiumCurrency;
    private float smoothCurrency, smoothPremiumCurency, smoothTotalResource;
    private float smoothVelocity, smoothVelocityPremiumCurrency, smoothVelocityTotalResource;
    public float smoothTime;

    private long ironOre, copperOre, siliconOre, coal, oil;
    private long reactorComponent, solarCell, powerCell, thrusterComponent, superConductor;
    private long ironIngot, copperIngot, siliconWafer, ironRod, ironPlate, ironScrew, wire, hardenedPlate;
    private long aiChip, circuitBoard, fiberOpticCable, integrationChip, motor, rotor, steelScrew, steelTube, steelPlate, steelIngot, steelBeam, stator, rubber, goldOre, goldIngot, metalGrid;
    [SerializeField] private TextMeshProUGUI copperOreText, siliconOreText, coalText, oilText, totalResourceText, currencyText, premiumCurrencyText;
    TextMeshProUGUI ironOreText, wireText, steelTubeTExt, steelIngotText, steelPlateText, aiChipText, circuitBoardText, ironIngotText, integrationChipText, hardenedPlateText, fiberOpticCableText, metalGridText, copperIngotText = new TextMeshProUGUI();
    TextMeshProUGUI steelScrewText, steelBeamText, statorText, rotorText, siliconWaferText, rubberText, motorText, ironScrewText, ironRodText, ironPlateText, goldIngotText, goldOreText, thrusterComponentText, superConductorText, reactorComponentText
        , solarCellText, powerCellText = new TextMeshProUGUI();

    private float smoothIronOre;

    public GameObject resourceIconPrefab;
    public GameObject resourceAmountTextPrefab;
    public GameObject iconPrefab;

    public Sprite ironOreSprite, copperOreSprite, siliconOreSprite, oilSprite, coalSprite, ironRodSprite, ironPlateSprite, ironScrewSprite, wireSprite, hardenedPlateSprite, ironIngotSprite, copperIngotSprite, siliconWaferSprite, goldOreSprite, reactorComponentSprite, solarCellSprite, powerCellSprite ,goldIngotSprite;
    public Sprite rotorSprite, motorSprite, statorSprite, metalGridSprite, circuitBoardSprite, steelIngotSprite, steelTubeSprite, steelBeamSprite, steelPlateSprite, rubberSprite, thrusterComponentSprite, superConductorSprite, steelScrewSprite, integrationChipSprite, aiChipSprite, fiberOpticCableSprite;

    IEnumerable<BaseResources> resources;

    #region Properties

    public float TotalResource
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

    public float Currency
    {
        get
        {
            return currency;
        }
        set
        {
            float newValue;
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

    public float PremiumCurrency
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

    public long IronOre { get => ironOre; set => ironOre = value; }
    public long CopperOre { get => copperOre; set => copperOre = value; }
    public long SiliconOre { get => siliconOre; set => siliconOre = value; }
    public long Coal { get => coal; set => coal = value; }
    public long Oil { get => oil; set => oil = value; }
    public long IronIngot { get => ironIngot; set => ironIngot = value; }
    public long CopperIngot { get => copperIngot; set => copperIngot = value; }
    public long SiliconWafer { get => siliconWafer; set => siliconWafer = value; }
    public long IronRod { get => ironRod; set => ironRod = value; }
    public long IronPlate { get => ironPlate; set => ironPlate = value; }
    public long IronScrew { get => ironScrew; set => ironScrew = value; }
    public long Wire { get => wire; set => wire = value; }
    public long HardenedPlate { get => hardenedPlate; set => hardenedPlate = value; }
    public long AiChip { get => aiChip; set => aiChip = value; }
    public long CircuitBoard { get => circuitBoard; set => circuitBoard = value; }
    public long FiberOpticCable { get => fiberOpticCable; set => fiberOpticCable = value; }
    public long IntegrationChip { get => integrationChip; set => integrationChip = value; }
    public long Motor { get => motor; set => motor = value; }
    public long Rotor { get => rotor; set => rotor = value; }
    public long SteelScrew { get => steelScrew; set => steelScrew = value; }
    public long SteelTube { get => steelTube; set => steelTube = value; }
    public long SteelPlate { get => steelPlate; set => steelPlate = value; }
    public long SteelIngot { get => steelIngot; set => steelIngot = value; }
    public long SteelBeam { get => steelBeam; set => steelBeam = value; }
    public long Stator { get => stator; set => stator = value; }
    public long Rubber { get => rubber; set => rubber = value; }
    public long GoldOre { get => goldOre; set => goldOre = value; }
    public long GoldIngot { get => goldIngot; set => goldIngot = value; }
    public long MetalGrid { get => metalGrid; set => metalGrid = value; }
    public long ReactorComponent { get => reactorComponent; set => reactorComponent = value; }
    public long SolarCell { get => solarCell; set => solarCell = value; }
    public long PowerCell { get => powerCell; set => powerCell = value; }
    public long ThrusterComponent { get => thrusterComponent; set => thrusterComponent = value; }
    public long SuperConductor { get => superConductor; set => superConductor = value; }

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

    /// <summary>
    /// Convert variable name to human readable seperated name
    /// </summary>
    /// <param name="oldString">Previous variable name</param>
    /// <returns></returns>
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

                returnString =  char.ToUpper(firstLetter) + oldString.Substring(1,index-1) + " " + oldString.Substring(index);

                return returnString;
            }
            index += 1;
        }
        if (returnString == "")
        {
            return char.ToUpper(oldString[0]) + oldString.Substring(1, oldString.Length-1);
        }

        return "";
    }

    private void Awake()
    {
        totalResourceText.text = totalResource.ToString();

        resourceValueDict = new Dictionary<BaseResources, long>
        {
            {BaseResources.coal, coal },
            {BaseResources.copperIngot, copperIngot},
            {BaseResources.ironOre, ironOre },
            {BaseResources.ironIngot, ironIngot },
            {BaseResources.ironPlate, ironPlate },
            {BaseResources.hardenedPlate, hardenedPlate },
            {BaseResources.copperOre, copperOre },
            {BaseResources.ironRod, ironRod },
            {BaseResources.ironScrew, ironScrew },
            {BaseResources.oil, oil },
            {BaseResources.siliconOre, siliconOre },
            {BaseResources.siliconWafer, siliconWafer },
            {BaseResources.wire, wire },
            {BaseResources.aiChip, aiChip },
            {BaseResources.circuitBoard, circuitBoard },
            {BaseResources.fiberOpticCable, fiberOpticCable },
            {BaseResources.integrationChip, integrationChip },
            {BaseResources.motor, motor },
            {BaseResources.rubber, rubber },
            {BaseResources.rotor, rotor },
            {BaseResources.stator, stator },
            {BaseResources.steelBeam, steelBeam },
            {BaseResources.steelIngot, steelIngot },
            {BaseResources.steelPlate, steelPlate },
            {BaseResources.steelTube, steelTube },
            {BaseResources.steelScrew, steelScrew },
            {BaseResources.goldOre, goldOre },
            {BaseResources.goldIngot, goldIngot },
            {BaseResources.metalGrid, metalGrid },
            {BaseResources.reactorComponent, reactorComponent },
            {BaseResources.solarCell, solarCell },
            {BaseResources.superConductor, superConductor },
            {BaseResources.thrusterComponent, thrusterComponent },
            {BaseResources.powerCell, powerCell },

        };

        resourceTextDict = new Dictionary<BaseResources, TextMeshProUGUI>
        {
            {BaseResources.coal, coalText },
            {BaseResources.copperIngot, copperIngotText},
            {BaseResources.ironOre, ironOreText },
            {BaseResources.ironIngot, ironIngotText },
            {BaseResources.ironPlate, ironPlateText },
            {BaseResources.hardenedPlate, hardenedPlateText },
            {BaseResources.copperOre, copperOreText },
            {BaseResources.ironRod, ironRodText },
            {BaseResources.ironScrew, ironScrewText },
            {BaseResources.oil, oilText },
            {BaseResources.siliconOre, siliconOreText },
            {BaseResources.siliconWafer, siliconWaferText },
            {BaseResources.wire, wireText },
            {BaseResources.aiChip, aiChipText },
            {BaseResources.circuitBoard, circuitBoardText },
            {BaseResources.fiberOpticCable, fiberOpticCableText },
            {BaseResources.integrationChip, integrationChipText },
            {BaseResources.motor, motorText },
            {BaseResources.rubber, rubberText },
            {BaseResources.rotor, rotorText },
            {BaseResources.stator, statorText },
            {BaseResources.steelBeam, steelBeamText },
            {BaseResources.steelIngot, steelIngotText },
            {BaseResources.steelPlate, steelPlateText },
            {BaseResources.steelTube, steelTubeTExt },
            {BaseResources.steelScrew, steelScrewText },
            {BaseResources.goldIngot, goldIngotText },
            {BaseResources.goldOre, goldOreText },
            {BaseResources.metalGrid, metalGridText },
            {BaseResources.reactorComponent, reactorComponentText },
            {BaseResources.solarCell, solarCellText },
            {BaseResources.superConductor, superConductorText },
            {BaseResources.thrusterComponent, thrusterComponentText },
            {BaseResources.powerCell, powerCellText },
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
            resourceInfo.transform.GetChild(0).GetComponent<Image>().sprite = GetSpriteFromResource(resource);
            resourceInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = GetValidName(resource.ToString());

            
            resourceInfo.transform.GetChild(3).GetChild(0).GetComponent<Button>().onClick.AddListener(() => { arrayCounter -= 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = CurrencyToString(resourceIncrementArray[arrayCounter]); });
            resourceInfo.transform.GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() => { arrayCounter += 1; arrayCounter = Mathf.Clamp(arrayCounter, 0, resourceIncrementArray.Length - 1); text.text = CurrencyToString(resourceIncrementArray[arrayCounter]); });

            resourceInfo.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => { AddResource(resource, resourceIncrementArray[arrayCounter]); });
            resourceTextDict[resource] = resourceInfo.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            resourceTextDict[resource].text =(GetResourceAmount(resource)).ToString();
        }
    }

    void Update()
    {
        smoothCurrency = Mathf.SmoothDamp(smoothCurrency, (float)currency, ref smoothVelocity, smoothTime);
        currencyText.text = CurrencyToString(smoothCurrency);

        smoothPremiumCurency = Mathf.SmoothDamp(smoothPremiumCurency, (float)premiumCurrency, ref smoothVelocityPremiumCurrency, smoothTime);
        premiumCurrencyText.text = CurrencyToString(smoothPremiumCurency);

        smoothTotalResource = Mathf.SmoothDamp(smoothTotalResource, totalResource, ref smoothVelocityTotalResource, smoothTime);
        totalResourceText.text = "Total Resource\n" + CurrencyToString((smoothTotalResource), 0);
    }

    public float AddResource(BaseResources resource, long amount)
    {
        resourceValueDict[resource] += amount;
        TotalResource += amount;
        OnResourceAmountChanged(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource] });
        resourceTextDict[resource].text = CurrencyToString(resourceValueDict[resource]);
        return amount;
    }

    public void ConsumeResource(BaseResources resource, long amount)
    {
        resourceValueDict[resource] -= amount;
        TotalResource -= amount;
        resourceTextDict[resource].text = CurrencyToString(resourceValueDict[resource]);
        OnResourceAmountChanged(this, new OnResourceAmountChangedEventArgs() { resource = resource, resourceAmount = resourceValueDict[resource] });
    }

    public long GetResourceAmount(BaseResources resource)
    {
        return resourceValueDict[resource];
    }

    public Sprite GetSpriteFromResource(BaseResources resource)
    {
        switch (resource)
        {
            case BaseResources.ironOre:
                return ironOreSprite;
            case BaseResources.copperOre:
                return copperOreSprite;
            case BaseResources.siliconOre:
                return siliconOreSprite;
            case BaseResources.coal:
                return coalSprite;
            case BaseResources.oil:
                return oilSprite;
            case BaseResources.ironIngot:
                return ironIngotSprite;
            case BaseResources.copperIngot:
                return copperIngotSprite;
            case BaseResources.siliconWafer:
                return siliconWaferSprite;
            case BaseResources.ironRod:
                return ironRodSprite;
            case BaseResources.ironPlate:
                return ironPlateSprite;
            case BaseResources.ironScrew:
                return ironScrewSprite;
            case BaseResources.wire:
                return wireSprite;
            case BaseResources.hardenedPlate:
                return hardenedPlateSprite;
            case BaseResources.rotor:
                return rotorSprite;
            case BaseResources.steelIngot:
                return steelIngotSprite;
            case BaseResources.steelPlate:
                return steelPlateSprite;
            case BaseResources.steelTube:
                return steelTubeSprite;
            case BaseResources.steelScrew:
                return steelScrewSprite;
            case BaseResources.steelBeam:
                return steelBeamSprite;
            case BaseResources.rubber:
                return rubberSprite;
            case BaseResources.stator:
                return statorSprite;
            case BaseResources.motor:
                return motorSprite;
            case BaseResources.fiberOpticCable:
                return fiberOpticCableSprite;
            case BaseResources.circuitBoard:
                return circuitBoardSprite;
            case BaseResources.integrationChip:
                return integrationChipSprite;
            case BaseResources.aiChip:
                return aiChipSprite;
            case BaseResources.goldOre:
                return goldOreSprite;
            case BaseResources.goldIngot:
                return goldIngotSprite;
            case BaseResources.metalGrid:
                return metalGridSprite;
            case BaseResources.powerCell:
                return powerCellSprite;
            case BaseResources.reactorComponent:
                return reactorComponentSprite;
            case BaseResources.thrusterComponent:
                return thrusterComponentSprite;
            case BaseResources.solarCell:
                return solarCellSprite;
            case BaseResources.superConductor:
                return superConductorSprite;
        }
        return null;
    }

    public void ADD_SCREW() { AddResource(BaseResources.ironScrew,20); }
    public void ADD_ROD() { AddResource(BaseResources.ironRod, 20); }
    public void ADD_IronOre() { AddResource(BaseResources.ironOre ,20000); }
    public void ADD_HardenedPlate() { AddResource(BaseResources.hardenedPlate, 20); }
}



public enum BaseResources
{
    ironOre = 1,
    copperOre = 2,
    siliconOre = 3,
    coal = 4,
    oil = 5,
    goldOre = 120,

    ironIngot = 6,
    copperIngot = 7,
    siliconWafer = 8,
    goldIngot = 99,

    // Created from only iron
    ironRod = 9,
    ironPlate = 10,
    ironScrew = 11,

    // Created from copper
    wire = 12,

    // 2 iron rod and 8 screw
    hardenedPlate = 15,

    // 6 iron rod and 30 wire
    rotor,

    // 2 iron ore and 2 coal
    steelIngot,

    // Created from steel
    steelPlate,
    steelTube,
    steelScrew,
    steelBeam,

    metalGrid,

    reactorComponent,
    thrusterComponent,
    solarCell,
    superConductor,
    powerCell,


    // created from 4 oil
    rubber,

    // Made from iron rod and wire
    stator,

    // Made from stator, rotor, 
    motor,

    // Made from caterium and steel alloy
    fiberOpticCable,

    // Made from plastic, wire and steel ingot
    circuitBoard,

    // Made from plastic, caterium, fiber optic cable
    integrationChip,

    // 100000000 researchPoint
    aiChip,
}