using UnityEngine;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(PaletteItem)),CanEditMultipleObjects]
	public class PaletteItemEditor : Editor
	{
        SerializedObject m_pi;

        SerializedProperty p_markType;
        SerializedProperty p_set;
        SerializedProperty p_module;
        SerializedProperty p_itemName;
        SerializedProperty p_inspectedScript;
        SerializedProperty p_assetPath;


        public static Color volColor = new Color (0.5f, 0.8f, 0.75f);

		void OnEnable ()
		{
			m_pi = new SerializedObject(targets);
        }

		public override void OnInspectorGUI ()
		{
			EditorGUIUtility.wideMode = true;

            m_pi.Update();
            p_markType = m_pi.FindProperty("markType");
            p_set = m_pi.FindProperty("m_set");
            p_module = m_pi.FindProperty("m_module");
            p_itemName = m_pi.FindProperty("itemName");
            p_inspectedScript = m_pi.FindProperty("inspectedScript");
            p_assetPath = m_pi.FindProperty("assetPath");

            EditorGUILayout.PropertyField(p_markType);
            p_set.intValue = EditorGUILayout.MaskField("Set",p_set.intValue,System.Enum.GetNames(typeof(PaletteItem.Set)));
            EditorGUILayout.PropertyField(p_module);
            EditorGUILayout.PropertyField(p_itemName);
            EditorGUILayout.PropertyField(p_inspectedScript);
            EditorGUILayout.PropertyField(p_assetPath);

            m_pi.ApplyModifiedProperties();
        }
	}
}
