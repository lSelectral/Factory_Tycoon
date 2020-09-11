using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mines and factories prefab instantiater class
/// </summary>
public class ProductionManager : Singleton<ProductionManager>
{
    public GameObject mineInfoPrefab;
    public ScriptableMine[] mines;

    public ScriptableCompound[] ingots;

    public ScriptableCompound[] primitives;
    public ScriptableCompound[] advanced;
    public ScriptableCompound[] T1;
    public ScriptableCompound[] T2;
    public ScriptableCompound[] T3;

    public GameObject mainPanel;

    public GameObject minePrefab;
    public GameObject compoundPrefab;

    public List<GameObject> instantiatedMines;
    public List<GameObject> instantiatedCompounds;

    public List<GameObject> instantiatedIngots;
    public List<GameObject> instantiatedPrimitives;
    public List<GameObject> instantiatedAdvanceds;
    public List<GameObject> instantiatedT1;
    public List<GameObject> instantiatedT2;
    public List<GameObject> instantiatedT3;

    public Sprite smelterSprite;
    public Sprite assemblerSprite;
    public Sprite manufacturerSprite;

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
            _mine.GetComponent<Mine_Btn>().scriptableMine = mines[i];
            instantiatedMines.Add(_mine);
        }

        for (int i = 0; i < ingots.Length; i++)
        {
            var _ingots = Instantiate(compoundPrefab, mainPanel.transform.Find("Ingots").GetChild(0));
            _ingots.GetComponent<Compounds>().scriptableCompound = ingots[i];
            _ingots.transform.Find("Building").GetComponent<Image>().sprite = smelterSprite;
            instantiatedIngots.Add(_ingots);
            instantiatedCompounds.Add(_ingots);
        }

        for (int i = 0; i < primitives.Length; i++)
        {
            var _primitive = Instantiate(compoundPrefab, mainPanel.transform.Find("Primitives").GetChild(0));
            _primitive.GetComponent<Compounds>().scriptableCompound = primitives[i];
            _primitive.transform.Find("Building").GetComponent<Image>().sprite = assemblerSprite;
            instantiatedPrimitives.Add(_primitive);
            instantiatedCompounds.Add(_primitive);
        }

        for (int i = 0; i < advanced.Length; i++)
        {
            var _advanced = Instantiate(compoundPrefab, mainPanel.transform.Find("Secondaries").GetChild(0));
            _advanced.GetComponent<Compounds>().scriptableCompound = advanced[i];
            _advanced.transform.Find("Building").GetComponent<Image>().sprite = assemblerSprite;
            instantiatedAdvanceds.Add(_advanced);
            instantiatedCompounds.Add(_advanced);
        }

        for (int i = 0; i < T1.Length; i++)
        {
            var _t1 = Instantiate(compoundPrefab, mainPanel.transform.Find("Advanced").GetChild(0));
            _t1.GetComponent<Compounds>().scriptableCompound = T1[i];
            _t1.transform.Find("Building").GetComponent<Image>().sprite = manufacturerSprite;
            instantiatedT1.Add(_t1);
            instantiatedCompounds.Add(_t1);
        }

        for (int i = 0; i < T2.Length; i++)
        {
            var _t2 = Instantiate(compoundPrefab, mainPanel.transform.Find("ADQ").GetChild(0));
            _t2.GetComponent<Compounds>().scriptableCompound = T2[i];
            _t2.transform.Find("Building").GetComponent<Image>().sprite = manufacturerSprite;
            instantiatedT2.Add(_t2);
            instantiatedCompounds.Add(_t2);
        }

        //for (int i = 0; i < T3.Length; i++)
        //{
        //    var _t3 = Instantiate(compoundPrefabComplex, mainPanel.transform.Find("").GetChild(0));
        //    _t3.GetComponent<Compounds>().scriptableCompound = T3[i];
        //    instantiatedT3.Add(_t3);

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
    production = 1,
    /// <summary>
    /// Sell produced products
    /// </summary>
    sell = 2,
}
