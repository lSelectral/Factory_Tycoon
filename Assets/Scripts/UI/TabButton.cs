using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private TabGroup tabGroup;

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
        foreach (TabButton btn in tabGroup.tabButtons)
        {
            if (btn != this)
                btn.transform.Find("selectionImage").gameObject.SetActive(false);
            transform.Find("selectionImage").gameObject.SetActive(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }

    void Start()
    {
        tabGroup.Subscribe(this);

        if (transform.GetSiblingIndex() == 0)
        {
            transform.Find("selectionImage").gameObject.SetActive(true);
            tabGroup.OnTabSelected(this);
        }
    }
}
