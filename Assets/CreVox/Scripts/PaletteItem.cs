using UnityEngine;

namespace CreVox
{

	public class PaletteItem : MonoBehaviour
	{
#if UNITY_EDITOR
		public enum Category
		{
			Build,
			BuildDeco,
			System,
			Chara,
		}
		public enum MarkerType
		{
			Ground,
			Wall,
			WallSeparator,
			Fence,
			FenceSeparator,
			Door,
			Stair,
//			Stairhalf,
//			WallHalf,
//			WallHalfSeparator,
			Item
		}

		public Category category = Category.System;
		public MarkerType markType = MarkerType.Item;
		public string itemName = "";
//		public LevelPiece inspectedScript;
#endif

		// Use this for initialization
//		void Start()
//		{
//	
//		}
	
		// Update is called once per frame
//		void Update()
//		{
//	
//		}
	}
}