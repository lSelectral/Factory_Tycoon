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

    }
}
