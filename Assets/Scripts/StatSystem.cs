using TMPro;
using UnityEngine;

public class StatSystem : Singleton<StatSystem>
{
    [SerializeField] private TextMeshProUGUI currencyPerSecondText;
    [SerializeField] private float currencyPerSecond;
    private float smoothCurrency, smoothCurrencyVelocity;

    public float CurrencyPerSecond
    {
        get { return currencyPerSecond; }
        set { currencyPerSecond = value; }
    }

    private void Update()
    {
        smoothCurrency = Mathf.SmoothDamp(smoothCurrency, currencyPerSecond, ref smoothCurrencyVelocity, .8f);
        currencyPerSecondText.text = "$ " + ResourceManager.Instance.CurrencyToString(smoothCurrency) + "/s";
    }
}