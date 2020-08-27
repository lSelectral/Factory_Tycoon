using TMPro;
using UnityEngine;

public class PageManager : Singleton<PageManager>
{
    public TextMeshProUGUI minePageInfoText;
    [SerializeField] private GameObject[] pages;

    public int index = 0;

    Vector3 touchStart;

    private void Start()
    {
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
                    ChangePage(true);
                }
                else
                {
                    ChangePage(false);
                }
            }
        }
    }

    public void ChangePage(bool isRight)
    {
        if (isRight)
            index += 1;
        else if (!isRight)
            index -= 1;

        index = Mathf.Clamp(index, 0, pages.Length - 1);

        SetPageSettings();
    }

    void SetPageSettings()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i == Mathf.Abs(index))
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
        minePageInfoText.text = pages[index].name;
    }
}
