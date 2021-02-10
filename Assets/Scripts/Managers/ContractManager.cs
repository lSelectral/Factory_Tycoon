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
    public GameObject contactAgreePrefab;
    [SerializeField] private GameObject contractPrefab;

    /* All Contracts available in game */ public ContractBase[] contracts;
    [SerializeField] private GameObject contractPanel;
    /* Completed contracts will moved to this panel */ [SerializeField] private GameObject completedContractPanel;
    /* Show number of activated contracts in contract panel */ [SerializeField] private GameObject activeContractCounter;

    Dictionary<ContractBase, List<ContractBase>> contractDependencyDictionary;

    [SerializeField] private GameObject contractFinishedInfoPanel;

    [SerializeField] private GameObject productionPanel;
    [SerializeField] private GameObject ProductionPanelBtn;

    public List<GameObject> instantiatedContracts;

    public List<List<BaseResources>> tempResources;

    public List<ContractBase> activatedContracts;
    public List<ContractBase> completedContracts;

    UnityEngine.Object[] assets;

    private void Awake()
    {
        tempResources = new List<List<BaseResources>>();
        instantiatedContracts = new List<GameObject>();
        completedContracts = new List<ContractBase>();
        contractDependencyDictionary = new Dictionary<ContractBase, List<ContractBase>>();
        activatedContracts = new List<ContractBase>();

        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
        GameManager.Instance.OnLevelUp += OnLevelUp;
    }

    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        for (int i = 0; i < instantiatedContracts.Count; i++)
        {
            var c = instantiatedContracts[i];
            if (c.transform.Find("Level_Lock(Clone)") != null && contracts.Length > 0 && contracts[i].unlockLevel == e.currentLevel)
            {
                c.transform.Find("Activation_Btn").GetComponent<Button>().interactable = true;
                Destroy(c.transform.Find("Level_Lock(Clone)").gameObject);
            }
            if (contracts[i].dependentContracts != null && contracts[i].dependentContracts.Length > 0)
            {
                c.transform.Find("Activation_Btn").GetComponent<Button>().interactable = false;
                var lockText = Instantiate(GameManager.Instance.levelLock, c.transform);
                lockText.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Unlocked when <color=red>{0}</color> completed", contracts[i].dependentContracts[0].contractName);
            }
        }

        for (int i = 0; i < contracts.Length; i++)
        {
            var contract = contracts[i];
            if (contract.contractName != "" && contract.unlockLevel == GameManager.Instance.CurrentLevel)
            {
                CreateContract(contract);

                if (contract.dependentContracts != null && contract.dependentContracts.Length > 0)
                    contractDependencyDictionary.Add(contract, contract.dependentContracts.ToList());
            }
        }
    }

    private void Start()
    {
        assets = Resources.LoadAll("Contracts");
        List<ContractBase> tempContracts = new List<ContractBase>();
        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];

            if (asset as ContractBase != null)
            {
                var contractBase = asset as ContractBase;
                tempContracts.Add(contractBase);
                if (contractBase.contractName != "" && contractBase.unlockLevel <= GameManager.Instance.CurrentLevel)
                {
                    CreateContract(contractBase);

                    if (contractBase.dependentContracts != null && contractBase.dependentContracts.Length > 0)
                        contractDependencyDictionary.Add(contractBase, contractBase.dependentContracts.ToList());
                }
            }
        }
        contracts = tempContracts.ToArray();

        // Set active contract counter
        activeContractCounter.GetComponentInChildren<TextMeshProUGUI>().text = activatedContracts.Count.ToString();
        if (activatedContracts != null && activatedContracts.Count <= 0)
            activeContractCounter.SetActive(false);
        else
            activeContractCounter.SetActive(true);
    }

    ///// <summary>
    ///// Not for rungame. Used in editor for creating automation assets automatically.
    ///// Used frome editor script CustomEditorWindow.cs
    ///// For creating simply click create automation contracts button. If missing just remove comment mark from editor script
    ///// </summary>
    ///// <returns>List of contracts for automation of production</returns>
    ///// <seealso cref="Assets\Scripts\Editor\CustomEditorWindow.cs"/>
    //public List<ContractBase> CreateAutomationContracts()
    //{
    //    List<ContractBase> automationContracts = new List<ContractBase>();
    //    foreach (BaseResources resource in Enum.GetValues(typeof(BaseResources)))
    //    {
    //        ScriptableMine mine = ProductionManager.Instance.GetScriptableProductionUnitFromResource(resource) as ScriptableMine;
    //        ScriptableCompound compound = ProductionManager.Instance.GetScriptableProductionUnitFromResource(resource) as ScriptableCompound;

    //        ScriptableMine[] mineArray = null;
    //        ScriptableCompound[] compoundArray = null;
    //        BaseResources[] requiredResources = { resource };
    //        int[] requiredResourceAmounts = { 500 };

    //        string pageNameToGo = "";
    //        int unlockLevel = 2;
    //        long xpReward = 100;

    //        if (mine != null)
    //        {
    //            mineArray = new ScriptableMine[] { mine };
    //            pageNameToGo = mine.ageBelongsTo.ToString();
    //            unlockLevel = mine.unlockLevel++;
    //            xpReward = mine.xpAmount * 15;
    //        }
    //        else if (compound != null)
    //        {
    //            compoundArray = new ScriptableCompound[] { compound };
    //            pageNameToGo = compound.ageBelongsTo.ToString();
    //            unlockLevel = compound.unlockLevel++;
    //            xpReward = compound.xpAmount * 15;
    //        }

    //        ContractBase contract = new ContractBase()
    //        {
    //            contractName = ResourceManager.Instance.GetValidNameForResource(resource),
    //            contractReward = 0,
    //            productsToUnlock = compoundArray,
    //            contractRewardType = ContractRewardType.automate,
    //            description = ResourceManager.Instance.GetValidNameForResource(resource) + " will automatically collected",
    //            icon = ResourceManager.Instance.GetSpriteFromResource(resource),
    //            pageNameToGo = pageNameToGo,
    //            unlockLevel = unlockLevel,
    //            rewardPanelHeader = string.Format("<color=red>Congrulations!</color> for Automating {0}", ResourceManager.Instance.GetValidNameForResource(resource)),
    //            rewardPanelDescription = string.Format("{0} will automatically processed.",ResourceManager.Instance.GetValidNameForResource(resource)),
    //            xpReward = xpReward,
    //            requiredResources = requiredResources,
    //            requiredResourceAmounts = requiredResourceAmounts,
    //        };
    //        automationContracts.Add(contract);
    //    }
    //    return automationContracts;
    //}

    public void CreateContract(ContractBase contract)
    {
        // Instantiate prefab and enter information from scriptable object
        var _contract = Instantiate(contractPrefab, contractPanel.transform);
        _contract.AddComponent<ContractHolder>();
        _contract.GetComponent<ContractHolder>().contract = contract;
        _contract.transform.Find("Texts").Find("Header").GetComponent<TextMeshProUGUI>().text = contract.contractName;
        _contract.transform.Find("Texts").Find("Description").GetComponent<TextMeshProUGUI>().text = contract.description;
        _contract.transform.Find("Texts").Find("Reward").GetComponent<TextMeshProUGUI>().text = contract.contractReward.ToString();
        _contract.transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount = 0;
        _contract.transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text = "Progress: %0";
        tempResources.Add(contract.requiredResources.ToList());

        if (contract.icon != null)
            _contract.transform.Find("Icon").GetComponent<Image>().sprite = contract.icon;

        for (int i = 0; i < contract.requiredResources.Length; i++)
        {
            var icon = Instantiate(ResourceManager.Instance.iconPrefab, _contract.transform.Find("Required_Resource"));
            icon.transform.Find("Frame").Find("Icon").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(contract.requiredResources[i]);
            icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(contract.requiredResourceAmounts[i]);
        }
        instantiatedContracts.Add(_contract);

        if (!completedContracts.Contains(contract))
        {
            _contract.transform.Find("Activation_Btn").GetComponent<Button>().onClick.AddListener(
            () => PopupManager.Instance.PopupConfirmationPanel(("Do you want to activate " + contract.contractName + " Contract"),
            () => ActivateContract(contract),
            () => PopupManager.Instance.confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false)));

            if (contract.dependentContracts != null && contract.dependentContracts.Length > 0)
            {
                _contract.transform.Find("Activation_Btn").GetComponent<Button>().interactable = false;
                var lockText = Instantiate(GameManager.Instance.levelLock, _contract.transform);
                lockText.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Unlocked when <color=red>{0}</color> completed", contract.dependentContracts[0].contractName);
            }
            else if (contract.unlockLevel > GameManager.Instance.CurrentLevel)
            {
                _contract.transform.Find("Activation_Btn").GetComponent<Button>().interactable = false;
                var lockText = Instantiate(GameManager.Instance.levelLock, _contract.transform);
                lockText.GetComponentInChildren<TextMeshProUGUI>().text = "Unlocked at level " + contract.unlockLevel.ToString();
            }
        }
        else if (completedContracts.Contains(contract))
        {
            _contract.transform.SetParent(completedContractPanel.transform);
        }
        else if (activatedContracts.Contains(contract))
        {
        }
    }

    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
    {
        if (!e.isConsumed)
            CheckAvailableResources();
    }

    void CheckAvailableResources()
    {
        for (int i = 0; i < contracts.Length; i++)
        {
            var contract = contracts[i];
            if (activatedContracts.Contains(contract) && !completedContracts.Contains(contract))
            {
                var requiredResources = contract.requiredResources;
                var requiredResourceAmounts = contract.requiredResourceAmounts;
                int totalResourceAmount = 0;

                for (int j = 0; j < contract.requiredResourceAmounts.Length; j++)
                {
                    totalResourceAmount += contract.requiredResourceAmounts[j];
                }

                var inputs = requiredResources.Zip(requiredResourceAmounts, (resource, amount) => (Resource: resource, Amount: amount));
                foreach (var (Resource, Amount) in inputs)
                {
                    if (ResourceManager.Instance.GetResourceAmount(Resource) >= Amount && tempResources[i].Contains(Resource))
                    {
                        tempResources[i].Remove(Resource);
                        ResourceManager.Instance.ConsumeResource(Resource, Amount);
                        instantiatedContracts[i].transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount += (Amount * 1f / totalResourceAmount);
                        instantiatedContracts[i].transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text =
                            "Progress: %" + (instantiatedContracts[i].transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount).ToString("F2");
                        instantiatedContracts[i].transform.Find("Required_Resource").GetChild(Array.IndexOf(requiredResources, Resource)).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
                    }
                    else if (ResourceManager.Instance.GetResourceAmount(Resource) < Amount)
                    {
                        //Debug.Log("Not enough " + input.Resource);
                    }
                }
                // Contract completed
                if (tempResources[i].Count == 0)
                {
                    activatedContracts.Remove(contract);
                    completedContracts.Add(contract);
                    instantiatedContracts[i].transform.Find("Activation_Btn").GetComponent<Button>().onClick.RemoveAllListeners();
                    instantiatedContracts[i].transform.Find("Activation_Btn").GetComponent<Button>().onClick.AddListener(
                    () => PopupManager.Instance.PopupConfirmationPanel("Contract already completed", null, null));

                    // Set active contract counter
                    if (activatedContracts != null && activatedContracts.Count > 0)
                    {
                        activeContractCounter.GetComponentInChildren<TextMeshProUGUI>().text = activatedContracts.Count.ToString();
                        activeContractCounter.SetActive(true);
                    }
                    else if (activatedContracts != null && activatedContracts.Count <= 0)
                    {
                        activeContractCounter.GetComponentInChildren<TextMeshProUGUI>().text = activatedContracts.Count.ToString();
                        activeContractCounter.SetActive(false);
                    }

                    // Check if this contract has dependency for another contracts
                    for (int j = 0; j < instantiatedContracts.Count; j++)
                    {
                        if (contracts[j].dependentContracts != null && contracts[j].dependentContracts.Contains(contract))
                        {
                            var counter = 0;
                            for (int k = 0; k < contracts[j].dependentContracts.Length; k++)
                            {
                                if (completedContracts.Contains(contracts[j].dependentContracts[k]))
                                {
                                    contractDependencyDictionary[contracts[j]].Remove(contracts[j].dependentContracts[k]);
                                    counter++;
                                }
                            }

                            var c = instantiatedContracts[j];
                            var lockTransform = c.transform.Find("Level_Lock(Clone)");
                            if (counter == contracts[j].dependentContracts.Length)
                            {
                                // All dependent contracts complete

                                if (lockTransform != null)
                                {
                                    c.transform.Find("Activation_Btn").GetComponent<Button>().interactable = true;
                                    Destroy(lockTransform.gameObject);
                                }
                            }
                            else
                            {
                                if (lockTransform != null)
                                {
                                    lockTransform.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Unlocked when <color=red>{0}</color> completed",
                                        contractDependencyDictionary[contracts[j]][0].contractName);
                                }
                            }
                        }
                    }

                    // Give reward according to contract reward type
                    if (contract.contractRewardType == ContractRewardType.Currency)
                        ResourceManager.Instance.Currency += contract.contractReward;

                    else if (contract.contractRewardType == ContractRewardType.PremiumCurrency)
                        ResourceManager.Instance.PremiumCurrency += contract.contractReward;

                    else if (contract.contractRewardType == ContractRewardType.incresedCoinEarning)
                        UpgradeSystem.Instance.EarnedCoinMultiplier += contract.contractReward;

                    else if (contract.contractRewardType == ContractRewardType.miningSpeedUp)
                        UpgradeSystem.Instance.MiningSpeedMultiplier += contract.contractReward;

                    else if (contract.contractRewardType == ContractRewardType.miningYieldUpgrade)
                        UpgradeSystem.Instance.MiningYieldMultiplier += contract.contractReward;

                    else if (contract.contractRewardType == ContractRewardType.productionEfficiencyUpgrade)
                        UpgradeSystem.Instance.ProductionEfficiencyMultiplier += contract.contractReward;

                    else if (contract.contractRewardType == ContractRewardType.productionSpeedUp)
                        UpgradeSystem.Instance.ProductionSpeedMultiplier += contract.contractReward;

                    else if (contract.contractRewardType == ContractRewardType.productionYieldUpgrade)
                        UpgradeSystem.Instance.ProductionYieldMultiplier += contract.contractReward;

                    else if (contract.contractRewardType == ContractRewardType.unitSpeedUp)
                        UpgradeSystem.Instance.SpeedUpDictionaryValue = new KeyValuePair<BaseResources, float>(contract.resourceToRewarded, contract.contractReward);

                    else if (contract.contractRewardType == ContractRewardType.unlockCompound)
                    {
                        foreach (GameObject obj in ProductionManager.Instance.instantiatedCompounds)
                        {
                            var compound = obj.GetComponent<Compounds>();
                            if (contract.productsToUnlock != null && contract.productsToUnlock.Contains(compound.scriptableCompound))
                            {
                                compound.ContractStatueCheckDictionary[contract] = true;

                                if (!compound.ContractStatueCheckDictionary.ContainsValue(false))
                                {
                                    compound.IsLockedByContract = false;
                                    Destroy(obj.transform.Find("Level_Lock(Clone)").gameObject);
                                }
                            }
                        }
                    }
                    else if (contract.contractRewardType == ContractRewardType.unlockMine)
                    {
                        foreach (GameObject obj in ProductionManager.Instance.instantiatedMines)
                        {
                            var mine = obj.GetComponent<Mine_Btn>();
                            if (contract.productsToUnlock.Contains(mine.scriptableMine))
                            {
                                mine.ContractStatueCheckDictionary[contract] = true;

                                if (!mine.ContractStatueCheckDictionary.ContainsValue(false))
                                {
                                    mine.IsLockedByContract = false;
                                    Destroy(obj.transform.Find("Level_Lock(Clone)").gameObject);
                                }
                            }
                        }
                    }
                    else if (contract.contractRewardType == ContractRewardType.automate)
                    {
                        var mineAndCompounds = ProductionManager.Instance.instantiatedMines.Zip(ProductionManager.Instance.instantiatedCompounds, (mine, compound) => (Mine: mine, Compound: compound));

                        foreach (var (Mine, Compound) in mineAndCompounds)
                        {
                            if (Mine != null && contract.productsToUnlock.Contains(Mine.GetComponent<Mine_Btn>().scriptableMine))
                                Mine.GetComponent<Mine_Btn>().IsAutomated = true;
                            else if (Compound != null && contract.productsToUnlock.Contains(Mine.GetComponent<Compounds>().scriptableCompound))
                                Compound.GetComponent<Compounds>().IsAutomated = true;
                        }
                    }

                    GameManager.Instance.AddXP(contract.xpReward);
                    ShowCompletedContract(contract, contract.pageNameToGo);
                    // Move completed contract to completed contracts panel
                    instantiatedContracts[i].transform.SetParent(completedContractPanel.transform);
                }
            }
        }
    }

    public void ActivateContract(ContractBase contract)
    {
        if (!activatedContracts.Contains(contract))
        {
            activatedContracts.Add(contract);


            for (int i = 0; i < instantiatedContracts.Count; i++)
            {
                if (contracts[i] == contract)
                {
                    instantiatedContracts[i].GetComponent<Image>().color = new Color(11 / 256f, 253 / 256f, 54 / 256f);
                    instantiatedContracts[i].transform.SetAsFirstSibling();

                    instantiatedContracts[i].transform.Find("Activation_Btn").GetComponent<Button>().onClick.RemoveAllListeners();
                }
            }

            Debug.Log(contract.contractName + " contract activated");

            if (activatedContracts != null && activatedContracts.Count > 0)
            {
                activeContractCounter.GetComponentInChildren<TextMeshProUGUI>().text = activatedContracts.Count.ToString();
                activeContractCounter.SetActive(true);
            }
            CheckAvailableResources();
        }
    }
    // TODO implement this function to deactivate contract on click
    public void DeActivateContract(ContractBase contract)
    {
        if (activatedContracts.Contains(contract))
        {
            activatedContracts.Remove(contract);

            for (int i = 0; i < instantiatedContracts.Count; i++)
            {
                if (contract == contract)
                {
                    instantiatedContracts[i].GetComponent<Image>().color = new Color(0,0,0);
                    instantiatedContracts[i].transform.SetAsLastSibling();

                    instantiatedContracts[i].transform.Find("Activation_Btn").GetComponent<Button>().onClick.AddListener(
                    () => PopupManager.Instance.PopupConfirmationPanel(("Do you want to activate " + contract.contractName + " Contract"),
                    () => ActivateContract(contract),
                    () => PopupManager.Instance.confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false)));

                    instantiatedContracts[i].transform.Find("Activation_Btn").GetComponent<Button>().onClick.RemoveAllListeners();
                }
            }
        }
    }

    void ShowCompletedContract(ContractBase contract, string pageName)
    {
        var panel = contractFinishedInfoPanel.transform;
        panel.Find("Icon").GetComponent<Image>().sprite = contract.icon;
        panel.Find("HEADER").GetComponent<TextMeshProUGUI>().text = contract.rewardPanelHeader;
        panel.Find("Description").GetComponent<TextMeshProUGUI>().text = contract.rewardPanelDescription;
        panel.Find("OK_Btn").GetComponent<Button>().onClick.AddListener(() => { contractFinishedInfoPanel.SetActive(false); });
        panel.Find("GoToPage_Btn").GetComponent<Button>().onClick.AddListener(() => { SetPageSettings(pageName);  contractFinishedInfoPanel.SetActive(false); });
        contractFinishedInfoPanel.SetActive(true);
    }

    /// <summary>
    /// Set page settings for go to page button
    /// </summary>
    /// <param name="pageName">Production page name to go</param>
    void SetPageSettings(string pageName)
    {
        if (pageName == "Map")
        {
            productionPanel.transform.parent.parent.parent.Find("Bottom_Panel").Find("Map").GetComponent<TabButton>().OnPointerClick(null);
            return;
        }

        // Navigate from any page to production page
        ProductionPanelBtn.GetComponent<TabButton>().OnPointerClick(null);

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

    //void OnContractExpired(ContractBase contract)
    //{
    //    for (int i = 0; i < contractPanel.transform.childCount; i++)
    //    {
    //        if (contractPanel.transform.GetChild(i).GetComponent<ContractBase>() == contract)
    //        {
    //            Destroy(contractPanel.transform.GetChild(i).gameObject);
    //        }
    //    }
    //}
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
    unlockCompound,
    unlockMine,
    automate,
    unitSpeedUp,
}

public enum ContractActivationType
{
    levelUp,
    resourceCollection,
    contractCompletion
}

public enum ContractType
{
    story = 0,
    producing = 1,
    war = 2,
}

[Serializable]
public class ContractHolder : MonoBehaviour
{
    public ContractBase contract;
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