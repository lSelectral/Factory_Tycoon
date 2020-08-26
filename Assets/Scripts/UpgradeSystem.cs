using UnityEngine;

public class UpgradeSystem : Singleton<UpgradeSystem>
{
    public float miningSpeedMultiplier = 1;
    public float miningYieldMultiplier = 1;

    public float productionSpeedMultiplier = 1;
    public float productionYieldMultiplier = 1;
    public float productionEfficiencyMultiplier = 1;

    public float earnedCoinMultiplier = 1;
    public float earnedXPMultiplier = 1;
}