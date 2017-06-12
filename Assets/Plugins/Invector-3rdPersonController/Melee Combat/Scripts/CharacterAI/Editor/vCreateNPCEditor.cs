using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Invector;
using Invector.CharacterController;
using UnityEngine.EventSystems;

#if UNITY_5_5_OR_NEWER
using UnityEngine.AI;
#endif

public class vCreateEnemyEditor : EditorWindow
{
    GUISkin skin;
    GameObject charObj;
    Animator charAnimator;
    RuntimeAnimatorController controller;
    Vector2 rect = new Vector2(500, 630);
    Vector2 scrool;
    Editor humanoidpreview;

    public enum CharacterType
    {
        EnemyAI,
        CompanionAI        
    }

    public CharacterType charType = CharacterType.EnemyAI;

    /// <summary>
    /// 3rdPersonController Menu 
    /// </summary>    
	[MenuItem("Invector/Melee Combat/Create NPC", false, 1)]
    public static void CreateNewCharacter()
    {
        GetWindow<vCreateEnemyEditor>();
    }

    bool isHuman, isValidAvatar, charExist;

    void OnGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        this.minSize = rect;
        this.titleContent = new GUIContent("Character", null, "EnemyAI Character Creator");

        GUILayout.BeginVertical("Character Creator Window", "window");
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");

        charType = (CharacterType)EditorGUILayout.EnumPopup("Character Type", charType);

        if (!charObj)
            EditorGUILayout.HelpBox("Make sure to select the FBX model and not a Prefab already with components attached!", MessageType.Info);
        else if (!charExist)
            EditorGUILayout.HelpBox("Missing a Animator Component", MessageType.Error);
        else if (!isHuman)
            EditorGUILayout.HelpBox("This is not a Humanoid", MessageType.Error);
        else if (!isValidAvatar)
            EditorGUILayout.HelpBox(charObj.name + " is a invalid Humanoid", MessageType.Info);

        charObj = EditorGUILayout.ObjectField("FBX Model", charObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

        if (GUI.changed && charObj != null && charObj.GetComponent<v_AIController>()==null)
            humanoidpreview = Editor.CreateEditor(charObj);
        if (charObj != null && charObj.GetComponent<v_AIController>() != null)
        {
            EditorGUILayout.HelpBox("This gameObject already contains the component v_AIController", MessageType.Warning);
        }
        controller = EditorGUILayout.ObjectField("Animator Controller: ", controller, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Need to know how it works?");
        if (GUILayout.Button("Video Tutorial"))
        {
            Application.OpenURL("https://www.youtube.com/watch?v=tuwg-H8vjqY&list=PLvgXGzhT_qehtuCYl2oyL-LrWoT7fhg9d&index=3");
        }
        GUILayout.EndHorizontal();

        if (charObj)
            charAnimator = charObj.GetComponent<Animator>();
        charExist = charAnimator != null;
        isHuman = charExist ? charAnimator.isHuman : false;
        isValidAvatar = charExist ? charAnimator.avatar.isValid : false;

        if (CanCreate())
        {
            DrawHumanoidPreview();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (controller != null)
            {
                if (GUILayout.Button("Create"))
                    Create();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    bool CanCreate()
    {
        return isValidAvatar && isHuman && charObj != null && charObj.GetComponent<v_AIController>() == null; ;
    }

    /// <summary>
    /// Draw the Preview window
    /// </summary>
    void DrawHumanoidPreview()
    {
        GUILayout.FlexibleSpace();

        if (humanoidpreview != null)
        {
            humanoidpreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(100, 400), "window");
        }
    }

    /// <summary>
    /// Created the AI Controller
    /// </summary>
    void Create()
    {
        // base for the character
        var _3rdPerson = GameObject.Instantiate(charObj, Vector3.zero, Quaternion.identity) as GameObject;
        if (!_3rdPerson)
            return;

        if (charType == CharacterType.CompanionAI)
        {
            _3rdPerson.tag = "CompanionAI";
            _3rdPerson.name = "vCompanionAI";
            _3rdPerson.AddComponent<v_AICompanion>();

            var p_layer = LayerMask.NameToLayer("CompanionAI");
            _3rdPerson.layer = p_layer;
            foreach (Transform t in _3rdPerson.transform.GetComponentsInChildren<Transform>())
                t.gameObject.layer = p_layer;
        }
        else
        {
            _3rdPerson.name = "vEnemyAI";
            _3rdPerson.tag = "Enemy";
            _3rdPerson.AddComponent<v_AIController>();

            var p_layer = LayerMask.NameToLayer("Enemy");
            _3rdPerson.layer = p_layer;
            foreach (Transform t in _3rdPerson.transform.GetComponentsInChildren<Transform>())
                t.gameObject.layer = p_layer;
        }        
        
        // rigidbody settings
        var rigidbody = _3rdPerson.AddComponent<Rigidbody>();
        rigidbody.useGravity = true;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rigidbody.mass = 50;

        // capsule collider settings
        var collider = _3rdPerson.AddComponent<CapsuleCollider>();
        collider.height = ColliderHeight(_3rdPerson.GetComponent<Animator>());
        collider.center = new Vector3(0, (float)System.Math.Round(collider.height * 0.5f, 2), 0);
        collider.radius = (float)System.Math.Round(collider.height * 0.15f, 2);        

        // navmesh settings
        var navMesh = _3rdPerson.AddComponent<NavMeshAgent>();
        navMesh.radius = 0.4f;
        navMesh.height = 1.8f;
        navMesh.speed = 1f;
        navMesh.angularSpeed = 300f;
        navMesh.acceleration = 8f;
        navMesh.stoppingDistance = 2f;
        navMesh.autoBraking = false;

        if (controller)
            _3rdPerson.GetComponent<Animator>().runtimeAnimatorController = controller;

        this.Close();

    }

    /// <summary>
    /// Capsule Collider height based on the Character height
    /// </summary>
    /// <param name="animator">animator humanoid</param>
    /// <returns></returns>
    float ColliderHeight(Animator animator)
    {
        var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        return (float)System.Math.Round(Vector3.Distance(foot.position, hips.position) * 2f, 2);
    }  

}
