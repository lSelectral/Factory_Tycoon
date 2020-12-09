using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

public class PrestigeSystem : Singleton<PrestigeSystem>
{
    private int prestigeLevel;

    public int PrestigeLevel
    {
        get { return prestigeLevel; }
        set { prestigeLevel = value; }
    }

    public void ResetGame()
    {
        prestigeLevel += 1;

        //ResourceManager.Instance.IronOre = 0;
        //ResourceManager.Instance.CopperOre = 0;
        //ResourceManager.Instance.SiliconOre = 0;
        //ResourceManager.Instance.Coal = 0;
        //ResourceManager.Instance.Oil = 0;
        //ResourceManager.Instance.GoldOre = 0;
        //ResourceManager.Instance.IronIngot = 0;
        //ResourceManager.Instance.CopperIngot = 0;
        //ResourceManager.Instance.SiliconWafer = 0;
        //ResourceManager.Instance.GoldIngot = 0;
        //ResourceManager.Instance.Wire = 0;
        //ResourceManager.Instance.HardenedPlate = 0;
        //ResourceManager.Instance.Rotor = 0;
        //ResourceManager.Instance.SteelIngot = 0;
        //ResourceManager.Instance.SteelPlate = 0;
        //ResourceManager.Instance.SteelTube = 0;
        //ResourceManager.Instance.SteelScrew = 0;
        //ResourceManager.Instance.SteelBeam = 0;
        //ResourceManager.Instance.MetalGrid = 0;
        //ResourceManager.Instance.ReactorComponent = 0;
        //ResourceManager.Instance.ThrusterComponent = 0;
        //ResourceManager.Instance.SolarCell = 0;
        //ResourceManager.Instance.SuperConductor = 0;
        //ResourceManager.Instance.PowerCell = 0;
        //ResourceManager.Instance.Rubber = 0;
        //ResourceManager.Instance.Stator = 0;
        //ResourceManager.Instance.Motor = 0;
        //ResourceManager.Instance.FiberOpticCable = 0;
        //ResourceManager.Instance.CircuitBoard = 0;
        //ResourceManager.Instance.IntegrationChip = 0;
        //ResourceManager.Instance.AiChip = 0;
        ResourceManager.Instance.TotalResource = 0;

        var enums = Enum.GetValues(typeof(BaseResources)).Cast<BaseResources>();
        foreach (BaseResources e in enums)
        {
            ResourceManager.Instance.resourceValueDict[e] = 0;
        }
    }
}
