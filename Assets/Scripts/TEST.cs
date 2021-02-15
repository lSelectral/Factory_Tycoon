using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class TEST : MonoBehaviour, IPointerDownHandler
{

    public void OnPointerDown(PointerEventData eventData)
    {


    }

    private void Start()
    {
        Debug.Log( (new BigDouble(15468.3, 7) + new BigDouble(219.3333,6) ).ToString());
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    SaveSystem.Instance.Save();
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    SaveSystem.Instance.Load();
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{

        //}
    }
}


//public class CONTRACT_TEST : MonoBehaviour
//{
//    [SerializeField] private GameObject contractPrefab;

//    // Panels
//    [SerializeField] private GameObject contractPanel;
//    /* Completed contracts will moved to this panel */
//    [SerializeField] private GameObject completedContractPanel;
//    /* Show number of activated contracts in contract panel */
//    [SerializeField] private GameObject activeContractCounter;
//    [SerializeField] private GameObject contractFinishedInfoPanel;
//    [SerializeField] private GameObject productionPanel;
//    [SerializeField] private GameObject ProductionPanelBtn;

//    public ContractBase[] contracts; // All contracts
//    public List<ContractHolder> instantiatedContracts;
//    public List<ContractHolder> activatedContracts;
//    public List<ContractHolder> completedContracts;

//    // This list purely for optimization. When new resources produced. If it is not needed don't fire methods for less CPU usage.
//    List<BaseResources> requiredResourcesForContracts;

//    private void Awake()
//    {
//        instantiatedContracts = new List<ContractHolder>();
//        completedContracts = new List<ContractHolder>();
//        activatedContracts = new List<ContractHolder>();
//        requiredResourcesForContracts = new List<BaseResources>();

//        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
//        GameManager.Instance.OnLevelUp += OnLevelUp;
//    }

//    private void Start()
//    {
//        for (int i = 0; i < contracts.Length; i++)
//        {
//            var contractBase = contracts[i];
//                CreateContract(contractBase);
//        }
//    }

//    void CreateContract(ContractBase contract)
//    {
//        if (contract.contractName == "" || contract.unlockLevel < GameManager.Instance.CurrentLevel) return;

//        // Instantiate prefab and enter information from scriptable object
//        var _contract = Instantiate(contractPrefab, contractPanel.transform);
//        _contract.AddComponent<ContractHolder>();
//        var contractHolder = _contract.GetComponent<ContractHolder>();
//        contractHolder.contract = contract;
//        contractHolder.requiredResources = contract.requiredResources.ToList();
//        contractHolder.requiredResourceAmounts = contract.requiredResourceAmounts.ToList();
//        contractHolder.dependencyContracts = contract.dependentContracts.ToList();

//        requiredResourcesForContracts.AddRange(contractHolder.requiredResources);

//        _contract.transform.Find("Texts").Find("Header").GetComponent<TextMeshProUGUI>().text = contract.contractName;
//        _contract.transform.Find("Texts").Find("Description").GetComponent<TextMeshProUGUI>().text = contract.description;
//        _contract.transform.Find("Texts").Find("Reward").GetComponent<TextMeshProUGUI>().text = contract.contractReward.ToString();
//        _contract.transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount = 0;
//        _contract.transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text = "Progress: %0";

//        if (contract.icon != null)
//            _contract.transform.Find("Icon").GetComponent<Image>().sprite = contract.icon;

//        // Add resource icons to contract panel
//        var requiredResourcePanel = _contract.transform.Find("Required_Resource");
//        for (int i = 0; i < contract.requiredResources.Length; i++)
//        {
//            var icon = Instantiate(ResourceManager.Instance.iconPrefab, requiredResourcePanel);
//            icon.transform.Find("Frame").Find("Icon").GetComponent<Image>().sprite =
//                ResourceManager.Instance.GetSpriteFromResource(contract.requiredResources[i]);
//            icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text =
//                contract.requiredResourceAmounts[i].ToString();
//        }

//        if (!completedContracts.Contains(contractHolder))
//        {
//            _contract.transform.Find("Activation_Btn").GetComponent<Button>().onClick.AddListener(
//            () => PopupManager.Instance.PopupConfirmationPanel(string.Format("Do you want to activate {0} Contract", contract.contractName),
//            () => ActivateContract(contractHolder),
//            () => PopupManager.Instance.confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false) ));

//            if (contractHolder.dependencyContracts != null && contractHolder.dependencyContracts.Count > 0)
//            {
//                _contract.transform.Find("Activation_Btn").GetComponent<Button>().interactable = false;
//                var lockText = Instantiate(GameManager.Instance.levelLock, _contract.transform);
//                lockText.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Unlocked when <color=red>{0}</color> completed",
//                    contract.dependentContracts[0].contractName);
//            }
//        }
//        else if (activatedContracts.Contains(contractHolder))
//        {
//            _contract.transform.SetAsFirstSibling();
//            _contract.GetComponent<Image>().color = new Color(11 / 256f, 253 / 256f, 54 / 256f);
//        }
//        else if (completedContracts.Contains(contractHolder))
//        {
//            _contract.transform.SetParent(completedContractPanel.transform);
//        }
        

//        instantiatedContracts.Add(contractHolder);
//    }

//    /// <summary>
//    /// Check available to resource to collect for active contracts
//    /// </summary>
//    /// <param name="_resource">
//    /// When new resource collected, this method will be fired OnResourceAmountChanged event.
//    /// If required resources list don't contain this resource, don't iterate. For performance.
//    /// </param>
//    void CheckAvailableResources(BaseResources _resource)
//    {
//        if (!requiredResourcesForContracts.Contains(_resource)) return;

//        for (int i = 0; i < activatedContracts.Count; i++)
//        {
//            ContractHolder contractHolder = activatedContracts[i];
//            ContractBase contract = contractHolder.contract;

//            var inputs = contract.requiredResources.Zip(contract.requiredResourceAmounts, (resource, amount) => (Resource: resource, Amount: amount));

//            BigDouble totalResource = 0;
//            for (int j = 0; j < contract.requiredResourceAmounts.Length; j++)
//            {
//                totalResource += contract.requiredResourceAmounts[j];
//            }

//            GetAvaliableResource(inputs, contractHolder, totalResource);

//            if (contractHolder.requiredResources.Count == 0)
//            {
//                OnContractCompleted(contractHolder);
//            }
//        }
//    }

//    /// <summary>
//    /// This is inside function for CheckAvailableResource.
//    /// Only for increasing readability
//    /// </summary>
//    /// <see cref="CheckAvailableResources(BaseResources)"/>
//    void GetAvaliableResource(IEnumerable<(BaseResources Resource, BigDouble Amount)> inputs, ContractHolder contractHolder, BigDouble totalResourceAmount)
//    {
//        var contractObj = contractHolder;
//        foreach ((BaseResources Resource, BigDouble Amount) in inputs)
//        {
//            if (contractHolder.requiredResources.Contains(Resource) && ResourceManager.Instance.GetResourceAmount(Resource) >= Amount)
//            {
//                ResourceManager.Instance.ConsumeResource(Resource, Amount);
//                contractHolder.requiredResources.Remove(Resource);

//                contractObj.transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount += (float)((Amount / totalResourceAmount).Mantissa);
//                contractObj.transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text =
//                    "Progress: %" + (contractObj.transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount).ToString("F2");
//                contractObj.transform.Find("Required_Resource").GetChild(contractHolder.requiredResources.IndexOf(Resource))
//                    .GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
//            }
//        }
//    }

//    void OnContractCompleted(ContractHolder contractHolder)
//    {
//        // Remove resources from list that doesn't needed anymore
//        for (int i = 0; i < contractHolder.contract.requiredResourceAmounts.Length; i++)
//            requiredResourcesForContracts.Remove(contractHolder.contract.requiredResources[i]);

//        activatedContracts.Remove(contractHolder);
//        completedContracts.Add(contractHolder);

//        contractHolder.transform.Find("Activation_Btn").GetComponent<Button>().onClick.RemoveAllListeners();

//        SetActiveContractCounter();

//        // Check contracts that depend on contract that completed
//        CheckDependencyContracts(contractHolder);

//        OnContractRewarded(contractHolder);

//        GameManager.Instance.AddXP(contractHolder.contract.xpReward);
//        ShowCompletedContract(contractHolder.contract, contractHolder.contract.pageNameToGo);
//        // Move completed contract to completed contracts panel
//        contractHolder.transform.SetParent(completedContractPanel.transform);
//    }

//    /// <summary>
//    /// Check contract dependency for instantiated object over completed contract
//    /// </summary>
//    /// <param name="contractHolder">Completed Contract</param>
//    void CheckDependencyContracts(ContractHolder contractHolder)
//    {

//        foreach (var contract in instantiatedContracts)
//        {
//            Transform lockTransform = contract.transform.Find("Level_Lock(Clone)");

//            // If one of the instantiated contracts, have dependency on this contract, remove it from list
//            if (contract.dependencyContracts.Contains(contractHolder.contract))
//                contract.dependencyContracts.Remove(contractHolder.contract);

//            // If no dependency anymore, remove lock.
//            if (lockTransform != null && contract.dependencyContracts.Count == 0)
//                Destroy(lockTransform);
//            // If still has another dependency, change lock text with new one 
//            else
//                lockTransform.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Unlocked when <color=red>{0}</color> completed",
//                    contract.dependencyContracts[0].contractName);
//        }
//    }

//    void OnContractRewarded(ContractHolder contractHolder)
//    {
//        var contract = contractHolder.contract;
//        // Give reward according to contract reward type
//        if (contract.contractRewardType == ContractRewardType.Currency)
//            ResourceManager.Instance.Currency += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.PremiumCurrency)
//            ResourceManager.Instance.PremiumCurrency += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.incresedCoinEarning)
//            UpgradeSystem.Instance.EarnedCoinMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.miningSpeedUp)
//            UpgradeSystem.Instance.MiningSpeedMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.miningYieldUpgrade)
//            UpgradeSystem.Instance.MiningYieldMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.productionEfficiencyUpgrade)
//            UpgradeSystem.Instance.ProductionEfficiencyMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.productionSpeedUp)
//            UpgradeSystem.Instance.ProductionSpeedMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.productionYieldUpgrade)
//            UpgradeSystem.Instance.ProductionYieldMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.unitSpeedUp)
//            UpgradeSystem.Instance.SpeedUpDictionaryValue = new KeyValuePair<BaseResources, float>(contract.resourceToRewarded, contract.contractReward);

//        else if (contract.contractRewardType == ContractRewardType.unlockProductionUnit)
//        {
//            for (int j = 0; j < ProductionManager.Instance.instantiatedProductionUnits.Count; j++)
//            {
//                var unit = ProductionManager.Instance.instantiatedProductionUnits[j].GetComponent<ProductionBase>();
//                if (contract.productsToUnlock != null && contract.productsToUnlock.Contains(unit.scriptableProductionBase))
//                {
//                    unit.ContractStatueCheckDictionary[contract] = true;
//                    if (!unit.ContractStatueCheckDictionary.ContainsValue(false))
//                    {
//                        unit.IsLockedByContract = false;
//                        Destroy(unit.transform.Find("Level_Lock(Clone)").gameObject);
//                    }
//                }
//            }
//        }
//    }

//    void SetActiveContractCounter()
//    {
//        activeContractCounter.GetComponentInChildren<TextMeshProUGUI>().text = activatedContracts.Count.ToString();
//        if (activatedContracts.Count > 0)
//            activeContractCounter.SetActive(true);
//        else
//            activeContractCounter.SetActive(false);
//    }

//    void ActivateContract(ContractHolder contractHolder)
//    {
//        if (!activatedContracts.Contains(contractHolder))
//        {
//            activatedContracts.Add(contractHolder);

//            contractHolder.GetComponent<Image>().color = new Color(11 / 256f, 253 / 256f, 54 / 256f);
//            contractHolder.transform.SetAsFirstSibling();
//            contractHolder.transform.Find("Activation_Btn").GetComponent<Button>().onClick.RemoveAllListeners();

//            Debug.Log(contractHolder.contract.contractName + " contract activated");
//            SetActiveContractCounter();
//        }
//    }

//    /// <summary>
//    /// Set page settings for go to page button
//    /// TODO Need some fix for all pages (Production page access has no problem)
//    /// </summary>
//    /// <param name="pageName">Production page name to go</param>
//    void SetPageSettings(string pageName)
//    {
//        if (pageName == "Map")
//        {
//            productionPanel.transform.parent.parent.parent.Find("Bottom_Panel").Find("Map").GetComponent<TabButton>().OnPointerClick(null);
//            return;
//        }

//        // Navigate from any page to production page
//        ProductionPanelBtn.GetComponent<TabButton>().OnPointerClick(null);

//        // Go to page in production
//        for (int i = 0; i < productionPanel.transform.childCount; i++)
//        {
//            var child = productionPanel.transform.GetChild(i);
//            if (productionPanel.transform.GetChild(i).name == pageName)
//            {
//                child.GetComponent<CanvasGroup>().interactable = true;
//                child.GetComponent<CanvasGroup>().alpha = 1;
//                child.GetComponent<CanvasGroup>().blocksRaycasts = true;

//                // Place to front for interaction
//                child.transform.SetAsLastSibling();
//            }
//            else
//            {
//                child.GetComponent<CanvasGroup>().interactable = false;
//                child.GetComponent<CanvasGroup>().alpha = 0;
//                child.GetComponent<CanvasGroup>().blocksRaycasts = false;
//            }
//        }
//        PageManager.Instance.minePageInfoText.text = pageName;
//    }
//    void ShowCompletedContract(ContractBase contract, string pageName)
//    {
//        var panel = contractFinishedInfoPanel.transform;
//        panel.Find("Icon").GetComponent<Image>().sprite = contract.icon;
//        panel.Find("HEADER").GetComponent<TextMeshProUGUI>().text = contract.rewardPanelHeader;
//        panel.Find("Description").GetComponent<TextMeshProUGUI>().text = contract.rewardPanelDescription;
//        panel.Find("OK_Btn").GetComponent<Button>().onClick.AddListener(() => { contractFinishedInfoPanel.SetActive(false); });
//        panel.Find("GoToPage_Btn").GetComponent<Button>().onClick.AddListener(() => { SetPageSettings(pageName); contractFinishedInfoPanel.SetActive(false); });
//        contractFinishedInfoPanel.SetActive(true);
//    }

//    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
//    {
//        // Create contracts that will be unlocked at current level
//        for (int i = 0; i < contracts.Length; i++)
//        {
//            CreateContract(contracts[i]);
//        }
//    }

//    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
//    {
//        if (!e.isConsumed)
//            CheckAvailableResources(e.resource);
//    }
//}



///*
// * Contracts will appear according to resource that can produced resource and
// * story requirements. 
// * 
// * 
// */
//// TODO When user missing resource for contract or any other thing show ad panel for earning that resource
//// TODO Add automatically activate new contracts for people who just want to play game not story

//public class ContractManager : Singleton<ContractManager>
//{
//    [SerializeField] private GameObject contractPrefab;

//    /* All Contracts available in game */ public ContractBase[] contracts;
//    [SerializeField] private GameObject contractPanel;
//    /* Completed contracts will moved to this panel */ [SerializeField] private GameObject completedContractPanel;
//    /* Show number of activated contracts in contract panel */ [SerializeField] private GameObject activeContractCounter;

//    Dictionary<ContractBase, List<ContractBase>> contractDependencyDictionary;

//    [SerializeField] private GameObject contractFinishedInfoPanel;

//    [SerializeField] private GameObject productionPanel;
//    [SerializeField] private GameObject ProductionPanelBtn;

//    public List<GameObject> instantiatedContracts;

//    //public List<List<BaseResources>> tempResources;
//    public Dictionary<ContractBase, List<BaseResources>> tempResourceDictionary;

//    public List<ContractBase> activatedContracts;
//    public List<ContractBase> completedContracts;

//    UnityEngine.Object[] assets;

//    private void Awake()
//    {
//        //tempResources = new List<List<BaseResources>>();
//        instantiatedContracts = new List<GameObject>();
//        completedContracts = new List<ContractBase>();
//        contractDependencyDictionary = new Dictionary<ContractBase, List<ContractBase>>();
//        tempResourceDictionary = new Dictionary<ContractBase, List<BaseResources>>();
//        activatedContracts = new List<ContractBase>();

//        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
//        GameManager.Instance.OnLevelUp += OnLevelUp;
//    }

//    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
//    {
//        for (int i = 0; i < instantiatedContracts.Count; i++)
//        {
//            var c = instantiatedContracts[i];
//            //if (c.transform.Find("Level_Lock(Clone)") != null && contracts.Length > 0 && contracts[i].unlockLevel == e.currentLevel)
//            //{
//            //    c.transform.Find("Activation_Btn").GetComponent<Button>().interactable = true;
//            //    Destroy(c.transform.Find("Level_Lock(Clone)").gameObject);
//            //}
//            if (contracts[i].dependentContracts != null && contracts[i].dependentContracts.Length > 0)
//            {
//                c.transform.Find("Activation_Btn").GetComponent<Button>().interactable = false;
//                var lockText = Instantiate(GameManager.Instance.levelLock, c.transform);
//                lockText.GetComponentInChildren<TextMeshProUGUI>().text = 
//                    string.Format("Unlocked when <color=red>{0}</color> completed", contracts[i].dependentContracts[0].contractName);
//            }
//        }

//        for (int i = 0; i < contracts.Length; i++)
//        {
//            var contract = contracts[i];
//            if (contract.contractName != "" && contract.unlockLevel == GameManager.Instance.CurrentLevel)
//            {
//                CreateContract(contract);
//            }
//        }
//    }

//    private void Start()
//    {
//        assets = Resources.LoadAll("Contracts");
//        List<ContractBase> tempContracts = new List<ContractBase>();
//        for (int i = 0; i < assets.Length; i++)
//        {
//            var asset = assets[i];

//            if (asset as ContractBase != null)
//            {
//                var contractBase = asset as ContractBase;
//                tempContracts.Add(contractBase);
//                if (contractBase.contractName != "" && contractBase.unlockLevel <= GameManager.Instance.CurrentLevel)
//                {
//                    CreateContract(contractBase);
//                }
//            }
//        }
//        contracts = tempContracts.ToArray();

//        SetActiveContractCounter();
//    }

//    GameObject GetGameObjectFromContract(ContractBase contract)
//    {
//        return instantiatedContracts.Where(c => c.GetComponent<ContractHolder>().contract == contract).FirstOrDefault();
//    }

//    ///// <summary>
//    ///// Not for rungame. Used in editor for creating automation assets automatically.
//    ///// Used frome editor script CustomEditorWindow.cs
//    ///// For creating simply click create automation contracts button. If missing just remove comment mark from editor script
//    ///// </summary>
//    ///// <returns>List of contracts for automation of production</returns>
//    ///// <seealso cref="Assets\Scripts\Editor\CustomEditorWindow.cs"/>
//    //public List<ContractBase> CreateAutomationContracts()
//    //{
//    //    List<ContractBase> automationContracts = new List<ContractBase>();
//    //    foreach (BaseResources resource in Enum.GetValues(typeof(BaseResources)))
//    //    {
//    //        ScriptableMine mine = ProductionManager.Instance.GetScriptableProductionUnitFromResource(resource) as ScriptableMine;
//    //        ScriptableCompound compound = ProductionManager.Instance.GetScriptableProductionUnitFromResource(resource) as ScriptableCompound;

//    //        ScriptableMine[] mineArray = null;
//    //        ScriptableCompound[] compoundArray = null;
//    //        BaseResources[] requiredResources = { resource };
//    //        int[] requiredResourceAmounts = { 500 };

//    //        string pageNameToGo = "";
//    //        int unlockLevel = 2;
//    //        long xpReward = 100;

//    //        if (mine != null)
//    //        {
//    //            mineArray = new ScriptableMine[] { mine };
//    //            pageNameToGo = mine.ageBelongsTo.ToString();
//    //            unlockLevel = mine.unlockLevel++;
//    //            xpReward = mine.xpAmount * 15;
//    //        }
//    //        else if (compound != null)
//    //        {
//    //            compoundArray = new ScriptableCompound[] { compound };
//    //            pageNameToGo = compound.ageBelongsTo.ToString();
//    //            unlockLevel = compound.unlockLevel++;
//    //            xpReward = compound.xpAmount * 15;
//    //        }

//    //        ContractBase contract = new ContractBase()
//    //        {
//    //            contractName = ResourceManager.Instance.GetValidNameForResource(resource),
//    //            contractReward = 0,
//    //            productsToUnlock = compoundArray,
//    //            contractRewardType = ContractRewardType.automate,
//    //            description = ResourceManager.Instance.GetValidNameForResource(resource) + " will automatically collected",
//    //            icon = ResourceManager.Instance.GetSpriteFromResource(resource),
//    //            pageNameToGo = pageNameToGo,
//    //            unlockLevel = unlockLevel,
//    //            rewardPanelHeader = string.Format("<color=red>Congrulations!</color> for Automating {0}", ResourceManager.Instance.GetValidNameForResource(resource)),
//    //            rewardPanelDescription = string.Format("{0} will automatically processed.",ResourceManager.Instance.GetValidNameForResource(resource)),
//    //            xpReward = xpReward,
//    //            requiredResources = requiredResources,
//    //            requiredResourceAmounts = requiredResourceAmounts,
//    //        };
//    //        automationContracts.Add(contract);
//    //    }
//    //    return automationContracts;
//    //}

//    public void CreateContract(ContractBase contract)
//    {
//        // Instantiate prefab and enter information from scriptable object
//        var _contract = Instantiate(contractPrefab, contractPanel.transform);
//        contractDependencyDictionary.Add(contract, contract.dependentContracts.ToList());
//        _contract.AddComponent<ContractHolder>();
//        _contract.GetComponent<ContractHolder>().contract = contract;
//        tempResourceDictionary.Add(contract, contract.requiredResources.ToList());
//        _contract.transform.Find("Texts").Find("Header").GetComponent<TextMeshProUGUI>().text = contract.contractName;
//        _contract.transform.Find("Texts").Find("Description").GetComponent<TextMeshProUGUI>().text = contract.description;
//        _contract.transform.Find("Texts").Find("Reward").GetComponent<TextMeshProUGUI>().text = contract.contractReward.ToString();
//        _contract.transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount = 0;
//        _contract.transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text = "Progress: %0";
//        //tempResources.Add(contract.requiredResources.ToList());

//        if (contract.icon != null)
//            _contract.transform.Find("Icon").GetComponent<Image>().sprite = contract.icon;

//        for (int i = 0; i < contract.requiredResources.Length; i++)
//        {
//            var icon = Instantiate(ResourceManager.Instance.iconPrefab, _contract.transform.Find("Required_Resource"));
//            icon.transform.Find("Frame").Find("Icon").GetComponent<Image>().sprite = 
//                ResourceManager.Instance.GetSpriteFromResource(contract.requiredResources[i]);
//            icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = 
//                ResourceManager.Instance.CurrencyToString(contract.requiredResourceAmounts[i]);
//        }
//        instantiatedContracts.Add(_contract);

//        if (!completedContracts.Contains(contract))
//        {
//            _contract.transform.Find("Activation_Btn").GetComponent<Button>().onClick.AddListener(
//            () => PopupManager.Instance.PopupConfirmationPanel(("Do you want to activate " + contract.contractName + " Contract"),
//            () => ActivateContract(contract),
//            () => PopupManager.Instance.confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false)));

//            if (contract.dependentContracts != null && contract.dependentContracts.Length > 0 && contractDependencyDictionary[contract].Count != 0)
//            {
//                _contract.transform.Find("Activation_Btn").GetComponent<Button>().interactable = false;
//                var lockText = Instantiate(GameManager.Instance.levelLock, _contract.transform);
//                lockText.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Unlocked when <color=red>{0}</color> completed", 
//                    contract.dependentContracts[0].contractName);
//            }
//            // If needed level is higher than current, won't instantiated.
//            //else if (contract.unlockLevel > GameManager.Instance.CurrentLevel)
//            //{
//            //    _contract.transform.Find("Activation_Btn").GetComponent<Button>().interactable = false;
//            //    var lockText = Instantiate(GameManager.Instance.levelLock, _contract.transform);
//            //    lockText.GetComponentInChildren<TextMeshProUGUI>().text = "Unlocked at level " + contract.unlockLevel.ToString();
//            //}
//        }
//        else if (completedContracts.Contains(contract))
//        {
//            _contract.transform.SetParent(completedContractPanel.transform);
//        }
//        else if (activatedContracts.Contains(contract))
//        {
//            _contract.transform.SetAsFirstSibling();
//            _contract.GetComponent<Image>().color = new Color(11 / 256f, 253 / 256f, 54 / 256f);
//        }
//    }

//    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
//    {
//        if (!e.isConsumed)
//            CheckAvailableResources();
//    }

//    void CheckAvailableResources()
//    {
//        for (int i = 0; i < activatedContracts.Count; i++)
//        {
//            var contract = activatedContracts[i];
//            var contractObj = GetGameObjectFromContract(contract);

//            var requiredResources = contract.requiredResources;
//            var requiredResourceAmounts = contract.requiredResourceAmounts;
//            int totalResourceAmount = 0;

//            for (int j = 0; j < contract.requiredResourceAmounts.Length; j++)
//            {
//                totalResourceAmount += contract.requiredResourceAmounts[j];
//            }

//            var inputs = requiredResources.Zip(requiredResourceAmounts, (resource, amount) => (Resource: resource, Amount: amount));
//            foreach (var (Resource, Amount) in inputs)
//            {
//                if (ResourceManager.Instance.GetResourceAmount(Resource) >= Amount && tempResourceDictionary[contract].Contains(Resource)  /*  && tempResources[i].Contains(Resource)*/)
//                {
//                    tempResourceDictionary[contract].Remove(Resource);
//                    ResourceManager.Instance.ConsumeResource(Resource, Amount);
//                    contractObj.transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount += (Amount * 1f / totalResourceAmount);
//                    contractObj.transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text =
//                        "Progress: %" + (contractObj.transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount).ToString("F2");
//                    contractObj.transform.Find("Required_Resource").GetChild(Array.IndexOf(requiredResources, Resource))
//                        .GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
//                }
//                //else if (ResourceManager.Instance.GetResourceAmount(Resource) < Amount)
//                //{
//                //    Debug.Log("Not enough " + Resource);
//                //}
//            }
//            // Contract completed
//            if (tempResourceDictionary[contract].Count == 0)
//            {
//                activatedContracts.Remove(contract);
//                completedContracts.Add(contract);
//                contractObj.transform.Find("Activation_Btn").GetComponent<Button>().onClick.RemoveAllListeners();
//                contractObj.transform.Find("Activation_Btn").GetComponent<Button>().onClick.AddListener(
//                () => PopupManager.Instance.PopupConfirmationPanel("Contract already completed", null, null));

//                SetActiveContractCounter();

//                CheckDependencyContracts(contract);

//                OnContractRewarded(contract);

//                GameManager.Instance.AddXP(contract.xpReward);
//                ShowCompletedContract(contract, contract.pageNameToGo);
//                // Move completed contract to completed contracts panel
//                contractObj.transform.SetParent(completedContractPanel.transform);
//            }
//        }
//    }

//    void CheckDependencyContracts(ContractBase contract)
//    {
//        var c = GetGameObjectFromContract(contract);
//        var lockTransform = c.transform.Find("Level_Lock(Clone)");
//        var dependedntContracts = c.GetComponent<ContractHolder>().contract.dependentContracts;

//        int counter = 0;
//        for (int i = 0; i < dependedntContracts.Length; i++)
//        {
//            if (completedContracts.Contains(dependedntContracts[i]))
//                counter++;
//        }
//        if (dependedntContracts.Length == counter)
//        {
//            // Contract completed
//            if (lockTransform != null)
//            {
//                c.transform.Find("Activation_Btn").GetComponent<Button>().interactable = true;
//                Destroy(lockTransform.gameObject);
//            }
//        }
//        else
//        {
//            if (lockTransform != null)
//            {
//                lockTransform.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Unlocked when <color=red>{0}</color> completed",
//                    contractDependencyDictionary[c.GetComponent<ContractHolder>().contract][0].contractName);
//            }
//        }
//    }

//    void SetActiveContractCounter()
//    {
//        activeContractCounter.GetComponentInChildren<TextMeshProUGUI>().text = activatedContracts.Count.ToString();
//        if (activatedContracts.Count > 0)
//            activeContractCounter.SetActive(true);
//        else
//            activeContractCounter.SetActive(false);
//    }

//    public void ActivateContract(ContractBase contract)
//    {
//        if (!activatedContracts.Contains(contract))
//        {
//            activatedContracts.Add(contract);

//            var contractObj = GetGameObjectFromContract(contract);
//            contractObj.GetComponent<Image>().color = new Color(11 / 256f, 253 / 256f, 54 / 256f);
//            contractObj.transform.SetAsFirstSibling();
//            contractObj.transform.Find("Activation_Btn").GetComponent<Button>().onClick.RemoveAllListeners();


//            Debug.Log(contract.contractName + " contract activated");

//            SetActiveContractCounter();
//            CheckAvailableResources();
//        }
//    }

//    // TODO implement this function to deactivate contract on click
//    public void DeActivateContract(ContractBase contract)
//    {
//        if (activatedContracts.Contains(contract))
//        {
//            activatedContracts.Remove(contract);

//            for (int i = 0; i < instantiatedContracts.Count; i++)
//            {
//                if (contract == contract)
//                {
//                    instantiatedContracts[i].GetComponent<Image>().color = new Color(0,0,0);
//                    instantiatedContracts[i].transform.SetAsLastSibling();

//                    instantiatedContracts[i].transform.Find("Activation_Btn").GetComponent<Button>().onClick.AddListener(
//                    () => PopupManager.Instance.PopupConfirmationPanel(("Do you want to activate " + contract.contractName + " Contract"),
//                    () => ActivateContract(contract),
//                    () => PopupManager.Instance.confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false)));

//                    instantiatedContracts[i].transform.Find("Activation_Btn").GetComponent<Button>().onClick.RemoveAllListeners();
//                }
//            }
//        }
//    }

//    void OnContractRewarded(ContractBase contract)
//    {
//        // Give reward according to contract reward type
//        if (contract.contractRewardType == ContractRewardType.Currency)
//            ResourceManager.Instance.Currency += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.PremiumCurrency)
//            ResourceManager.Instance.PremiumCurrency += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.incresedCoinEarning)
//            UpgradeSystem.Instance.EarnedCoinMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.miningSpeedUp)
//            UpgradeSystem.Instance.MiningSpeedMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.miningYieldUpgrade)
//            UpgradeSystem.Instance.MiningYieldMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.productionEfficiencyUpgrade)
//            UpgradeSystem.Instance.ProductionEfficiencyMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.productionSpeedUp)
//            UpgradeSystem.Instance.ProductionSpeedMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.productionYieldUpgrade)
//            UpgradeSystem.Instance.ProductionYieldMultiplier += contract.contractReward;

//        else if (contract.contractRewardType == ContractRewardType.unitSpeedUp)
//            UpgradeSystem.Instance.SpeedUpDictionaryValue = new KeyValuePair<BaseResources, float>(contract.resourceToRewarded, contract.contractReward);

//        else if (contract.contractRewardType == ContractRewardType.unlockProductionUnit)
//        {
//            for (int j = 0; j < ProductionManager.Instance.instantiatedProductionUnits.Count; j++)
//            {
//                var unit = ProductionManager.Instance.instantiatedProductionUnits[j].GetComponent<ProductionBase>();
//                if (contract.productsToUnlock != null && contract.productsToUnlock.Contains(unit.scriptableProductionBase))
//                {
//                    unit.ContractStatueCheckDictionary[contract] = true;
//                    if (!unit.ContractStatueCheckDictionary.ContainsValue(false))
//                    {
//                        unit.IsLockedByContract = false;
//                        Destroy(unit.transform.Find("Level_Lock(Clone)").gameObject);
//                    }
//                }
//            }
//        }
//    }

//    void ShowCompletedContract(ContractBase contract, string pageName)
//    {
//        var panel = contractFinishedInfoPanel.transform;
//        panel.Find("Icon").GetComponent<Image>().sprite = contract.icon;
//        panel.Find("HEADER").GetComponent<TextMeshProUGUI>().text = contract.rewardPanelHeader;
//        panel.Find("Description").GetComponent<TextMeshProUGUI>().text = contract.rewardPanelDescription;
//        panel.Find("OK_Btn").GetComponent<Button>().onClick.AddListener(() => { contractFinishedInfoPanel.SetActive(false); });
//        panel.Find("GoToPage_Btn").GetComponent<Button>().onClick.AddListener(() => { SetPageSettings(pageName);  contractFinishedInfoPanel.SetActive(false); });
//        contractFinishedInfoPanel.SetActive(true);
//    }

//    /// <summary>
//    /// Set page settings for go to page button
//    /// </summary>
//    /// <param name="pageName">Production page name to go</param>
//    void SetPageSettings(string pageName)
//    {
//        if (pageName == "Map")
//        {
//            productionPanel.transform.parent.parent.parent.Find("Bottom_Panel").Find("Map").GetComponent<TabButton>().OnPointerClick(null);
//            return;
//        }

//        // Navigate from any page to production page
//        ProductionPanelBtn.GetComponent<TabButton>().OnPointerClick(null);

//        // Go to page in production
//        for (int i = 0; i < productionPanel.transform.childCount; i++)
//        {
//            var child = productionPanel.transform.GetChild(i);
//            if (productionPanel.transform.GetChild(i).name == pageName)
//            {
//                child.GetComponent<CanvasGroup>().interactable = true;
//                child.GetComponent<CanvasGroup>().alpha = 1;
//                child.GetComponent<CanvasGroup>().blocksRaycasts = true;

//                // Place to front for interaction
//                child.transform.SetAsLastSibling();
//            }
//            else
//            {
//                child.GetComponent<CanvasGroup>().interactable = false;
//                child.GetComponent<CanvasGroup>().alpha = 0;
//                child.GetComponent<CanvasGroup>().blocksRaycasts = false;
//            }
//        }
//        PageManager.Instance.minePageInfoText.text = pageName;
//    }

//    //void OnContractExpired(ContractBase contract)
//    //{
//    //    for (int i = 0; i < contractPanel.transform.childCount; i++)
//    //    {
//    //        if (contractPanel.transform.GetChild(i).GetComponent<ContractBase>() == contract)
//    //        {
//    //            Destroy(contractPanel.transform.GetChild(i).gameObject);
//    //        }
//    //    }
//    //}
//}