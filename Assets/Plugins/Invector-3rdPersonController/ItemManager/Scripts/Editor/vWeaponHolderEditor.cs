using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;

[CanEditMultipleObjects]
[CustomEditor(typeof(vWeaponHolder),true)]
public class vWeaponHolderEditor : Editor
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

        GUILayout.BeginVertical("Weapon Holder", "window");
        GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
        GUILayout.Space(10);

        EditorGUILayout.BeginVertical();       
        base.OnInspectorGUI();
        
        GUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);
    }
}