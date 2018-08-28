using UnityEngine;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(LevelPiece))]
	public class LevelPieceEditor : Editor
	{
		LevelPiece lp;
        bool drawIsSolid;
        bool drawDef;

		public override void OnInspectorGUI ()
        {
            EditorGUI.BeginChangeCheck ();

            DrawInspector ();

            drawDef = EditorGUILayout.Foldout (drawDef, "Default Inspector");
            if (drawDef) {
                EditorGUI.indentLevel++;
                DrawDefaultInspector ();
                EditorGUI.indentLevel = 0;
            }

            if (EditorGUI.EndChangeCheck ())
                EditorUtility.SetDirty (lp);
        }

        public void DrawInspector()
        {
            lp = (LevelPiece)target;
            EditorGUILayout.LabelField ("Event", EditorStyles.boldLabel);
            using (var h = new EditorGUILayout.HorizontalScope ("HelpBox")) {
                lp.eventRange = (LevelPiece.EventRange)EditorGUILayout.EnumPopup ("Event Range", lp.eventRange);
            }
            EditorGUILayout.Separator ();

            EditorGUILayout.LabelField ("Level Piece Setting", EditorStyles.boldLabel);
            using (var v = new EditorGUILayout.VerticalScope ("HelpBox")) {
                lp.pivot = (LevelPiece.PivotType)EditorGUILayout.EnumPopup ("Pivot", lp.pivot);
                EditorGUI.indentLevel++;
                drawIsSolid = EditorGUILayout.Foldout (drawIsSolid, "Is Solid");
                if (drawIsSolid) {
                    using (var v2 = new EditorGUILayout.VerticalScope ("HelpBox")) {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < lp.isSolid.Length; i++) {
                            lp.isSolid [i] = EditorGUILayout.ToggleLeft (((Direction)i).ToString (), lp.isSolid [i]);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Separator ();
		}

        public virtual void OnEditorGUI(ref BlockItem item)
		{
            lp = (LevelPiece)target;
			EditorGUI.BeginChangeCheck ();
			using (var v = new EditorGUILayout.VerticalScope ("box")) {
				lp.PProperties [0].tComponent = FocalComponent.DefaultEventRange;
				lp.PProperties [0].tObject = lp;
				lp.PProperties [0].tActive = EditorGUILayout.ToggleLeft (lp.PProperties [0].tComponent.ToString (),lp.PProperties [0].tActive, EditorStyles.boldLabel);
				if (lp.PProperties [0].tActive) {
					lp.PProperties [0].tRange = (LevelPiece.EventRange)EditorGUILayout.EnumPopup ("Event Range", lp.PProperties [0].tRange);
					item.attributes [0] = "true," + lp.PProperties [0].tComponent + "," + lp.PProperties [0].tRange;
					EditorGUILayout.LabelField (item.attributes [0], EditorStyles.miniTextField);
				} else {
					item.attributes [0] = "false," + lp.PProperties [0].tComponent + "," + lp.PProperties [0].tRange;
				}
			}
			if (EditorGUI.EndChangeCheck ())
				EditorUtility.SetDirty (lp);
        }
	}
}