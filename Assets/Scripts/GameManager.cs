using System.Collections;
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

            if (currentXP >= requiredXPForNextLevel)
            {
                CurrentLevel += 1;
                currentXP -= requiredXPForNextLevel;
                CalculateRequiredXPforNextLevel();
                OnLevelUp(this, new OnLevelUpEventArgs { currentLevel = currentLevel });
                PopupManager.Instance.PopupPanel("You reached Level " + currentLevel.ToString(), "With unlocking new levels you will be able to build new buildings");
            }
            levelText.text = "LVL " + currentLevel.ToString();
        }
    }

    private void Awake()
    {
        requiredXPForNextLevel = 500;
        fillBar = LevelObject.transform.Find("Outline").Find("Fill").GetComponent<Image>();
        levelText = LevelObject.transform.Find("Image").Find("LevelText").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        levelText.text = "LVL " + currentLevel.ToString() ;
    }

    private void Update()
    {
        smoothCurrentXp = Mathf.SmoothDamp(smoothCurrentXp, (float)currentXP, ref smoothCurrentXpVelocity, .5f);
        fillBar.fillAmount = smoothCurrentXp / requiredXPForNextLevel;
    }

    public void AddXP(float value) { CurrentXP += value; }

    public void ADDXPDEBUG() { CurrentXP += 100; }

    private void CalculateRequiredXPforNextLevel()
    {
        requiredXPForNextLevel = Mathf.CeilToInt(requiredXPForNextLevel * Mathf.PI);
    }
}
