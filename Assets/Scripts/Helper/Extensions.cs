using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T Next<T>(this T src) where T : Enum
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }

    public static int GetNextIndex<T>(this T[] item, int currentIndex) where T : class
    {
        return (item.Length == currentIndex + 1) ? 0 : currentIndex + 1;
    }

    public static int GetPreviousIndex<T>(this T[] item, int currentIndex) where T : class
    {
        return (currentIndex == 0) ? item.Length - 1 : currentIndex-1;
    }

    public static void DebugList<T>(List<T> listToDebug) where T : class
    {
        string debugText = listToDebug.ToString() + " count: " + listToDebug.Count + "\n";
        for (int i = 0; i < listToDebug.Count; i++)
        {
            debugText += listToDebug[i] + "\n";
        }
        Debug.Log(debugText);
    }

}

static class StringExtensions
{
    /// <summary>
    /// Split string to parts at given intervals.
    /// </summary>
    /// <param name="s">Input string</param>
    /// <param name="partLength">Part Interval</param>
    /// <returns>An array of string that splitted with given intervals</returns>
    public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
    {
        if (s == null)
            throw new ArgumentNullException(nameof(s));
        if (partLength <= 0)
            throw new ArgumentException("Part length has to be positive.", nameof(partLength));

        for (var i = 0; i < s.Length; i += partLength)
            yield return s.Substring(i, Math.Min(partLength, s.Length - i));
    }

}

public static class RectTransformExtensions
{
    public static void SetLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }
}