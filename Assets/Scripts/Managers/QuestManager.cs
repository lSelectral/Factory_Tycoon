using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : Singleton<QuestManager>
{
    [SerializeField] GameObject questFillBarPrefab;
    [SerializeField] GameObject questPrefab;

    [SerializeField] GameObject questPanel;
    [SerializeField] GameObject completedQuestPanel;

    public QuestBase[] questBases;
    public List<GameObject> questList;
    public List<QuestBase> completedQuests;
    public List<QuestHolder> instantiatedQuests;

    private void Awake()
    {
        instantiatedQuests = new List<QuestHolder>();
        questList = new List<GameObject>();
        completedQuests = new List<QuestBase>();
        // TODO FOR DEBUG. Remove it in production
        //for (int i = 0; i < questBases.Length; i++)
        //{
        //    questBases[i].OnAfterDeSerialize();
        //}

        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
        ResourceManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        GameManager.Instance.OnLevelUp += OnLevelUp;

        for (int i = 0; i < questBases.Length; i++)
        {
            var quest = questBases[i];

            var _quest = Instantiate(questPrefab, questPanel.transform);
            _quest.transform.Find("Header").GetComponent<TextMeshProUGUI>().text = quest.questName;
            _quest.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = quest.description;
            _quest.AddComponent<QuestHolder>();
            _quest.GetComponent<QuestHolder>().questBase = quest;
            _quest.GetComponent<QuestHolder>().completedIntervals = new List<int>();

            for (int j = 0; j < quest.intervals.Length; j++)
            {
                Instantiate(questFillBarPrefab, _quest.transform.Find("ProgressBar"));
            }
            instantiatedQuests.Add(_quest.GetComponent<QuestHolder>());
            questList.Add(_quest);
        }
    }

    /// <summary>
    /// Check Quest method for quickly checking required value depending to quest.
    /// </summary>
    /// <param name="questType">Quest type that should match</param>
    /// <param name="questAchivementRequirement">Quest Achievement requirement that should match</param>
    /// <param name="valueToPass">Value that should be equal or higher</param>
    /// <param name="resourceForQuest">Only required for resource quest</param>
    /// <param name="resourceAmount">Only required for resource quest. Should be higher than 0. Value doesn't matter.</param>
    void CheckQuest(QuestType questType, QuestAchivementRequirement questAchivementRequirement, BigDouble valueToPass,
                    BaseResources? resourceForQuest = null, bool isResourceQuest = false)
    {
        for (int i = 0; i < instantiatedQuests.Count; i++)
        {
            QuestHolder quest = instantiatedQuests[i];
            QuestBase questBase = instantiatedQuests[i].questBase;

            if (questBase.questType == questType && questBase.questAchiveRequirement == questAchivementRequirement)
            {
                if (questAchivementRequirement == QuestAchivementRequirement.earnResource && resourceForQuest != null)
                    continue;
                else if (questAchivementRequirement == QuestAchivementRequirement.spendResource && resourceForQuest != null)
                    continue;
                TempFunction(quest, questBase, i, valueToPass);
            }
        }
    }

    void TempFunction(QuestHolder quest, QuestBase questBase, int i, BigDouble valueToPass)
    {
        for (int j = 0; j < questBase.intervals.Length; j++)
        {
            if (!quest.completedIntervals.Contains(j) && valueToPass >= questBase.intervals[j])
            {
                OnQuestIntervalCompleted(questBase, j);
                quest.completedIntervals.Add(j);

                string rewardString = string.Format("You earned {0} {1} by completing {2} Tier {3}", 
                    questBase.rewardAmounts[j], questBase.rewardType, questBase.questName, j.ToString());
                Debug.Log(rewardString);
                PopupManager.Instance.PopupPanel(questBase.questName + " Reward", rewardString);
                questList[i].transform.Find("ProgressBar").GetChild(j).GetChild(0).GetComponent<Image>().fillAmount = 1;

                if (quest.completedIntervals.Count == questBase.intervals.Length)
                {
                    Debug.Log("Quest Completed fully");
                    quest.transform.SetParent(completedQuestPanel.transform);
                }
            }
        }
    }

    private void OnCurrencyChanged(object sender, ResourceManager.OnCurrencyChangedEventArgs e)
    {
        CheckQuest(QuestType.incremental, QuestAchivementRequirement.earnCurreny, SaveSystem.Instance.totalEarnedCurrency);
    }

    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
    {
        CheckQuest(QuestType.incremental, QuestAchivementRequirement.earnResource, e.resourceAmount, e.resource, true);
    }

    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        CheckQuest(QuestType.incremental, QuestAchivementRequirement.gainLevel, e.currentLevel);
    }

    /// <summary>
    /// Grant reward when part of the quest completed
    /// </summary>
    /// <param name="quest">Quest that completed fully or partially</param>
    /// <param name="completedQuestInterval">Part of the quest</param>
    void OnQuestIntervalCompleted(QuestBase quest, int completedQuestInterval)
    {
        switch (quest.rewardType)
        {
            case RewardType.Currency:
                ResourceManager.Instance.Currency += quest.rewardAmounts[completedQuestInterval];
                break;
            case RewardType.PremiumCurrency:
                ResourceManager.Instance.PremiumCurrency += quest.rewardAmounts[completedQuestInterval];
                break;
            case RewardType.Multiplier:
                UpgradeSystem.Instance.EarnedCoinMultiplier += quest.rewardAmounts[completedQuestInterval];
                break;
        }
    }
}

public enum QuestType
{
    single = 1,
    incremental = 2,
    repeatable = 3,
}

public enum QuestAchivementRequirement
{
    earnCurreny,
    spendCurrency,

    earnPremiumCurrency,
    spendPremiumCurrency,

    earnResource,
    spendResource,

    gainLevel,

    completeContract,
}

public enum RewardType
{
    Currency = 1,
    PremiumCurrency = 2,
    Multiplier = 3,
}