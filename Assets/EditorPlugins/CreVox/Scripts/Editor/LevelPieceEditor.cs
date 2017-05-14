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

		bool isHold {
			get{ return lp.isHold; }
			set {
				lp.isHold = value;
				EditorUtility.SetDirty (lp as Object);
			}
		}

		private int maxX, minX, maxY, minY, maxZ, minZ;

		public override void OnInspectorGUI ()
		{
            lp = (LevelPiece)target;
            maxX = lp.maxX;
            minX = lp.minX;
            maxY = lp.maxY;
            minY = lp.minY;
            maxZ = lp.maxZ;
            minZ = lp.minZ;


            EditorGUI.BeginChangeCheck ();
			lp.pivot = (LevelPiece.PivotType)EditorGUILayout.EnumPopup ("Pivot", lp.pivot);
			drawIsSolid = EditorGUILayout.Foldout (drawIsSolid, "Is Solid");
			if (drawIsSolid) {
				EditorGUI.indentLevel++;
				using (var v = new EditorGUILayout.VerticalScope ("HelpBox")) {
					for (int i = 0; i < lp.isSolid.Length; i++) {
						lp.isSolid [i] = EditorGUILayout.ToggleLeft (((Direction)i).ToString (), lp.isSolid [i]);
					}
				}
				EditorGUI.indentLevel--;
			}

			lp.isHold = EditorGUILayout.Foldout (lp.isHold, " Hold other Block", "toggle");
			if (lp.isHold) {
				DrawInit ();
				DrawList ();
			}

			drawDef = EditorGUILayout.Foldout (drawDef, "Default Inspector");
			if (drawDef) {
				EditorGUI.indentLevel++;
				DrawDefaultInspector ();
				EditorGUI.indentLevel = 0;
			}

			if (EditorGUI.EndChangeCheck ())
				EditorUtility.SetDirty (lp);
		}

        public virtual void OnEditorGUI(ref BlockItem item)
        {
            EditorGUILayout.LabelField("Nothing to edit");
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

				if (GUILayout.Button ("init", GUILayout.Height (30), GUILayout.Width (50))) {
					CreateHoldBlockList ();
				}
			}
		}

		void DrawList ()
		{
			using (var v = new EditorGUILayout.VerticalScope ("HelpBox")) {
				for (int i = 0; i < lp.holdBlocks.Count; i++) {
                    LevelPiece.Hold holdBlock = lp.holdBlocks [i];
					using (var h = new EditorGUILayout.HorizontalScope ("textfield")) {
						GUILayout.Label (" (" + holdBlock.offset.x + "," + holdBlock.offset.y + "," + holdBlock.offset.z + ")", "In TitleText");
						GUILayout.Space(Screen.width - 200);
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
						if (!(x == 0 && y == 0 && z == 0)) {
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
}