using UnityEngine;
using System.Collections;
using System;

namespace CreVox
{

	public static class PathCollect
	{
		public static string rularTag = "VoxelEditorBase";

		public static string assetsPath = "Assets/EditorPlugins/CreVox";

		public static string resourcesPath = "Assets/Resources/";
		public static string resourceSubPath = "CreVox/";
		
		public static string chunk = "CreVox/Chunk";
		public static string box = "CreVox/BoxCursor";
		public static string save = "CreVox/VolumeData";
		public static string gram = "CreVox/Grammar";
		public static string artDeco = "CreVox/DecoPrefabs";
		public static string artPack = "CreVox/VolumeArtPack/";
		public static string pieces = artPack + "LevelPieces";
		public static string defaultVoxelMaterial = pieces + "/LevelPieces_Voxel";
		public static string camSetting = "CreVox/CamSetting";
		public static string setting = "CreVox/GlobalSetting";
	}
}