using UnityEngine;
using UnityEditor;

namespace CreVox
{
    [CustomEditor(typeof(EventPiece))]
    public class EventPieceEditor : LevelPieceEditor
	{
		EventPiece ep;

		void OnEnable()
		{
			ep = (EventPiece)target;
			if (ep.eventRange.Length != 6)
				ep.eventRange = new LevelPiece.EventRange[6];
		}

		public override void OnInspectorGUI ()
		{
			Color def = GUI.color;
			EditorGUI.BeginChangeCheck ();

			EditorGUILayout.LabelField ("Event", EditorStyles.boldLabel);
			using (var h = new EditorGUILayout.HorizontalScope ("Box")) {
				ep.eventRange[5] = (LevelPiece.EventRange)EditorGUILayout.EnumPopup ("Event Range", ep.eventRange[5]);
			}
			EditorGUILayout.Separator ();

			EditorGUILayout.LabelField ("Modified Component", EditorStyles.boldLabel);
			GUI.color = (ep.mp == null) ? Color.red : Color.green;
			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				GUI.color = def;

				ep.mp = (MoverProperty)EditorGUILayout.ObjectField (
					"Target", ep.mp, typeof(MoverProperty), true);
				
				if ((Object)(ep.mp) != null) {
					string nodes = "Nodes";
					for (int i = 0; i < ep.mp.m_nodes.Length; i++) {
						nodes += "\n\u3000" + ((i == ep.mp.m_nodes.Length - 1) ? "└" : "├") + "─ [" + i + "] ";
						Transform n = ep.mp.m_nodes [i];
						if (n != null) {
							nodes += n.name + n.localPosition.ToString ();
						}
					}
					EditorGUILayout.LabelField ("Modifiable Field : ", nodes, 
						EditorStyles.miniLabel, 
						GUILayout.Height (17 + 12 * ep.mp.m_nodes.Length));
				} else {
					EditorGUILayout.HelpBox ("↗ Drag a component into object field...", MessageType.None, true);
				}
			}
			if (EditorGUI.EndChangeCheck ())
				EditorUtility.SetDirty (ep);
		}

        public override void OnEditorGUI(ref BlockItem item)
        {
            EventPiece ep = (EventPiece)target;
            EditorGUI.BeginChangeCheck();

            if (item.attributes[(int)ATBT_EVN_PCE.EVENTType] == "")
                item.attributes[(int)ATBT_EVN_PCE.EVENTType] = EventGroup.Blue.ToString();

            EventGroup eventGrp =
                (EventGroup)System.Enum.Parse(typeof(EventGroup), item.attributes[(int)ATBT_EVN_PCE.EVENTType]);
            eventGrp = (EventGroup)EditorGUILayout.EnumPopup("EventGroup", eventGrp);
            item.attributes[(int)ATBT_EVN_PCE.EVENTType] = eventGrp.ToString();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(ep);
                ep.SetupPiece(item);
            }
        }
    }
}
