using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : Singleton<QuestManager>
{
    [SerializeField] GameObject questFillBarPrefab;
    public QuestBase[] questBases;
    [SerializeField] GameObject questPrefab;
    [SerializeField] GameObject questPanel;
    [SerializeField] GameObject completedQuestPanel;

    public List<GameObject> questList;
    public List<QuestBase> completedQuests;
    public List<QuestInfo> currentQuestInfos;

    Object[] assets;

    private void Awake()
    {
        currentQuestInfos = new List<QuestInfo>();
        questList = new List<GameObject>();
        completedQuests = new List<QuestBase>();
        // TODO FOR DEBUG. Remove it in production
        for (int i = 0; i < questBases.Length; i++)
        {
            questBases[i].OnAfterDeSerialize();
        }

        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
        ResourceManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        GameManager.Instance.OnLevelUp += OnLevelUp;

        assets = Resources.LoadAll("Quests");

        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];

            if (asset as QuestBase != null)
            {
                var quest = asset as QuestBase;
                var _quest = Instantiate(questPrefab, questPanel.transform);
                _quest.transform.Find("Header").GetComponent<TextMeshProUGUI>().text = quest.questName;
                _quest.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = quest.description;

                for (int j = 0; j < quest.intervals.Length; j++)
                {
                    Instantiate(questFillBarPrefab, _quest.transform.Find("ProgressBar"));
                }
                questList.Add(_quest);
            }
        }
    }

    private void OnCurrencyChanged(object sender, ResourceManager.OnCurrencyChangedEventArgs e)
    {
        for (int i = 0; i < questBases.Length; i++)
        {
            if (questBases[i].questType == QuestType.incremental && questBases[i].questAchiveRequirement == QuestAchivementRequirement.currency)
            {
                for (int j = 0; j < questBases[i].intervals.Length; j++)
                {
                    if (!questBases[i].completedIntervals.Contains(j) && SaveSystem.Instance.totalEarnedCurrency >= questBases[i].intervals[j])
                    {
                        OnQuestCompleted(questBases[i],j);
                        questBases[i].completedIntervals.Add(j);
                        Debug.Log("You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + 
                            " by completing " + questBases[i].questName + " Tier" + j.ToString());
                        PopupManager.Instance.PopupPanel(questBases[i].questName + " Reward", "You earned " + questBases[i].rewardAmounts[j] 
                            + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());

                        questList[i].transform.Find("ProgressBar").GetChild(j).GetChild(0).GetComponent<Image>().fillAmount = 1;

                        if (questBases[i].completedIntervals.Count == questBases[i].intervals.Length)
                        {
                            Debug.Log("Quest Completed fully");
                            questList[i].transform.SetParent(completedQuestPanel.transform);
                        }
                    }
                }
            }
            else if (questBases[i].questType == QuestType.incremental && questBases[i].questAchiveRequirement == QuestAchivementRequirement.premiumCurrency)
            {
                for (int j = 0; j < questBases[i].intervals.Length; j++)
                {
                    if (!questBases[i].completedIntervals.Contains(j) && SaveSystem.Instance.totalEarnedPremiumCurrency >= questBases[i].intervals[j])
                    {
                        completedQuests.Add(questBases[i]);
                        OnQuestCompleted(questBases[i],j);
                        questBases[i].completedIntervals.Add(j);
                        Debug.Log("You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " 
                            + questBases[i].questName + " Tier" + j.ToString());
                        PopupManager.Instance.PopupPanel(questBases[i].questName + " Reward", "You earned " + questBases[i].rewardAmounts[j] 
                            + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());

                        questList[i].transform.Find("ProgressBar").GetChild(j).GetChild(0).GetComponent<Image>().fillAmount = 1;

                        if (questBases[i].completedIntervals.Count == questBases[i].intervals.Length)
                        {
                            Debug.Log("Quest Completed fully");
                            questList[i].transform.SetParent(completedQuestPanel.transform);
                        }
                    }
                }
            }
        }
    }

    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
    {
        for (int i = 0; i < questBases.Length; i++)
        {
            // For object produce and collect milestones
            if (questBases[i].questType == QuestType.incremental && questBases[i].questAchiveRequirement == QuestAchivementRequirement.resource 
                && questBases[i].resource == e.resource)
            {
                for (int j = 0; j < questBases[i].intervals.Length; j++)
                {
                    if (!questBases[i].completedIntervals.Contains(j) && e.resourceAmount >= questBases[i].intervals[j])
                    {
                        OnQuestCompleted(questBases[i],j);
                        questBases[i].completedIntervals.Add(j);
                        Debug.Log("You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " 
                            + questBases[i].questName + " Tier" + j.ToString());
                        PopupManager.Instance.PopupPanel(questBases[i].questName + " Reward", "You earned " + questBases[i].rewardAmounts[j] 
                            + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());

                        questList[i].transform.Find("ProgressBar").GetChild(j).GetChild(0).GetComponent<Image>().fillAmount = 1;

                        if (questBases[i].completedIntervals.Count == questBases[i].intervals.Length)
                        {
                            Debug.Log("Quest Completed fully");
                            questList[i].transform.SetParent(completedQuestPanel.transform);
                            questList[i].AddComponent<LayoutElement>();
                        }
                    }
                }
            }
        }
    }

    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        for (int i = 0; i < questBases.Length; i++)
        {
            // For object produce and collect milestones
            if (questBases[i].questType == QuestType.incremental && questBases[i].questAchiveRequirement == QuestAchivementRequirement.level)
            {
                for (int j = 0; j < questBases[i].intervals.Length; j++)
                {
                    if (!questBases[i].completedIntervals.Contains(j) && e.currentLevel == questBases[i].intervals[j])
                    {
                        OnQuestCompleted(questBases[i],j);
                        questBases[i].completedIntervals.Add(j);
                        Debug.Log("You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " 
                            + questBases[i].questName + " Tier" + j.ToString());
                        PopupManager.Instance.PopupPanel(questBases[i].questName + " Reward", "You earned " + questBases[i].rewardAmounts[j] 
                            + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());
                    }
                }
            }
        }
    }

    /// <summary>
    /// Grant reward when part of the quest completed
    /// </summary>
    /// <param name="quest">Quest that completed fully or partially</param>
    /// <param name="completedQuestInterval">Part of the quest</param>
    void OnQuestCompleted(QuestBase quest, int completedQuestInterval)
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
/* TODO implement this class for holding quest infos.
 Shouldn't change scriptable object value it will crash the build */
public class QuestInfo
{
    public QuestBase questBase;
    public int[] completedIntervals;
}

public enum QuestType
{
    single = 1,
    incremental = 2,
    repeatable = 3,

}

public enum QuestAchivementRequirement
{
    resource = 4,
    currency = 5,
    premiumCurrency,
    level,
    totalResource,
    contract,
}

public enum RewardType
{
    Currency = 1,
    PremiumCurrency = 2,
    Multiplier = 3,
}