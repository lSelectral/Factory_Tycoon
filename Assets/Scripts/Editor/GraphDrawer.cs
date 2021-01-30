using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine.UIElements;

public class GraphDrawer : EditorWindow
{
    AnimationCurve curve = new AnimationCurve();
    AnimationCurve curve2 = new AnimationCurve();

    AnimationCurve foodCurve = new AnimationCurve();

    void Q()
    {
        var assets = Resources.LoadAll("AGES");

        assets = Resources.LoadAll("AGES");
        List<ScriptableCompound> compoundList = new List<ScriptableCompound>();
        List<ScriptableMine> mineList = new List<ScriptableMine>();

        ScriptableProductionBase[] units = assets.Where(a => (a as ScriptableProductionBase) != null).Cast<ScriptableProductionBase>().ToArray();

        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];

            if (asset as ScriptableObject != null)
            {
                var sc = asset as ScriptableObject;
                if (sc.GetType() == typeof(ScriptableMine))
                    mineList.Add(sc as ScriptableMine);
                else if (sc.GetType() == typeof(ScriptableCompound))
                    compoundList.Add(sc as ScriptableCompound);
            }
        }

        int j = 0;
        int k = 0;
        for (int i = 0; i < mineList.Count; i++)
        {
            //Debug.Log(mineList[i].pricePerProduct);
            curve.AddKey(j, mineList[i].pricePerProduct);
            j += 2;
        }

        for (int i = 0; i < compoundList.Count; i++)
        {
            curve2.AddKey(k, compoundList[i].pricePerProduct);
            k += 2;
        }

        var mineAndCompounds = mineList.Zip(compoundList, (mine, compound) => (m:mine, c: compound));
        int l = 0;
        foreach (var Q in mineAndCompounds)
        {
            Debug.Log(Q.m.foodAmount);
            if (Q.m.foodAmount > 0)
                foodCurve.AddKey(l, Q.m.foodAmount);
            if (Q.c.foodAmount > 0)
                foodCurve.AddKey(l + 2, Q.c.foodAmount);
            j += 2;
        }
    }

    [MenuItem("Window/Create Curve For Object")]
    static void Init()
    {
        GraphDrawer window = (GraphDrawer)EditorWindow.GetWindow(typeof(GraphDrawer));
        window.Show();
    }

    private void OnGUI()
    {
        Q();

        EditorGUILayout.LabelField("<color=white>Mine Price Per Products</color>", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.CurveField(curve, GUILayout.Height(210));
        EditorGUILayout.Space(25);
        EditorGUILayout.LabelField("<color=white>Compound Price Per Products</color>", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.CurveField(curve2, GUILayout.Height(210));

        EditorGUILayout.Space(25);
        EditorGUILayout.LabelField("<color=white>Food amounts</color>", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.CurveField(foodCurve, GUILayout.Height(210));
    }
}
