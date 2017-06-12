using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(vMeleeAttackControl))]
public class vMeleeAttackControlEditor : Editor
{
    vMeleeAttackControl attackControl;
    string currentBodyPart;
    string oldBodyPart;
    bool inAddBodyPart;
    bool inEditBodyPart;
    bool isHuman;
    vAttackType currentAttackType;
    GUISkin skin;
    GUISkin defaultSkin;
    int indexSelected;

    enum WeponSide
    {
        LeftLowerArm, RightLowerArm
    }

    void OnEnable()
    {
        try
        {
            attackControl = (vMeleeAttackControl)target;
            currentAttackType = attackControl.meleeAttackType;
            skin = Resources.Load("skin") as GUISkin;
        }
        catch { }
        indexSelected = -1;
        isHuman = true;
    }

    public override void OnInspectorGUI()
    {
        defaultSkin = GUI.skin;
        if (skin) GUI.skin = skin;
        GUILayout.BeginVertical("EnableDamageControl", "window");
        GUILayout.Space(30);
        base.OnInspectorGUI();
        GUILayout.BeginVertical();

        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.Box("nº", GUILayout.Width(40));
        GUILayout.Box("BodyPart", GUILayout.ExpandWidth(true));
        GUILayout.Box("", GUILayout.Width(20));
        GUILayout.EndHorizontal();
        for (int i = 0; i < attackControl.bodyParts.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUIStyle labelCenter = EditorStyles.miniLabel;
            labelCenter.alignment = TextAnchor.MiddleCenter;
            GUILayout.Box(i.ToString("00"), labelCenter, GUILayout.Width(40));
            Color color = GUI.color;
            GUI.color = indexSelected == i ? new Color(1, 1, 0, 1) : color;
            if (GUILayout.Button(attackControl.bodyParts[i], EditorStyles.miniButton))
            {
                if (indexSelected == i)
                {
                    inEditBodyPart = false;
                    indexSelected = -1;
                    oldBodyPart = "";
                }
                else
                {
                    indexSelected = i;
                    inEditBodyPart = true;
                    oldBodyPart = attackControl.bodyParts[indexSelected];
                    try
                    {
                        oldBodyPart = Enum.Parse(typeof(vHumanBones), oldBodyPart).ToString();
                        isHuman = true;
                    }
                    catch { isHuman = false; }

                }
            }
            GUI.color = color;
            if (attackControl.bodyParts.Count > 1 && !inEditBodyPart && GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                attackControl.bodyParts.RemoveAt(i);
                GUILayout.EndHorizontal();
                break;
            }
            else if (attackControl.bodyParts.Count == 1 || inEditBodyPart)
            {
                GUILayout.Label("", GUILayout.Width(20));
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUILayout.EndVertical();
        if (inEditBodyPart)
        {
            EditBodyPart();
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Add New Body Part", EditorStyles.miniButton))
        {
            inAddBodyPart = true;
            isHuman = true;
            currentBodyPart = "RightLowerArm";
        }
        if (currentAttackType != attackControl.meleeAttackType)
        {
            currentAttackType = attackControl.meleeAttackType;
            if (currentAttackType == vAttackType.MeleeWeapon)
            {
                var noMeleeWeapon = attackControl.bodyParts.FindAll(bodyPart => bodyPart != "LeftLowerArm" || bodyPart != "RightLowerArm");
                if (noMeleeWeapon.Count > 0)
                {
                    attackControl.bodyParts.RemoveAll(bodyPart => !(bodyPart == "LeftLowerArm" || bodyPart == "RightLowerArm"));
                }
            }
        }

        if (inAddBodyPart) AddBodyPart();

        GUILayout.EndVertical();
        GUI.skin = defaultSkin;
    }

    void AddBodyPart()
    {
        GUILayout.BeginVertical("box");

        if (attackControl.meleeAttackType == vAttackType.Unarmed)
        {
            isHuman = Convert.ToBoolean(EditorGUILayout.Popup("Member Type", Convert.ToInt32(isHuman), new string[] { "Generic", "Human" }));
            if (isHuman)
            {
                try
                {
                    currentBodyPart = EditorGUILayout.EnumPopup("Body Part", (vHumanBones)Enum.Parse(typeof(vHumanBones), currentBodyPart)).ToString();
                }
                catch { currentBodyPart = "RightLowerArm"; }
            }
            else
            {
                currentBodyPart = EditorGUILayout.TextField("BodyPart Name", currentBodyPart);
            }
        }
        else
        {
            currentBodyPart = EditorGUILayout.EnumPopup("Body Part", (WeponSide)Enum.Parse(typeof(WeponSide), currentBodyPart)).ToString();
        }
        bool isValid = true;
        if (attackControl.bodyParts.Contains(currentBodyPart))
        {
            EditorGUILayout.HelpBox("This Body Part already exist,select another name", MessageType.Error);
            isValid = false;
        }
        GUILayout.BeginHorizontal();
        if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
        {
            attackControl.bodyParts.Add(currentBodyPart);
            inAddBodyPart = false;

        }
        if (GUILayout.Button("Cancel", EditorStyles.miniButton))
        {
            inAddBodyPart = false;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    void EditBodyPart()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Box("Editing BodyParty " + indexSelected.ToString("00"), GUILayout.ExpandWidth(true));
        if (attackControl.meleeAttackType == vAttackType.Unarmed)
        {
            isHuman = Convert.ToBoolean(EditorGUILayout.Popup("Member Type", Convert.ToInt32(isHuman), new string[] { "Generic", "Human" }));
            if (isHuman)
            {
                try
                {
                    oldBodyPart = EditorGUILayout.EnumPopup("Body Part", (vHumanBones)Enum.Parse(typeof(vHumanBones), oldBodyPart)).ToString();
                }
                catch { oldBodyPart = currentBodyPart = "RightLowerArm"; }
            }
            else
            {
                oldBodyPart = EditorGUILayout.TextField("BodyPart Name", oldBodyPart);
            }
        }
        else
        {
            oldBodyPart = EditorGUILayout.EnumPopup("Body Part", (WeponSide)Enum.Parse(typeof(WeponSide), oldBodyPart)).ToString();
        }
        bool isValid = true;
        if (attackControl.bodyParts.Contains(oldBodyPart) && oldBodyPart != attackControl.bodyParts[indexSelected])
        {
            EditorGUILayout.HelpBox("This Body Part already exist,select another name", MessageType.Error);
            isValid = false;
        }
        GUILayout.BeginHorizontal();
        if (isValid && GUILayout.Button("Ok", EditorStyles.miniButton))
        {
            attackControl.bodyParts[indexSelected] = oldBodyPart;
            inEditBodyPart = false;
            indexSelected = -1;
        }
        if (GUILayout.Button("Cancel", EditorStyles.miniButton))
        {
            indexSelected = -1;
            inEditBodyPart = false;
            oldBodyPart = "";
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
}
