using UnityEngine;
using System;

namespace CreVox
{

    public class PaletteItem : MonoBehaviour
    {
        [Flags]
        public enum Set
        {
            InDoor = 1 << 0,
            OutDoor = 1 << 1,
            Rise = 1 << 2,
            Sink = 1 << 3,
            Pass = 1 << 4,
        }

        public enum Module
        {
            Door,
            Wall, //WallPillar,
            Fence, //FencePillar,
            Ground,
            Ceiling,
            Stair, //StairPlatform,StairConstructure,
            Block,

            System = 50,
            Charac,
            Trap,
            Drop,
            Break,

            Unuse = 99
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
            Roof
        }
        
        public MarkerType markType = MarkerType.Item;
        public int m_set;
        public Module m_module;
        public string itemName = "";
        public LevelPiece inspectedScript;
        public string assetPath;
    }
}