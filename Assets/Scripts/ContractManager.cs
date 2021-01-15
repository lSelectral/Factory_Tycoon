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


public class ContractManager : Singleton<ContractManager>
{
    public GameObject contactAgreePrefab;
    [SerializeField] private GameObject contractPrefab;

    /* All Contracts available in game */ public ContractBase[] contracts;
    [SerializeField] private GameObject contractPanel;
    /* Completed contracts will moved to this panel */ [SerializeField] private GameObject completedContractPanel;
    /* Show number of activated contracts in contract panel */ [SerializeField] private GameObject activeContractCounter;

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
        if (instantiatedContracts == null)
            instantiatedContracts = new List<GameObject>();
        if (completedContracts == null)
            completedContracts = new List<ContractBase>();

        if (activatedContracts == null)
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
                c.GetComponent<Button>().interactable = true;
                Destroy(c.transform.Find("Level_Lock(Clone)").gameObject);
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

            if (asset as ScriptableObject != null)
            {
                var sc = asset as ScriptableObject;
                if (sc.GetType() == typeof(ContractBase) && (sc as ContractBase).contractName != "")
                {
                    tempContracts.Add(sc as ContractBase);
                    CreateContract(sc as ContractBase);
                }
            }
        }
        contracts = tempContracts.ToArray();

        if (activatedContracts != null && activatedContracts.Count <= 0)
        {
            activeContractCounter.GetComponentInChildren<TextMeshProUGUI>().text = "0";
            activeContractCounter.SetActive(false);
        }
        else
        {
            activeContractCounter.GetComponentInChildren<TextMeshProUGUI>().text = activatedContracts.Count.ToString();
            activeContractCounter.SetActive(true);
        }
    }

    /// <summary>
    /// Not for rungame. Used in editor for creating automation assets automatically.
    /// Used frome editor script CustomEditorWindow.cs
    /// For creating simply click create automation contracts button. If missing just remove comment mark from editor script
    /// </summary>
    /// <returns>List of contracts for automation of production</returns>
    /// <seealso cref="Assets\Scripts\Editor\CustomEditorWindow.cs"/>
    public List<ContractBase> CreateAutomationContracts()
    {
        List<ContractBase> automationContracts = new List<ContractBase>();
        foreach (BaseResources resource in Enum.GetValues(typeof(BaseResources)))
        {
            ScriptableMine mine = ProductionManager.Instance.GetScriptableProductionUnitFromResource(resource) as ScriptableMine;
            ScriptableCompound compound = ProductionManager.Instance.GetScriptableProductionUnitFromResource(resource) as ScriptableCompound;

            ScriptableMine[] mineArray = null;
            ScriptableCompound[] compoundArray = null;
            BaseResources[] requiredResources = { resource };
            int[] requiredResourceAmounts = { 500 };

            string pageNameToGo = "";
            int unlockLevel = 2;
            long xpReward = 100;

            if (mine != null)
            {
                mineArray = new ScriptableMine[] { mine };
                pageNameToGo = mine.ageBelongsTo.ToString();
                unlockLevel = mine.unlockLevel++;
                xpReward = mine.xpAmount * 15;
            }
            else if (compound != null)
            {
                compoundArray = new ScriptableCompound[] { compound };
                pageNameToGo = compound.ageBelongsTo.ToString();
                unlockLevel = compound.unlockLevel++;
                xpReward = compound.xpAmount * 15;
            }

            ContractBase contract = new ContractBase()
            {
                contractName = ResourceManager.Instance.GetValidNameForResource(resource),
                contractReward = 0,
                compoundsToUnlock = compoundArray,
                contractRewardType = ContractRewardType.automate,
                description = ResourceManager.Instance.GetValidNameForResource(resource) + " will automatically collected",
                icon = ResourceManager.Instance.GetSpriteFromResource(resource),
                minesToUnlock = mineArray,
                pageNameToGo = pageNameToGo,
                unlockLevel = unlockLevel,
                rewardPanelHeader = string.Format("<color=red>Congrulations!</color> for Automating {0}", ResourceManager.Instance.GetValidNameForResource(resource)),
                rewardPanelDescription = string.Format("{0} will automatically processed.",ResourceManager.Instance.GetValidNameForResource(resource)),
                xpReward = xpReward,
                requiredResources = requiredResources,
                requiredResourceAmounts = requiredResourceAmounts,
            };
            automationContracts.Add(contract);
        }
        return automationContracts;
    }

    public void CreateContract(ContractBase contract)
    {
        // Instantiate prefab and enter information from scriptable object
        var _contract = Instantiate(contractPrefab, contractPanel.transform);
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
            icon.transform.Find("Image").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(contract.requiredResources[i]);
            icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(contract.requiredResourceAmounts[i]);
        }
        instantiatedContracts.Add(_contract);

        if (!completedContracts.Contains(contract))
        {
            _contract.GetComponent<Button>().onClick.AddListener(
            () => PopupManager.Instance.PopupConfirmationPanel(("Do you want to activate " + contract.contractName + " Contract"),
            () => ActivateContract(contract),
            () => PopupManager.Instance.confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false)));

            if (contract.unlockLevel > GameManager.Instance.CurrentLevel)
            {
                _contract.GetComponent<Button>().interactable = false;
                var lockText = Instantiate(GameManager.Instance.levelLock, _contract.transform);
                lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT LEVEL " + contract.unlockLevel.ToString();
            }
        }
        else
        {
            _contract.transform.SetParent(completedContractPanel.transform);
        }
    }

    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
    {
        CheckAvailableResources();
    }

    void CheckAvailableResources()
    {
        for (int i = 0; i < contracts.Length; i++)
        {
            if (activatedContracts.Contains(contracts[i]) && !completedContracts.Contains(contracts[i]) && !completedContracts.Contains(contracts[i]))
            {
                var requiredResources = contracts[i].requiredResources;
                var requiredResourceAmounts = contracts[i].requiredResourceAmounts;
                int totalResourceAmount = 0;

                for (int j = 0; j < contracts[i].requiredResourceAmounts.Length; j++)
                {
                    totalResourceAmount += contracts[i].requiredResourceAmounts[j];
                }

                var inputs = requiredResources.Zip(requiredResourceAmounts, (resource, amount) => (Resource: resource, Amount: amount));
                foreach (var input in inputs)
                {
                    if (ResourceManager.Instance.GetResourceAmount(input.Resource) >= input.Amount && tempResources[i].Contains(input.Resource))
                    {
                        tempResources[i].Remove(input.Resource);
                        ResourceManager.Instance.ConsumeResource(input.Resource, input.Amount);
                        instantiatedContracts[i].transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount += (input.Amount * 1f / totalResourceAmount);
                        instantiatedContracts[i].transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text =
                            "Progress: %" + (instantiatedContracts[i].transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount).ToString("F2");
                        instantiatedContracts[i].transform.Find("Required_Resource").GetChild(Array.IndexOf(requiredResources, input.Resource)).GetChild(0).GetChild(0).gameObject.SetActive(true);
                    }
                    else if (ResourceManager.Instance.GetResourceAmount(input.Resource) < input.Amount)
                    {
                        //Debug.Log("Not enough " + input.Resource);
                    }
                }
                // Contract completed
                if (tempResources[i].Count == 0)
                {
                    activatedContracts.Remove(contracts[i]);
                    completedContracts.Add(contracts[i]);
                    instantiatedContracts[i].GetComponent<Button>().onClick.RemoveAllListeners();
                    instantiatedContracts[i].GetComponent<Button>().onClick.AddListener(
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

                    // Give reward according to contract reward type
                    if (contracts[i].contractRewardType == ContractRewardType.Currency)
                        ResourceManager.Instance.Currency += contracts[i].contractReward;
                    else if (contracts[i].contractRewardType == ContractRewardType.PremiumCurrency)
                        ResourceManager.Instance.PremiumCurrency += contracts[i].contractReward;
                    else if (contracts[i].contractRewardType == ContractRewardType.incresedCoinEarning)
                        UpgradeSystem.Instance.EarnedCoinMultiplier += contracts[i].contractReward;
                    else if (contracts[i].contractRewardType == ContractRewardType.miningSpeedUp)
                        UpgradeSystem.Instance.MiningSpeedMultiplier += contracts[i].contractReward;
                    else if (contracts[i].contractRewardType == ContractRewardType.miningYieldUpgrade)
                        UpgradeSystem.Instance.MiningYieldMultiplier += contracts[i].contractReward;
                    else if (contracts[i].contractRewardType == ContractRewardType.productionEfficiencyUpgrade)
                        UpgradeSystem.Instance.ProductionEfficiencyMultiplier += contracts[i].contractReward;
                    else if (contracts[i].contractRewardType == ContractRewardType.productionSpeedUp)
                        UpgradeSystem.Instance.ProductionSpeedMultiplier += contracts[i].contractReward;
                    else if (contracts[i].contractRewardType == ContractRewardType.productionYieldUpgrade)
                        UpgradeSystem.Instance.ProductionYieldMultiplier += contracts[i].contractReward;
                    else if (contracts[i].contractRewardType == ContractRewardType.unlockCompound)
                    {
                        foreach (GameObject obj in ProductionManager.Instance.instantiatedCompounds)
                        {
                            var compound = obj.GetComponent<Compounds>();
                            if (contracts[i].compoundsToUnlock.Contains(compound.scriptableCompound))
                            {
                                compound.ContractStatueCheckDictionary[contracts[i]] = true;

                                if (!compound.ContractStatueCheckDictionary.ContainsValue(false))
                                {
                                    compound.IsLockedByContract = false;
                                    Destroy(obj.transform.Find("Level_Lock(Clone)").gameObject);
                                }
                            }
                        }
                    }
                    else if (contracts[i].contractRewardType == ContractRewardType.unlockMine)
                    {
                        foreach (GameObject obj in ProductionManager.Instance.instantiatedMines)
                        {
                            var mine = obj.GetComponent<Mine_Btn>();
                            if (contracts[i].minesToUnlock.Contains(mine.scriptableMine))
                            {
                                mine.ContractStatueCheckDictionary[contracts[i]] = true;

                                if (!mine.ContractStatueCheckDictionary.ContainsValue(false))
                                {
                                    mine.IsLockedByContract = false;
                                    Destroy(obj.transform.Find("Level_Lock(Clone)").gameObject);
                                }
                            }
                        }
                    }
                    else if (contracts[i].contractRewardType == ContractRewardType.automate)
                    {
                        var mineAndCompounds = ProductionManager.Instance.instantiatedMines.Zip(ProductionManager.Instance.instantiatedCompounds, (mine, compound) => (Mine: mine, Compound: compound));

                        foreach (var obj in mineAndCompounds)
                        {
                            if (obj.Mine != null && contracts[i].minesToUnlock.Contains(obj.Mine.GetComponent<Mine_Btn>().scriptableMine))
                                obj.Mine.GetComponent<Mine_Btn>().IsAutomated = true;
                            else if (obj.Compound != null && contracts[i].compoundsToUnlock.Contains(obj.Mine.GetComponent<Compounds>().scriptableCompound))
                                obj.Compound.GetComponent<Compounds>().IsAutomated = true;
                        }
                    }

                    GameManager.Instance.AddXP(contracts[i].xpReward);
                    ShowCompletedContract(contracts[i], contracts[i].pageNameToGo);
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

                    instantiatedContracts[i].GetComponent<Button>().onClick.RemoveAllListeners();
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
    // TODO implement this function to activated contract on click
    public void DeActivateContract(ContractBase contract)
    {
        if (activatedContracts.Contains(contract))
        {
            activatedContracts.Remove(contract);

            for (int i = 0; i < instantiatedContracts.Count; i++)
            {
                if (contracts[i] == contract)
                {
                    instantiatedContracts[i].GetComponent<Image>().color = new Color(0,0,0);
                    instantiatedContracts[i].transform.SetAsLastSibling();

                    instantiatedContracts[i].GetComponent<Button>().onClick.AddListener(
                    () => PopupManager.Instance.PopupConfirmationPanel(("Do you want to activate " + contract.contractName + " Contract"),
                    () => ActivateContract(contract),
                    () => PopupManager.Instance.confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false)));

                    instantiatedContracts[i].GetComponent<Button>().onClick.RemoveAllListeners();
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
}

public enum ContractActivationType
{
    levelUp,
    resourceCollection,
    contractCompletion
}