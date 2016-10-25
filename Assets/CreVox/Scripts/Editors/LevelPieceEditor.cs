using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace CreVox
{

	[CustomEditor(typeof(LevelPiece))]
	public class LevelPieceEditor : Editor
	{
		LevelPiece lp;

		LevelPiece.PivotType pivot;
		bool isSolid = true;

		private void OnEnable()
		{
			lp = (LevelPiece)target;
		}

		public override void OnInspectorGUI()
		{
			lp.pivot = (LevelPiece.PivotType)EditorGUILayout.EnumPopup ("Pivot", lp.pivot);
			lp.isStair = EditorGUILayout.Toggle ("Is Stair", lp.isStair);
			isSolid = EditorGUILayout.Foldout (isSolid, "Is Solid");
			if (isSolid) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < lp.isSolid.Length; i++) {
					lp.isSolid [i] = EditorGUILayout.Toggle (((Block.Direction)i).ToString (), lp.isSolid [i]);
				}
				EditorGUI.indentLevel--;
			}
		}
	}
}