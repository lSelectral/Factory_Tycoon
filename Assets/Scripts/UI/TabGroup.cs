using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;

    public TabButton selectedTab;

    [SerializeField]
    private List<GameObject> panels;

    public void Subscribe(TabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }
        tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if (selectedTab == null && button != selectedTab)
        {

        }
    }

    public void OnTabSelected(TabButton button)
    {
        selectedTab = button;
        ResetTabs();

        int index = button.transform.GetSiblingIndex();
        SelectPage(index); 
    }

    public void SelectPage(int index)
    {
        for (int i = 0; i < panels.Count; i++)
        {
            if (i == index)
            {
                panels[i].GetComponent<CanvasGroup>().interactable = true;
                panels[i].GetComponent<CanvasGroup>().alpha = 1;
                panels[i].GetComponent<CanvasGroup>().blocksRaycasts = true;

                // Place to front for interaction
                panels[i].transform.SetAsLastSibling();
            }
            else
            {
                panels[i].GetComponent<CanvasGroup>().interactable = false;
                panels[i].GetComponent<CanvasGroup>().alpha = 0;
                panels[i].GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }
    }

    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }

    public void ResetTabs()
    {
        foreach (TabButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab)
            {
                continue;
            }
        }
    }
}
