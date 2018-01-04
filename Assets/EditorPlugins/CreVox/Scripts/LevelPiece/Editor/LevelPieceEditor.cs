using UnityEngine;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(LevelPiece))]
	public class LevelPieceEditor : Editor
	{
		LevelPiece lp;
        bool drawIsSolid;
        bool drawHold;
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

                drawHold = EditorGUILayout.Foldout (drawHold, "BlockHold");
                EditorGUI.indentLevel--;
                if (drawHold) {
                    lp.isHold = EditorGUILayout.Toggle ("Hold other Block", lp.isHold);
                    EditorGUI.BeginDisabledGroup (!lp.isHold);
                    DrawInit ();
                    if (lp.holdBlocks.Count > 0)
                        DrawList ();
                    EditorGUI.EndDisabledGroup ();
                }
            }
            EditorGUILayout.Separator ();
		}

        public virtual void OnEditorGUI(ref BlockItem item)
		{
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

        void DrawInit ()
		{
            using (var h = new EditorGUILayout.HorizontalScope ("HelpBox")) {
                GUILayout.BeginVertical ();
				GUILayout.Label ("MaxX", "miniLabel");
                lp.maxX = EditorGUILayout.IntField (lp.maxX);
				GUILayout.Label ("MinX", "miniLabel");
                lp.minX = EditorGUILayout.IntField (lp.minX);
				GUILayout.EndVertical ();

                GUILayout.BeginVertical ();
				GUILayout.Label ("MaxY", "miniLabel");
				lp.maxY = EditorGUILayout.IntField (lp.maxY);
				GUILayout.Label ("MinY", "miniLabel");
                lp.minY = EditorGUILayout.IntField (lp.minY);
				GUILayout.EndVertical ();

                GUILayout.BeginVertical ();
				GUILayout.Label ("MaxZ", "miniLabel");
				lp.maxZ = EditorGUILayout.IntField (lp.maxZ);
				GUILayout.Label ("MinZ", "miniLabel");
                lp.minZ = EditorGUILayout.IntField (lp.minZ);
                GUILayout.EndVertical ();

                if (GUILayout.Button ("init", GUILayout.ExpandHeight(true),GUILayout.Height (80), GUILayout.Width (50))) {
					CreateHoldBlockList ();
				}
			}
		}

		void DrawList ()
		{
			using (var v = new EditorGUILayout.VerticalScope ("HelpBox")) {
                foreach (var holdBlock in lp.holdBlocks) {
					using (var h = new EditorGUILayout.HorizontalScope ("textfield")) {
                        GUILayout.Label (" (" + holdBlock.offset.x + "," + holdBlock.offset.y + "," + holdBlock.offset.z + ")", "In TitleText", GUILayout.Width (Screen.width - 160));
						GUILayout.Label ("Solid", "miniLabel");
						holdBlock.isSolid = EditorGUILayout.Toggle (holdBlock.isSolid);
						if (GUILayout.Button ("Remove","miniButton"))
							lp.holdBlocks.Remove(holdBlock);
					}
				}
			}
		}

		void CreateHoldBlockList ()
		{
			lp.holdBlocks.Clear ();
			for (int y = lp.minY; y < lp.maxY + 1; y++) {
				for (int x = lp.minX; x < lp.maxX + 1; x++) {
					for (int z = lp.minZ; z < lp.maxZ + 1; z++) {
						LevelPiece.Hold newHold = new LevelPiece.Hold ();
						newHold.offset.x = x;
						newHold.offset.y = y;
						newHold.offset.z = z;
						newHold.isSolid = false;
						lp.holdBlocks.Add (newHold);
					}
				}
			}
		}
	}
}