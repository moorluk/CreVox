using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreVox
{
    [Serializable]
    public class Node
    {
        public int id = -1;
        public int treeIndex = -1;

        public int type = 0;
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

        public void Init()
        {
            id = 0;
            treeIndex = 0;
            type = 1;
            pos = Vector3.zero;
            posR = Vector3.zero;
            rot = Vector3.zero;
            rotR = Vector3.zero;
            scl = Vector3.one;
            sclR = Vector3.zero;
            rotS = turnSide.one;
            probability = 1.0f;
        }

        public int FindListByNode(List<TreeElement> _list)
        {
            Predicate<TreeElement> checkNode = delegate(TreeElement obj) {
                return obj.self.id.Equals(this.id);
            };
            return _list.FindIndex(1,checkNode);
        }

        public GameObject Generate (GameObject root)
        {
            if (source != null) {
                #if UNITY_EDITOR
                instance = PrefabUtility.InstantiatePrefab (source) as GameObject;
                #else
                instance = GameObject.Instantiate(source); 
                #endif
            } else {
                instance = new GameObject (root.name + ".child");
            }
            instance.transform.parent = root.transform;
            instance.transform.localPosition = CalculateV3 (pos, posR);
            instance.transform.localRotation = Quaternion.Euler (CalculateV3 (rot, rotR, true));
            instance.transform.localScale = CalculateV3 (scl, sclR);

            return instance;
        }

        private Vector3 CalculateV3 (Vector3 _base, Vector3 _random, bool _rotation = false)
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
                default:
                    break;
                }
            }
            UnityEngine.Random.InitState (System.Guid.NewGuid ().GetHashCode ());
            Vector3 v = new Vector3 (
                            _base.x + UnityEngine.Random.Range (-_random.x, _random.x),
                            _base.y + UnityEngine.Random.Range (-_random.y, _random.y) + _turn,
                            _base.z + UnityEngine.Random.Range (-_random.z, _random.z)
                        );
            return v;
        }
    }
}
