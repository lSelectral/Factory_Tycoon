using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ContractHolder : MonoBehaviour
{
    public ContractBase contract;
    public List<BaseResources> requiredResources;
    public List<BigDouble> requiredResourceAmounts;
    public List<ContractBase> dependencyContracts;
    public bool isContractCompleted;
}