using UnityEngine;
using UnityEditor;

namespace CreVox
{
    [CustomEditor (typeof(PrefabPiece))]
    public class PrefabPieceEditor : LevelPieceEditor
    {
        string[] artpacks;
        PrefabPiece pp;
        GameObject tempObjold;

        void OnEnable ()
        {
            artpacks = VGlobal.GetArtPacks ().ToArray ();
            pp = (PrefabPiece)target;
        }

        public override void OnInspectorGUI ()
        {
            EditorGUI.BeginChangeCheck ();

            EditorGUILayout.LabelField ("ArtPack", EditorStyles.boldLabel);
            using (var h = new EditorGUILayout.HorizontalScope ("helpbox")) {
                pp.APId = EditorGUILayout.Popup ("ArtPack", pp.APId, artpacks);
                pp.artPack = artpacks [pp.APId];
            }
            if (EditorGUI.EndChangeCheck ())
                EditorUtility.SetDirty (pp);
        }

        public override void OnEditorGUI (ref BlockItem item)
        {
            PrefabPiece pp = (PrefabPiece)target;
            if (pp.artPrefab != null)
                tempObjold = pp.artPrefab;

            using (var ch = new EditorGUI.ChangeCheckScope ()) {
                pp.artPrefab = (GameObject)EditorGUILayout.ObjectField ("Prefab", pp.artPrefab, typeof(GameObject), false);
                if (ch.changed) {
                    if (pp.artPrefab == null || item == null)
                        return;
                    string _path = AssetDatabase.GetAssetPath (pp.artPrefab);
                    _path = _path.Substring (_path.IndexOf (PathCollect.resourceSubPath));
                    string _name = System.IO.Path.GetFileNameWithoutExtension (_path);
                    _path = _path.Remove (PathCollect.artDeco.Length);
                    Debug.Log (_path + "< >" + PathCollect.artDeco);
                    if (_path == PathCollect.artDeco) {
                        item.attributes [0] = _name;
                        if (pp.artInstance != null)
                            GameObject.DestroyImmediate (pp.artInstance, false);
                        EditorUtility.SetDirty (pp);
                        pp.SetupPiece (item);
                    } else {
                        item.attributes [0] = "";
                        pp.artPrefab = tempObjold;
                    }
                }
            }
            using (var ch = new EditorGUI.ChangeCheckScope ()) {
                pp.isRoot = EditorGUILayout.Toggle ("Force Zero Position", pp.isRoot);
                if (ch.changed) {
                    item.attributes [1] = pp.isRoot.ToString ();
                    EditorUtility.SetDirty (pp);
                    pp.SetupPiece (item);
                }
            }

            EditorGUILayout.HelpBox (PathCollect.artDeco + "/" + pp.artPack + "/" + item.attributes [0], MessageType.None);
            EditorGUILayout.LabelField ("Path:", PathCollect.artDeco + "/(ArtPack)/", EditorStyles.miniLabel);
            EditorGUILayout.LabelField ("ArtPack:", pp.artPack, EditorStyles.miniLabel);
            EditorGUILayout.LabelField ("Prefab Name:", item.attributes [0], EditorStyles.miniLabel);
        }
    }
}
