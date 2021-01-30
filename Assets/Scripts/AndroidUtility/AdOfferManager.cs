using UnityEngine;

public class AdOfferManager : Singleton<AdOfferManager>
{
    bool isMiningSpeedBoostActive;
    float remainedMSBoostTime;
    float miningSpeed;

    bool isMiningYieldBoostActive;
    float remainedMYBoostTime;
    int miningYield;

    bool isProductionSpeedBoostActive;
    float remainedPSBoostTime;
    float productionSpeed;

    bool isProductionYieldBoostActive;
    float remainedPYBoostTime;
    int productionYield;

    bool isProductionEfficiencyBoostActive;
    float remainedPEBoostTime;
    float productionEfficiency;

    void Update()
    {
        if (isMiningSpeedBoostActive)
        {
            if (remainedMSBoostTime > 0)
                remainedMSBoostTime -= Time.deltaTime;
            else
            {
                UpgradeSystem.Instance.MiningSpeedMultiplier -= miningSpeed;
                isMiningSpeedBoostActive = false;
            }
        }
        if (isMiningYieldBoostActive)
        {
            if (remainedMYBoostTime > 0)
                remainedMYBoostTime -= Time.deltaTime;
            else
            {
                UpgradeSystem.Instance.MiningYieldMultiplier -= miningYield;
                isMiningYieldBoostActive = false;
            }
        }
        if (isProductionSpeedBoostActive)
        {
            if (remainedPSBoostTime > 0)
                remainedPSBoostTime -= Time.deltaTime;
            else
            {
                UpgradeSystem.Instance.ProductionSpeedMultiplier -= productionSpeed;
                isProductionSpeedBoostActive = false;
            }
        }
        if (isProductionYieldBoostActive)
        {
            if (remainedPYBoostTime > 0)
                remainedPYBoostTime -= Time.deltaTime;
            else
            {
                isProductionYieldBoostActive = false;
                UpgradeSystem.Instance.ProductionYieldMultiplier -= productionYield;
            }
        }
        if (isProductionEfficiencyBoostActive)
        {
            if (remainedPEBoostTime > 0)
                remainedPEBoostTime -= Time.deltaTime;
            else
            {
                isProductionEfficiencyBoostActive = false;
                UpgradeSystem.Instance.ProductionEfficiencyMultiplier -= productionEfficiency;
            }
        }
    }

    void IncrementMiningSpeed(float speed, float time)
    {
        UpgradeSystem.Instance.MiningSpeedMultiplier += speed;
        remainedMSBoostTime += time;
        isMiningSpeedBoostActive = true;
        miningSpeed = speed;
    }
    void IncrementMiningYield(int yield, float time)
    {
        UpgradeSystem.Instance.MiningYieldMultiplier += yield;
        remainedMYBoostTime += time;
        isMiningYieldBoostActive = true;
        miningYield = yield;
    }
    void IncrementProductionSpeed(float speed, float time)
    {
        UpgradeSystem.Instance.ProductionSpeedMultiplier += speed;
        remainedPSBoostTime += time;
        isProductionSpeedBoostActive = true;
        productionSpeed = speed;
    }
    void IncrementProductionYield(int yield, float time)
    {
        UpgradeSystem.Instance.ProductionYieldMultiplier += yield;
        remainedPYBoostTime += time;
        isProductionYieldBoostActive = true;
        productionYield = yield;
    }
    void IncrementProductionEfficiency(float efficiency, float time)
    {
        UpgradeSystem.Instance.ProductionEfficiencyMultiplier += efficiency;
        remainedPEBoostTime += time;
        isProductionEfficiencyBoostActive = true;
        productionEfficiency = efficiency;
    }

    public void DEBUGMININGSPEED() { IncrementMiningSpeed(1, 5f); }
}