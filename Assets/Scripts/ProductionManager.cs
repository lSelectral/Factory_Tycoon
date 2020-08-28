using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mines and factories prefab instantiater class
/// </summary>
public class ProductionManager : Singleton<ProductionManager>
{
    public ScriptableMine[] mines;

    public ScriptableCompound[] ingots;

    public ScriptableCompound[] primitives;
    public ScriptableCompound[] advanced;
    public ScriptableCompound[] T1;
    public ScriptableCompound[] T2;
    public ScriptableCompound[] T3;

    public GameObject mainPanel;

    public GameObject minePrefab;
    public GameObject compoundPrefabIngot;
    public GameObject compoundPrefabPrimitive;
    public GameObject compoundPrefabComplex;

    public List<GameObject> instantiatedMines;
    public List<GameObject> instantiatedCompounds;

    const string minePanel = "Ores";
    const string ingotPanel = "Ingots";
    const string primitivesPanel = "Primitives";
    const string secondariesPanel = "Secondaries";
    const string AdvancedPanel = "Advanced";

    private void Awake()
    {
        for (int i = 0; i < mines.Length; i++)
        {
            var _mine = Instantiate(minePrefab, mainPanel.transform.Find("Ores").GetChild(0));
            _mine.GetComponent<Mine_Btn>().mine = mines[i];
            instantiatedMines.Add(_mine);
        }

        for (int i = 0; i < ingots.Length; i++)
        {
            var _ingots = Instantiate(compoundPrefabIngot, mainPanel.transform.Find("Ingots").GetChild(0));
            _ingots.GetComponent<Compounds>().scriptableCompound = ingots[i];
            instantiatedCompounds.Add(_ingots);
        }

        for (int i = 0; i < primitives.Length; i++)
        {
            var _primitive = Instantiate(compoundPrefabPrimitive, mainPanel.transform.Find("Primitives").GetChild(0));
            _primitive.GetComponent<Compounds>().scriptableCompound = primitives[i];
            instantiatedCompounds.Add(_primitive);
        }

        for (int i = 0; i < advanced.Length; i++)
        {
            var _advanced = Instantiate(compoundPrefabPrimitive, mainPanel.transform.Find("Secondaries").GetChild(0));
            _advanced.GetComponent<Compounds>().scriptableCompound = advanced[i];
            instantiatedCompounds.Add(_advanced);
        }

        for (int i = 0; i < T1.Length; i++)
        {
            var _t1 = Instantiate(compoundPrefabComplex, mainPanel.transform.Find("Advanced").GetChild(0));
            _t1.GetComponent<Compounds>().scriptableCompound = T1[i];
        }

        for (int i = 0; i < T2.Length; i++)
        {
            var _t2 = Instantiate(compoundPrefabComplex, mainPanel.transform.Find("ADQ").GetChild(0));
            _t2.GetComponent<Compounds>().scriptableCompound = T2[i];
        }

        //for (int i = 0; i < T3.Length; i++)
        //{
        //    var _t3 = Instantiate(compoundPrefabComplex, mainPanel.transform.Find("").GetChild(0));
        //    _t3.GetComponent<Compounds>().scriptableCompound = T3[i];
        //}
    }
}

/// <summary>
/// Working Mode for Mines and Factories
/// </summary>
public enum WorkingMode
{
    /// <summary>
    /// Use produced materials for further and advanced production
    /// </summary>
    production,
    /// <summary>
    /// Sell produced products
    /// </summary>
    sell,
}
