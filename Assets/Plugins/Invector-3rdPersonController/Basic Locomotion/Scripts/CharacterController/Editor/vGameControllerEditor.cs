using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;

[CustomEditor(typeof(vGameController), true)]
public class vGameControllerEditor : Editor
{
    GUISkin skin;
    private Texture2D m_Logo = null;

    void OnEnable()
    {
        m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
    }

    public override void OnInspectorGUI()
    {
        var oldSkin = GUI.skin;
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;        

        GUILayout.BeginVertical("GameController by Invector", "window");
        GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
        GUILayout.Space(5);

        EditorGUILayout.BeginVertical();
        GUI.skin = oldSkin;
        base.OnInspectorGUI();
        GUI.skin = skin;
        GUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
}