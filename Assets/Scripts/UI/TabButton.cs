using UnityEngine;
using UnityEngine.EventSystems;

public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private TabGroup tabGroup;

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
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
            tabGroup.OnTabSelected(this);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
