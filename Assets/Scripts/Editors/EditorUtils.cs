using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class EditorUtils
{
    public static List<T> GetListFromEnum<T>()
    {
        List<T> enumList = new List<T>();
        System.Array enums = System.Enum.GetValues(typeof(T));
        foreach (T e in enums)
        {
            enumList.Add(e);
        }
        return enumList;
    }
}
