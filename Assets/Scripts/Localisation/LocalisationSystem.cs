using System.Collections.Generic;

public class LocalisationSystem
{
    public enum Language
    {
        English,
        Turkish,
    }

    public static Language currentLanguage = Language.English;

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
            case Language.English:
                localisedEn.TryGetValue(key, out value);
                break;
            case Language.Turkish:
                localisedTR.TryGetValue(key, out value);
                break;
        }
        return value;
    }

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
}
