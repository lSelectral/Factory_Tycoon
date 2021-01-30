using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PopupManager : Singleton<PopupManager>
{
    [SerializeField] private GameObject popupPrefab;
    public GameObject confirmationPopUpPrefab;

    private Animator animator;

    private void Awake()
    {
        animator = popupPrefab.GetComponent<Animator>();
    }

    public void PopupPanel(string header, string description)
    {
        popupPrefab.transform.SetAsLastSibling();
        popupPrefab.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = header;
        popupPrefab.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = description;
        animator.SetTrigger("Rise");
        StartCoroutine(ReturnToStart());
    }

    private IEnumerator ReturnToStart(float time = 3f)
    {
        yield return new WaitForSeconds(time);
        animator.SetTrigger("Fall");
    }

    public void PopupConfirmationPanel(string body, UnityAction action1, UnityAction action2, ContractBase cb = null)
    {
        Transform acceptBtn = confirmationPopUpPrefab.transform.Find("AcceptBtn");
        Transform cancelBtn = confirmationPopUpPrefab.transform.Find("CancelBtn");

        acceptBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        cancelBtn.GetComponent<Button>().onClick.RemoveAllListeners();

        confirmationPopUpPrefab.transform.Find("BODY").GetComponent<TextMeshProUGUI>().text = body;
        acceptBtn.GetComponent<Button>().onClick.AddListener(action1);
        acceptBtn.GetComponent<Button>().onClick.AddListener(() => TweenAnimation.Instance.ShowHideElement(confirmationPopUpPrefab.transform.parent.gameObject));
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false));
        confirmationPopUpPrefab.transform.Find("CancelBtn").GetComponent<Button>().onClick.AddListener(action2);

        // Activate panel and set foremost
        TweenAnimation.Instance.ShowHideElement(confirmationPopUpPrefab.transform.parent.gameObject);
        //confirmationPopUpPrefab.transform.parent.gameObject.SetActive(true);
        confirmationPopUpPrefab.transform.parent.SetAsLastSibling();


        if (action1 == null)
            acceptBtn.gameObject.SetActive(false);
        else
            acceptBtn.gameObject.SetActive(true);
        if (action2 == null)
            cancelBtn.gameObject.SetActive(false);
        else
            cancelBtn.gameObject.SetActive(true);

        //if (action1 == null && action2 == null)
        //TweenAnimation.Instance.SmoothHide(confirmationPopUpPrefab);

        if (action1 == null && action2 == null)
            TweenAnimation.Instance.ShowHideElement(confirmationPopUpPrefab.transform.parent.gameObject);
    }
}
