﻿using System.Diagnostics;
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

    public Sprite backgroundImage;

    public Sprite icon;

    public int unlockLevel;

    public int xpAmount = 10;

    public bool isLockedByContract;

    public Age ageBelongsTo;

    public Tier tier;

    public bool isUnlocked;
}