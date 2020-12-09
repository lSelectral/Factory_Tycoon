using System;
using UnityEngine;

public class UpgradeSystem : Singleton<UpgradeSystem>
{
    public GameObject upgradePanel;

    #region Events

    public class OnMiningSpeedChangedEventArgs: EventArgs
    {
        public float miningSpeed;
    }

    public event EventHandler<OnMiningSpeedChangedEventArgs> OnMiningSpeedChanged;

    public class OnMiningYieldChangedEventArgs : EventArgs
    {
        public long miningYield;
    }

    public event EventHandler<OnMiningYieldChangedEventArgs> OnMiningYieldChanged;

    public class OnProductionSpeedChangedEventArgs : EventArgs
    {
        public float productionSpeed;
    }

    public event EventHandler<OnProductionSpeedChangedEventArgs> OnProductionSpeedChanged;

    public class OnProductionYieldChangedEventArgs : EventArgs
    {
        public long productionYield;
    }

    public event EventHandler<OnProductionYieldChangedEventArgs> OnProductionYieldChanged;

    public class OnProductionEfficiencyChangedEventArgs : EventArgs
    {
        public float productionEfficiency;
    }

    public event EventHandler<OnProductionEfficiencyChangedEventArgs> OnProductionEfficiencyChanged;

    public class OnEarnedCoinMultiplierChangedEventArgs : EventArgs
    {
        public float earnedCoinMultiplier;
    }

    public event EventHandler<OnEarnedCoinMultiplierChangedEventArgs> OnEarnedCoinMultiplierChanged;

    public class OnEarnedXpMultiplierChangedEventArgs : EventArgs
    {
        public float earnedXpMultiplier;
    }

    public event EventHandler<OnEarnedXpMultiplierChangedEventArgs> OnEarnedXpMultiplierChanged;

    #endregion

    [SerializeField] private float miningSpeedMultiplier = 1;
    [SerializeField] private long miningYieldMultiplier = 1;
    [SerializeField] private float productionSpeedMultiplier = 1;
    [SerializeField] private long productionYieldMultiplier = 1;
    [SerializeField] private float productionEfficiencyMultiplier = 1;
    [SerializeField] private float earnedCoinMultiplier = 1;
    [SerializeField] private float earnedXPMultiplier = 1;

    // Set events
    public float MiningSpeedMultiplier { get => miningSpeedMultiplier; set { miningSpeedMultiplier = value; OnMiningSpeedChanged(this, new OnMiningSpeedChangedEventArgs() { miningSpeed = miningSpeedMultiplier }); } }
    public long MiningYieldMultiplier { get => miningYieldMultiplier; set { miningYieldMultiplier = value; OnMiningYieldChanged(this, new OnMiningYieldChangedEventArgs() { miningYield = miningYieldMultiplier }); } }
    public float ProductionSpeedMultiplier { get => productionSpeedMultiplier; set { productionSpeedMultiplier = value; OnProductionSpeedChanged(this, new OnProductionSpeedChangedEventArgs() { productionSpeed = productionSpeedMultiplier }); } }
    public long ProductionYieldMultiplier { get => productionYieldMultiplier; set { productionYieldMultiplier = value; OnProductionYieldChanged(this, new OnProductionYieldChangedEventArgs() { productionYield = productionYieldMultiplier }); } }
    public float ProductionEfficiencyMultiplier { get => productionEfficiencyMultiplier; set { productionEfficiencyMultiplier = value; OnProductionEfficiencyChanged(this, new OnProductionEfficiencyChangedEventArgs() { productionEfficiency = productionEfficiencyMultiplier }); } }
    public float EarnedCoinMultiplier { get => earnedCoinMultiplier; set { earnedCoinMultiplier = value; OnEarnedCoinMultiplierChanged(this, new OnEarnedCoinMultiplierChangedEventArgs() { earnedCoinMultiplier = earnedCoinMultiplier }); } }
    public float EarnedXPMultiplier { get => earnedXPMultiplier; set { earnedXPMultiplier = value; OnEarnedXpMultiplierChanged(this, new OnEarnedXpMultiplierChangedEventArgs() { earnedXpMultiplier = earnedXPMultiplier }); } }
}