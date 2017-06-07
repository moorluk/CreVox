using UnityEngine;
using UnityEditor;
using System.IO;

namespace CreVox
{
	[CustomEditor(typeof(PrefabPiece))]
    public class PrefabPieceEditor : LevelPieceEditor
	{
		string[] artpacks;
		PrefabPiece pp;
		GameObject tempObjold;
		void OnEnable()
		{
			artpacks = VGlobal.GetArtPacks ().ToArray ();
			pp = (PrefabPiece)target;
		}

		public override void OnInspectorGUI ()
		{

			EditorGUI.BeginChangeCheck ();
			pp.APId = EditorGUILayout.Popup ("ArtPack", pp.APId, artpacks);
			pp.artPack = artpacks[pp.APId];
			DrawDefaultInspector ();
			if (EditorGUI.EndChangeCheck ())
				EditorUtility.SetDirty (pp);
		}

        public override void OnEditorGUI(ref BlockItem item)
        {
			PrefabPiece pp = (PrefabPiece)target;
			if (pp.artPrefab != null)
				tempObjold = pp.artPrefab;

            EditorGUI.BeginChangeCheck();
			pp.artPrefab = (GameObject)EditorGUILayout.ObjectField ("Prefab", pp.artPrefab, typeof(GameObject),false);
			if (EditorGUI.EndChangeCheck ()) {
				if (pp.artPrefab != null && item != null) {
					string _path = AssetDatabase.GetAssetPath (pp.artPrefab);
					_path = _path.Substring (_path.IndexOf (PathCollect.resourceSubPath));
					string _name = Path.GetFileNameWithoutExtension (_path);
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
			EditorGUI.BeginChangeCheck();
			pp.isRoot = EditorGUILayout.Toggle ("Force Zero Position", pp.isRoot);
			if (EditorGUI.EndChangeCheck ()) {
				item.attributes [1] = pp.isRoot.ToString ();
				EditorUtility.SetDirty (pp);
				pp.SetupPiece (item);
			}

			EditorGUILayout.HelpBox (PathCollect.artDeco + "/" + pp.artPack + "/ArtPrefab/" + item.attributes [0],MessageType.None);
			EditorGUILayout.LabelField ("Path:",PathCollect.artDeco +"/(ArtPack)/ArtPrefab",EditorStyles.miniLabel);
			EditorGUILayout.LabelField ("ArtPack:",pp.artPack,EditorStyles.miniLabel);
			EditorGUILayout.LabelField ("Prefab Name:",item.attributes [0],EditorStyles.miniLabel);
        }
    }
}
