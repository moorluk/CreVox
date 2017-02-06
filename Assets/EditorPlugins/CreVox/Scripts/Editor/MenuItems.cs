using UnityEngine;
using System.IO;
using UnityEditor;

namespace CreVox
{

	public static class MenuItems
	{

		[MenuItem("Tools/CreVox/Show Palette _&p")]
		private static void ShowPalette()
		{
			PaletteWindow.ShowPalette();
		}

		[MenuItem("Tools/CreVox/ArtPack Check")]
		private static void ShowArtPack()
		{
			ArtPackWindow.ShowPalette();
		}

		[MenuItem("GameObject/3D Object/Volume (CreVox)")]
		private static void AddVolume()
		{
			GameObject newVol = new GameObject ();
			newVol.name = "New Volume";
			newVol.AddComponent<Volume> ();
			Volume volume = newVol.GetComponent<Volume> ();
			volume.Reset ();
			volume.workFile = "";
			volume.tempPath = "";
			volume.Init (1, 1, 1);
			VolumeEditor.WriteVData (volume);
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