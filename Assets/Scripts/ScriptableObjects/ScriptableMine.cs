using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName ="New Mine", menuName = "Factory/Mine")]
public class ScriptableMine : ScriptableObject
{
    public string Name;

    public string resourceName;

    public BaseResources baseResource;

    public float collectTime;

    public int outputValue;

    public int pricePerProduct;

    public long incomeAmount;

    public Sprite backgroundImage;

    public int unlockLevel;

    public int xpAmount = 10;

    public bool isLockedByContract;

    public ContractBase lockedByContract;
}
