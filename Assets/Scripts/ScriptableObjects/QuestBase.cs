using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
public class QuestBase : ScriptableObject
{
    public string questName;

    public string description;

    public RewardType rewardType;

    public QuestType questType;

    public BaseResources resource;

    public QuestAchiveRequirement questAchiveRequirement;

    public int[] intervals;

    public List<int> completedIntervals;

    public long[] rewardAmounts;

    public void OnAfterDeSerialize()
    {
        completedIntervals = new List<int>();
    }
}