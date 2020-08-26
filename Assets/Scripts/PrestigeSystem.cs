using UnityEngine;
using System.Collections;

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

        // Reset game
    }
}
