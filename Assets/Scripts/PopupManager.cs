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

    public void PopupConfirmationPanel(string body, UnityAction action1, UnityAction action2)
    {
        confirmationPopUpPrefab.transform.Find("AcceptBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        confirmationPopUpPrefab.transform.Find("CancelBtn").GetComponent<Button>().onClick.RemoveAllListeners();


        confirmationPopUpPrefab.transform.parent.gameObject.SetActive(true);
        confirmationPopUpPrefab.transform.parent.SetAsLastSibling();

        confirmationPopUpPrefab.transform.Find("BODY").GetComponent<TextMeshProUGUI>().text = body;
        confirmationPopUpPrefab.transform.Find("AcceptBtn").GetComponent<Button>().onClick.AddListener(action1);
        confirmationPopUpPrefab.transform.Find("AcceptBtn").GetComponent<Button>().onClick.AddListener(() => confirmationPopUpPrefab.transform.parent.gameObject.SetActive(false));
        confirmationPopUpPrefab.transform.Find("CancelBtn").GetComponent<Button>().onClick.AddListener(action2);

        if (action1 == null)
            Destroy(confirmationPopUpPrefab.transform.Find("AcceptBtn").gameObject);
        if (action2 == null)
            Destroy(confirmationPopUpPrefab.transform.Find("CancelBtn").gameObject);

        if (action1 == null && action2 == null)
            TweenAnimation.Instance.SmoothHide(confirmationPopUpPrefab);
            
    }
}
