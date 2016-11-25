using UnityEngine;
using System.IO;
using UnityEditor;

namespace CreVox
{

	public static class MenuItems
	{

		[MenuItem("Tools/Level Creator/Show Palette _&p")]
		private static void ShowPalette()
		{
			PaletteWindow.ShowPalette();
		}

//		[MenuItem("Assets/Create/Volume Data")]
//		private static void CreateVolumeData()
//		{
//			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
//			VolumeData vData = ScriptableObject.CreateInstance<VolumeData> ();
//			AssetDatabase.CreateAsset(vData, path + "/New VolumeData.asset");
//		}
//
//		[MenuItem("Assets/Create/Volume GlobalSetting")]
//		private static void CreateVolumeGlobal()
//		{
//			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
//			VGlobal vData = ScriptableObject.CreateInstance<VGlobal> ();
//			AssetDatabase.CreateAsset(vData, path + "/GlobalSetting.asset");
//		}

	}
}