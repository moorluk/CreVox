using UnityEngine;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(LevelPiece))]
	public class LevelPieceEditor : Editor
	{
		LevelPiece lp;
		
		bool drawIsSolid = true;
		bool drawDef = false;

		private void OnEnable ()
		{
			lp = (LevelPiece)target;
		}

		public override void OnInspectorGUI ()
		{
			lp.pivot = (LevelPiece.PivotType)EditorGUILayout.EnumPopup ("Pivot", lp.pivot);
			lp.isStair = EditorGUILayout.Toggle ("Is Stair", lp.isStair);
			drawIsSolid = EditorGUILayout.Foldout (drawIsSolid, "Is Solid");
			if (drawIsSolid) {
				for (int i = 0; i < lp.isSolid.Length; i++) {
					lp.isSolid [i] = EditorGUILayout.Toggle (((Block.Direction)i).ToString(), lp.isSolid [i]);
				}
			}
			
			GUILayout.Space (5);
			drawDef = EditorGUILayout.Foldout (drawDef, "Default Inspector");
			if (drawDef)
				DrawDefaultInspector ();
		}
	}
}