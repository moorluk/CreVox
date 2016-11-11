using UnityEngine;
using System.IO;
using UnityEditor;

namespace CreVox
{

	public static class MenuItems
	{
//		[MenuItem("Tools/Level Creator/New Level Scene")]
//		private static void NewLevel()
//		{
//			EditorUtils.NewLevel();
//		}

		[MenuItem("Tools/Level Creator/Show Palette _&p")]
		private static void ShowPalette()
		{
			PaletteWindow.ShowPalette();
		}

		[MenuItem("Assets/Create/Volume Data")]
		private static void CreateVolumeData()
		{
			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
			VolumeData vData = ScriptableObject.CreateInstance<VolumeData> ();

			//使用 holder 建立名為 dataHolder.asset 的資源
			AssetDatabase.CreateAsset(vData, path + "/New VolumeData.asset");
		}


	}
}