﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject LevelObject;
    public GameObject levelLock;
    private Image fillBar;
    private TextMeshProUGUI levelText;

    private float smoothCurrentXp;
    private float smoothCurrentXpVelocity;

    public class OnLevelUpEventArgs : EventArgs
    {
        public int currentLevel;
    }

    public event EventHandler<OnLevelUpEventArgs> OnLevelUp;

    private int currentLevel;
    private float currentXP;
    private long requiredXPForNextLevel;

    public long RequiredXPforNextLevel
    {
        get
        {
            return requiredXPForNextLevel;
        }
        set
        {
            requiredXPForNextLevel = value;
        }
    }


    public int CurrentLevel
    {
        get
        {
            return currentLevel;
        }
        set
        {
            if (value <= 0)
                currentLevel = 0;
            else
                this.currentLevel = value;
        }
    }


    public float CurrentXP
    {
        get
        {
            return currentXP;
        }
        set
        {
            this.currentXP = value;
            while (currentXP >= requiredXPForNextLevel)
            {
                CurrentLevel++;
                currentXP -= requiredXPForNextLevel;
                CalculateRequiredXPforNextLevel();
                OnLevelUp(this, new OnLevelUpEventArgs { currentLevel = currentLevel });
                PopupManager.Instance.PopupPanel("You reached Level " + currentLevel.ToString(), "With unlocking new levels you will be able to build new buildings");
                levelText.text = "LVL " + currentLevel.ToString();
            }
        }
    }

    private void Awake()
    {
        Input.multiTouchEnabled = false;
        requiredXPForNextLevel = 100;
        fillBar = LevelObject.transform.Find("Outline").Find("Fill").GetComponent<Image>();
        levelText = LevelObject.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        levelText.text = "LVL " + currentLevel.ToString() ;
    }

    private void Update()
    {
        smoothCurrentXp = Mathf.SmoothDamp(smoothCurrentXp, currentXP, ref smoothCurrentXpVelocity, .5f);
        fillBar.fillAmount = smoothCurrentXp / requiredXPForNextLevel;
    }

    public void AddXP(float value) { CurrentXP += value * UpgradeSystem.Instance.EarnedXPMultiplier; }

    public void ADDXPDEBUG() { CurrentXP += 100; }

    private void CalculateRequiredXPforNextLevel()
    {
        requiredXPForNextLevel = (long)(requiredXPForNextLevel * Mathf.Exp(1.063f));
    }
}
