using UnityEngine;
using UnityEditor;

namespace CreVox
{
    [CustomEditor (typeof(MissingPiece))]
    public class MissingPieceEditor : LevelPieceEditor
    {
        MissingPiece mp;

        void OnEnable ()
        {
            mp = (MissingPiece)target;
        }

        public override void OnInspectorGUI ()
        {
            HelpBoxX ("Change BlockItem's Name");
        }

        public override void OnEditorGUI (ref BlockItem item)
        {
            mp.usePrefab = EditorGUILayout.ToggleLeft ("Use Prefab", mp.usePrefab);
            if (mp.usePrefab) {
                EditorGUILayout.LabelField ("Fixed Item Name", item.pieceName);
                mp.tempObj = (PaletteItem)EditorGUILayout.ObjectField ("Item Marker", mp.tempObj, typeof(PaletteItem), true);
                if (mp.tempObj != null) { 
                    if (mp.tempObj.markType != PaletteItem.MarkerType.Item) {
                        Debug.LogWarning ("Not a Item!!!");
                    } else {
                        item.pieceName = mp.tempObj.gameObject.name;
                    }
                    mp.tempObj = null;
                }
                HelpBoxX ("1. Drag a Item Marker into object field\n2. <color=red><b> REFRESH </b>VolumeData</color>");
            } else {
                item.pieceName = EditorGUILayout.TextField ("Fixed Item Name", item.pieceName);
                HelpBoxX ("1. Fix the Wrong Name\n2. <color=red><b> REFRESH </b>VolumeData</color>");
            }
        }

        void HelpBoxX (string _text)
        {
            GUIContent _info = EditorGUIUtility.IconContent ("console.infoicon");
            _info.text = _text;
            EditorGUILayout.LabelField (_info, GetHBXStyle ());
        }

        private GUIStyle GetHBXStyle ()
        {
            GUIStyle style = GUI.skin.GetStyle ("helpbox");
            style.richText = true;
            style.fontSize = 12;

            return style;
        }
    }
}
