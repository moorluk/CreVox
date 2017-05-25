using UnityEngine;
using UnityEditor;
using System.IO;

namespace CreVox
{
	[CustomEditor(typeof(PrefabPiece))]
    public class PrefabPieceEditor : LevelPieceEditor
    {
        public override void OnEditorGUI(ref BlockItem item)
        {
			PrefabPiece pp = (PrefabPiece)target;

            EditorGUI.BeginChangeCheck();

			pp.artPrefab = (GameObject)EditorGUILayout.ObjectField ("Prefab", pp.artPrefab, typeof(GameObject));
			EditorGUILayout.HelpBox (item.attributes [0],MessageType.None);

            if (EditorGUI.EndChangeCheck())
			{
				if (pp.artPrefab != null) {
					string _path = AssetDatabase.GetAssetPath (pp.artPrefab);
					_path = _path.Substring (_path.IndexOf (PathCollect.resourceSubPath));
					_path = _path.Remove (_path.Length - Path.GetExtension (_path).Length);
					item.attributes [0] = _path;
				}
				if (pp.artInstance != null)
					GameObject.DestroyImmediate (pp.artInstance, false);
                EditorUtility.SetDirty(pp);
                pp.SetupPiece(item);
			}
        }
    }
}
