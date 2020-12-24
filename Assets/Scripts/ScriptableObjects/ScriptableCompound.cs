using UnityEngine;

[CreateAssetMenu(fileName = "New Part", menuName = "Factory/Compound")]
public class ScriptableCompound : ScriptableObject
{
    public string Description;
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

    public ItemType itemType;

    public string pricePerProduct;

    public string incomePerSecond;

    private void OnValidate()
    {
        // Debug price per product
        //if (buildTime > 0 && outputValue > 0 && basePricePerProduct > 0 && inputAmounts.Length > 0 && inputResources.Length > 0)
        //{
        //    var _pricePerProduct = ResourceManager.Instance.CurrencyToString(ProductionManager.Instance.GetPricePerProductForEDITOR(inputResources, inputAmounts, buildTime) + basePricePerProduct);
        //    Debug.Log(string.Format("{0} price per product is: <b><color=blue>{1}</color></b>",partName, (_pricePerProduct)));
        //    pricePerProduct = (_pricePerProduct);

        //    var _incomePerSecond = ResourceManager.Instance.CurrencyToString((ProductionManager.Instance.GetPricePerProductForEDITOR(inputResources, inputAmounts, buildTime) + basePricePerProduct) * outputValue / buildTime);
        //    Debug.Log(string.Format("{0} income per second is: <b><color=red>{1}</color></b>",partName, (_incomePerSecond)));
        //    incomePerSecond = (_incomePerSecond);
        //}
    }
}