using UnityEngine;
using System.Collections;
using UnityEditor;
using Invector;

[CanEditMultipleObjects]
[CustomEditor(typeof(vCharacterStandalone), true)]
public class vCharacterStandaloneEditor : Editor
{
    GUISkin skin;
    private Texture2D m_Logo = null;

    void OnEnable()
    {
        m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
    }   

    public override void OnInspectorGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;        
        GUILayout.BeginVertical("Character Standalone", "window");

        GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Add this component on objects to make it a target", MessageType.Info);

        base.OnInspectorGUI();        

        EditorGUILayout.EndVertical();

        GUILayout.Space(10);
    }
}
