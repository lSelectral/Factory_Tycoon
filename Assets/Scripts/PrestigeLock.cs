using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrestigeLock : MonoBehaviour
{
    public Age ageBelongsTo;
    Transform requirementPanel;

    private void Awake()
    {
        requirementPanel = transform;
        SetupRequirementPanel(ageBelongsTo);
    }

    private void SetupRequirementPanel(Age age)
    {
        requirementPanel.Find("Header").GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("REQUIREMENTS FOR {0}", age.ToString().ToUpper());
        requirementPanel.Find("Story_Progress").Find("Header").GetComponent<TextMeshProUGUI>().text = string.Format("Story progress for {0}", age.ToString());
        var completedStoryText = requirementPanel.Find("Story_Progress").Find("CompletedText").GetComponent<TextMeshProUGUI>().text = string.Format("{0}/{1} COMPLETED",0,1);
        requirementPanel.Find("Story_Progress").Find("FillBar").GetChild(0).GetComponent<Image>();
        requirementPanel.Find("Required_Artifacts").Find("Slot").GetComponent<Slot>();
        requirementPanel.Find("Required_Artifacts").Find("SacrificeBtn").GetComponent<Button>();
        requirementPanel.Find("Required_Artifacts").Find("RequirementText").GetComponent<TextMeshProUGUI>().text = string.Format("");
        requirementPanel.Find("Required_Artifacts").Find("SacrificeCountText").GetComponent<TextMeshProUGUI>().text = string.Format("");
    }
}

/*
 * 
 * I have multiple panels each contain different age.
 * Each unlocked panel will contain requirement panel that will ask for things before able to interact with page content.
 * 
 * Should save story progress
 * Current story
 * Placed and current artifact and its count
 * 
 * 
 */