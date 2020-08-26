using UnityEngine;
using System.Collections;

public class ProductSeller : Singleton<ProductSeller>
{
    public void SellResource(BaseResources resource, int amount)
    {
        long resourceValue = 0;
        foreach (var mine in ProductionManager.Instance.mines)
        {
            if (mine.baseResource == resource)
                resourceValue = mine.incomeAmount;
        }

        foreach (var ingot in ProductionManager.Instance.ingots)
        {
            if (ingot.product == resource)
                resourceValue = ingot.incomeAmount;
        }

        foreach (var primitive in ProductionManager.Instance.primitives)
        {
            if (primitive.product == resource)
                resourceValue = primitive.incomeAmount;
        }

        foreach (var advanced in ProductionManager.Instance.advanced)
        {
            if (advanced.product == resource)
                resourceValue = advanced.incomeAmount;
        }

        foreach (var t1 in ProductionManager.Instance.T1)
        {
            if (t1.product == resource)
                resourceValue = t1.incomeAmount;
        }

        foreach (var t2 in ProductionManager.Instance.T2)
        {
            if (t2.product == resource)
                resourceValue = t2.incomeAmount;
        }

        foreach (var t3 in ProductionManager.Instance.T3)
        {
            if (t3.product == resource)
                resourceValue = t3.incomeAmount;
        }
        ResourceManager.Instance.Currency += resourceValue * amount;
    }
}