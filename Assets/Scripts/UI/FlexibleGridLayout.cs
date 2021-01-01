using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns,

    }

    public int rows;
    public int columns;
    public Vector2 cellSize;
    public Vector2 spacing;
    public FitType fitType;

    bool FitX
    {
        get
        {
            return fitX;
        }
        set
        {
            fitX = value;
        }
    }

    public bool fitX;
    public bool fitY;

    public override void SetLayoutHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
        {
            float sqrt = Mathf.Sqrt(transform.childCount);
            columns = Mathf.CeilToInt(sqrt);
            rows = Mathf.CeilToInt(sqrt);
        }
        if (fitType == FitType.Width || fitType == FitType.FixedColumns)
        {
            rows = Mathf.CeilToInt(transform.childCount / (float)columns);
        }
        if (fitType == FitType.Height || fitType == FitType.FixedRows)
        {
            columns = Mathf.CeilToInt(transform.childCount / (float)rows);
        }
        
        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellwidth = parentWidth / (float)columns - ((spacing.x / (float)columns) * 2) - (padding.left / (float)columns) - (padding.right / (float)columns);
        float cellHeight = parentHeight / (float)rows - ((spacing.y / (float)rows) * 2) - (padding.top / (float)rows) - (padding.bottom / (float)rows);

        //if (cellHeight > cellwidth)
        //{
        //    cellHeight = cellwidth;
        //}
        //else
        //    cellwidth = cellHeight;


        cellSize.x = fitX? cellwidth: cellSize.x;
        cellSize.y = fitY? cellHeight: cellSize.y;

        int columnCount = 0;
        int rowCount = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / columns;
            columnCount = i % columns;

            var item = rectChildren[i];

            var xpos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
            var ypos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

            SetChildAlongAxis(item, 0, xpos, cellSize.x);
            SetChildAlongAxis(item, 1, ypos, cellSize.y);
        }
    }

    public override void SetLayoutVertical()
    {
    }

    public override void CalculateLayoutInputVertical()
    {
    }
}
