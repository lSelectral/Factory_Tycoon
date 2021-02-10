using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// This class set and watch events that should occur in game.
/// Ex: ( Combat between AIs and Player, Auto Trading, Contract Creation when certain events occur. Like Leveling, unlocking new production unit or winning a war )
/// </summary>
public class GameEventManager : Singleton<GameEventManager>
{
    public Age currentAge;
    public int storyPart;


    
}