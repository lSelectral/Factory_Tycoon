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

    Object[] assets;

    private void Awake()
    {
        if (questList == null)
            questList = new List<GameObject>();
        if (completedQuests == null)
            completedQuests = new List<QuestBase>();
        // FOR DEBUG
        //for (int i = 0; i < questBases.Length; i++)
        //{
        //    questBases[i].OnAfterDeSerialize();
        //}

        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
        ResourceManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        GameManager.Instance.OnLevelUp += OnLevelUp;

        assets = Resources.LoadAll("Quests");

        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];

            if (asset as ScriptableObject != null)
            {
                var sc = asset as ScriptableObject;
                if (sc.GetType() == typeof(QuestBase))
                {
                    var quest = sc as QuestBase;
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
    }

    private void Start()
    {
        
    }

    private void OnCurrencyChanged(object sender, ResourceManager.OnCurrencyChangedEventArgs e)
    {
        for (int i = 0; i < questBases.Length; i++)
        {
            if (questBases[i].questType == QuestType.incremental && questBases[i].questAchiveRequirement == QuestAchiveRequirement.currency)
            {
                for (int j = 0; j < questBases[i].intervals.Length; j++)
                {
                    if (!questBases[i].completedIntervals.Contains(j) && SaveSystem.Instance.totalEarnedCurrency >= questBases[i].intervals[j])
                    {
                        OnQuestCompleted(questBases[i],j);
                        questBases[i].completedIntervals.Add(j);
                        Debug.Log("You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());
                        PopupManager.Instance.PopupPanel(questBases[i].questName + " Reward", "You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());

                        questList[i].transform.Find("ProgressBar").GetChild(j).GetChild(0).GetComponent<Image>().fillAmount = 1;

                        if (questBases[i].completedIntervals.Count == questBases[i].intervals.Length)
                        {
                            Debug.Log("Quest Completed fully");
                            questList[i].transform.SetParent(completedQuestPanel.transform);
                        }
                    }
                }
            }
            else if (questBases[i].questType == QuestType.incremental && questBases[i].questAchiveRequirement == QuestAchiveRequirement.premiumCurrency)
            {
                for (int j = 0; j < questBases[i].intervals.Length; j++)
                {
                    if (!questBases[i].completedIntervals.Contains(j) && SaveSystem.Instance.totalEarnedPremiumCurrency >= questBases[i].intervals[j])
                    {
                        completedQuests.Add(questBases[i]);
                        OnQuestCompleted(questBases[i],j);
                        questBases[i].completedIntervals.Add(j);
                        Debug.Log("You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());
                        PopupManager.Instance.PopupPanel(questBases[i].questName + " Reward", "You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());

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
            if (questBases[i].questType == QuestType.incremental && questBases[i].questAchiveRequirement == QuestAchiveRequirement.resource && questBases[i].resource == e.resource)
            {
                for (int j = 0; j < questBases[i].intervals.Length; j++)
                {
                    if (!questBases[i].completedIntervals.Contains(j) && e.resourceAmount >= questBases[i].intervals[j])
                    {
                        OnQuestCompleted(questBases[i],j);
                        questBases[i].completedIntervals.Add(j);
                        Debug.Log("You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());
                        PopupManager.Instance.PopupPanel(questBases[i].questName + " Reward", "You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());

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
            if (questBases[i].questType == QuestType.incremental && questBases[i].questAchiveRequirement == QuestAchiveRequirement.level)
            {
                for (int j = 0; j < questBases[i].intervals.Length; j++)
                {
                    if (!questBases[i].completedIntervals.Contains(j) && e.currentLevel == questBases[i].intervals[j])
                    {
                        OnQuestCompleted(questBases[i],j);
                        questBases[i].completedIntervals.Add(j);
                        Debug.Log("You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());
                        PopupManager.Instance.PopupPanel(questBases[i].questName + " Reward", "You earned " + questBases[i].rewardAmounts[j] + " " + questBases[i].rewardType + " by completing " + questBases[i].questName + " Tier" + j.ToString());
                    }
                }
            }
        }
    }

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

public enum QuestType
{
    single = 1,
    incremental = 2,
    repeatable = 3,

}

public enum QuestAchiveRequirement
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