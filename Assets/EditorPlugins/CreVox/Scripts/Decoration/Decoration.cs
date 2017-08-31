using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Basic.UnityVector2;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreVox
{
    public enum DecoType
    {
        Node,
        Tree,
        RandomOne
    }

    public enum turnSide
    {
        one,
        two,
        four
    }

    [Serializable]
    public class Decoration
    {
        #if UNITY_EDITOR
        public bool showNode = true;
        #endif

        public int type = 0;

        public Node node = new Node ();
        public List<Decoration> treeNodes = new List<Decoration> ();
        public List<Decoration> selectNodes = new List<Decoration> ();

        public void Generate (GameObject parent)
        {
            switch (type) {
            case (int)DecoType.Node:
                node.Generate (parent);
                break;
            case (int)DecoType.Tree:
                GameObject r = node.Generate (parent);
                foreach (Decoration d in treeNodes) {
                    d.Generate (r);
                }
                break;
            case (int)DecoType.RandomOne:
                break;
            }
        }
    }

    [Serializable]
    public class Node
    {
        public Node ()
        {
        }

        public GameObject source;
        public GameObject instance;

        public Vector3 pos = Vector3.zero;
        public Vector3 posR = Vector3.zero;
        public Vector3 rot = Vector3.zero;
        public Vector3 rotR = Vector3.zero;
        public Vector3 scl = Vector3.one;
        public Vector3 sclR = Vector3.zero;

        public turnSide rotS = turnSide.one;

        public float probability = 1.0f;

        public GameObject Generate (GameObject root)
        {
            #if UNITY_EDITOR
            instance = PrefabUtility.InstantiatePrefab (source) as GameObject;
            #else
            instance = GameObject.Instantiate(source);
            #endif
            instance.transform.parent = root.transform;
            instance.transform.localPosition = CalculateV3 (pos, posR);
            instance.transform.localRotation = Quaternion.Euler (CalculateV3 (rot, rotR));
            instance.transform.localScale = CalculateV3 (scl, sclR);

            return instance;
        }

        public Vector3 CalculateV3 (Vector3 _base, Vector3 _random)
        {
            UnityEngine.Random.InitState (System.Guid.NewGuid ().GetHashCode ());
            Vector3 v = new Vector3 (
                _base.x + UnityEngine.Random.Range (-_random.x, _random.x),
                _base.y + UnityEngine.Random.Range (-_random.y, _random.y),
                _base.z + UnityEngine.Random.Range (-_random.z, _random.z)
                        );
            return v;
        }
    }
}
