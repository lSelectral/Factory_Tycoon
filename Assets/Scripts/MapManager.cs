using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : Singleton<MapManager>
{
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
        playerCurrentMapPart = new Map_Part[] { playerStartMapPart };
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

        #region Setting up the resource panel

        List<long> playerResourceAmounts = new List<long>(ResourceManager.Instance.resourceValueDict.Values);
        List<BaseResources> playerResourceTypes = new List<BaseResources>(ResourceManager.Instance.resourceValueDict.Keys);

        int j = 0;
        var panelCount = Mathf.CeilToInt(playerResourceTypes.Count / 2);
        for (int i = 0; i < panelCount; i++)
        {
            var res1 = Instantiate(resourceLayoutPrefab, countryResourceContentTransform);
            res1.transform.Find("Left").GetComponentInChildren<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(playerResourceTypes[j]);
            res1.transform.Find("Left").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(playerResourceAmounts[j]);

            if (playerResourceTypes.Count >= j + 1)
            {
                var res2 = Instantiate(resourceLayoutPrefab, countryResourceContentTransform);
                res2.transform.Find("Right").GetComponentInChildren<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(playerResourceTypes[j + 1]);
                res2.transform.Find("Right").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(playerResourceAmounts[j + 1]);
            }
            j += 2;
        }

        #endregion

        // Setup Map Info Panel
        mapStatPanel.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = newClickedMap.CountryName + " - LEVEL " + newClickedMap.CountryLevel;
        var infoPanel = mapStatPanel.Find("InfoPanel");
        infoPanel.Find("Top").Find("Money").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.MoneyAmount);
        infoPanel.Find("Top").Find("AttackPower").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.AttackPower);
        infoPanel.Find("Bottom").Find("Food").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.FoodAmount);
        infoPanel.Find("Bottom").Find("DefensePower").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.DefensePower);

        var resourceBtn = mapStatPanel.Find("ResourcePanelBtn").GetComponent<Button>();
        resourceBtn.onClick.AddListener(() => { if(clickedMapPart != null) countryResourceContentTransform.parent.parent.gameObject.SetActive(true); });
        
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
}
