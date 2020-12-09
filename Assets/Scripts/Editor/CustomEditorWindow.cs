using UnityEditor;
using UnityEngine;

public class CustomEditorWindow : EditorWindow
{
    [MenuItem("Window/DEBUGGER")]
    public static void ShowWindow()
    {
        GetWindow<CustomEditorWindow>("DebugWindow");
    }

    BaseResources resource = BaseResources._0_stone;

    string resourceAmountString = "Add Resource Amount";

    string xpString = "Enter XP value";
    string premiumCurrencyString = "Enter Premium Currency Amount";

    private void OnGUI()
    {
        GUILayout.BeginVertical("Add Resource");
        resourceAmountString = EditorGUILayout.TextField("Resource Amout", resourceAmountString);
        GUILayout.Label("Add Resource", EditorStyles.boldLabel);

        resource = (BaseResources)EditorGUI.EnumPopup(new Rect(5, 80, 380, 20), "Select Resource", resource);

        if (GUILayout.Button("+" + resourceAmountString + " " + ResourceManager.Instance.GetValidName(resource.ToString()) ))
        {
            ResourceManager.Instance.AddResource(resource, int.Parse(resourceAmountString));
        }
        GUILayout.EndVertical();

        xpString = EditorGUILayout.TextField("XP Amount", xpString);

        if (GUILayout.Button("Add XP"))
        {
            GameManager.Instance.AddXP(float.Parse(xpString));
        }

        GUILayout.Space(10);

        premiumCurrencyString = EditorGUILayout.TextField("Premium Currency Amount", premiumCurrencyString);
        if (GUILayout.Button("Add Premium Currency"))
        {
            ResourceManager.Instance.PremiumCurrency += float.Parse(premiumCurrencyString);
        }


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("SAVE"))
        {
            PrestigeSystem.Instance.ResetGame();
        }
        if (GUILayout.Button("LOAD"))
        {
            SaveSystem.Instance.Load();
        }
        GUILayout.EndHorizontal();

        //var resources = Enum.GetValues(typeof(BaseResources)).Cast<BaseResources>();
        //Vector2 scrollView = GUILayout.BeginScrollView(new Vector2(100, 100),false,true);
        

        //foreach (var resource in resources)
        //{
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label(ResourceManager.Instance.GetValidName(resource.ToString()));
        //    GUILayout.EndHorizontal();
        //}
        //GUILayout.EndScrollView();
    }
}

public enum OPTIONS
{
    Position = 0,
    Rotation = 1,
    Scale = 2,
}