﻿using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CreVox
{

	public static class Serialization
	{
		#if UNITY_EDITOR
		public static string saveFolderName = PathCollect.save;

		public static string GetSaveLocation(string _path = null)
		{
			string saveLocation;
			if (_path == null)
				_path = saveFolderName;
			
			saveLocation = Application.dataPath 
				+ PathCollect.resourcesPath.Substring(6) 
				+ _path.Remove(_path.LastIndexOf("/"));

			return EditorUtility.SaveFilePanel("save map", saveLocation, "", "bytes");
		}

		public static string GetLoadLocation(string _path = null)
		{
			string loadLocation;
			if (_path == null)
				_path = saveFolderName;
			
			loadLocation = Application.dataPath 
				+ PathCollect.resourcesPath.Substring(6) 
				+ _path.Remove(_path.LastIndexOf("/"));

			return EditorUtility.OpenFilePanel("load map", loadLocation, "bytes");
		}

		public static void SaveWorld(Volume volume, string _path = null)
		{
			string saveFile;
			if (_path == null)
				saveFile = GetSaveLocation();
			else
				saveFile = _path;

			Save save = new Save(volume);
//			if (save.blocks.Count == 0)
//				return;

			Debug.Log ("Volume[" + volume.transform.name + "] Save Path : \n" + saveFile);

			IFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, save);
			stream.Close();
			AssetDatabase.Refresh();
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
		#endif

		public static Save LoadRTWorld(string path)
		{
			TextAsset ta = Resources.Load(path) as TextAsset;

			if (ta == null)
				return null;

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream(ta.bytes);

			Save save = (Save)formatter.Deserialize(stream);
			stream.Close();
			return save;
		}
	}
}