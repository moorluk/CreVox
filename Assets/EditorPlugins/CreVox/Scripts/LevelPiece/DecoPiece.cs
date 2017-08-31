using UnityEngine;
//using System.Collections;
using System;
using System.Collections.Generic;

namespace CreVox
{
    [ExecuteInEditMode]
    public class DecoPiece : LevelPiece
    {
        public List<Decoration> decos = new List<Decoration> ();
        public Dictionary<Decoration,List<Decoration>> trees = new Dictionary<Decoration, List<Decoration>> ();
        public Dictionary<Decoration,List<Decoration>> randomOnes = new Dictionary<Decoration, List<Decoration>> ();
        public Dictionary<Decoration,List<Decoration>> randomAlls = new Dictionary<Decoration, List<Decoration>> ();
        public GameObject root = null;

        void Start ()
        {
            SetupPiece (null);
        }

        public override void SetupPiece(BlockItem item)
        {
            if (root) GameObject.DestroyImmediate (root);
            root = new GameObject ("Decoration Root");
            root.transform.parent = transform;
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.Euler (Vector3.zero);
            root.transform.localScale = Vector3.one;

            for (int i = 0; i < decos.Count; i++) {
                decos [i].Generate (root);
            }
        }
    }
}