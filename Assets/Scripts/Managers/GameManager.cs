using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// 
/// TODO GAME MECHANIC ADDITION
/// 
/// More advanced products quickly make earlier products obselete because of difference of the price.
/// Make contract, quests and addition. (War need, Gathering tool)
/// 
/// 
/// </summary>


public class GameManager : Singleton<GameManager>
{
    public Age currentAge = Age._0_StoneAge;
    [SerializeField] int requiredXPforFirstLevel;
    [SerializeField] private GameObject levelObject;
    public GameObject levelLock;
    private Image fillBar;
    private TextMeshProUGUI levelText;

    /// <summary>
    /// This variable holds value for panel that player currently see.
    /// All panels in game active all the time because production is depend on it.
    /// We don't want to render animations for lightweight performance.
    /// We will activate animations for only active panel.
    /// </summary>
    [SerializeField] private GameObject visiblePanelForPlayer;

    /// <summary>
    /// This panel is sub production panel. If Player is currently looking at production panel
    /// get which sub panel
    /// </summary>
    [SerializeField] private GameObject visibleSubPanelForPlayer;

    public class OnLevelUpEventArgs : EventArgs
    {
        public int currentLevel;
    }

    public event EventHandler<OnLevelUpEventArgs> OnLevelUp;

    public class OnMainPanelChangedEventArgs : EventArgs
    {
        public GameObject panel;
    }

    public event EventHandler<OnMainPanelChangedEventArgs> OnMainPanelChanged;

    public class OnSubPanelChangedEventArgs : EventArgs
    {
        public GameObject panel;
    }

    public event EventHandler<OnSubPanelChangedEventArgs> OnSubPanelChanged;

    private int currentLevel;
    private float currentXP;
    [SerializeField] long requiredXPForNextLevel;

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
            levelText.text = "LVL " + currentLevel.ToString();
        }
    }

    float smoothCurrentXP;
    float startTimeXP;
    float oldXPValue;
    public float CurrentXP
    {
        get
        {
            return currentXP;
        }
        set
        {
            startTimeXP = Time.time;
            oldXPValue = currentXP;
            this.currentXP = value;
            while (currentXP >= requiredXPForNextLevel)
            {
                CurrentLevel++;
                currentXP -= requiredXPForNextLevel;
                CalculateRequiredXPforNextLevel();
                OnLevelUp(this, new OnLevelUpEventArgs { currentLevel = currentLevel });
                if (!SaveSystem.Instance.isLoading)
                    PopupManager.Instance.PopupPanel("You reached Level " + currentLevel.ToString(), "With unlocking new levels you will be able to produce new technologies");
                levelText.text = "LVL " + currentLevel.ToString();
            }
        }
    }

    public GameObject VisibleSubPanelForPlayer { 
        get => visibleSubPanelForPlayer; 
        set
        {
            visibleSubPanelForPlayer = value;
            OnSubPanelChanged?.Invoke(this, new OnSubPanelChangedEventArgs() { panel = visibleSubPanelForPlayer });
        }
    }
    public GameObject VisiblePanelForPlayer { 
        get => visiblePanelForPlayer; 
        set 
        { 
            visiblePanelForPlayer = value;
            OnMainPanelChanged?.Invoke(this, new OnMainPanelChangedEventArgs() { panel = visiblePanelForPlayer });
        } 
    }

    private void Awake()
    {
        #if UNITY_ANDROID
            LocalisationSystem.currentLanguage = Application.systemLanguage;
        #endif

        visiblePanelForPlayer = ProductionManager.Instance.mainPanel.transform.parent.gameObject;
        visibleSubPanelForPlayer = ProductionManager.Instance.mainPanel.transform.Find("_0_StoneAge").gameObject;

        Input.multiTouchEnabled = false;
        requiredXPForNextLevel = requiredXPforFirstLevel;
        fillBar = levelObject.transform.Find("Outline").Find("Fill").GetComponent<Image>();
        levelText = levelObject.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        CurrentLevel = 1;
    }

    private void Start()
    {
        levelText.text = "LVL " + currentLevel.ToString() ;
    }

    private void Update()
    {
        smoothCurrentXP = Mathf.SmoothStep(oldXPValue, currentXP, Time.time - startTimeXP);
        fillBar.fillAmount = smoothCurrentXP / requiredXPForNextLevel;
    }

    public void AddXP(float value) { CurrentXP += value * UpgradeSystem.Instance.EarnedXPMultiplier; }

    public void ADDXPDEBUG() { CurrentXP += 100; }

    private void CalculateRequiredXPforNextLevel()
    {
        requiredXPForNextLevel = (long)(requiredXPForNextLevel * Mathf.Exp(1.063f));
    }
}
