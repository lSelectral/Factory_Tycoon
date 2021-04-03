using UnityEngine;

public class ScriptableResearch : ScriptableObject
{
    public string researchName;
    public ResearchItem[] researchItems;
    public int[] requiredResourceAmounts;
    public float completionTime;
    public RewardType rewardType;
}