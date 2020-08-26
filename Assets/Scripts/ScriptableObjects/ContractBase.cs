using UnityEngine;

[CreateAssetMenu(fileName = "New Contract", menuName = "Contract")]
public class ContractBase : ScriptableObject
{
    public string contractName;
    public string description;

    public long contractReward;
    public float contractTime; // In seconds
    public ContractRewardType contractRewardType;
    public ScriptableCompound[] compoundsToUnlock;
    public ScriptableMine[] minesToUnlock;
    public BaseResources[] requiredResources;
    public int[] requiredResourceAmounts;

    public int unlockLevel;

    public Sprite icon;

    public string rewardPanelHeader;

    public string rewardPanelDescription;
    public string pageNameToGo;
    public long xpReward;
}