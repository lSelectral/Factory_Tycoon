using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;
using System.Linq;

/// <summary>
/// Slot manager is manager for both artifact and slots
/// </summary>
public class SlotManager : Singleton<SlotManager>
{
    public ScriptableArtifact[] scriptableArtifacts; // Holder for artifact obj
    public List<Slot> instantiatedSlots; // Holder for instantiated slots

    // Panels
    public GameObject artifactSelectionPanel;
    [SerializeField] Transform itemPanel;
    [SerializeField] Transform characterStatPanel;

    // Everything related to character stat panel
    TextMeshProUGUI combatText;
    TextMeshProUGUI defenseText;
    TextMeshProUGUI goldBonusText;
    TextMeshProUGUI foodBonusText;
    TextMeshProUGUI populationBonusText;
    TextMeshProUGUI allXPBonusText;

    // Everything related to equip panel
    public Transform artifactEquipPanel;
    TextMeshProUGUI headerText;
    Image icon;
    TextMeshProUGUI ageText;
    TextMeshProUGUI rarityText;
    TextMeshProUGUI setText;
    TextMeshProUGUI powerText;
    Button equipBtn;

    public GameObject slotPrefab; // Prefab for slots

    public Slot lastSelectedSlot; // Currently selected slot

    void SetCharacterStatTexts()
    {
        combatText.text = UpgradeSystem.Instance.CombatPowerMultiplier.ToString();
        defenseText.text = UpgradeSystem.Instance.DefensePowerMultiplier.ToString();
        goldBonusText.text = UpgradeSystem.Instance.EarnedCoinMultiplier.ToString();
        foodBonusText.text = "NONE";
        populationBonusText.text = "NONE";
        allXPBonusText.text = UpgradeSystem.Instance.EarnedXPMultiplier.ToString();
    }

    private void Awake()
    {
        // Set text transforms for character stat panel
        combatText = characterStatPanel.transform.Find("CombatText").GetComponent<TextMeshProUGUI>();
        defenseText = characterStatPanel.transform.Find("DefenseText").GetComponent<TextMeshProUGUI>();
        goldBonusText = characterStatPanel.transform.Find("GoldText").GetComponent<TextMeshProUGUI>();
        foodBonusText = characterStatPanel.transform.Find("FoodText").GetComponent<TextMeshProUGUI>();
        populationBonusText = characterStatPanel.transform.Find("PopulationText").GetComponent<TextMeshProUGUI>();
        allXPBonusText = characterStatPanel.transform.Find("XPText").GetComponent<TextMeshProUGUI>();

        // Set transforms for equip panel
        headerText = artifactEquipPanel.Find("Header").GetComponent<TextMeshProUGUI>();
        icon = artifactEquipPanel.Find("Icon").GetChild(0).GetComponent<Image>();
        var statPanel = artifactEquipPanel.Find("StatPanel");
        ageText = statPanel.GetChild(2).GetComponent<TextMeshProUGUI>();
        rarityText = statPanel.GetChild(3).GetComponent<TextMeshProUGUI>();
        setText = statPanel.GetChild(4).GetComponent<TextMeshProUGUI>();
        powerText = statPanel.GetChild(5).GetComponent<TextMeshProUGUI>();
        equipBtn = artifactEquipPanel.Find("EquipBtn").GetComponent<Button>();
    }

    private void Start()
    {
        for (int i = 0; i < scriptableArtifacts.Length; i++)
        {
            InstantiateSlot(scriptableArtifacts[i]);
        }
        SetCharacterStatTexts();
    }

    public Slot InstantiateSlot(ScriptableArtifact _artifact)
    {
        var obj = Instantiate(slotPrefab, itemPanel);
        var slot = obj.GetComponent<Slot>();
        slot.isInventorySlot = true;
        slot.artifactPart = _artifact.bodyPart;
        slot.artifact = _artifact;
        slot.transform.GetChild(0).GetComponent<Image>().sprite = _artifact.icon;
        obj.SetActive(false);
        instantiatedSlots.Add(slot);
        return slot;
    }

    public void AddRandomArtifactToInventory(Age age)
    {
        var _scriptableArtifacts = scriptableArtifacts.Where(a => a.ageBelongsTo == age).ToArray();
        var slot = InstantiateSlot(_scriptableArtifacts[UnityEngine.Random.Range(0,_scriptableArtifacts.Length)]);
        ShowItemPanel(artifactPart);
    }

    ArtifactPart artifactPart;
    public void ShowItemPanel(ArtifactPart artifactPart, Age? age = null)
    {
        artifactSelectionPanel.SetActive(true);
        this.artifactPart = artifactPart;
        for (int i = 0; i < instantiatedSlots.Count; i++)
        {
            var slot = instantiatedSlots[i];
            if (age != null && slot.artifact.ageBelongsTo != age) return;
            if (slot.artifact.bodyPart == artifactPart)
                slot.gameObject.SetActive(true);
            else if (artifactPart == ArtifactPart.All)
                slot.gameObject.SetActive(true);
            else
                slot.gameObject.SetActive(false);
        }
    }

    public void OnItemPanelClose() // Assigned to close button for item panel
    {
        for (int i = 0; i < itemPanel.childCount; i++)
            itemPanel.GetChild(i).gameObject.SetActive(false);
    }

    public void SetEquipPanel(ScriptableArtifact artifact, UnityAction equipAction = null, string equipBtnText = "Equip")
    {
        artifactEquipPanel.gameObject.SetActive(true);
        headerText.text = GetRarityText(artifact.artifactName, artifact.rarity);
        icon.sprite = artifact.icon;
        ageText.text = artifact.ageBelongsTo.ToString();
        rarityText.text = string.Format("RARITY\n{0}",GetRarityText(artifact.rarity.ToString(),artifact.rarity));
        setText.text = artifact.setBelongsTo.ToString();
        powerText.text = string.Format("<color=green>+%{0}</color>\n{1}",artifact.powerAmount,artifact.artifactPower.ToString());
        equipBtn.onClick.RemoveAllListeners();
        equipBtn.onClick.AddListener(equipAction);
        equipBtn.GetComponentInChildren<TextMeshProUGUI>().text = equipBtnText;
    }

    public string GetRarityText(string text, ArtifactTier rarity)
    {
        switch (rarity)
        {
            case ArtifactTier.common:
                return text;
            case ArtifactTier.rare:
                return string.Format("<color=#00bbff>{0}</color>", text);
            case ArtifactTier.epic:
                return string.Format("<color=purple>{0}</color>", text);
            case ArtifactTier.legendary:
                return string.Format("<color=orange>{0}</color>", text);
            case ArtifactTier.onlyYouHaveIt:
                return string.Format("<color=red>{0}</color>", text);
            default:
                return text;
        }
    }

    public void OnArtifactEquipped(ScriptableArtifact artifact)
    {
        float power = artifact.powerAmount;
        switch (artifact.artifactPower)
        {
            case ArtifacPower.attackPower:
                UpgradeSystem.Instance.CombatPowerMultiplier += power;
                break;
            case ArtifacPower.defensePower:
                UpgradeSystem.Instance.DefensePowerMultiplier += power;
                break;
            case ArtifacPower.gatheringSpeed:
                UpgradeSystem.Instance.MiningSpeedMultiplier += power;
                break;
            case ArtifacPower.warItemProductionSpeed:
                break;
            case ArtifacPower.foodProductionSpeed:
                break;
            case ArtifacPower.foodProductionYield:
                break;
            case ArtifacPower.tradingGoodProductionSpeed:
                break;
            case ArtifacPower.contractRewardMultiplier:
                UpgradeSystem.Instance.contractRewardMultiplier += power;
                break;
            case ArtifacPower.contractXPMultiplier:
                UpgradeSystem.Instance.contractXPMultiplier += power;
                break;
            case ArtifacPower.questRewardMultiplier:
                UpgradeSystem.Instance.questRewardMultiplier += power;
                break;
            case ArtifacPower.questXPMultiplier:
                UpgradeSystem.Instance.questXPMultiplier += power;
                break;
            case ArtifacPower.allXPMultiplier:
                UpgradeSystem.Instance.EarnedXPMultiplier += power;
                break;
            case ArtifacPower.allCurrencyMultiplier:
                UpgradeSystem.Instance.EarnedCoinMultiplier += power;
                break;
            case ArtifacPower.higherTierArtifactEarnChance:
                break;
            case ArtifacPower.unTouchable:
                break;
        }
        SetCharacterStatTexts();
    }

    public void OnArtifactUnEquipped(ScriptableArtifact artifact)
    {
        float power = artifact.powerAmount;
        switch (artifact.artifactPower)
        {
            case ArtifacPower.attackPower:
                UpgradeSystem.Instance.CombatPowerMultiplier -= power;
                break;
            case ArtifacPower.defensePower:
                UpgradeSystem.Instance.DefensePowerMultiplier -= power;
                break;
            case ArtifacPower.gatheringSpeed:
                UpgradeSystem.Instance.MiningSpeedMultiplier -= power;
                break;
            case ArtifacPower.warItemProductionSpeed:
                break;
            case ArtifacPower.foodProductionSpeed:
                break;
            case ArtifacPower.foodProductionYield:
                break;
            case ArtifacPower.tradingGoodProductionSpeed:
                break;
            case ArtifacPower.contractRewardMultiplier:
                UpgradeSystem.Instance.contractRewardMultiplier -= power;
                break;
            case ArtifacPower.contractXPMultiplier:
                UpgradeSystem.Instance.contractXPMultiplier -= power;
                break;
            case ArtifacPower.questRewardMultiplier:
                UpgradeSystem.Instance.questRewardMultiplier -= power;
                break;
            case ArtifacPower.questXPMultiplier:
                UpgradeSystem.Instance.questXPMultiplier -= power;
                break;
            case ArtifacPower.allXPMultiplier:
                UpgradeSystem.Instance.EarnedXPMultiplier -= power;
                break;
            case ArtifacPower.allCurrencyMultiplier:
                UpgradeSystem.Instance.EarnedCoinMultiplier -= power;
                break;
            case ArtifacPower.higherTierArtifactEarnChance:
                break;
            case ArtifacPower.unTouchable:
                break;
        }
        SetCharacterStatTexts();
    }
}