using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreVox
{

    [Serializable]
    public class NIndex
    {
        public int id = -1;
        public int treeIndex = -1;
        public float probability = 1.0f;

        public int FindListByNode(List<TreeElement> _list)
        {
            Predicate<TreeElement> checkNode = delegate(TreeElement obj) {
                return obj.self.id.Equals(id);
            };
            return _list.FindIndex(0,checkNode);
        }
    }

    [Serializable]
    public class Node : NIndex
    {
        public DecoType type;
        public GameObject source;
        public GameObject instance;
        public Vector3 pos;
        public Vector3 posR;
        public Vector3 rot;
        public Vector3 rotR;
        public Vector3 scl;
        public Vector3 sclR;
        public turnSide rotS;

        public Node(){
            id = 0;
            treeIndex = 0;
            type = DecoType.Node;
            pos = Vector3.zero;
            posR = Vector3.zero;
            rot = Vector3.zero;
            rotR = Vector3.zero;
            scl = Vector3.one;
            sclR = Vector3.zero;
            rotS = turnSide.one;
            probability = 1.0f;
        }

        public GameObject Generate (GameObject root)
        {
            #if UNITY_EDITOR
            instance = source != null ? PrefabUtility.InstantiatePrefab (source) as GameObject : new GameObject ("Empty TreeElement");
            #else
            instance = source != null ? GameObject.Instantiate (source) as GameObject : new GameObject ("Empty TreeElement"); 
            #endif
            instance.name += " (" + treeIndex + ")";
            instance.transform.parent = root.transform;
            instance.transform.localPosition = CalculateV3 (pos, posR);
            instance.transform.localRotation = Quaternion.Euler (CalculateV3 (rot, rotR, true));
            instance.transform.localScale = CalculateV3 (scl, sclR);

            return instance;
        }

        Vector3 CalculateV3 (Vector3 _base, Vector3 _random, bool _rotation = false)
        {
            float _turn = 0;
            if (_rotation) {
                switch (rotS) {
                case turnSide.two:
                    _turn = 180 * Mathf.Floor (UnityEngine.Random.value * 2);
                    break;
                case turnSide.four:
                    _turn = 90 * Mathf.Floor (UnityEngine.Random.value * 4);
                    break;
                }
            }
            UnityEngine.Random.InitState (Guid.NewGuid ().GetHashCode ());
            Vector3 v = new Vector3 (
                            _base.x + UnityEngine.Random.Range (-_random.x, _random.x),
                            _base.y + UnityEngine.Random.Range (-_random.y, _random.y) + _turn,
                            _base.z + UnityEngine.Random.Range (-_random.z, _random.z)
                        );
            return v;
        }
    }
}
