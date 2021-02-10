using TMPro;
using UnityEngine;

public class PageManager : Singleton<PageManager>
{
    public TextMeshProUGUI minePageInfoText;
    [SerializeField] private GameObject[] pages;

    Touch touch;
    public int index = 0;
    Vector3 touchStart;
    TouchPhase touchPhase;

    private void Start()
    {
        SetPageSettings();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!TutorialManager.Instance.isTutorialActive)
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
#endif

//#if UNITY_ANDROID
//        if (Input.touchCount == 1)
//        {
//            touch = Input.GetTouch(0);
//            touchStart = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
//            touch.phase = TouchPhase.Began;
//        }

//        if (touch.phase == TouchPhase.Moved)
//        {
//            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
//            if (Mathf.Abs(direction.x) > 2)
//            {
//                if (direction.x > 0)
//                {
//                    ChangePage(true);
//                }
//                else
//                {
//                    ChangePage(false);
//                }
//            }
//        }
//        #endif
    }

    public void ChangePage(bool isRight)
    {
        if (isRight)
            index = pages.GetNextIndex(index);
        else if (!isRight)
            index = pages.GetPreviousIndex(index);

        index = Mathf.Clamp(index, 0, pages.Length - 1);
        //index = pages.GetNextIndex(index);

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

                GameManager.Instance.VisibleSubPanelForPlayer = pages[i];
            }
            else
            {
                pages[i].GetComponent<CanvasGroup>().interactable = false;
                pages[i].GetComponent<CanvasGroup>().alpha = 0;
                pages[i].GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }

        string pageName = /*ResourceManager.Instance.GetValidName*/((pages[index].name).Substring(3));
        minePageInfoText.text = pageName;
    }
}