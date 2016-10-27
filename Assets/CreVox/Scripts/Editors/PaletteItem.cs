using UnityEngine;

namespace CreVox
{

	public class PaletteItem : MonoBehaviour
	{
#if UNITY_EDITOR
		public enum Category
		{
			System,
			Build,
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
		public Object inspectedScript; 
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