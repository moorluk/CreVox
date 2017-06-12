using UnityEngine;
using UnityEditor;

namespace CreVox
{
    [CustomEditor(typeof(EventPiece))]
    public class EventPieceEditor : LevelPieceEditor
    {
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
