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
    [SerializeField] private List<BaseResources> tempResourceList;
    private string partName;
    private long outputValue;
    private float buildTime;
    private float remainedBuildTime;
    private BaseResources product;
    private bool isCharging;
    private CompoundWorkingMode workingMode;
    private bool isLockedByContract;
    float pricePerProduct;

    int compoundLevel = 1;
    double upgradeCost;

    bool isUpgradePanelActive;

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

    Image fillBar;
    Image icon;
    TextMeshProUGUI mineNameText;
    Button btn;
    Transform subResourceIcons;
    Button upgradeBtn;
    Button workingModeBtn;
    TextMeshProUGUI workingModeText;

    void Start()
    {
        IsAutomated = false;
        GameManager.Instance.OnLevelUp += OnLevelUp;
        UpgradeSystem.Instance.OnProductionEfficiencyChanged += OnProductionEfficiencyChanged;
        UpgradeSystem.Instance.OnProductionSpeedChanged += OnProductionSpeedChanged;
        UpgradeSystem.Instance.OnProductionYieldChanged += OnProductionYieldChanged;

        if (scriptableCompound.unlockLevel > GameManager.Instance.CurrentLevel)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT LEVEL " + scriptableCompound.unlockLevel.ToString();
        }
        else if (scriptableCompound.isLockedByContract)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + scriptableCompound.lockedByContract.contractName + " Contract";
        }
        
        inputResources = scriptableCompound.inputResources;
        tempResourceList = scriptableCompound.inputResources.ToList();
        inputAmounts = scriptableCompound.inputAmounts;
        partName = scriptableCompound.partName;
        outputValue = scriptableCompound.outputValue;
        buildTime = scriptableCompound.buildTime;
        product = scriptableCompound.product;
        isLockedByContract = scriptableCompound.isLockedByContract;

        pricePerProduct = ProductionManager.Instance.GetPricePerProductForCompound(inputResources,inputAmounts,BuildTime) + scriptableCompound.basePricePerProduct;  
        workingMode = CompoundWorkingMode.sell;

        fillBar = transform.Find("FillBar").transform.Find("Fill").GetComponent<Image>();
        mineNameText = transform.Find("Main_Panel").transform.Find("Mine_Name").GetComponent<TextMeshProUGUI>();
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

        mineNameText.text = partName;
        workingModeBtn = transform.Find("WorkingModeBtn").GetComponent<Button>();
        workingModeText = workingModeBtn.GetComponentInChildren<TextMeshProUGUI>();
        workingModeBtn.onClick.AddListener(() => ChangeWorkingMode());

        SetWorkModeColor();
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
            if (scriptableCompound.isLockedByContract)
            {
                var lockText = Instantiate(GameManager.Instance.levelLock, transform);
                lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + scriptableCompound.lockedByContract.contractName + " Contract";
                scriptableCompound.isLockedByContract = false;
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
        PricePerProduct = ProductionManager.Instance.GetPricePerProductForCompound(inputResources, inputAmounts, BuildTime);
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
        PricePerProduct = ProductionManager.Instance.GetPricePerProductForCompound(inputResources, inputAmounts, BuildTime);
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
        PricePerProduct = ProductionManager.Instance.GetPricePerProductForCompound(inputResources, inputAmounts, BuildTime);
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
            UpgradeCost = UpgradeSystem.Instance.GetNewUpgradeCost(1, newUpgradeCost, CompoundLevel);
            SetUpgradePanel(upgradeMultiplier);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PricePerProduct = ProductionManager.Instance.GetPricePerProductForCompound(inputResources, inputAmounts, BuildTime);
        Produce();
        if (isCharging) RemainedBuildTime -= .35f;
    }

    void ShowUpgradePanel()
    {
        PricePerProduct = ProductionManager.Instance.GetPricePerProductForCompound(inputResources, inputAmounts, BuildTime);
        UpgradeSystem.Instance.ShowUpgradePanel(SetUpgradePanel, Upgrade, isUpgradePanelActive, UpgradeCost, CompoundLevel);
    }

    void SetUpgradePanel(int levelUpgradeMultiplier)
    {
        PricePerProduct = ProductionManager.Instance.GetPricePerProductForCompound(inputResources, inputAmounts, BuildTime);
        UpgradeSystem.Instance.SetUpgradePanel(levelUpgradeMultiplier, outputValue, CompoundLevel, BuildTime, PricePerProduct, UpgradeCost, partName, false);
    }
}