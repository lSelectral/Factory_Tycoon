using System;

public static class Extensions
{
    /// <summary>
    /// Custom extension for getting next value for IEnumerables
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="src"></param>
    /// <returns></returns>
        public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));



        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }
}   