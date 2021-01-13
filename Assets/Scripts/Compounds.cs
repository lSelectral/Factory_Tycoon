using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class Compounds : MonoBehaviour, IPointerClickHandler
{
    public ScriptableCompound scriptableCompound;

    [SerializeField] private bool isAutomated;

    private BaseResources[] inputResources;
    private int[] inputAmounts;
    ItemType[] itemTypes;
    long foodAmount;
    long attackAmount;
    [SerializeField] private List<BaseResources> tempResourceList;
    private string partName;
    private long outputValue;
    private float buildTime;
    private float remainedBuildTime;
    private BaseResources product;
    private bool isCharging;
    private CompoundWorkingMode workingMode;

    private bool isLockedByContract;
    ContractBase[] contracts;
    Dictionary<ContractBase, bool> contractStatueCheckDictionary;

    float pricePerProduct;

    int compoundLevel = 1;
    double upgradeCost;
    bool isUpgradePanelActive;

    #region Properties
    public float PricePerProduct
    {
        get { return pricePerProduct; }
        set { pricePerProduct = value; }
    }

    public List<BaseResources> RemainedResources
    {
        get { return tempResourceList; }
        set { tempResourceList = value; }
    }

    public float RemainedBuildTime
    {
        get { return remainedBuildTime; }
        set { remainedBuildTime = value; }
    }

    public float BuildTime
    {
        get { return buildTime; }
        set { buildTime = value; }
    }

    public long OutputValue
    {
        get { return outputValue; }
        set { outputValue = value; }
    }

    public int CompoundLevel
    {
        get { return compoundLevel; }
        set { compoundLevel = value; }
    }

    public double UpgradeCost
    {
        get { return upgradeCost; }
        set { upgradeCost = value; }
    }

    public bool IsAutomated
    {
        get { return isAutomated; }
        set { isAutomated = value; }
    }

    public bool IsLockedByContract
    {
        get { return isLockedByContract; }
        set { isLockedByContract = value; }
    }

    public CompoundWorkingMode WorkingMode
    {
        get { return workingMode; }
        set { workingMode = value; workingModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString()); }
    }

    public BaseResources[] InputResources { get => inputResources; set => inputResources = value; }
    public BaseResources Product { get => product; set => product = value; }
    public ItemType[] ItemTypes { get => itemTypes; set => itemTypes = value; }
    public long AttackAmount { get => attackAmount; set => attackAmount = value; }
    public long FoodAmount { get => foodAmount; set => foodAmount = value; }
    public int[] InputAmounts { get => inputAmounts; set => inputAmounts = value; }
    public Dictionary<ContractBase, bool> ContractStatueCheckDictionary { get => contractStatueCheckDictionary; set => contractStatueCheckDictionary = value; }
    public ContractBase[] Contracts { get => contracts; set => contracts = value; }
    #endregion

    Image fillBar;
    Image icon;
    TextMeshProUGUI compoundNameText;
    Button btn;
    Transform subResourceIcons;
    Button upgradeBtn;
    Button workingModeBtn;
    TextMeshProUGUI workingModeText;

    void Start()
    {
        // TODO ONLY FOR DEBUG REMOVE IT
        IsAutomated = true;

        // Events
        GameManager.Instance.OnLevelUp += OnLevelUp;
        ResourceManager.Instance.OnPricePerProductChanged += OnPricePerProductChanged;
        UpgradeSystem.Instance.OnProductionEfficiencyChanged += OnProductionEfficiencyChanged;
        UpgradeSystem.Instance.OnProductionSpeedChanged += OnProductionSpeedChanged;
        UpgradeSystem.Instance.OnProductionYieldChanged += OnProductionYieldChanged;

        contracts = scriptableCompound.lockedByContracts;
        contractStatueCheckDictionary = new Dictionary<ContractBase, bool>();
        if (contracts != null)
        {
            for (int i = 0; i < contracts.Length; i++)
            {
                contractStatueCheckDictionary.Add(contracts[i], false);
            }
        }

        inputResources = scriptableCompound.inputResources;
        tempResourceList = scriptableCompound.inputResources.ToList();
        itemTypes = scriptableCompound.itemTypes;
        foodAmount = scriptableCompound.foodAmount;
        attackAmount = scriptableCompound.attackAmount;
        inputAmounts = scriptableCompound.inputAmounts;
        partName = scriptableCompound.resourceName;
        outputValue = scriptableCompound.outputValue;
        buildTime = scriptableCompound.collectTime;
        product = scriptableCompound.product;

        if (scriptableCompound.lockedByContracts != null && scriptableCompound.lockedByContracts.Length > 0)
            isLockedByContract = true;

        pricePerProduct = scriptableCompound.pricePerProduct;  
        workingMode = CompoundWorkingMode.sell;

        fillBar = transform.Find("FillBar").transform.Find("Fill").GetComponent<Image>();
        compoundNameText = transform.Find("Main_Panel").transform.Find("Mine_Name").GetComponent<TextMeshProUGUI>();
        icon = transform.Find("Main_Panel").Find("Icon").GetComponent<Image>();
        btn = transform.Find("Button").GetComponent<Button>();
        upgradeBtn = transform.Find("Upgrade_Btn").GetComponent<Button>();
        upgradeBtn.onClick.AddListener(() => ShowUpgradePanel());
        UpgradeCost = outputValue * pricePerProduct * 25;
        icon.sprite = scriptableCompound.icon;

        subResourceIcons = transform.Find("Main_Panel").Find("Sub_Resource_Icons");

        if (subResourceIcons != null)
        {
            
            for (int i = 0; i < inputResources.Length; i++)
            {
                var icon = Instantiate(ResourceManager.Instance.iconPrefab, subResourceIcons);
                icon.transform.Find("Image").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(inputResources[i]);
                icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = inputAmounts[i].ToString();
            }
        }

        compoundNameText.text = partName;
        workingModeBtn = transform.Find("WorkingModeBtn").GetComponent<Button>();
        workingModeText = workingModeBtn.GetComponentInChildren<TextMeshProUGUI>();
        workingModeBtn.onClick.AddListener(() => ChangeWorkingMode());

        SetWorkModeColor();

        if (scriptableCompound.unlockLevel > GameManager.Instance.CurrentLevel)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT LEVEL " + scriptableCompound.unlockLevel.ToString();
        }
        else if (contracts != null && contracts.Length > 0)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + scriptableCompound.lockedByContracts[0].contractName + " Contract";
        }
    }

    #region Event Methods
    private void OnPricePerProductChanged(object sender, ResourceManager.OnPricePerProductChangedEventArgs e)
    {
        PricePerProduct = ProductionManager.Instance.GET_OPTIMAL_PRICE_PER_PRODUCT(this) + scriptableCompound.basePricePerProduct;
    }

    private void OnProductionYieldChanged(object sender, UpgradeSystem.OnProductionYieldChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnProductionSpeedChanged(object sender, UpgradeSystem.OnProductionSpeedChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnProductionEfficiencyChanged(object sender, UpgradeSystem.OnProductionEfficiencyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        if (transform.Find("Level_Lock(Clone)") != null && scriptableCompound.unlockLevel == e.currentLevel)
        {
            Destroy(transform.Find("Level_Lock(Clone)").gameObject);

            // Check if there is contract for that compound hasn't completed yet
            if (contractStatueCheckDictionary.ContainsValue(false))
            {
                for (int i = 0; i < contracts.Length; i++)
                {
                    if (contractStatueCheckDictionary[contracts[i]] == false)
                    {
                        var lockText = Instantiate(GameManager.Instance.levelLock, transform);
                        lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + contracts[i].contractName + " Contract";
                    }
                }

            }
            else
            {
                var contracts = ContractManager.Instance.contracts;
                for (int i = 0; i < contracts.Length; i++)
                {
                    if (contracts[i].contractRewardType == ContractRewardType.automate && contracts[i].compoundsToUnlock[0] == scriptableCompound)
                    {
                        ContractManager.Instance.CreateContract(contracts[i]);
                    }
                }
            }
        }
    }
    #endregion

    void Update()
    {
        if (isAutomated)
        {
            Produce();
        }

        if (isCharging)
        {
            if (remainedBuildTime > 0)
            {
                remainedBuildTime -= Time.deltaTime * UpgradeSystem.Instance.ProductionSpeedMultiplier;
                fillBar.fillAmount = ((buildTime - remainedBuildTime) / buildTime);
            }
            else
            {
                isCharging = false;

                if (workingMode == CompoundWorkingMode.production)
                {
                    ResourceManager.Instance.AddResource(product, (outputValue * UpgradeSystem.Instance.ProductionYieldMultiplier));
                    StatSystem.Instance.PopupText(transform, outputValue, partName);
                }
                else if (workingMode == CompoundWorkingMode.sell)
                {
                    ResourceManager.Instance.Currency += outputValue * 1f * UpgradeSystem.Instance.ProductionYieldMultiplier* (pricePerProduct * 1f);
                    StatSystem.Instance.PopupText(transform, PricePerProduct, "Gold");
                }

                tempResourceList = inputResources.ToList();
                GameManager.Instance.AddXP(scriptableCompound.xpAmount);
                remainedBuildTime = 0;
                fillBar.fillAmount = 0;
            }
        }
    }

    void Produce()
    {
        if (!isCharging && workingMode != CompoundWorkingMode.stopProduction && transform.Find("Level_Lock(Clone)") == null)
        {
            var inputs = inputResources.Zip(inputAmounts, (resource, amount) => (Resource: resource, Amount: amount));
            foreach (var input in inputs)
            {
                if (ResourceManager.Instance.GetResourceAmount(input.Resource) >= input.Amount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier && tempResourceList.Contains(input.Resource))
                {
                    Debug.Log(string.Format("{0} added to {1} recipe", input.Resource, product));
                    Debug.Log(input.Resource + " added to recipe");
                    tempResourceList.Remove(input.Resource);
                    ResourceManager.Instance.ConsumeResource(input.Resource, (long)(input.Amount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier));

                    if (subResourceIcons != null)
                        subResourceIcons.GetChild(Array.IndexOf(inputResources, input.Resource)).GetChild(0).GetChild(0).gameObject.SetActive(true);
                }
                else if (ResourceManager.Instance.GetResourceAmount(input.Resource) < input.Amount)
                {
                    Debug.Log("Not enough " + input.Resource);
                }
            }

            if (tempResourceList.Count == 0)
            {
                Debug.Log(partName + " recipe completed");
                isCharging = true;
                remainedBuildTime = buildTime;

                foreach (var input in inputs)
                {
                    subResourceIcons.GetChild(Array.IndexOf(inputResources, input.Resource)).GetChild(0).GetChild(0).gameObject.SetActive(false);
                }
            }
        }
    }

    void ChangeWorkingMode()
    {
        Array a = Enum.GetValues(typeof(CompoundWorkingMode));
        int j = 0;
        for (int i = 0; i < a.Length; i++)
        {
            j = i + 1;
            if ((CompoundWorkingMode)(a.GetValue(i)) == workingMode)
                break;
        }
        if (j < a.Length)
            workingMode = (CompoundWorkingMode)a.GetValue(j);
        else
            workingMode = (CompoundWorkingMode)a.GetValue(j - a.Length);
        workingModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());
        SetWorkModeColor();
    }

    void SetWorkModeColor()
    {
        if (workingMode == CompoundWorkingMode.production)
            workingModeBtn.GetComponent<Image>().color = Color.green;
        else if (workingMode == CompoundWorkingMode.sell)
            workingModeBtn.GetComponent<Image>().color = Color.white;
        else if (workingMode == CompoundWorkingMode.stopProduction)
            workingModeBtn.GetComponent<Image>().color = Color.red;
    }

    void Upgrade()
    {
        var upgradeMultiplier = UpgradeSystem.Instance.upgradeMultiplier;
        var newLevel = CompoundLevel + upgradeMultiplier;
        var newOutputValue = UpgradeSystem.Instance.GetNewOutputAmount(upgradeMultiplier, OutputValue, CompoundLevel);
        var newBuildTime = UpgradeSystem.Instance.GetNewCollectTime(upgradeMultiplier, BuildTime);
        var newPricePerProduct = UpgradeSystem.Instance.GetNewPricePerProduct(upgradeMultiplier, pricePerProduct, CompoundLevel);
        var newUpgradeCost = UpgradeSystem.Instance.GetNewUpgradeCost(upgradeMultiplier, UpgradeCost, CompoundLevel);

        if (ResourceManager.Instance.Currency >= newUpgradeCost)
        {
            ResourceManager.Instance.Currency -= newUpgradeCost;

            CompoundLevel = newLevel;
            OutputValue = newOutputValue;
            BuildTime = newBuildTime;
            PricePerProduct = newPricePerProduct;
            ResourceManager.Instance.SetNewPricePerProduct(product, pricePerProduct);
            UpgradeCost = UpgradeSystem.Instance.GetNewUpgradeCost(1, newUpgradeCost, CompoundLevel);
            SetUpgradePanel(upgradeMultiplier);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Produce();
        if (isCharging) RemainedBuildTime -= .35f;
    }

    void ShowUpgradePanel()
    {
        UpgradeSystem.Instance.ShowUpgradePanel(SetUpgradePanel, Upgrade, isUpgradePanelActive, UpgradeCost, CompoundLevel);
    }

    void SetUpgradePanel(int levelUpgradeMultiplier)
    {
        UpgradeSystem.Instance.SetUpgradePanel(levelUpgradeMultiplier, outputValue, CompoundLevel, BuildTime, PricePerProduct, UpgradeCost, partName, false);
    }
}