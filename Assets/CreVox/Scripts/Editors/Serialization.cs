using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace CreVox
{

	public static class Serialization
	{
		public static string saveFolderName = PathCollect.saveData;

		public static string GetSaveLocation(string _path = null)
		{
			string saveLocation;
			if (_path != null)
				saveLocation = _path;
			else
				saveLocation = saveFolderName + "/";

			return EditorUtility.SaveFilePanel("save map", saveLocation, "volume", "bytes");
		}

		public static string GetLoadLocation(string _path = null)
		{
			string loadLocation;
			if (_path != null)
				loadLocation = _path;
			else
				loadLocation = saveFolderName + "/";

			return EditorUtility.OpenFilePanel("load map", loadLocation, "bytes");
		}

		public static void SaveWorld(Volume volume, string _path = null)
		{
			string saveFile;
			if (_path == null)
				saveFile = GetSaveLocation();
			else
				saveFile = _path;

			Debug.Log("Save path: " + saveFile);

			Save save = new Save(volume);
			if (save.blocks.Count == 0)
				return;

			IFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, save);
			stream.Close();
		}

		public static Save LoadWorld(string _path = null)
		{
			string loadFile;
			if (_path == null)
				loadFile = GetLoadLocation();
			else
				loadFile = _path;
			
			if (!File.Exists(loadFile) || loadFile == null)
				return null;

			IFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(loadFile, FileMode.Open);

			Save save = (Save)formatter.Deserialize(stream);
			stream.Close();
			return save;
		}

		public static Save LoadRTWorld(string path)
		{
			TextAsset ta = Resources.Load(path) as TextAsset;
			Debug.Log("Load path: " + path + " ---" + (ta != null?"Success":"Fail"));

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream(ta.bytes);

			Save save = (Save)formatter.Deserialize(stream);
			stream.Close();
			return save;
		}
	}
}