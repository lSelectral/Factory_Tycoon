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

    private void Awake()
    {
        allMaps = new List<Map_Part>();
        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;

        playerCurrentMapPart = new Map_Part[] { playerStartMapPart };

        var resourceBtn = mapStatPanel.Find("ResourcePanelBtn").GetComponent<Button>();
        resourceBtn.onClick.AddListener(() => { if (clickedMapPart != null) countryResourceContentTransform.parent.parent.gameObject.SetActive(true); });

        // TODO Remove or fix this part, this is just for debug
        clickedMapPart = playerStartMapPart;
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
        mapStatPanel.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = newClickedMap.CountryName + " - LEVEL " + newClickedMap.CountryLevel;
        var infoPanel = mapStatPanel.Find("InfoPanel");
        infoPanel.Find("Top").Find("Money").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.MoneyAmount);
        infoPanel.Find("Top").Find("AttackPower").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.AttackPower);
        infoPanel.Find("Top").Find("DefensePower").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.DefensePower);
        infoPanel.Find("Bottom").Find("Food").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.FoodAmount);
        infoPanel.Find("Bottom").Find("CountryLives").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.CombatLives);
    }

    public void SetupResourcePanel(Map_Part map)
    {
        List<BNum> mapResourceAmounts = new List<BNum>();
        List<BaseResources> mapResourceTypes = new List<BaseResources>();
        //if (!map.IsPlayerOwned)
        //{
        //    mapResourceAmounts = new List<long>(map.ResourceValueDict.Values);
        //    mapResourceTypes = new List<BaseResources>(map.ResourceValueDict.Keys);
        //    Debug.Log("Not player owned map");
        //    DebugList(mapResourceAmounts);
        //}
        //else
        //{
        //    mapResourceAmounts = new List<long>(ResourceManager.Instance.resourceValueDict.Values);
        //    mapResourceTypes = new List<BaseResources>(ResourceManager.Instance.resourceValueDict.Keys);
        //    Debug.Log("Player owned map");
        //    DebugList(mapResourceAmounts);
        //}
        mapResourceAmounts = new List<BNum>(map.ResourceValueDict.Values);
        mapResourceTypes = new List<BaseResources>(map.ResourceValueDict.Keys);
        //Debug.Log(mapResourceTypes.Count);
        //DebugList(mapResourceTypes);
        //DebugList(mapResourceAmounts);
        int j = 0;
        var panelCount = Mathf.CeilToInt(mapResourceTypes.Count / 2);
        for (int i = 0; i < panelCount; i++)
        {
            var resource = countryResourceContentTransform.GetChild(i);
            resource.transform.Find("Left").Find("Image").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(mapResourceTypes[j]);
            resource.transform.Find("Left").Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(mapResourceAmounts[j]);

            if (mapResourceTypes.Count > j + 1)
            {
                //Debug.Log(mapResourceTypes[j + 1]);
                resource.transform.Find("Right").Find("Image").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(mapResourceTypes[j + 1]);
                resource.transform.Find("Right").Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(mapResourceAmounts[j + 1]);
            }
            j += 2;
        }
    }

    public void DebugList<T>(List<T> listToDebug)
    {
        string debugText = listToDebug.ToString() + " count: " + listToDebug.Count + "\n" ;
        for (int i = 0; i < listToDebug.Count; i++)
        {
            debugText += listToDebug[i] + "\n";
        }
        Debug.Log(debugText);
    }
}
