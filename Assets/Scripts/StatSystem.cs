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

    private void Update()
    {
        smoothCurrencyPerSecond = Mathf.SmoothDamp(smoothCurrencyPerSecond, currencyPerSecond, ref smoothCurrencyVelocity, .8f);
        currencyPerSecondText.text = "$ " + ResourceManager.Instance.CurrencyToString(smoothCurrencyPerSecond) + "/s";
    }
}