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
            Item = -3,
            Ground = -2,
            Stair = -1,
            Door = 0,
            Wall = 1,
            WallSeparator,
            Fence,
            FenceSeparator,
            Roof,
            //Stairhalf,
            //WallHalf,
            //WallHalfSeparator,
        }

        public Category category = Category.System;
        public MarkerType markType = MarkerType.Item;
        public string itemName = "";
        public LevelPiece inspectedScript;
        public string assetPath;
    }
}