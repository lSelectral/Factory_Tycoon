using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

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

}   