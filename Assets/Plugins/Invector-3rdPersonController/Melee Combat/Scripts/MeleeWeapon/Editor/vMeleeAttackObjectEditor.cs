using UnityEngine;
using System.Collections;
using UnityEditor;
using Invector;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(vMeleeAttackObject), true)]
public class vMeleeAttackObjectEditor : Editor
{
    vMeleeAttackObject attackObject;
    GUISkin skin;
    bool fodoutEvents;
    bool showDefenseRange;

    void OnSceneGUI()
    {
        if (!attackObject) return;
        var meleeWeapon = (attackObject as vMeleeWeapon);
        if (meleeWeapon != null && showDefenseRange)
        {
            if (meleeWeapon.meleeType != vMeleeType.OnlyAttack)
            {
                var root = meleeWeapon.GetComponentInParent<vMeleeManager>();

                if (root)
                {
                    Handles.color = new Color(0, 1, 0, 0.2f);
                    var center = new Vector3(root.transform.position.x, meleeWeapon.transform.position.y, root.transform.position.z);
                    Handles.DrawSolidArc(center, root.transform.up, root.transform.forward, meleeWeapon.defenseRange, 1.5f);
                    Handles.DrawSolidArc(center, root.transform.up, root.transform.forward, -meleeWeapon.defenseRange, 1.5f);
                }
            }
        }
    }

    void OnEnable()
    {
        attackObject = (vMeleeAttackObject)target;
        skin = Resources.Load("skin") as GUISkin;
    }

    public override void OnInspectorGUI()
    {
        var oldSkin = GUI.skin;

        if (skin)        
            GUI.skin = skin;
        
        serializedObject.Update();
        var meleeWeapon = (attackObject as vMeleeWeapon);
        GUILayout.BeginVertical(meleeWeapon != null ? "Melee Weapon" : "Melee Attack Object", "window");

        if (skin) GUILayout.Space(30);

        if (meleeWeapon != null)
        {
            if (meleeWeapon.meleeType == vMeleeType.OnlyAttack)
                EditorGUILayout.HelpBox("The Defense Settings is ignored in this mode", MessageType.Info);
            else if (meleeWeapon.meleeType == vMeleeType.OnlyDefense)
                EditorGUILayout.HelpBox("The Attack Settings is ignored in this mode", MessageType.Info);
        }

        base.OnInspectorGUI();

        if (meleeWeapon != null)
        {
            var root = meleeWeapon.GetComponentInParent<vMeleeManager>();
            if (root && meleeWeapon.meleeType != vMeleeType.OnlyAttack)
            {
                showDefenseRange = EditorGUILayout.Toggle("Show Defense Range", showDefenseRange);
            }
            else showDefenseRange = false;
        }

        GUILayout.BeginVertical("box");

        fodoutEvents = EditorGUILayout.Foldout(fodoutEvents, "Attack Object Events");
        if (fodoutEvents)
        {
            GUI.skin = oldSkin;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onEnableDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onDisableDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onDamageHit"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onRecoilHit"));
            GUI.skin = skin;
        }
        GUILayout.EndVertical();

        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
   
}
