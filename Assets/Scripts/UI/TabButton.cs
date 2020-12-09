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
                btn.GetComponent<Image>().color = Color.white;
            GetComponent<Image>().color = Color.red;
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

    // Start is called before the first frame update
    void Start()
    {
        tabGroup.Subscribe(this);

        if (transform.GetSiblingIndex() == 0)
        {
            GetComponent<Image>().color = Color.red;
            tabGroup.OnTabSelected(this);
        }
    }
}
