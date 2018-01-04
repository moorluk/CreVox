using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CreVox
{

	public static class EditorUtils
	{
		public static List<T> GetListFromEnum<T>()
		{
			List<T> enumList = new List<T>();
			System.Array enums = System.Enum.GetValues(typeof(T));
			foreach (T e in enums) {
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
			string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { path });
			for (int i = 0; i < guids.Length; i++) {
				assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				asset = AssetDatabase.LoadAssetAtPath(assetPath,
					typeof(GameObject)) as GameObject;
				tmp = asset.GetComponent<T>();
				if (tmp != null) {
					assetList.Add(tmp);
				}
			}
			return assetList;
		}

		public static bool ChkEventCallback(EditorApplication.CallbackFunction _event, string _mathodName) {
			bool result = false;
			if (_event == null) {
				Debug.Log(_event + " not exist...");
				return result;
			}

			for (int i = _event.GetInvocationList ().Length - 1; i > 0; i--) {
				Debug.LogWarning (_event.GetInvocationList () [i].Method + " : " + i + "/" + _event.GetInvocationList ().Length);
				if (_event.GetInvocationList () [i].Method.ToString ().Contains (_mathodName))
					result = true;
			}
			return result;
		}
	}
}