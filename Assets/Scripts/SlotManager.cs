using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class SlotManager : Singleton<SlotManager>
{
    public ScriptableArtifact[] scriptableArtifacts; // Holder for artifact obj
    public List<Slot> instantiatedSlots; // Holder for instantiated slots

    // Panels
    public GameObject artifactSelectionPanel;
    public Transform itemPanel;

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

    public Slot selectedCharacterSlot; // Currently selected slot

    private void Awake()
    {
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
            var slot = InstantiateSlot(scriptableArtifacts[i]);
            slot.gameObject.SetActive(false);
            instantiatedSlots.Add(slot);
        }
    }

    public Slot InstantiateSlot(ScriptableArtifact _artifact)
    {
        var obj = Instantiate(slotPrefab, itemPanel);
        var slot = obj.GetComponent<Slot>();
        slot.artifactPart = _artifact.bodyPart;
        slot.artifact = _artifact;
        slot.transform.GetChild(0).GetComponent<Image>().sprite = _artifact.icon;
        return slot;
    }

    public void ShowItemPanel(ArtifactPart artifactPart)
    {
        artifactSelectionPanel.SetActive(true);

        for (int i = 0; i < instantiatedSlots.Count; i++)
        {
            var slot = instantiatedSlots[i];
            if (slot.artifact.bodyPart == artifactPart)
                slot.gameObject.SetActive(true);
            else if (artifactPart == ArtifactPart.All)
                slot.gameObject.SetActive(true);
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

    string GetRarityText(string text, ArtifactTier rarity)
    {
        switch (rarity)
        {
            case ArtifactTier.common:
                return text;
            case ArtifactTier.rare:
                return string.Format("<color=blue>{0}</color>", text);
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
    }
}