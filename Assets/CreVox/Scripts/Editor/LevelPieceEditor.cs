using UnityEngine;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(LevelPiece))]
	public class LevelPieceEditor : Editor
	{
		LevelPiece lp;
		
		bool drawIsSolid = true;

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
				EditorGUI.indentLevel++;
				for (int i = 0; i < lp.isSolid.Length; i++) {
					lp.isSolid [i] = EditorGUILayout.Toggle (((Direction)i).ToString(), lp.isSolid [i]);
				}
				EditorGUI.indentLevel--;
			}
			
			GUILayout.Space (5);
		}
	}
}