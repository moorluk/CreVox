using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(vObjectDamage))]
public class vObjectDamageEditor : Editor
{
    GUISkin skin;
    void OnEnable()
    {
        skin = Resources.Load("skin") as GUISkin;
    }

    public override void OnInspectorGUI()
    {
        if (skin != null)
            GUI.skin = skin;
        GUILayout.BeginVertical("Object Damage", "window");
        GUILayout.Space(30);
        base.OnInspectorGUI();
        vObjectDamage objDamage = (vObjectDamage)target;
        GUILayout.Space(5);

        if (objDamage.useCollision)
        {
            objDamage.continuousDamage = false;
        }

        objDamage.useCollision = Convert.ToBoolean(EditorGUILayout.Popup("Method", Convert.ToInt32(objDamage.useCollision), new string[] { "OnTriggerEnter", "OnCollisionEnter" }));

        if (!objDamage.useCollision)
        {
            objDamage.continuousDamage = EditorGUILayout.Toggle("Continuos Damage", objDamage.continuousDamage);
            if (objDamage.continuousDamage)
            {
                objDamage.damageFrequency = EditorGUILayout.FloatField("Damage Frequency", objDamage.damageFrequency);
            }
        }
        GUILayout.EndVertical();
        if (GUI.changed) EditorUtility.SetDirty(target);
    }
}
