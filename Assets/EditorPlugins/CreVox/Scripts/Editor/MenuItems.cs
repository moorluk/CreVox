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

		[MenuItem("GameObject/3D Object/Volume (CreVox)")]
		private static void AddVolume()
		{
			GameObject newVol = new GameObject ();
			newVol.name = "New Volume";
			newVol.AddComponent<Volume> ();
			Volume volume = newVol.GetComponent<Volume> ();
			volume.Reset ();
			volume.Init (1, 1, 1);
			volume.workFile = "";
			volume.tempPath = "";
			string sPath = Application.dataPath + PathCollect.resourcesPath.Substring (6) + PathCollect.save;
			sPath = EditorUtility.SaveFilePanel("save vData", sPath, volume.name + "_vData", "asset");
			sPath = sPath.Substring (sPath.IndexOf (PathCollect.resourceSubPath));
			volume.vd = VolumeData.GetVData (sPath);
			volume._useBytes = false;
			volume.BuildVolume (new Save (), volume.vd);
		}

		[MenuItem("GameObject/3D Object/Volume Manager (CreVox)")]
		private static void AddVolumeManager()
		{
			GameObject newVol = new GameObject ();
			newVol.name = "VolumeManager";
			newVol.AddComponent<VolumeManager> ();
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