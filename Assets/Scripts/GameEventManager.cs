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




/*
// Abstract mark indicates, you can't assign this class in editor.
// You can only use its child. This is block prior errors.
public abstract class BulletType : MonoBehaviour
{
    // Protected is less stric than private, more stric than public
    // Protected variables only accesible to this class and its child.
    protected string name;
    protected int bulletDamage;
    protected int bulletSpeed;

    protected virtual void Start()
    {

    }

    // Virtual means you can override this method in child classes
    protected virtual void Damage() { }

    protected virtual void PlaySound() { }

    protected virtual void ShowEffect() { }
}

// This class inherits from BulletType class.
public class FireBullet : BulletType
{
    // If you want to completely ignore parent class variable to by adding new keyword.
    new int bulletSpeed = 3;

    protected override void Start()
    {
        // when overriding a method automatically adds this method.
        // base means parent class. with base you can access parent methods.
        base.Start();

        // If you remove base method, parent method won't be called in this class.

        // Here I accessed parent class variable and set its value.
        // This doesn't effect parent or other child classes. That's the beauty.
        name = "Fire Bullet";
    }

    void StopSound() 
    { 
    }

    protected override void PlaySound()
    {
        // Sound played from parent.
        base.PlaySound();

        // You can add your own variable and methods in parent method.
        StopSound();
    }
}

// FireBullet inherited from BulletType, and LavaBullet inherited from FireBullet.
// You can do this as much as you want.
public class LavaBullet : FireBullet
{
    protected override void PlaySound()
    {
        // Here base will be FireBullet
        base.PlaySound();
    }
}

public class IceBullet : BulletType
{
    // Add as much as you thing.
}

public class Player
{
    public BulletType currentBulletType;
}
*/