using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public static class Serialization {
	public static string saveFolderName = "Assets/saveData";

	public static string GetSaveLocation()
	{
		string saveLocation = saveFolderName + "/";

		if (!Directory.Exists (saveLocation))
			Directory.CreateDirectory (saveLocation);

		return EditorUtility.SaveFilePanel ("save map", saveLocation, "world", "bin");
	}

	public static string GetLoadLocation()
	{
		string loadLocation = saveFolderName + "/";

		if (!Directory.Exists (loadLocation))
			Directory.CreateDirectory (loadLocation);

		return EditorUtility.OpenFilePanel ("load map", loadLocation, "bin");
	}

	public static void SaveWorld(World world)
	{
		string saveFile = GetSaveLocation ();

		Save save = new Save (world);
		if (save.blocks.Count == 0)
			return;

		IFormatter formatter = new BinaryFormatter ();
		FileStream stream = new FileStream (saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize (stream, save);
		stream.Close ();
	}

	public static Save LoadWorld(World world) {
		string loadFile = GetLoadLocation ();
		if (!File.Exists (loadFile) || loadFile == null)
			return null;

		IFormatter formatter = new BinaryFormatter ();
		FileStream stream = new FileStream (loadFile, FileMode.Open);

		Save save = (Save)formatter.Deserialize (stream);
		stream.Close ();

		/*world.Reset ();
		world.Init (save.chunkX, save.chunkY, save.chunkZ);

		foreach (var block in save.blocks) {
			world.SetBlock (block.Key.x, block.Key.y, block.Key.z, block.Value);
			Debug.Log ("Load: " + block.Key.ToString ());
		}*/
		return save;
	}
}
