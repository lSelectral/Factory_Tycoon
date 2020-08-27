using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ContractManager : Singleton<ContractManager>
{
    public GameObject contactAgreePrefab;
    [SerializeField] private GameObject contractPrefab;
    [SerializeField] private ContractBase[] contracts;
    [SerializeField] private GameObject contractPanel;
    [SerializeField] private GameObject activeContractCounter;

    [SerializeField] private GameObject contractFinishedInfoPanel;

    [SerializeField] private GameObject productionPanel;
    [SerializeField] private GameObject ProductionPanelBtn;

    private List<GameObject> instantiatedContracts;

    private List<List<BaseResources>> tempResources;

    List<ContractBase> activatedContracts;
    List<ContractBase> completedContracts;

    public class OnContractCompleteEventArgs: EventArgs
    {
        public ContractBase contract;
    }

    public event EventHandler<OnContractCompleteEventArgs> OnContractComplete;

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
    }


    private void Start()
    {
        for (int i = 0; i < contracts.Length; i++)
        {
            tempResources.Add(contracts[i].requiredResources.ToList());

            var _contract = Instantiate(contractPrefab, contractPanel.transform);
            _contract.transform.Find("Texts").Find("Header").GetComponent<TextMeshProUGUI>().text = contracts[i].contractName;
            _contract.transform.Find("Texts").Find("Description").GetComponent<TextMeshProUGUI>().text = contracts[i].description;
            _contract.transform.Find("Texts").Find("Reward").GetComponent<TextMeshProUGUI>().text = contracts[i].contractReward.ToString();
            _contract.transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount = 0;
            _contract.transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text = "Progress: %0";

            if (contracts[i].icon != null)
                _contract.transform.Find("Icon").GetComponent<Image>().sprite = contracts[i].icon;

            for (int j = 0; j < contracts[i].requiredResources.Length; j++)
            {
                var icon = Instantiate(ResourceManager.Instance.iconPrefab, _contract.transform.Find("Required_Resource"));
                icon.transform.Find("Image").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(contracts[i].requiredResources[j]);
                icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(contracts[i].requiredResourceAmounts[j]);
            }
            instantiatedContracts.Add(_contract);

            string contractName = contracts[i].contractName;
            ContractBase contract = contracts[i];

            _contract.GetComponent<Button>().onClick.AddListener(
                () => PopupManager.Instance.PopupConfirmationPanel(("Do you want to activate " + contract.contractName),
                () => ActivateContract(contract), 
                () => PopupManager.Instance.confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false)));
        }
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

    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
    {
        CheckAvailableResources();
    }

    void CheckAvailableResources()
    {
        for (int i = 0; i < contracts.Length; i++)
        {
            if (activatedContracts.Contains(contracts[i]) && !completedContracts.Contains(contracts[i]))
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
                        instantiatedContracts[i].transform.Find("PercentageCompleted").GetComponent<TextMeshProUGUI>().text = "Progress: %" + ((instantiatedContracts[i].transform.Find("Outline").Find("Fill").GetComponent<Image>().fillAmount).ToString("F2"));
                        instantiatedContracts[i].transform.Find("Required_Resource").GetChild(Array.IndexOf(requiredResources, input.Resource)).GetChild(0).GetChild(0).gameObject.SetActive(true);
                    }
                    else if (ResourceManager.Instance.GetResourceAmount(input.Resource) < input.Amount)
                    {
                        //Debug.Log("Not enough " + input.Resource);
                    }
                }

                if (tempResources[i].Count == 0)
                {
                    completedContracts.Add(contracts[i]);
                    OnContractComplete(this, new OnContractCompleteEventArgs() { contract = contracts[i] });
                    instantiatedContracts[i].GetComponent<Button>().onClick.AddListener(
                    () => PopupManager.Instance.PopupConfirmationPanel("Contract already completed", null, null));


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
                                // Destroy lock to unlock this element
                                Destroy(obj.transform.Find("Level_Lock(Clone)").gameObject);
                            }
                        }
                    }
                    else if (contracts[i].contractRewardType == ContractRewardType.unlockMine)
                    {
                        foreach (GameObject obj in ProductionManager.Instance.instantiatedMines)
                        {
                            var mine = obj.GetComponent<Mine_Btn>();
                            if (contracts[i].minesToUnlock.Contains(mine.mine))
                            {
                                // Destroy lock to unlock this element
                                Destroy(obj.transform.Find("Level_Lock(Clone)").gameObject);
                            }
                        }
                    }

                    GameManager.Instance.AddXP(contracts[i].xpReward);

                    Debug.Log(contracts[i].contractName + " contract completed");

                    ShowCompletedContract(contracts[i], contracts[i].pageNameToGo);
                }
            }
        }
    }

    public void ActivateContract(ContractBase contract)
    {
        activatedContracts.Add(contract);


        for (int i = 0; i < instantiatedContracts.Count; i++)
        {
            if (contracts[i] == contract)
            {
                instantiatedContracts[i].GetComponent<Image>().color = new Color(11 / 256f, 253 / 256f, 54 / 256f);
                instantiatedContracts[i].transform.SetAsFirstSibling();
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
}

public enum ContractActivationType
{
    levelUp,
    resourceCollection,
    contractCompletion
}