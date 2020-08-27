using System.Collections.Generic;
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

    public long incomeAmount;

    public int unlockLevel;

    public int xpAmount = 25;

    public bool isLockedByContract;

    public ContractBase lockedByContract;
}
