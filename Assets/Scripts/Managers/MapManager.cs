using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : Singleton<MapManager>
{
    public List<Map_Part> allMaps;

    public Transform mapTransform;

    public Map_Part clickedMapPart;

    public Transform countryResourceContentTransform;

    [SerializeField] GameObject resourceLayoutPrefab;
 
    [SerializeField] Transform mapStatPanel;

    // This value set in editor
    // Start place of player when the game starts
    [SerializeField] Map_Part playerStartMapPart;

    // Variable that tracks down playerMap
    public Map_Part[] playerCurrentMapPart;

    TextMeshProUGUI countryNameLevelText;
    TextMeshProUGUI moneyText;
    TextMeshProUGUI attackPowerText;
    TextMeshProUGUI defensePowerText;
    TextMeshProUGUI foodText;
    TextMeshProUGUI countryLivesText;

    private void Awake()
    {
        allMaps = new List<Map_Part>();
        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;

        playerCurrentMapPart = new Map_Part[] { playerStartMapPart };

        var resourceBtn = mapStatPanel.Find("ResourcePanelBtn").GetComponent<Button>();
        resourceBtn.onClick.AddListener(() => { if (clickedMapPart != null) countryResourceContentTransform.parent.parent.gameObject.SetActive(true); });

        // TODO Remove or fix this part, this is just for debug
        clickedMapPart = playerStartMapPart;

        countryNameLevelText = mapStatPanel.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        var infoPanel = mapStatPanel.Find("InfoPanel");
        moneyText = infoPanel.Find("Top").Find("Money").GetChild(1).GetComponent<TextMeshProUGUI>();
        attackPowerText = infoPanel.Find("Top").Find("AttackPower").GetChild(1).GetComponent<TextMeshProUGUI>();
        defensePowerText = infoPanel.Find("Top").Find("DefensePower").GetChild(1).GetComponent<TextMeshProUGUI>();
        foodText = infoPanel.Find("Bottom").Find("Food").GetChild(1).GetComponent<TextMeshProUGUI>();
        countryLivesText = infoPanel.Find("Bottom").Find("CountryLives").GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        var panelSize = Mathf.CeilToInt(ResourceManager.Instance.resourceValueDict.Count / 2);
        // Instantiate map resource panel
        for (int i = 0; i < panelSize; i++)
        {
            var res1 = Instantiate(resourceLayoutPrefab, countryResourceContentTransform);

            if (ResourceManager.Instance.resourceValueDict.Count % 2 != 0 && i == ResourceManager.Instance.resourceValueDict.Count - 1)
            {
                Destroy(res1.transform.Find("Right").Find("Image").gameObject);
                Destroy(res1.transform.Find("Right").Find("Text (TMP)").gameObject);
            }
        }
    }

    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
    {
        SetupResourcePanel(clickedMapPart);
    }

    public void OnMapClick(Map_Part newClickedMap, float scaleFactor = 1.23f)
    {
        #region Highlighting the selected map

        if (clickedMapPart == null)
        {
            clickedMapPart = newClickedMap;

            newClickedMap.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            newClickedMap.transform.SetAsLastSibling();
            var outline = newClickedMap.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.red;
            outline.effectDistance = Vector2.one * 3f;
        }
        else if (clickedMapPart != newClickedMap)
        {
            clickedMapPart.transform.localScale = Vector3.one;

            // Only delete outlines created by this script
            // Some outlines are necessary for map visualization
            var outlines = clickedMapPart.GetComponents<Outline>();
            foreach (var o in outlines)
            {
                if (o.effectColor == Color.red)
                    Destroy(o);
            }
            newClickedMap.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            newClickedMap.transform.SetAsLastSibling();
            var outline = newClickedMap.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.red;
            outline.effectDistance = Vector2.one * 3f;
            clickedMapPart = newClickedMap;
        }
        #endregion

        SetupResourcePanel(newClickedMap);

        SetupInfoPanel(newClickedMap);

        var attackBtn = mapStatPanel.Find("AttackBtn").GetComponent<Button>();
        attackBtn.onClick.RemoveAllListeners();

        if (playerCurrentMapPart.Contains(clickedMapPart))
            attackBtn.enabled = false;
        else if (clickedMapPart != null)
        {
            attackBtn.enabled = true;
            attackBtn.onClick.AddListener(() => CombatManager.Instance.AttackCountry(playerCurrentMapPart, new Map_Part[] { clickedMapPart }, clickedMapPart));
        }
    }

    public void SetupInfoPanel(Map_Part newClickedMap)
    {
        // Setup Map Info Panel
        countryNameLevelText.text = newClickedMap.CountryName + " - LEVEL " + newClickedMap.CountryLevel;
        moneyText.text = (newClickedMap.MoneyAmount).ToString();
        attackPowerText.text = (newClickedMap.AttackPower).ToString();
        defensePowerText.text = (newClickedMap.DefensePower).ToString();
        foodText.text = (newClickedMap.FoodAmount).ToString();
        countryLivesText.text = (newClickedMap.CombatLives).ToString();
    }

    public void SetupResourcePanel(Map_Part map)
    {
        List<BigDouble> mapResourceAmounts = new List<BigDouble>();
        List<BaseResources> mapResourceTypes = new List<BaseResources>();
        mapResourceAmounts = new List<BigDouble>(map.ResourceValueDict.Values);
        mapResourceTypes = new List<BaseResources>(map.ResourceValueDict.Keys);
        int j = 0;
        var panelCount = Mathf.CeilToInt(mapResourceTypes.Count / 2);
        for (int i = 0; i < panelCount; i++)
        {
            var resource = countryResourceContentTransform.GetChild(i);
            resource.transform.Find("Left").Find("Image").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(mapResourceTypes[j]);
            resource.transform.Find("Left").Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = (mapResourceAmounts[j]).ToString();

            if (mapResourceTypes.Count > j + 1)
            {
                resource.transform.Find("Right").Find("Image").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(mapResourceTypes[j + 1]);
                resource.transform.Find("Right").Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = mapResourceAmounts[j + 1].ToString();
            }
            j += 2;
        }
    }
}
