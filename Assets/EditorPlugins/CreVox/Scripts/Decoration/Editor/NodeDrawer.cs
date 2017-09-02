using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace CreVox
{
    [CustomPropertyDrawer(typeof(Node))]
    public class NodeDrawer:PropertyDrawer
    {
        const float row = 16;
        const float row2 = 19;
        const float iconSize = 68;
        const float labelW = 16;
        float labelWdef = EditorGUIUtility.labelWidth;

        static string[] tName = new string[3]{"offset", "rotate", "scale"};

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            Rect p = EditorGUI.IndentedRect (position);
            p = new Rect (p.x, p.y, p.width, p.height);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginProperty (p, label, property);

            var source = property.FindPropertyRelative ("source");
            var posX = property.FindPropertyRelative ("pos").FindPropertyRelative ("x");
            var posY = property.FindPropertyRelative ("pos").FindPropertyRelative ("y");
            var posZ = property.FindPropertyRelative ("pos").FindPropertyRelative ("z");
            var posRX = property.FindPropertyRelative ("posR").FindPropertyRelative ("x");
            var posRY = property.FindPropertyRelative ("posR").FindPropertyRelative ("y");
            var posRZ = property.FindPropertyRelative ("posR").FindPropertyRelative ("z");
            var rotX = property.FindPropertyRelative ("rot").FindPropertyRelative ("x");
            var rotY = property.FindPropertyRelative ("rot").FindPropertyRelative ("y");
            var rotZ = property.FindPropertyRelative ("rot").FindPropertyRelative ("z");
            var rotRX = property.FindPropertyRelative ("rotR").FindPropertyRelative ("x");
            var rotRY = property.FindPropertyRelative ("rotR").FindPropertyRelative ("y");
            var rotRZ = property.FindPropertyRelative ("rotR").FindPropertyRelative ("z");
            var rotS = property.FindPropertyRelative ("rotS");
            var sclX = property.FindPropertyRelative ("scl").FindPropertyRelative ("x");
            var sclY = property.FindPropertyRelative ("scl").FindPropertyRelative ("y");
            var sclZ = property.FindPropertyRelative ("scl").FindPropertyRelative ("z");
            var sclRX = property.FindPropertyRelative ("sclR").FindPropertyRelative ("x");
            var sclRY = property.FindPropertyRelative ("sclR").FindPropertyRelative ("y");
            var sclRZ = property.FindPropertyRelative ("sclR").FindPropertyRelative ("z");
            var prob = property.FindPropertyRelative ("probability");

            EditorGUIUtility.labelWidth = 35;
            // Prefab preview
            Rect pvRect = new Rect (p.x+1, p.y, iconSize, iconSize);
            Texture pv = AssetPreview.GetAssetPreview (source.objectReferenceValue as GameObject);
            if (pv == null)
                pv = Texture2D.blackTexture;
            EditorGUI.DrawPreviewTexture (pvRect, pv);

            // transform tab
            Rect tabRect = new Rect (p.x + iconSize + 3, p.y, p.width - iconSize - 3, row);
            DecoPieceEditor.showTab = GUI.SelectionGrid (tabRect, DecoPieceEditor.showTab, tName, 3, "ButtonMid");
            tabRect.y += row2;
            switch (DecoPieceEditor.showTab) {
            case 0:
                DrawTransform (tabRect, posX, posY, posZ, posRX, posRY, posRZ);
                break;
            case 1:
                DrawTransform (tabRect, rotX, rotY, rotZ, rotRX, rotRY, rotRZ);
                break;
            case 2:
                DrawTransform (tabRect, sclX, sclY, sclZ, sclRX, sclRY, sclRZ);
                break;
            }
            // Set Probability & how many side can turn
            tabRect.y += row2 * 1.7f;
            tabRect.width = tabRect.width / 2;
            EditorGUIUtility.labelWidth = 35;
            EditorGUI.Slider (tabRect, prob, 0f, 1.0f, "Prob:");
            tabRect.x += tabRect.width;
            rotS.intValue = EditorGUI.Popup (tabRect, "Side:", rotS.intValue, Enum.GetNames (typeof(turnSide)));

            EditorGUI.EndProperty ();
            EditorGUI.indentLevel = indent;
        }

        void DrawTransform(Rect p, SerializedProperty x, SerializedProperty y, SerializedProperty z, 
            SerializedProperty rx, SerializedProperty ry, SerializedProperty rz)
        {
            float axisW = p.width / 3;

            EditorGUIUtility.labelWidth = labelW;
            Rect r1 = new Rect (p.x, p.y, axisW, row);
            EditorGUI.PropertyField (r1, x, new GUIContent ("X"));
            r1.x += axisW;
            EditorGUI.PropertyField (r1, y, new GUIContent ("Y"));
            r1.x += axisW;
            EditorGUI.PropertyField (r1, z, new GUIContent ("Z"));

            EditorGUIUtility.labelWidth += 5;
            Rect r2 = new Rect (p.x + 5, p.y + row, axisW - 5, row);
            EditorGUI.PropertyField (r2, rx, new GUIContent ("  ±"));
            r2.x += axisW;
            EditorGUI.PropertyField (r2, ry, new GUIContent ("  ±"));
            r2.x += axisW;
            EditorGUI.PropertyField (r2, rz, new GUIContent ("  ±"));
            EditorGUIUtility.labelWidth = labelWdef;
        }

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            return iconSize;
        }
    }
}
