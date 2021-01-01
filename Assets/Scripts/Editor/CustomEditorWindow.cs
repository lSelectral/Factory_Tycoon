using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
    string currencyString = "Enter Currency Amount";

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

        currencyString = EditorGUILayout.TextField("Currency Amount", currencyString);
        if (GUILayout.Button("Add Currency"))
        {
            ResourceManager.Instance.Currency += float.Parse(currencyString);
        }

        // SAVE and LOAD
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

        // Automatically create automation contracts
        if (GUILayout.Button("Create Automation Contracts"))
        {
            foreach (ContractBase contract in ContractManager.Instance.CreateAutomationContracts())
            {
                AssetDatabase.CreateAsset(contract, "Assets/Resources/Contracts/Automation_Contracts/" + contract.contractName + "_automation" + ".asset");
                AssetDatabase.SaveAssets();
            }
        }

        if (GUILayout.Button("Create Multiple UI Image"))
        {
            CreateUiImage();
        }

        if (GUILayout.Button("Set Pixel Size of Maps"))
            SetPixelSizeAmount();
    }

    void CreateUiImage()
    {
        var assets = Resources.LoadAll("MAP_2");

        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i] as Sprite;
            if (asset != null && asset.name != "Katman_1")
            {
                var obj = new GameObject("Part_" + i);
                obj.transform.SetParent(MapManager.Instance.mapTransform);
                obj.AddComponent<Image>();
                obj.GetComponent<Image>().sprite = asset;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                obj.transform.localScale = Vector3.one;
            }
        }
    }


    void SetPixelSizeAmount()
    {
        int pixelSize = 0;
        for (int i = 0; i < MapManager.Instance.mapTransform.childCount; i++)
        {
            Transform m = MapManager.Instance.mapTransform.GetChild(i);

            var pixels = m.GetComponent<Image>().sprite.texture.GetPixels32();
            foreach (var pixel in pixels)
            {
                if (pixel.a > 1)
                    pixelSize++;
            }
            m.GetComponent<Map_Part>().pixelSize = pixelSize;
            pixelSize = 0;
        }
    }
}