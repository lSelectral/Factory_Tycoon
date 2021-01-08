using System;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;

[CreateAssetMenu(fileName = "New Part", menuName = "Factory/Compound")]
public class ScriptableCompound : ScriptableObject
{
    public string Description;

    [SerializeField] bool isFixedArraySize = true;
    public BaseResources[] inputResources;
    public int[] inputAmounts;

    public BaseResources product;

    public string partName;

    public float buildTime;

    public int outputValue;

    public float basePricePerProduct;

    //public float pricePerProduct;

    public int unlockLevel;

    public int xpAmount = 25;

    public bool isLockedByContract;

    public ContractBase lockedByContract;

    public Sprite icon;

    public Age ageBelongsTo;

    public Tier tier;

    public ItemType[] itemType;

    public string pricePerProductText;

    public float _pricePerProduct;

    public string incomePerSecondText;
    public float incomePerSecond;
    
    public string optimalIncomePerSecondText;
    [Tooltip("Minimum income per second calculated by ingredients")]
    public float optimalPricePerSecond;

    [SerializeField] bool isOptimal;

    /// <summary>
    /// Set some of the values of scriptable object automatically according to hierarchy and naming
    /// </summary>
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (isFixedArraySize && inputAmounts.Length != inputResources.Length)
            {
                if (inputAmounts.Length > inputResources.Length)
                    Array.Resize(ref inputResources, inputAmounts.Length);
                else
                    Array.Resize(ref inputAmounts, inputResources.Length);
            }

            partName = name;

            foreach (Tier tier in Enum.GetValues(typeof(Tier)))
            {
                if (tier.ToString() == Directory.GetParent(AssetDatabase.GetAssetPath(this)).Name)
                    this.tier = tier;
            }

            foreach (Age age in Enum.GetValues(typeof(Age)))
            {
                if (age.ToString() == Directory.GetParent(AssetDatabase.GetAssetPath(this)).Parent.Name)
                    this.ageBelongsTo = age;
            }

            foreach (BaseResources res in Enum.GetValues(typeof(BaseResources)))
            {
                if (name == ResourceManager.Instance.GetValidNameForResource(res))
                    product = res;
            }

            if (buildTime > 0 && outputValue > 0 && inputAmounts.Length > 0 && inputResources.Length > 0)
            {
                _pricePerProduct = (ProductionManager.Instance.GetPricePerProductForEDITOR(inputResources, inputAmounts) + basePricePerProduct);
                //Debug.Log(string.Format("{0} price per product is: <b><color=blue>{1}</color></b>", partName, (_pricePerProduct)));
                pricePerProductText = ResourceManager.Instance.CurrencyToString(_pricePerProduct);

                incomePerSecond = (_pricePerProduct+basePricePerProduct) * outputValue / buildTime;
                incomePerSecondText = ResourceManager.Instance.CurrencyToString(incomePerSecond);


                optimalPricePerSecond = (ProductionManager.Instance.GetIncomePerSecondForEDITOR(inputResources, inputAmounts))/* + (basePricePerProduct * outputValue / buildTime)*/;
                //Debug.Log(string.Format("{0} income per second is: <b><color=red>{1}</color></b>", partName, (_incomePerSecond)));
                optimalIncomePerSecondText = ResourceManager.Instance.CurrencyToString(optimalPricePerSecond);

                if (incomePerSecond >= optimalPricePerSecond)
                    isOptimal = true;
                else
                    isOptimal = false;
            }
        }
    }
}