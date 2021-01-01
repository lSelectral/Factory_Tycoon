using TMPro;
using UnityEngine;

public class StatSystem : Singleton<StatSystem>
{
    [SerializeField] private TextMeshProUGUI currencyPerSecondText, resourcePerSecondText;
    [SerializeField] private float currencyPerSecond, resourcePerSecond;
    private float smoothCurrencyPerSecond, smoothCurrencyVelocity, smoothResourcePerSecond, smoothResourceVelocity;

    public float CurrencyPerSecond
    {
        get { return currencyPerSecond; }
        set { currencyPerSecond = value; }
    }

    public float ResourcePerSecond
    {
        get { return resourcePerSecond; }
        set { resourcePerSecond = value; }
    }

    //private void Update()
    //{
    //    smoothCurrencyPerSecond = Mathf.SmoothDamp(smoothCurrencyPerSecond, currencyPerSecond, ref smoothCurrencyVelocity, .8f);
    //    currencyPerSecondText.text = "$ " + ResourceManager.Instance.CurrencyToString(smoothCurrencyPerSecond) + "/s";
    //}

    public void PopupText(Transform _transform, float outputValue, string resourceName, float popupTime = 1f)
    {
        var obj = new GameObject("FloatingText");
        var text = obj.AddComponent<TextMeshProUGUI>();
        
        var rect = obj.GetComponent<RectTransform>();

        rect.anchoredPosition = _transform.GetComponent<RectTransform>().position;
        rect.sizeDelta = Vector2.zero;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        //rect.sizeDelta = _transform.GetComponent<RectTransform>().rect.size;

        obj.transform.SetParent(_transform);
        obj.transform.SetAsLastSibling();
        text.text = string.Format("+{0} {1}", ResourceManager.Instance.CurrencyToString(outputValue), resourceName);
        text.raycastTarget = false;
        obj.transform.localScale = new Vector3(1, 1, 1);
        text.fontSize = 80;
        text.alignment = TextAlignmentOptions.Midline;

        Vector3 moveAmount = new Vector3(0, (_transform.GetComponent<RectTransform>().rect.height / 4)) / popupTime;
        CodeMonkey.Utils.FunctionUpdater.Create(delegate ()
        {
            obj.transform.position += moveAmount * Time.deltaTime;
            popupTime -= Time.deltaTime;
            if (popupTime <= 0f)
            {
                Destroy(obj);
                return true;
            }
            else
                return false;
        }, "WorldTextPopup");
    }
}