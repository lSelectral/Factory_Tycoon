using System.Collections.Generic;
using UnityEngine;

public class LocalisationSystem
{
    public static SystemLanguage currentLanguage = SystemLanguage.English;

    public static Dictionary<string, string> localisedEn;
    public static Dictionary<string, string> localisedTR;

    public static bool isInit;

    public static CSVLoader csvLoader;

    public static void Init()
    {
        csvLoader = new CSVLoader();
        csvLoader.LoadCSV();

        UpdateDictionaries();

        isInit = true;
    }

    public static Dictionary<string,string> GetDictionaryForEditor()
    {
        if (!isInit) Init();
        return localisedEn;
    }

    public static void UpdateDictionaries()
    {
        localisedEn = csvLoader.GetDictionaryValues("en");
        localisedTR = csvLoader.GetDictionaryValues("tr");
    }

    public static string GetLocalisedValue(string key)
    {
        if (!isInit) Init();

        string value = key;

        switch (currentLanguage)
        {
            case SystemLanguage.English:
                localisedEn.TryGetValue(key, out value);
                break;
            case SystemLanguage.Turkish:
                localisedTR.TryGetValue(key, out value);
                break;
            default:
                localisedEn.TryGetValue(key, out value);
                break;
        }
        return value;
    }

#if UNITY_EDITOR
    public static void Add(string key, string value)
    {
        if (GetLocalisedValue(key) == string.Empty)
        {
            if (value.Contains("\""))
            {
                value.Replace('"', '\"');
            }
            if (csvLoader == null) csvLoader = new CSVLoader();

            csvLoader.LoadCSV();
            csvLoader.Add(key, value);
            csvLoader.LoadCSV();
            UpdateDictionaries();
        }
    }

    public static void Replace(string key, string value)
    {
        if (GetLocalisedValue(key) != string.Empty)
        {
            if (value.Contains("\""))
            {
                value.Replace('"', '\"');
            }
            if (csvLoader == null) csvLoader = new CSVLoader();

            csvLoader.LoadCSV();
            csvLoader.Edit(key, value);
            csvLoader.LoadCSV();
            UpdateDictionaries();
        }
    }

    public static void Remove(string key)
    {
        if (csvLoader == null) csvLoader = new CSVLoader();

        csvLoader.LoadCSV();
        csvLoader.Remove(key);
        csvLoader.LoadCSV();
        UpdateDictionaries();
    }
#endif
}
