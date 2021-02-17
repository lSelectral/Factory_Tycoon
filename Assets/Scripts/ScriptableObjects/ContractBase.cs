using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Contract", menuName = "Contract")]
public class ContractBase : ScriptableObject
{
    public string contractName;
    [TextArea] public string description;
    public ContractType contractType;
    public float contractReward;
    public ContractRewardType contractRewardType;
    [Tooltip("Chosen resource will rewarded if contract reward type is set to Unit Speed Up")]
    [SearchableEnum] public BaseResources resourceToRewarded;

    public ContractBase[] dependentContracts;
    public ScriptableProductionBase[] productsToUnlock;
    [SearchableEnum] public BaseResources[] requiredResources;
    public BigDouble[] requiredResourceAmounts;

    public int unlockLevel;

    [PreviewSprite] public Sprite icon;
    [TextArea] public string rewardPanelHeader = "<color=red>Congrulations</color>";
    [TextArea] public string rewardPanelDescription;
    public AvailableMainPages mainPageToGo = AvailableMainPages.Production;
    public string pageNameToGo;
    [HideInInspector] public bool isPageNameToGoValid;
    public float xpReward;

    public Age ageBelongsTo;
    public int history;

    private void OnValidate()
    {
        if (contractName == "")
            contractName = name;
        if (requiredResources == null || requiredResources.Length == 0)
            requiredResources = new BaseResources[] { BaseResources._0_stick };

        if (requiredResources != null && requiredResources.Length > requiredResourceAmounts.Length)
            Array.Resize(ref requiredResourceAmounts, requiredResources.Length);

        if (requiredResourceAmounts.Length == 0 || requiredResourceAmounts[0] == 0)
            requiredResourceAmounts = new BigDouble[] { new BigDouble(10) };
    }
}