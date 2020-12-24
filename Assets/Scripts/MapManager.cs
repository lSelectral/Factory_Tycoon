using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : Singleton<MapManager>
{
    public Transform mapTransform;

    [HideInInspector] public Map_Part clickedMapPart;

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

    public void ConquerArea(Map_Part[] map1, Map_Part[] map2)
    {
        for (int i = 0; i < map2.Length; i++)
        {
            map2[i].GetComponent<Image>().color = map1[0].GetComponent<Image>().color;
        }
    }

    public void OnMapClick(Map_Part newClickedMap, float scaleFactor = 1.23f)
    {
        #region Highlighting the selected map
        // Don't refresh if still on same object
        if (clickedMapPart != newClickedMap && clickedMapPart != null)
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
        }
        #endregion

        // Setup Map Info Panel
        mapStatPanel.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = newClickedMap.CountryName + " - LEVEL " + newClickedMap.CountryLevel;
        var infoPanel = mapStatPanel.Find("InfoPanel");
        infoPanel.Find("Top").Find("Money").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.MoneyAmount);
        infoPanel.Find("Top").Find("AttackPower").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.AttackPower);
        infoPanel.Find("Bottom").Find("Food").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(newClickedMap.FoodAmount);
        infoPanel.Find("Bottom").Find("CurrentAge").GetComponentInChildren<TextMeshProUGUI>().text = (newClickedMap.CurrentAgeOfNation.ToString());

    }
}
