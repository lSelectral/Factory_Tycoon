using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Contract", menuName = "Contract")]
public class ContractBase : ScriptableObject
{
    public string contractName;
    [TextArea] public string description;
    public ContractType contractType;
    public int storyIndex;
    public int totalStoryCount;
    public float contractReward;
    public RewardType contractRewardType;
    [Tooltip("Chosen resource will rewarded if contract reward type is set to Unit Speed Up")]
    [SearchableEnum] public BaseResources resourceToRewarded;

    public ContractBase[] dependentContracts;
    public ScriptableProductionBase[] productsToUnlock;
    [SearchableEnum] public BaseResources[] requiredResources;
    public BigDouble[] requiredResourceAmounts;

    public int unlockLevel;

    [PreviewSprite] public Sprite icon;
    [TextArea] public string rewardPanelHeader = "<color=red>Congrulations</color>";
    [TextArea] public string rewardPanelDescription;
    [SearchableEnum] public AvailableMainPages mainPageToGo = AvailableMainPages.Production;
    public string pageNameToGo;
    [HideInInspector] public bool isPageNameToGoValid;
    public float xpReward;

    [SearchableEnum] public Age ageBelongsTo;
    public int history;

    private void OnValidate()
    {
        if (contractName == "")
            contractName = name;
        if (requiredResources == null || requiredResources.Length == 0)
            requiredResources = new BaseResources[] { BaseResources._0_stick };

        if (requiredResourceAmounts == null || requiredResourceAmounts[0] == 0)
            requiredResourceAmounts = new BigDouble[] { new BigDouble(10) };

        if (requiredResources != null && requiredResources.Length > requiredResourceAmounts.Length)
            Array.Resize(ref requiredResourceAmounts, requiredResources.Length);

        
    }
}


//public class InventoryManager : MonoBehaviour
//{
//    [SerializeField] Transform content; //Where all panels will be hold. Scroll rect is parent of this object. This is content of scroll view.
//    Transform foodPanel, healthPanel, drinkPanel; // These are just child of *content* add them at awake

//    [SerializeField] GameObject slotPrefab; //Slot is a UI element with frame and as child have empty image.

//    Transform activePanel; // Hold the active page value (Ex: food panel, drink panel etc.)

//    public void CreateItem(Transform panelToInstantiate, Item itemToİnstantiate)
//    {
//        GameObject slot = Instantiate(slotPrefab, panelToInstantiate);
//        slot.transform.GetChild(0).GetComponent<Image>().sprite = itemToİnstantiate.icon;
//        slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = itemToİnstantiate.itemName; // If you want a label (optioanal)
//        slot.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = itemToİnstantiate.itemCost.ToString(); // This is optional if you want a label
//        if (itemToİnstantiate as Food != null) // This is food
//        {
//            Character.energyAmount += (itemToİnstantiate as Food).energyAmount;
//        }
//        else if (itemToİnstantiate as Drink != null) // This is drink
//            Character.energy++;
//    }

//    public void ChangePage(Transform transform)
//    {
//        // This is just place holder if you have more complicated system change code according to it. Currently we have only 3 panel
//        foodPanel.gameObject.SetActive(false);
//        drinkPanel.gameObject.SetActive(false);
//        healthPanel.gameObject.SetActive(false);

//        if (transform == foodPanel)
//            foodPanel.gameObject.SetActive(true);
//        else if (transform == drinkPanel)
//            foodPanel.gameObject.SetActive(true);
//        else if (transform == healthPanel)
//            foodPanel.gameObject.SetActive(true);
//    }

//    void ScrollToItem(GameObject slot)
//    {
//        content.parent.GetComponent<ScrollView>().ScrollTo(slot.GetComponent<VisualElement>()); // I am not sure if that will work. Didn't try.
//    }

//    // Got this from stackoverflow. @maksymuik https://stackoverflow.com/a/30769550/9969193
//    public void SnapTo(RectTransform target)
//    {
//        Canvas.ForceUpdateCanvases();

//        contentPanel.anchoredPosition =
//            (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
//            - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
//    }
//}

//public class Item : ScriptableObject // Test item class. Scriptable object
//{
//    public string itemName;
//    public int itemCost;
//    public Sprite icon;
//}

//public class Food : Item // Child of item class.
//{
//    public int energyAmount;
//    public int deliciousness;
//}

//public class Drink : Item
//{
//    public bool isHot;
//}