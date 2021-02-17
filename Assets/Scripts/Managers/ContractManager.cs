using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
 * Contracts will appear according to resource that can produced resource and
 * story requirements. 
 * 
 * 
 */
// TODO When user missing resource for contract or any other thing show ad panel for earning that resource
// TODO Add automatically activate new contracts for people who just want to play game not story

public class ContractManager : Singleton<ContractManager>
{
    [SerializeField] private GameObject contractPrefab;

    // Panels
    [SerializeField] private GameObject contractPanel;
    /* Completed contracts will moved to this panel */
    [SerializeField] private GameObject completedContractPanel;
    /* Show number of activated contracts in contract panel */
    [SerializeField] private GameObject activeContractCounter;
    [SerializeField] private GameObject contractFinishedInfoPanel;

    [SerializeField] TabGroup mainPanels;
    [SerializeField] private GameObject productionPanel;
    [SerializeField] private GameObject ProductionPanelBtn;

    public ContractBase[] contracts; // All contracts
    public List<ContractHolder> instantiatedContracts;
    public List<ContractHolder> activatedContracts;
    public List<ContractHolder> completedContracts;

    // This list purely for optimization. When new resources produced. If it is not needed don't fire methods for less CPU usage.
    // CheckAvailableResources method fire, when every resource collected. So its call should be minimized.
    List<BaseResources> requiredResourcesForContracts;

    private void Awake()
    {
        instantiatedContracts = new List<ContractHolder>();
        completedContracts = new List<ContractHolder>();
        activatedContracts = new List<ContractHolder>();
        requiredResourcesForContracts = new List<BaseResources>();

        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
        GameManager.Instance.OnLevelUp += OnLevelUp;
    }

    private void Start()
    {
        for (int i = 0; i < contracts.Length; i++)
        {
            var contractBase = contracts[i];
            if (contractBase.contractName != "" && contractBase.unlockLevel <= GameManager.Instance.CurrentLevel)
                CreateContract(contractBase);
        }
    }

    void CreateContract(ContractBase contract)
    {
        if (contract.contractName == "" || contract.unlockLevel != GameManager.Instance.CurrentLevel) return;

        // Instantiate prefab and enter information from scriptable object
        var _contract = Instantiate(contractPrefab, contractPanel.transform);
        _contract.AddComponent<ContractHolder>();
        var contractHolder = _contract.GetComponent<ContractHolder>();
        contractHolder.contract = contract;
        contractHolder.requiredResources = contract.requiredResources.ToList();
        contractHolder.requiredResourceAmounts = contract.requiredResourceAmounts.ToList();
        contractHolder.dependencyContracts = contract.dependentContracts.ToList();

        requiredResourcesForContracts.AddRange(contractHolder.requiredResources);

        for (int i = 0; i < completedContracts.Count; i++)
        {
            var c = completedContracts[i];
            if (contract.dependentContracts != null && contract.dependentContracts.Length > 0)
                if (contractHolder.dependencyContracts.Contains(c.contract))
                    contractHolder.dependencyContracts.Remove(c.contract);
        }

        _contract.transform.Find("Texts").Find("Header").GetComponent<TextMeshProUGUI>().text = contract.contractName;
        _contract.transform.Find("Texts").Find("Description").GetComponent<TextMeshProUGUI>().text = contract.description;
        _contract.transform.Find("Texts").Find("Reward").GetComponent<TextMeshProUGUI>().text = contract.contractReward.ToString();
        _contract.transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount = 0;
        _contract.transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text = "Progress: %0";

        if (contract.icon != null)
            _contract.transform.Find("Icon").GetComponent<Image>().sprite = contract.icon;

        // Add resource icons to contract panel
        var requiredResourcePanel = _contract.transform.Find("Required_Resource");
        for (int i = 0; i < contract.requiredResources.Length; i++)
        {
            var icon = Instantiate(ResourceManager.Instance.iconPrefab, requiredResourcePanel);
            icon.transform.Find("Frame").Find("Icon").GetComponent<Image>().sprite =
                ResourceManager.Instance.GetSpriteFromResource(contract.requiredResources[i]);
            icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text =
                contract.requiredResourceAmounts[i].ToString();
        }

        if (!completedContracts.Contains(contractHolder))
        {
            _contract.transform.Find("Activation_Btn").GetComponent<Button>().onClick.AddListener(
            () => PopupManager.Instance.PopupConfirmationPanel(string.Format("Do you want to activate {0} Contract", contract.contractName),
            () => ActivateContract(contractHolder),
            () => PopupManager.Instance.confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false)));

            if (contractHolder.dependencyContracts != null && contractHolder.dependencyContracts.Count > 0)
            {
                _contract.transform.Find("Activation_Btn").GetComponent<Button>().interactable = false;
                var lockText = Instantiate(GameManager.Instance.levelLock, _contract.transform);
                lockText.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Unlocked when <color=red>{0}</color> completed",
                    contract.dependentContracts[0].contractName);
            }
        }
        else if (activatedContracts.Contains(contractHolder))
        {
            _contract.transform.SetAsFirstSibling();
            _contract.GetComponent<Image>().color = new Color(11 / 256f, 253 / 256f, 54 / 256f);
        }
        else if (completedContracts.Contains(contractHolder))
        {
            _contract.transform.SetParent(completedContractPanel.transform);
        }


        instantiatedContracts.Add(contractHolder);
    }

    /// <summary>
    /// Check available to resource to collect for active contracts
    /// </summary>
    /// <param name="_resource">
    /// When new resource collected, this method will be fired OnResourceAmountChanged event.
    /// If required resources list don't contain this resource, don't iterate. For performance.
    /// </param>
    void CheckAvailableResources(BaseResources _resource)
    {
        if (!requiredResourcesForContracts.Contains(_resource)) return;

        for (int i = 0; i < activatedContracts.Count; i++)
        {
            ContractHolder contractHolder = activatedContracts[i];
            ContractBase contract = contractHolder.contract;

            IEnumerable<(BaseResources Resource, BigDouble Amount)> inputs = contract.requiredResources.Zip(contract.requiredResourceAmounts, (resource, amount) => (Resource: resource, Amount: amount));

            BigDouble totalResource = 0;
            for (int j = 0; j < contract.requiredResourceAmounts.Length; j++)
            {
                totalResource += contract.requiredResourceAmounts[j];
            }

            GetAvaliableResource(inputs, contractHolder, totalResource);

            if (contractHolder.requiredResources.Count == 0)
            {
                OnContractCompleted(contractHolder);
            }
        }
    }

    /// <summary>
    /// This is inside function for CheckAvailableResource.
    /// Only for increasing readability
    /// </summary>
    /// <see cref="CheckAvailableResources(BaseResources)"/>
    void GetAvaliableResource(IEnumerable<(BaseResources Resource, BigDouble Amount)> inputs, ContractHolder contractHolder, BigDouble totalResourceAmount)
    {
        var contractObj = contractHolder;
        foreach ((BaseResources Resource, BigDouble Amount) in inputs)
        {
            if (contractHolder.requiredResources.Contains(Resource) && ResourceManager.Instance.GetResourceAmount(Resource) >= Amount)
            {
                contractObj.transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount += (float)((Amount / totalResourceAmount).Mantissa);
                contractObj.transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text =
                    "Progress: %" + (contractObj.transform.Find("Outline")
                    .Find("Fill").GetComponent<Image>().fillAmount).ToString("F2");
                contractObj.transform.GetChild(0)
                    .GetChild(contractHolder.requiredResources.IndexOf(Resource))
                    .GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);

                ResourceManager.Instance.ConsumeResource(Resource, Amount);
                contractHolder.requiredResources.Remove(Resource);
            }
        }
    }

    void OnContractCompleted(ContractHolder contractHolder)
    {
        // Remove resources from list that doesn't needed anymore
        for (int i = 0; i < contractHolder.contract.requiredResourceAmounts.Length; i++)
            requiredResourcesForContracts.Remove(contractHolder.contract.requiredResources[i]);

        activatedContracts.Remove(contractHolder);
        completedContracts.Add(contractHolder);

        contractHolder.transform.Find("Activation_Btn").GetComponent<Button>().onClick.RemoveAllListeners();

        SetActiveContractCounter();

        // Check contracts that depend on contract that completed
        CheckDependencyContracts(contractHolder);

        OnContractRewarded(contractHolder);

        GameManager.Instance.AddXP(contractHolder.contract.xpReward);
        ShowCompletedContract(contractHolder.contract);
        // Move completed contract to completed contracts panel
        contractHolder.transform.SetParent(completedContractPanel.transform);
    }

    /// <summary>
    /// Check contract dependency for instantiated object over completed contract
    /// </summary>
    /// <param name="contractHolder">Completed Contract</param>
    void CheckDependencyContracts(ContractHolder contractHolder)
    {
        foreach (var contract in instantiatedContracts)
        {
            Transform lockTransform = contract.transform.Find("Level_Lock(Clone)");

            // If one of the instantiated contracts, have dependency on this contract, remove it from list
            if (contract.dependencyContracts.Contains(contractHolder.contract))
                contract.dependencyContracts.Remove(contractHolder.contract);

            // If no dependency anymore, remove lock.
            if (lockTransform != null && contract.dependencyContracts.Count == 0)
            {
                contract.transform.Find("Activation_Btn").GetComponent<Button>().interactable = true;
                Destroy(lockTransform.gameObject);
            }
            // If still has another dependency, change lock text with new one or has nothing to lock from start
            else if (lockTransform != null)
                lockTransform.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Unlocked when <color=red>{0}</color> completed",
                    contract.dependencyContracts[0].contractName);
        }
    }

    void OnContractRewarded(ContractHolder contractHolder)
    {
        var contract = contractHolder.contract;
        // Give reward according to contract reward type

        switch (contract.contractRewardType)
        {
            case ContractRewardType.Currency:
                ResourceManager.Instance.Currency += contract.contractReward;
                break;
            case ContractRewardType.PremiumCurrency:
                ResourceManager.Instance.PremiumCurrency += contract.contractReward;
                break;
            case ContractRewardType.miningSpeedUp:
                UpgradeSystem.Instance.MiningSpeedMultiplier += contract.contractReward;
                break;
            case ContractRewardType.productionSpeedUp:
                UpgradeSystem.Instance.ProductionSpeedMultiplier += contract.contractReward;
                break;
            case ContractRewardType.miningYieldUpgrade:
                UpgradeSystem.Instance.MiningYieldMultiplier += contract.contractReward;
                break;
            case ContractRewardType.productionYieldUpgrade:
                UpgradeSystem.Instance.ProductionYieldMultiplier += contract.contractReward;
                break;
            case ContractRewardType.productionEfficiencyUpgrade:
                UpgradeSystem.Instance.ProductionEfficiencyMultiplier += contract.contractReward;
                break;
            case ContractRewardType.incresedCoinEarning:
                UpgradeSystem.Instance.EarnedCoinMultiplier += contract.contractReward;
                break;
            case ContractRewardType.unlockProductionUnit:
                {
                    for (int j = 0; j < ProductionManager.Instance.instantiatedProductionUnits.Count; j++)
                    {
                        var unit = ProductionManager.Instance.instantiatedProductionUnits[j].GetComponent<ProductionBase>();
                        if (contract.productsToUnlock != null && contract.productsToUnlock.Contains(unit.scriptableProductionBase))
                        {
                            unit.ContractStatueCheckDictionary[contract] = true;
                            if (!unit.ContractStatueCheckDictionary.ContainsValue(false))
                            {
                                unit.IsLockedByContract = false;
                                Destroy(unit.transform.Find("Level_Lock(Clone)").gameObject);
                            }
                        }
                    }
                }
                break;
            case ContractRewardType.unitSpeedUp:
                UpgradeSystem.Instance.SpeedUpDictionaryValue = new KeyValuePair<BaseResources, float>(contract.resourceToRewarded, contract.contractReward);
                break;
        }
    }

    void SetActiveContractCounter()
    {
        activeContractCounter.GetComponentInChildren<TextMeshProUGUI>().text = activatedContracts.Count.ToString();
        if (activatedContracts.Count > 0)
            activeContractCounter.SetActive(true);
        else
            activeContractCounter.SetActive(false);
    }

    void ActivateContract(ContractHolder contractHolder)
    {
        if (!activatedContracts.Contains(contractHolder))
        {
            activatedContracts.Add(contractHolder);

            contractHolder.GetComponent<Image>().color = new Color(11 / 256f, 253 / 256f, 54 / 256f);
            contractHolder.transform.SetAsFirstSibling();
            contractHolder.transform.Find("Activation_Btn").GetComponent<Button>().onClick.RemoveAllListeners();

            Debug.Log(contractHolder.contract.contractName + " contract activated");
            SetActiveContractCounter();
            CheckAvailableResources(BaseResources._0_berry);
        }
    }

    /// <summary>
    /// Set page settings for go to page button
    /// TODO Need some fix for all pages (Production page access has no problem)
    /// </summary>
    /// <param name="pageName">Production page name to go</param>
    void GoToPage(string pageName)
    {
        // Go to page in production
        for (int i = 0; i < productionPanel.transform.childCount; i++)
        {
            var child = productionPanel.transform.GetChild(i);
            if (productionPanel.transform.GetChild(i).name == pageName)
            {
                child.GetComponent<CanvasGroup>().interactable = true;
                child.GetComponent<CanvasGroup>().alpha = 1;
                child.GetComponent<CanvasGroup>().blocksRaycasts = true;

                // Place to front for interaction
                child.transform.SetAsLastSibling();
            }
            else
            {
                child.GetComponent<CanvasGroup>().interactable = false;
                child.GetComponent<CanvasGroup>().alpha = 0;
                child.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }
        PageManager.Instance.minePageInfoText.text = pageName;
    }

    void GoToPage(AvailableMainPages page, string subPage = "")
    {
        switch (page)
        {
            case AvailableMainPages.Production:
                mainPanels.tabButtons[0].OnPointerClick(null);
                break;
            case AvailableMainPages.Quest:
                mainPanels.tabButtons[1].OnPointerClick(null);
                break;
            case AvailableMainPages.Contract:
                mainPanels.tabButtons[2].OnPointerClick(null);
                break;
            case AvailableMainPages.Resource:
                mainPanels.tabButtons[3].OnPointerClick(null);
                break;
            case AvailableMainPages.Map:
                mainPanels.tabButtons[4].OnPointerClick(null);
                break;
            case AvailableMainPages.Shop:
                mainPanels.tabButtons[5].OnPointerClick(null);
                break;
        }
        if (subPage != "" && page == AvailableMainPages.Production)
            GoToPage(subPage);
    }

    void ShowCompletedContract(ContractBase contract)
    {
        var panel = contractFinishedInfoPanel.transform;
        panel.Find("Icon").GetComponent<Image>().sprite = contract.icon;
        panel.Find("HEADER").GetComponent<TextMeshProUGUI>().text = contract.rewardPanelHeader;
        panel.Find("Description").GetComponent<TextMeshProUGUI>().text = contract.rewardPanelDescription;
        panel.Find("OK_Btn").GetComponent<Button>().onClick.AddListener(() => { contractFinishedInfoPanel.SetActive(false); });
        panel.Find("GoToPage_Btn").GetComponent<Button>().onClick.AddListener(() => { GoToPage(contract.mainPageToGo ,contract.pageNameToGo); contractFinishedInfoPanel.SetActive(false); });
        contractFinishedInfoPanel.SetActive(true);
    }

    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        // Create contracts that will be unlocked at current level
        for (int i = 0; i < contracts.Length; i++)
        {
            var contractBase = contracts[i];
            if (contractBase.contractName != "" && contractBase.unlockLevel == GameManager.Instance.CurrentLevel)
                CreateContract(contractBase);
        }
    }

    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
    {
        if (!e.isConsumed)
            CheckAvailableResources(e.resource);
    }
}

public enum AvailableMainPages
{
    Production,
    Quest,
    Contract,
    Resource,
    Map,
    Shop,
}

public enum ContractRewardType
{
    Currency,
    PremiumCurrency,
    miningSpeedUp,
    productionSpeedUp,
    miningYieldUpgrade,
    productionYieldUpgrade,
    productionEfficiencyUpgrade,
    incresedCoinEarning,
    unlockProductionUnit,
    unitSpeedUp,
}

public enum ContractActivationType
{
    levelUp,
    resourceCollection,
    contractCompletion,
    story,
}

public enum ContractType
{
    story = 0,
    producing = 1,
    war = 2,
}

//public class Contracts
//{
//    /// <summary>
//    /// This method get contract with its object name
//    /// Methods original name is GetContractWithName
//    /// </summary>
//    /// <param name="Name">Name of the gameobject in asset folder</param>
//    /// <returns>Returns contract that matches with given value</returns>
//    protected static ContractBase G(string Name)
//    {
//        return ContractManager.Instance.contracts.Where(c => c.name == Name).First();
//    }
//}

//public class StoneAgeContracts : Contracts
//{
//    public static ContractBase HUNTING_TOOLS_CONTRACT => G("");
//    public static ContractBase BERRY_COLLECTOR_CONTRACT => G("");
//    public static ContractBase MINING_CONTRACT { get { return G(""); } }
//    public static ContractBase WAR_BRINGER_CONTRACT => G("");
//}

//public class BronzeAgeContracts : Contracts
//{
//    public static ContractBase BLACKSMITH_CONTRACT => G("");
//}