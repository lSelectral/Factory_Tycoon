using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : Singleton<MapManager>
{
    public Transform mapTransform;

    public Map_Part clickedMapPart;

    public Map_Part playerStartMapPart;

    public Map_Part playerCurrentMapPart;

    public void MergeArea(Map_Part map1, Map_Part map2)
    {
        map2.GetComponent<Image>().color = map1.GetComponent<Image>().color;
    }

    public void MapClickEffet(Map_Part newMap, float scaleFactor = 1.13f)
    {
        if (clickedMapPart != newMap)
        {
            if (clickedMapPart != null)
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
            }

            newMap.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            newMap.transform.SetAsLastSibling();
            var outline = newMap.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.red;
            outline.effectDistance = Vector2.one * 3f;

            clickedMapPart = newMap;
        }
    }
}
