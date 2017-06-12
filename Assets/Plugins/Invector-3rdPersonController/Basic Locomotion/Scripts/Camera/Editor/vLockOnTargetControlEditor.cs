using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;

[CustomEditor(typeof(vLockOnTargetControl),true)]
public class vLockOnTargetControlEditor : Editor
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

        vLockOnTargetControl lockon = (vLockOnTargetControl)target;

        GUILayout.BeginVertical("Lock-On Target", "window");
        GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
        GUILayout.Space(5);

        if (lockon.layerOfObstacles == 0)
        {
            EditorGUILayout.HelpBox("Please assign the Layer of Obstacles to 'Default' ", MessageType.Warning);
        }

        EditorGUILayout.BeginVertical();       
        base.OnInspectorGUI();        
        GUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
}