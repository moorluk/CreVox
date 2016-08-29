using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CreVox{

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

    public static List<T> GetAssetsWithScript<T>(string path) where T : MonoBehaviour
    {
        T tmp;
        string assetPath;
        GameObject asset;
        List<T> assetList = new List<T>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new
                                                   string[] { path });
        for (int i = 0; i < guids.Length; i++)
        {
            assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            asset = AssetDatabase.LoadAssetAtPath(assetPath,
                                                   typeof(GameObject)) as GameObject;
            tmp = asset.GetComponent<T>();
            if (tmp != null)
            {
                assetList.Add(tmp);
            }
        }
        return assetList;
    }
}
}