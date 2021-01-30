using UnityEngine;

[CreateAssetMenu(fileName = "New Contract", menuName = "Contract")]
public class ContractBase : ScriptableObject
{
    public string contractName;
    public string description;

    public long contractReward;
    public ContractRewardType contractRewardType;

    public ContractBase[] dependentContracts;
    public ScriptableProductionBase[] productsToUnlock;
    public BaseResources[] requiredResources;
    public int[] requiredResourceAmounts;

    public int unlockLevel;

    [PreviewSprite] public Sprite icon;
    public string rewardPanelHeader = "<color=red>Congrulations</color>";
    public string rewardPanelDescription;
    public string pageNameToGo;
    public long xpReward;

    public Age ageBelongsTo;
    public int history;

    private void OnValidate()
    {
        if (contractName == "")
            contractName = name;
    }
}