using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Invector;
using Invector.CharacterController;
using UnityEngine.EventSystems;
using Invector.CharacterController.Actions;

public class vCreateMeleeCharacterEditor : EditorWindow
{
    GUISkin skin;
    GameObject charObj;
    Animator charAnimator;
    RuntimeAnimatorController controller;
    vThirdPersonCameraListData cameraListData;
    GameObject hud;
    Vector2 rect = new Vector2(500, 630);
    Vector2 scrool;
	Editor humanoidpreview;
		
    /// <summary>
	/// 3rdPersonController Menu 
    /// </summary>    
    [MenuItem("Invector/Melee Combat/Create Melee Controller", false, 0)]
    public static void CreateNewCharacter()
    {
        GetWindow<vCreateMeleeCharacterEditor>();
    }
   
    bool isHuman, isValidAvatar, charExist;
    void OnEnable()
    {
        hud = Resources.Load("HUD") as GameObject;
    }

    void OnGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        this.minSize = rect;
        this.titleContent = new GUIContent("Character", null, "Third Person Character Creator");
       
        GUILayout.BeginVertical("Character Creator Window", "window");
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");

        if (!charObj)
            EditorGUILayout.HelpBox("Make sure your FBX model is set as Humanoid!", MessageType.Info);
        else if (!charExist)
            EditorGUILayout.HelpBox("Missing a Animator Component", MessageType.Error);
        else if (!isHuman)
            EditorGUILayout.HelpBox("This is not a Humanoid", MessageType.Error);
        else if (!isValidAvatar)
            EditorGUILayout.HelpBox(charObj.name + " is a invalid Humanoid", MessageType.Info);

        charObj = EditorGUILayout.ObjectField("FBX Model", charObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

        if (GUI.changed && charObj != null && charObj.GetComponent<vThirdPersonController>()==null)
            humanoidpreview = Editor.CreateEditor(charObj);
        if(charObj != null && charObj.GetComponent<vThirdPersonController>() != null)
        {
            EditorGUILayout.HelpBox("This gameObject already contains the component vThirdPersonController", MessageType.Warning);
        }
        controller = EditorGUILayout.ObjectField("Animator Controller: ", controller, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
        cameraListData = EditorGUILayout.ObjectField("Camera List Data: ", cameraListData, typeof(vThirdPersonCameraListData), false) as vThirdPersonCameraListData;
        hud = EditorGUILayout.ObjectField("Hud Controller: ", hud, typeof(GameObject), false) as GameObject;
        if(hud!=null && hud.GetComponent<vHUDController>()==null)
        {
            EditorGUILayout.HelpBox("This object does not contain a vHUDController", MessageType.Warning);
        }
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Need to know how it works?");
        if (GUILayout.Button("Video Tutorial"))
        {
            Application.OpenURL("https://www.youtube.com/watch?v=KQ5xha36tfE");
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
        return isValidAvatar && isHuman && charObj!=null && charObj.GetComponent<vThirdPersonController>()==null;
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
    /// Created the Third Person Controller
    /// </summary>
    void Create()
    {
        // base for the character
        var _3rdPerson = GameObject.Instantiate(charObj, Vector3.zero, Quaternion.identity) as GameObject;
        if (!_3rdPerson)
            return;             

        _3rdPerson.name = "vThirdPersonMelee";        
        _3rdPerson.AddComponent<vThirdPersonController>();
        _3rdPerson.AddComponent<vMeleeCombatInput>();
        _3rdPerson.AddComponent<vGenericAction>();

        var rigidbody = _3rdPerson.AddComponent<Rigidbody>();
        var collider = _3rdPerson.AddComponent<CapsuleCollider>();

        // camera
        GameObject camera = null;
        if (Camera.main == null)
        {
            var cam = new GameObject("vThirdPersonCamera");
            cam.AddComponent<Camera>();
            cam.AddComponent<FlareLayer>();
            cam.AddComponent<GUILayer>();
            cam.AddComponent<AudioListener>();
            camera = cam;
            camera.GetComponent<Camera>().tag = "MainCamera";
            camera.GetComponent<Camera>().nearClipPlane = 0.01f;
        }
        else
        {
            camera = Camera.main.gameObject;
            camera.gameObject.name = "vThirdPersonCamera";
        }
        var tpcamera = camera.GetComponent<vThirdPersonCamera>();

        if (tpcamera == null)
            tpcamera = camera.AddComponent<vThirdPersonCamera>();

        // define the camera cursorObject       
        tpcamera.target = _3rdPerson.transform;       
        if (cameraListData != null)
        {
            tpcamera.CameraStateList = cameraListData;
        }

        GameObject gC = null;
        var gameController = FindObjectOfType<vGameController>();
        if (gameController == null)
        {
            gC = new GameObject("vGameController");
            gC.AddComponent<vGameController>();
        }
        
        CreateHud();
        _3rdPerson.tag = "Player";

        var p_layer = LayerMask.NameToLayer("Player");
        _3rdPerson.layer = p_layer;

        foreach (Transform t in _3rdPerson.transform.GetComponentsInChildren<Transform>())
            t.gameObject.layer = p_layer;

        var s_layer = LayerMask.NameToLayer("StopMove");
        _3rdPerson.GetComponent<vThirdPersonMotor>().stopMoveLayer = LayerMask.GetMask(LayerMask.LayerToName(s_layer));        

        // rigidbody
        rigidbody.useGravity = true;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rigidbody.mass = 50;

        // capsule collider 
        collider.height = ColliderHeight(_3rdPerson.GetComponent<Animator>());
        collider.center = new Vector3(0, (float)System.Math.Round(collider.height * 0.5f, 2), 0);
        collider.radius = (float)System.Math.Round(collider.height * 0.15f, 2);

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

    /// <summary>
    /// Return Hud Object
    /// </summary>
    /// <returns></returns>
    vHUDController CreateHud()
    {       
        var _hud = FindObjectOfType<vHUDController>();
        if (_hud) return _hud;
        if (hud == null) return null;
        var canvas = FindObjectOfType<Canvas>();
        if (FindObjectOfType<EventSystem>() == null)
        {
            #if UNITY_5_3_OR_NEWER
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            #else
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule), typeof(TouchInputModule));
            #endif
        }
        if (canvas == null || (canvas != null && !canvas.tag.Equals("PlayerUI")))
        {
            var canvasObj = new GameObject("vUI");
            canvasObj.tag = "PlayerUI";
            canvas = canvasObj.AddComponent<Canvas>();
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        if (canvas.GetComponent<UnityEngine.UI.CanvasScaler>() != null)
        {
            canvas.GetComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvas.GetComponent<UnityEngine.UI.CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
        ////Check HUD
        //RectTransform Hud = canvas.transform.FindChild("HUD") as RectTransform;
        GameObject hudObject = null;
        if (_hud == null)
        {
            hudObject = Instantiate(hud);
           
            if (hudObject)
                hudObject.GetComponent<RectTransform>().SetParent(canvas.transform);
            var rect = hudObject.GetComponent<RectTransform>();
            rect.offsetMax = new Vector2(0, 0);

            hudObject.name = "HUD";
        }
        if (hudObject.GetComponent<vHUDController>() == null)
            hudObject.gameObject.AddComponent<vHUDController>();
        //HUD Components       

        return hudObject.GetComponent<vHUDController>();
    }

}
