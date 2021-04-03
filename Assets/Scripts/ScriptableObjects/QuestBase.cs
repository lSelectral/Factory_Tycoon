using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
public class QuestBase : ScriptableObject
{
    public string questName;

    public string description;

    public QuestRewardType rewardType;

    public QuestType questType;

    [SearchableEnum] public BaseResources resource;

    public QuestAchivementRequirement questAchiveRequirement;

    public BigDouble[] intervals = { 100, 1000, 10000, 100000, 1000000 };

    public List<int> completedIntervals;

    public long[] rewardAmounts = { 25, 100, 200, 300, 500 };

    public void OnAfterDeSerialize()
    {
        completedIntervals = new List<int>();
    }
}