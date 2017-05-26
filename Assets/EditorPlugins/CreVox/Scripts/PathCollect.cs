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
		
		public static string chunk = resourceSubPath + "Chunk";
		public static string box = resourceSubPath + "BoxCursor";
		public static string save = resourceSubPath + "VolumeData";
		public static string artDeco = resourceSubPath + "ArtResources";
		public static string artPack = resourceSubPath + "VolumeArtPack";
		public static string pieces = artPack + "/LevelPieces";
		public static string defaultVoxelMaterial = pieces + "/LevelPieces_Voxel";
		public static string camSetting = resourceSubPath + "CamSetting";
		public static string setting = resourceSubPath + "GlobalSetting";
	}
}