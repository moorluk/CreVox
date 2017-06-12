using UnityEngine;
using System.Collections;
using UnityEditor;
using Invector;
using System;

public class vCreateMeleeWeaponEditor : EditorWindow
{
    GUISkin skin;
    GameObject obj;  
    Vector2 rect = new Vector2(480, 210);
    Vector2 scrool;

    [MenuItem("Invector/Melee Combat/Create Melee Weapon", false, 3)]
    public static void CreateNewWeapon()
    {
        GetWindow<vCreateMeleeWeaponEditor>();
    }   

    void OnGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        this.minSize = rect;
        this.titleContent = new GUIContent("Melee Weapon", null, "Melee Weapon Creator Window");
       
        GUILayout.BeginVertical("Melee Weapon Creator Window", "window");
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        
        EditorGUILayout.HelpBox("Make sure that your object doens't have any colliders or scripts, just the mesh", MessageType.Info);       

        obj = EditorGUILayout.ObjectField("FBX Model", obj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

        
        if(obj != null && obj.GetComponent<vMeleeWeapon>() != null)
        {
            EditorGUILayout.HelpBox("This gameObject already contains the component vMeleeWeapon", MessageType.Warning);
        }
        
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Need to know how it works?");
        if (GUILayout.Button("Video Tutorial"))
        {
            Application.OpenURL("https://www.youtube.com/watch?v=1aA_PU9-G-0&index=3&list=PLvgXGzhT_qehtuCYl2oyL-LrWoT7fhg9d");
        }
        GUILayout.EndHorizontal();       
      
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (obj != null)
        {
            if (GUILayout.Button("Create"))
                Create();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();       

        GUILayout.EndVertical();
    }   

    /// <summary>
    /// Created the Third Person Controller
    /// </summary>
    void Create()
    {
        var meleeWeapon = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
        meleeWeapon.gameObject.name = obj.name;        
        var weaponObj = new GameObject(meleeWeapon.name);
        weaponObj.transform.position = meleeWeapon.transform.position;
        weaponObj.transform.rotation = meleeWeapon.transform.rotation;
        weaponObj.gameObject.tag = "Weapon";
        var components = new GameObject("Components");
        components.transform.position = meleeWeapon.transform.position;
        components.transform.rotation = meleeWeapon.transform.rotation;
        components.gameObject.tag = "Weapon";

        var hitBox = new GameObject("hitBox", typeof(BoxCollider), typeof(vHitBox));
        hitBox.transform.position = meleeWeapon.transform.position;
        hitBox.transform.rotation = meleeWeapon.transform.rotation;
        hitBox.gameObject.tag = "Weapon";
        var layer = LayerMask.NameToLayer("Ignore Raycast");
        hitBox.gameObject.layer = layer;

        components.transform.SetParent(weaponObj.transform);
        hitBox.transform.SetParent(components.transform);
        var weapon = weaponObj.AddComponent<vMeleeWeapon>();
        weapon.hitBoxes = new System.Collections.Generic.List<vHitBox>();
        weapon.hitBoxes.Add(hitBox.GetComponent<vHitBox>());
        meleeWeapon.transform.SetParent(components.transform);
        meleeWeapon.transform.localPosition = Vector3.zero;
        meleeWeapon.transform.localEulerAngles = Vector3.zero;
        meleeWeapon.gameObject.tag = "Weapon";

        this.Close();
        
    }

}
