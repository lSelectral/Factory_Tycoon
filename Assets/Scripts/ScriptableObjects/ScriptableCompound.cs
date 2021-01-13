using System;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;

[CreateAssetMenu(fileName = "New Part", menuName = "Factory/Compound")]
public class ScriptableCompound : ScriptableProductionBase
{
    [SerializeField] bool isFixedArraySize = true;
    public BaseResources[] inputResources;
    public int[] inputAmounts;

    // If product is housing, enter how much population it will give
    public long housingAmount;

    // If product is weapon, enter how much attack it will give
    public long attackAmount;

    public float basePricePerProduct;

    public string optimalPricePerProductText;
    public float optimalPricePerProduct;

    public string optimalIncomePerSecondText;
    [Tooltip("Minimum income per second calculated by ingredients")]
    public float optimalPricePerSecond;

    [SerializeField] bool isOptimal;

    /// <summary>
    /// Set some of the values of scriptable object automatically according to hierarchy and naming
    /// </summary>
    public override void OnValidate()
    {
        base.OnValidate();

        // If set to true, Input Resource and Amount array size will set to biggest one's size
        if (isFixedArraySize && inputAmounts.Length != inputResources.Length)
            {
                if (inputAmounts.Length > inputResources.Length)
                    Array.Resize(ref inputResources, inputAmounts.Length);
                else
                    Array.Resize(ref inputAmounts, inputResources.Length);
            }

            // Set Price and optimal price values
            if (collectTime > 0 && outputValue > 0 && inputAmounts.Length > 0 && inputResources.Length > 0)
            {
                optimalPricePerProduct = ProductionManager.Instance.GET_OPTIMAL_PRICE_PER_PRODUCT_EDITOR(this);
                optimalPricePerProductText = ResourceManager.Instance.CurrencyToString(optimalPricePerProduct);

                pricePerProduct = (optimalPricePerProduct + basePricePerProduct);
                pricePerProductText = ResourceManager.Instance.CurrencyToString(pricePerProduct);

                optimalPricePerSecond = ProductionManager.Instance.GetIncomePerSecondForEDITOR(inputResources, inputAmounts);
                optimalIncomePerSecondText = ResourceManager.Instance.CurrencyToString(optimalPricePerSecond);

                incomePerSecond = (pricePerProduct) * outputValue / collectTime;
                incomePerSecondText = ResourceManager.Instance.CurrencyToString(incomePerSecond);

                if (incomePerSecond >= optimalPricePerSecond)
                    isOptimal = true;
                else
                    isOptimal = false;
            }
    }
}