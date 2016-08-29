using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using CreVox;

public static class SerializationOld {
	public static string saveFolderName = CreVox.Serialization.saveFolderName;

	public static string GetLoadLocation()
	{
		string loadLocation = saveFolderName + "/";

		if (!Directory.Exists (loadLocation))
			Directory.CreateDirectory (loadLocation);

		return EditorUtility.OpenFilePanel ("load map", loadLocation, "bin");
	}

	public static Save LoadWorld(World world) {
		string loadFile = GetLoadLocation ();
		if (!File.Exists (loadFile) || loadFile == null)
			return null;

		IFormatter formatter = new BinaryFormatter ();
		FileStream stream = new FileStream (loadFile, FileMode.Open);

		global::Save save = (global::Save)formatter.Deserialize (stream);
		stream.Close ();
		return save;
	}
}