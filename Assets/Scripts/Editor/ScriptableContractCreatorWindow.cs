using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ScriptableContractCreatorWindow : EditorWindow
{
    [MenuItem("Window/Contract Creator")]
    public static void ShowWindow()
    {
        GetWindow<ScriptableContractCreatorWindow>("Contract Creator");
    }

    public string contractName;
    public string description;

    public long contractReward;
    public ContractRewardType contractRewardType;
    public ScriptableCompound[] compoundsToUnlock;
    public ScriptableMine[] minesToUnlock;
    public BaseResources[] requiredResources;
    public int[] requiredResourceAmounts;

    public int unlockLevel;

    public Sprite icon;
    public string rewardPanelHeader = "<color=red>Congrulations</color>";
    public string rewardPanelDescription;
    public string pageNameToGo;
    public long xpReward;


    void OnGUI()
    {
        // "target" can be any class derrived from ScriptableObject 
        // (could be EditorWindow, MonoBehaviour, etc)
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);

        SerializedProperty cN = so.FindProperty("contractName");
        SerializedProperty dT = so.FindProperty("description");
        SerializedProperty uL = so.FindProperty("unlockLevel");
        SerializedProperty ico = so.FindProperty("icon");
        SerializedProperty rPH = so.FindProperty("rewardPanelHeader");
        SerializedProperty rPD = so.FindProperty("rewardPanelDescription");
        SerializedProperty pNTG = so.FindProperty("pageNameToGo");
        SerializedProperty xpRew = so.FindProperty("xpReward");

        SerializedProperty rR = so.FindProperty("requiredResources");
        SerializedProperty rRA = so.FindProperty("requiredResourceAmounts");
        SerializedProperty cR = so.FindProperty("contractReward");
        SerializedProperty cTU = so.FindProperty("compoundsToUnlock");
        SerializedProperty mTU = so.FindProperty("minesToUnlock");
        SerializedProperty cRT = so.FindProperty("contractRewardType");

        EditorGUILayout.PropertyField(cN, true);
        EditorGUILayout.PropertyField(dT, true);

        EditorGUILayout.PropertyField(rR, true);
        EditorGUILayout.PropertyField(rRA, true);
        EditorGUILayout.PropertyField(uL, true);
        EditorGUILayout.PropertyField(ico,true);
        EditorGUILayout.PropertyField(rPH,true);
        EditorGUILayout.PropertyField(rPD,true);
        EditorGUILayout.PropertyField(pNTG,true);
        EditorGUILayout.PropertyField(xpRew,true);

        EditorGUILayout.PropertyField(cRT, true);

        //if (contractRewardType == ContractRewardType.unlockMine)
        //    EditorGUILayout.PropertyField(mTU, true);
        //else if (contractRewardType == ContractRewardType.unlockCompound)
        //    EditorGUILayout.PropertyField(cTU, true);
        //else if (contractRewardType != ContractRewardType.automate)
        //    EditorGUILayout.PropertyField(cR, true);

        so.ApplyModifiedProperties();
    }

    private void OnValidate()
    {
        if (requiredResources.Length != requiredResourceAmounts.Length)
        {
            Debug.LogError("Required Resources and respective amounts array must be on same size");
        }

        var isPageNameToGoValid = false;
        for (int i = 0; i < ProductionManager.Instance.mainPanel.transform.childCount; i++)
        {
            var c = ProductionManager.Instance.mainPanel.transform.GetChild(i);
            if (c.name == pageNameToGo)
            {
                isPageNameToGoValid = true;
                break;
            }
            else
                isPageNameToGoValid = false;
        }
        if (!isPageNameToGoValid)
            Debug.LogError("Given page name to is invalid");
    }
}
