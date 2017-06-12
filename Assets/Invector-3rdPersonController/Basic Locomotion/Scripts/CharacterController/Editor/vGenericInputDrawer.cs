using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace Invector.CharacterController
{
    [CustomPropertyDrawer(typeof(GenericInput))]
    public class vGenericInputDrawer : PropertyDrawer
    {
        int indexOfInput;
        static string[] inputNames = new string[0];

        static UnityEngine.Object inputManager;
        GUISkin skin;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var oldSkin = GUI.skin;
            if (!skin) skin = Resources.Load("skin") as GUISkin;
            EditorGUI.indentLevel = 0;
            if (skin) GUI.skin = skin;
            var rect1 = position;
            var rect2 = position;
            GUI.Box(position, "");
            rect1.width = 35;
            rect1.height = 15;
            rect1.y += 5;
            rect1.x += 5;
            rect2.width -= 45;
            rect2.height = 15;
            rect2.y += 5;
            rect2.x += 42;
            var useInput = property.FindPropertyRelative("useInput");
            if(useInput!=null)
            {
                useInput.boolValue = GUI.Toggle(rect1, useInput.boolValue, "USE", EditorStyles.miniButton);
                if (useInput.boolValue == false) property.isExpanded = false;
                GUI.enabled = useInput.boolValue;
            }

            property.isExpanded = GUI.Toggle(rect2, property.isExpanded, property.displayName, EditorStyles.miniButton);

            if (property.isExpanded)
            {
                var keyboard = property.FindPropertyRelative("keyboard");
                var joystick = property.FindPropertyRelative("joystick");
                var mobile = property.FindPropertyRelative("mobile");

                var keyboardAxis = property.FindPropertyRelative("keyboardAxis");
                var joystickAxis = property.FindPropertyRelative("joystickAxis");
                var joystickAxisInvert = property.FindPropertyRelative("joystickAxisInvert");
                var keyboardAxisInvert = property.FindPropertyRelative("keyboardAxisInvert");
                var mobileAxisInvert = property.FindPropertyRelative("mobileAxisInvert");
                var mobileAxis = property.FindPropertyRelative("mobileAxis");

                var inputs = GetInputNames();
                position.x +=5;
                position.y += 5;
                position.height = 15;
                position.width -= 70;

                if (keyboard != null && keyboardAxis != null)
                {
                    position.y += 20;                    
                    DrawInputEnum(keyboard, inputs, position, true);
                    var _position = position;
                    _position.width = 35;
                    _position.x = position.width + 20;
                    keyboardAxis.boolValue = GUI.Toggle(_position, keyboardAxis.boolValue, "Axis", EditorStyles.miniButton);
                    _position.width = 20;
                    _position.x += 36;
                    keyboardAxisInvert.boolValue = GUI.Toggle(_position, keyboardAxisInvert.boolValue, "-1", EditorStyles.miniButton);
                }
                if (joystick != null)
                {
                    position.y += 20;
                    DrawInputEnum(joystick, inputs, position);
                    var _position = position;
                    _position.width = 35;
                    _position.x = position.width + 20;                    
                    joystickAxis.boolValue = GUI.Toggle(_position, joystickAxis.boolValue, "Axis", EditorStyles.miniButton);
                    _position.width = 20;
                    _position.x += 36;
                    joystickAxisInvert.boolValue = GUI.Toggle(_position, joystickAxisInvert.boolValue, "-1", EditorStyles.miniButton);
                }
                if (mobile != null)
                {
                    position.y += 20;
                    DrawInputEnum(mobile, inputs, position);
                    var _position = position;
                    _position.width = 35;
                    _position.x = position.width + 20;
                    mobileAxis.boolValue = GUI.Toggle(_position, mobileAxis.boolValue, "Axis", EditorStyles.miniButton);
                    _position.width = 20;
                    _position.x += 36;
                    mobileAxisInvert.boolValue = GUI.Toggle(_position, mobileAxisInvert.boolValue, "-1", EditorStyles.miniButton);
                }
            }
            GUI.enabled = true;
            GUI.skin = oldSkin;
        }

        void DrawInputEnum(SerializedProperty input, string[] inputs, Rect rect, bool withKeys = false)
        {
            if (input != null && inputs.Length > 0)
            {
                var indexOfInput = 0;
                if (withKeys)
                {
                    var keys = Enum.GetNames(typeof(KeyCode));
                    var _inputs = new string[inputs.Length + keys.Length];
                    inputs.CopyTo(_inputs, 0);
                    keys.CopyTo(_inputs, inputs.Length);
                    inputs = _inputs;
                }

                if (Array.Exists(inputs, i => i == input.stringValue))
                    indexOfInput = Array.IndexOf(inputs, input.stringValue);

                indexOfInput = EditorGUI.Popup(rect, input.displayName, indexOfInput, inputs);
                input.stringValue = inputs[indexOfInput];
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return !property.isExpanded ? 25 : 92;
        }

        public static string[] GetInputNames()
        {
            if (!inputManager)
                inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];

            SerializedObject obj = new SerializedObject(inputManager);

            SerializedProperty axisArray = obj.FindProperty("m_Axes");

            if (inputNames.Length != axisArray.arraySize)
                inputNames = new string[axisArray.arraySize];
            for (int i = 0; i < axisArray.arraySize; ++i)
            {
                var axis = axisArray.GetArrayElementAtIndex(i);
                var name = axis.FindPropertyRelative("m_Name").stringValue;
                inputNames[i] = name;
            }

            return inputNames;
        }
    }
}