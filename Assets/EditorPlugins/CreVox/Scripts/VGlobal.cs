using UnityEngine;
using System.Collections.Generic;

//using System.Collections;

namespace CreVox
{
	[CreateAssetMenu(menuName = "CreVox/Global Setting")]
	public class VGlobal : ScriptableObject
	{
		[System.Serializable]
		public struct ArtPackParent
		{
			public string pack;
			public string parentPack;
		}

		public bool saveBackup;
		public bool FakeDeco;
		public bool debugRuler;

		public float editDis;

		public int chunkSize = 16;
		public float tileSize = 0.5f;
		public float w = 3f;
		public float h = 2f;
		public float d = 3f;
		public float hw = 1.5f;
		public float hh = 1f;
		public float hd = 1.5f;

		public List<ArtPackParent> artpackParentList;

		public static VGlobal GetSetting(string _settingPath = null)
		{
			VGlobal vg;
			vg = (VGlobal)Resources.Load ((_settingPath != null) ? _settingPath : PathCollect.setting, typeof(VGlobal));
			return vg;
		}
	}
}