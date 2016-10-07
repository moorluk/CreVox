using UnityEngine;
using System.Collections;
using System;

namespace CreVox
{

	public static class PathCollect
	{
		public static string rularTag = "VoxelEditorBase";

		public static string editorPath = Application.dataPath;
		public static string assetsPath = "Assets/Crevox";
		public static string saveData = assetsPath + "/saveData";

		public static string resourcesPath = "Assets/Resources/";
		public static string resourceSubPath = "Prefabs";

		public static string testmap = resourceSubPath + "/testmap";
		public static string chunk = resourceSubPath + "/chunk";
		public static string pieces = resourceSubPath + "/LevelPieces";

		public static string camSettingPath = "CamSetting";

	}
}