using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Mine_Btn : ProductionBase
{
    public ScriptableMine scriptableMine;

    protected override void Start()
    {
        base.Start();

        workModeBtn.onClick.AddListener(() => ChangeWorkingMode(true));
        workingMode = WorkingMode.sell;
        workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());

        IncomePerSecond = (outputPerSecond * pricePerProduct);
        StatSystem.Instance.CurrencyPerSecond += incomePerSecond;
    }

    #region Event Methods

    #endregion

    GameObject CreateIconObj()
    {
        GameObject obj = new GameObject(resourceName);
        obj.transform.SetParent(sourceImage.transform);
        var rect = obj.AddComponent<RectTransform>();
        obj.transform.localPosition = Vector3.zero;
        rect.offsetMax = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.sizeDelta = new Vector2(130, 130);
        rect.localScale = Vector3.one;

        var icon = obj.AddComponent<Image>();
        icon.sprite = ResourceManager.Instance.GetSpriteFromResource(producedResource);
        return obj;
    }

    GameObject _icon;
    LTDescr _iconMoveAnimation;
    LTDescr _iconScaleAnimation;
    protected override void Update()
    {
        if (isAutomated) Produce();

        if (isCharging)
        {
            if (remainedCollectTime > 0)
            {
                remainedCollectTime -= Time.deltaTime * UpgradeSystem.Instance.MiningSpeedMultiplier;
                fillBar.fillAmount = ((collectTime - remainedCollectTime) / collectTime);

                // BUG Objects moving to screen point, so when scrolling animations too, scroll.
                if (remainedCollectTime <= 0.6f)
                {
                    if (_icon == null)
                    {
                        _icon = CreateIconObj();
                        _iconScaleAnimation = LeanTween.size(_icon.GetComponent<RectTransform>(), new Vector2(290, 290), .7f).setOnComplete(() => _iconScaleAnimation = null) ;
                        _iconMoveAnimation = LeanTween.move(_icon, icon.transform.position, .7f)
                            .setOnUpdateObject((float value, object obj) =>
                            {
                                _icon.transform.position = new Vector3(_icon.transform.position.x, 
                                    Mathf.Clamp(_icon.transform.position.y+30, sourceImage.transform.position.y, icon.transform.position.y), 0);
                            })
                            .setOnComplete(() => { Destroy(_icon); _iconMoveAnimation = null; });
                    }
                }
            }
            else
            {
                //float currency;
                //float resource;
                AddResourceAndMoney(/*out currency, out resource*/);

                if (CheckIfPanelActive())
                {
                    if (workingMode == WorkingMode.production)
                        StatSystem.Instance.PopupText(transform, OutputValue, resourceName);
                    else if (workingMode == WorkingMode.sell)
                        StatSystem.Instance.PopupText(transform, pricePerProduct, "Gold");
                }

                isCharging = false;
                remainedCollectTime = 0;
                fillBar.fillAmount = 0;
                LeanTween.cancel(tool.gameObject);
            }
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        if (isCharging)
        {
            if (_iconMoveAnimation != null)
                _iconMoveAnimation.time -= .3f;
            if (_iconScaleAnimation != null)
                _iconScaleAnimation.time -= .3f;
        }
    }

    void AddResourceAndMoney(/*out float currency, out float resourceAmount*/)
    {
        switch (workingMode)
        {
            case WorkingMode.production:
                /*resourceAmount = */ResourceManager.Instance.AddResource(producedResource, new BigDouble(outputValue * UpgradeSystem.Instance.MiningYieldMultiplier,0));
                break;
            case WorkingMode.sell:
                ResourceManager.Instance.AddResource(producedResource, new BigDouble(outputValue * UpgradeSystem.Instance.MiningYieldMultiplier,0));
                SellResource();
                break;
        }
        //resourceAmount = ResourceManager.Instance.AddResource(mine.baseResource, (long)(mine.outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
        //currency = incomeAmount;
        //ResourceManager.Instance.Currency += incomeAmount;
        GameManager.Instance.AddXP(XPAmount);
    }
}