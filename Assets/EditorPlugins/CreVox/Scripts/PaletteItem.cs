using UnityEngine;

namespace CreVox
{

	public class PaletteItem : MonoBehaviour
	{
		public enum Category
		{
			Build,
			Deco,
			System,
			Trap,
			Sign,
			Movement,
			Chara,
			Obstacle
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
        public LevelPiece inspectedScript;

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