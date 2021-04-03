using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;
using UnityEngine.UI;

/*
 * For passing the ages, there is some basic requireents that all ages have in common.
 * All previous resources should be unlocked.
 * Level requirement should be meet.
 * Story contracts should be completed for current age
 * 
 * 
 */

public class PrestigeSystem : Singleton<PrestigeSystem>
{
    [SerializeField] GameObject prestigeBlockPrefab;
    [SerializeField] Transform requirementPanelForAgePanels;
    [SerializeField] Transform restartPanel;

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
        CheckStoryCompletionUI(e.contract.storyIndex, e.contract.totalStoryCount);
    }

    void CheckStoryCompletionUI(int storyIndex, int totalStoryCount)
    {
        var requirement = restartPanel.Find("Requirements");
        requirement.Find("LeftFillBar").GetComponent<Image>().fillAmount = storyIndex / (totalStoryCount * 1f);
        requirement.Find("LeftText").GetComponent<TextMeshProUGUI>().text = string.Format("{0}/{1} Story Completed", storyIndex, totalStoryCount);
        if (storyIndex == totalStoryCount)
            requirement.Find("CheckBox_Left").GetChild(0).gameObject.SetActive(true);
        else
            requirement.Find("CheckBox_Left").GetChild(0).gameObject.SetActive(false);
    }

    public void CheckProductionUnitUnlockingUI(ProductionBase unlockedUnit = null) // When one of the production unit unlocked, it will setup Prestige Panel UI elements
    {
        var requirement = restartPanel.Find("Requirements");
        if (unlockedUnit != null)
        {
            if (unlockedUnit.scriptableProductionBase.ageBelongsTo != currentAge) return;
            var currentAgeUnits = ProductionManager.Instance.instantiatedProductionUnits.Where(u => u.scriptableProductionBase.ageBelongsTo == currentAge).ToArray();
            int totalCount = currentAgeUnits.Length;
            int unlockedCount = currentAgeUnits.Where(u => u.IsUnlocked).Count();

            requirement.Find("RightFillBar").GetComponent<Image>().fillAmount = unlockedCount / (totalCount * 1f);
            requirement.Find("RightText").GetComponent<TextMeshProUGUI>().text = string.Format("{0}/{1} Unit Unlocked", unlockedCount, totalCount);
            if (unlockedCount == totalCount)
                requirement.Find("CheckBox_Right").GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            requirement.Find("RightFillBar").GetComponent<Image>().fillAmount = 0;
            requirement.Find("RightText").GetComponent<TextMeshProUGUI>().text = 
                string.Format("{0}/{1} Unit Unlocked", 0, 
                ProductionManager.Instance.scriptableProductionUnitList.Where(s => s.ageBelongsTo == currentAge).Count());
            requirement.Find("CheckBox_Right").GetChild(0).gameObject.SetActive(false);
        }
    }

    private void SetupRequirementPanel(Age age, int curerntStoryIndex, int totalStoryCount, ArtifactTier rarity)
    {
        requirementPanelForAgePanels.Find("Header").GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("REQUIREMENTS FOR {0}", age.ToString().ToUpper());
        requirementPanelForAgePanels.Find("Story_Progress").Find("Header").GetComponent<TextMeshProUGUI>().text = string.Format("Story progress for {0}", age.ToString());
        var completedStoryText = requirementPanelForAgePanels.Find("Story_Progress").Find("CompletedText").GetComponent<TextMeshProUGUI>().text = 
            string.Format("{0}/{1} COMPLETED", curerntStoryIndex, totalStoryCount);
        requirementPanelForAgePanels.Find("Story_Progress").Find("FillBar").GetChild(0).GetComponent<Image>().fillAmount = curerntStoryIndex * 1f / totalStoryCount * 1f;
        requirementPanelForAgePanels.Find("Required_Artifacts").Find("Slot").GetComponent<Slot>();
        requirementPanelForAgePanels.Find("Required_Artifacts").Find("SacrificeBtn").GetComponent<Button>();
        requirementPanelForAgePanels.Find("Required_Artifacts").Find("RequirementText").GetComponent<TextMeshProUGUI>().text = // Minimum rarity 
            string.Format("{0} sacrifice - Sacrifice item should be {1} or higher",age, SlotManager.Instance.GetRarityText(rarity.ToString(),rarity));
        requirementPanelForAgePanels.Find("Required_Artifacts").Find("SacrificeCountText")
            .GetComponent<TextMeshProUGUI>().text = string.Format("{0}/{1}",currentSacrificeCount,requiredSacrificeCount);
    }

    public int GetRequiredArtifactCount(Age age)
    {
        return (int)age;
    }

    public bool CheckProgressionAvailability()
    {
        if (ContractManager.Instance.isCurrentAgeStoryCompleted && CheckUnlockedProductionUnits()) 
        {
            return true;
        }
        return false;
    }

    bool CheckUnlockedProductionUnits()
    {
        // Get all production units for current age and check if all of them is unlocked
        return ProductionManager.Instance.instantiatedProductionUnits.Where(u => u.scriptableProductionBase.ageBelongsTo == currentAge).All(u => u.IsUnlocked);
    }

    public void ReturnToMonke() // Set everythin to 0 (almost), TODO If player have special artifact that doesn't reset resource, don't reset at all.
    {
        if (!CheckProgressionAvailability()) return;

        CurrentAge = (Age) (((int)currentAge) + 1);
        Debug.Log(currentAge);
        ContractManager.Instance.isCurrentAgeStoryCompleted = false;
        CheckStoryCompletionUI(0, ContractManager.Instance.contracts.Where(c => c.ageBelongsTo == currentAge && c.contractType == ContractType.story).Count());

        var ageUnits = ProductionManager.Instance.instantiatedProductionUnits.Where(u => u.scriptableProductionBase.ageBelongsTo == currentAge);
        foreach (ProductionBase unit in ageUnits)
        {
            unit.IsLockedByAge = false;
        }
        CheckProductionUnitUnlockingUI();

        for (int i = 0; i < GetRequiredArtifactCount(currentAge); i++)
        {
            SlotManager.Instance.AddRandomArtifactToInventory(currentAge);
        }

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
        //for (int i = 0; i < ProductionManager.Instance.mainPanel.transform.childCount; i++)
        //{
        //    var c = ProductionManager.Instance.mainPanel.transform.GetChild(i);
        //    if (c.name != "_0_StoneAge")
        //    {
        //        for (int j = 0; j < c.GetChild(0).childCount; j++)
        //        {
        //            Destroy(c.GetChild(0).GetChild(j).gameObject);
        //        }
        //    }
        //}
        #endregion

        SaveSystem.Instance.Save();
    }
}