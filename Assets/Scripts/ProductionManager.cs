using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mines and factories prefab instantiater class
/// </summary>
public class ProductionManager : Singleton<ProductionManager>
{
    public ScriptableMine[] minesForAges;
    public ScriptableCompound[] compoundsForAges;

    public List<ScriptableMine> mineList;
    public List<ScriptableCompound> compoundList;

    public GameObject mainPanel;
    // Hide objects on screen but still work on background
    public Transform temporaryMovementPanel;
    public Transform contentHolderPrefab;

    public GameObject collapsablePanelPrefab;

    public GameObject minePrefab;
    public GameObject compoundPrefab;

    public List<GameObject> instantiatedMines;
    public List<GameObject> instantiatedCompounds;

    public Sprite smelterSprite;
    public Sprite assemblerSprite;
    public Sprite manufacturerSprite;

    /// <summary>
    /// Instantiate mine prefab and create expand and collapse button for tier groups
    /// </summary>
    /// <param name="mine"></param>
    void InstantiateMine(ScriptableMine mine)
    {
        var _contentHolder = mainPanel.transform.Find(mine.ageBelongsTo.ToString()).GetChild(0);

        Transform tierSeperatedContainer = null;


        if (_contentHolder.Find(mine.tier.ToString()) == null)
        {
            var collapsablePanel = Instantiate(collapsablePanelPrefab, _contentHolder);
            collapsablePanel.GetComponentInChildren<TextMeshProUGUI>().text = "Tier " + mine.tier.ToString().Substring(mine.tier.ToString().Length - 1);
            collapsablePanel.name = mine.tier.ToString();

            tierSeperatedContainer = Instantiate(contentHolderPrefab, _contentHolder).transform;
            tierSeperatedContainer.gameObject.name = "container_" + mine.tier;
        }
        else
            tierSeperatedContainer = _contentHolder.transform.GetChild(_contentHolder.Find(mine.tier.ToString()).GetSiblingIndex() + 1);

        var _mine = Instantiate(minePrefab, tierSeperatedContainer);
        _mine.GetComponent<Mine_Btn>().scriptableMine = mine;
        instantiatedMines.Add(_mine);

        var expandBtn = _contentHolder.Find(mine.tier.ToString()).Find("Image").Find("ExpandBtn").GetComponent<Button>();
        var collapseBtn = _contentHolder.Find(mine.tier.ToString()).Find("Image").Find("CollapseBtn").GetComponent<Button>();

        expandBtn.onClick.RemoveAllListeners();
        expandBtn.onClick.AddListener(() => 
        { 
            temporaryMovementPanel.Find("container_"+mine.tier).SetParent(_contentHolder);
            tierSeperatedContainer.SetSiblingIndex(_contentHolder.Find(mine.tier.ToString()).GetSiblingIndex()+1);
        });

        collapseBtn.onClick.RemoveAllListeners();
        collapseBtn.onClick.AddListener(() => 
        {
            _contentHolder.transform.Find("container_" + mine.tier).SetParent(temporaryMovementPanel);
        });
    }

    void InstantiateCompound(ScriptableCompound compound)
    {
        var _contentHolder = mainPanel.transform.Find(compound.ageBelongsTo.ToString()).GetChild(0);

        Transform tierSeperatedContainer = null;


        if (_contentHolder.Find(compound.tier.ToString()) == null)
        {
            var collapsablePanel = Instantiate(collapsablePanelPrefab, _contentHolder);
            collapsablePanel.GetComponentInChildren<TextMeshProUGUI>().text = "Tier " + compound.tier.ToString().Substring(compound.tier.ToString().Length - 1);
            collapsablePanel.name = compound.tier.ToString();

            tierSeperatedContainer = Instantiate(contentHolderPrefab, _contentHolder).transform;
            tierSeperatedContainer.gameObject.name = "container_" + compound.tier;
        }
        else
            tierSeperatedContainer = _contentHolder.transform.GetChild(_contentHolder.Find(compound.tier.ToString()).GetSiblingIndex() + 1);

        var _compound = Instantiate(compoundPrefab, tierSeperatedContainer);
        _compound.GetComponent<Compounds>().scriptableCompound = compound;
        instantiatedCompounds.Add(_compound);

        var expandBtn = _contentHolder.Find(compound.tier.ToString()).Find("Image").Find("ExpandBtn").GetComponent<Button>();
        var collapseBtn = _contentHolder.Find(compound.tier.ToString()).Find("Image").Find("CollapseBtn").GetComponent<Button>();

        expandBtn.onClick.RemoveAllListeners();
        expandBtn.onClick.AddListener(() =>
        {
            temporaryMovementPanel.Find("container_" + compound.tier).SetParent(_contentHolder);
            tierSeperatedContainer.SetSiblingIndex(_contentHolder.Find(compound.tier.ToString()).GetSiblingIndex() + 1);
        });

        collapseBtn.onClick.RemoveAllListeners();
        collapseBtn.onClick.AddListener(() =>
        {
            _contentHolder.transform.Find("container_" + compound.tier).SetParent(temporaryMovementPanel);
        });
    }

    private void Awake()
    {
        mineList = new List<ScriptableMine>();
        compoundList = new List<ScriptableCompound>();

        var assets = Resources.LoadAll("AGES");

        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];
            
            if (asset as ScriptableObject != null)
            {
                var sc = asset as ScriptableObject;
                if (sc.GetType() == typeof(ScriptableMine) && (sc as ScriptableMine).ageBelongsTo == Age._0_stoneAge)
                    mineList.Add(sc as ScriptableMine);
                else if (sc.GetType() == typeof(ScriptableCompound) && (sc as ScriptableCompound).ageBelongsTo == Age._0_stoneAge)
                    compoundList.Add(sc as ScriptableCompound);
            }
        }
        //Debug.Log("Mine count is: " + mineList.Count);
        //Debug.Log("Compound count is: " + compoundList.Count);


        for (int i = 0; i < mineList.Count; i++)
        {
            InstantiateMine(mineList[i]);
        }

        for (int j = 0; j < compoundList.Count; j++)
        {
            InstantiateCompound(compoundList[j]);
        }
    }
}

/// <summary>
/// Working Mode for Mines and Factories
/// </summary>
public enum MineWorkingMode
{
    /// <summary>
    /// Use produced materials for further and advanced production
    /// </summary>
    production = 1,
    /// <summary>
    /// Sell produced products
    /// </summary>
    sell = 2,
}

/// <summary>
/// Working Mode for Mines and Factories
/// </summary>
public enum CompoundWorkingMode
{
    /// <summary>
    /// Use produced materials for further and advanced production
    /// </summary>
    production = 1,
    /// <summary>
    /// Sell produced products
    /// </summary>
    sell = 2,
    /// <summary>
    /// Stop the production for recipes that require another resource
    /// so this recipe will not consume any resource if player not wants it.
    /// </summary>
    stopProduction = 3,
}

public enum Age
{
    _0_stoneAge = 0,
    _1_ironAge = 1,
    _2_goldenAge = 2,
    _3_warAge = 3,
    _4_factoryAge = 4,
    _5_spaceAge = 5,
    _6_modernAge = 6,
    _7_roboticAge = 7,
    _8_intergalacticAge = 8,
    _9_galacticWarAge = 9,
}

public enum Tier
{
    tier1,
    tier2,
    tier3,
    tier4,
}