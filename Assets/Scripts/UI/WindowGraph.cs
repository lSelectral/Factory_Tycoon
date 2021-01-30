using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class WindowGraph : MonoBehaviour
{
    [SerializeField] Sprite circleSprite;
    RectTransform graphContainer;
    RectTransform labelTemplateX;
    RectTransform labelTemplateY;

    private void Awake()
    {
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();

        List<long> values = new List<long>() { 12, 32, 23, 56, 41, 98, 123, 321, 65, 455 };
        ShowGraph(values);
    }

    void CreateCircle(Vector2 anchoredPosition)
    {
        GameObject obj = new GameObject("circle", typeof(Image));
        obj.transform.SetParent(graphContainer, false);
        obj.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(15, 15);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
    }

    void ShowGraph(List<long> valueList)
    {
        float xSize = 50f;
        float yMaximum = 100f;
        float graphHeight = graphContainer.sizeDelta.y;

        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = xSize + i * xSize;
            float yPosition = valueList[i] / yMaximum * graphHeight;
            CreateCircle(new Vector2(xPosition, yPosition));
        }
    }
}
