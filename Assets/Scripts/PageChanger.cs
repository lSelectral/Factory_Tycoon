using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PageChanger : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    [SerializeField] private bool isRight;

    Vector3 touchStart;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => ChangePage());

        SetPageSettings();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Mathf.Abs(direction.x) > 2)
            {
                if (direction.x > 0)
                {
                    isRight = true;
                    ChangePage();
                }
                else
                {
                    isRight = false;
                    ChangePage();
                }
            }
        }
    }

    void ChangePage()
    {
        if (isRight)
            PageManager.Instance.index += 1;
        else if (!isRight)
            PageManager.Instance.index -= 1;

        PageManager.Instance.index = Mathf.Clamp(PageManager.Instance.index, 0, pages.Length - 1);

        SetPageSettings();
    }

    void SetPageSettings()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i == Mathf.Abs(PageManager.Instance.index))
            {
                pages[i].GetComponent<CanvasGroup>().interactable = true;
                pages[i].GetComponent<CanvasGroup>().alpha = 1;
                pages[i].GetComponent<CanvasGroup>().blocksRaycasts = true;

                // Place to front for interaction
                pages[i].transform.SetAsLastSibling();
            }
            else
            {
                pages[i].GetComponent<CanvasGroup>().interactable = false;
                pages[i].GetComponent<CanvasGroup>().alpha = 0;
                pages[i].GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }
        PageManager.Instance.minePageInfoText.text = pages[PageManager.Instance.index].name;
    }
}