using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;
using UnityEngine.UI;

public class PrestigeSystem : Singleton<PrestigeSystem>
{
    [SerializeField] GameObject prestigeBlockPrefab;
    [SerializeField] Transform requirementPanel;

    #region Value Variable and Properties
    private int prestigeLevel;
    private Age currentAge = Age._0_StoneAge;

    int currentSacrificeCount;
    int requiredSacrificeCount;

    public int PrestigeLevel
    {
        get { return prestigeLevel; }
        set { prestigeLevel = value; }
    }

    public Age CurrentAge { get => currentAge; set => currentAge = value; }
    #endregion

    private void Awake()
    {
        ContractManager.Instance.OnStoryCompletedEvent += OnStoryCompletedEvent;
    }

    private void OnStoryCompletedEvent(object sender, ContractManager.OnStoryCompletedEventArgs e)
    {
        Debug.Log(string.Format("{0} story completed. {1}/{2}",e.contract.contractName,e.contract.storyIndex,e.contract.totalStoryCount));
        SetupRequirementPanel(Age._1_BronzeAge, e.contract.storyIndex, e.contract.totalStoryCount, ArtifactTier.rare);
    }

    private void SetupRequirementPanel(Age age, int curerntStoryIndex, int totalStoryCount, ArtifactTier rarity)
    {
        requirementPanel.Find("Header").GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("REQUIREMENTS FOR {0}", age.ToString().ToUpper());
        requirementPanel.Find("Story_Progress").Find("Header").GetComponent<TextMeshProUGUI>().text = string.Format("Story progress for {0}", age.ToString());
        var completedStoryText = requirementPanel.Find("Story_Progress").Find("CompletedText").GetComponent<TextMeshProUGUI>().text = 
            string.Format("{0}/{1} COMPLETED", curerntStoryIndex, totalStoryCount);
        requirementPanel.Find("Story_Progress").Find("FillBar").GetChild(0).GetComponent<Image>().fillAmount = curerntStoryIndex * 1f / totalStoryCount * 1f;
        requirementPanel.Find("Required_Artifacts").Find("Slot").GetComponent<Slot>();
        requirementPanel.Find("Required_Artifacts").Find("SacrificeBtn").GetComponent<Button>();
        requirementPanel.Find("Required_Artifacts").Find("RequirementText").GetComponent<TextMeshProUGUI>().text = // Minimum rarity 
            string.Format("{0} sacrifice - Sacrifice item should be {1} or higher",age, SlotManager.Instance.GetRarityText(rarity.ToString(),rarity));
        requirementPanel.Find("Required_Artifacts").Find("SacrificeCountText").GetComponent<TextMeshProUGUI>().text = string.Format("{0}/{1}",currentSacrificeCount,requiredSacrificeCount);
    }

    public int GetRequiredArtifactCount(Age age)
    {
        return (int)age;
    }

    public void ReturnToMonke() // Set everythin to 0 (almost), TODO If player have special artifact that doesn't reset resource, don't reset.
    {
        SlotManager.Instance.AddRandomArtifactToInventory(PrestigeSystem.Instance.CurrentAge);

        #region Reset resource and production units.
        ResourceManager.Instance.Currency = 0;
        ResourceManager.Instance.AttackAmount = 0;
        ResourceManager.Instance.FoodAmount = 0;

        var resources = Enum.GetValues(typeof(BaseResources)).Cast<BaseResources>();
        ResourceManager.Instance.resourceValueDict = new Dictionary<BaseResources, BigDouble>();
        foreach (var res in resources)
        {
            ResourceManager.Instance.resourceValueDict.Add(res, 0);
        }

        // Player will return to stone age and will lose everything. 
        for (int i = 0; i < ProductionManager.Instance.mainPanel.transform.childCount; i++)
        {
            var c = ProductionManager.Instance.mainPanel.transform.GetChild(i);
            if (c.name != "_0_StoneAge")
            {
                for (int j = 0; j < c.GetChild(0).childCount; j++)
                {
                    Destroy(c.GetChild(0).GetChild(j).gameObject);
                }
            }
        }
        #endregion
    }
}