using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(v_AIMotor),true)]
public class v_AIEditor : Editor
{
    GUISkin skin;
    Transform waiPointSelected;
    string[] excludingProperties = new string[] { "onIdle", "onChase", "onPatrol" };
    bool openEvents;
    void OnEnable()
    {
        v_AIMotor motor = (v_AIMotor)target;

        if (motor.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            PopUpLayerInfoEditor window = ScriptableObject.CreateInstance<PopUpLayerInfoEditor>();
            window.position = new Rect(Screen.width, Screen.height / 2, 360, 100);
            window.ShowPopup();
        }
    }

    public void OnSceneGUI()
    {
        if (Selection.activeGameObject != null && (PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.Prefab || !Selection.activeGameObject.activeSelf))
            return;
        v_AIMotor motor = (v_AIMotor)target;
        if (!motor) return;
        if (!motor.displayGizmos) return;
		Handles.color = new Color(0, 0, 0, 0.1f);
		Handles.DrawSolidDisc(motor.transform.position, Vector3.up, motor.distanceToLostTarget);  
        Handles.color = new Color(1, 1, 0, 0.2f);
        Handles.DrawSolidArc(motor.transform.position, Vector3.up, motor.transform.forward, motor.fieldOfView, motor.maxDetectDistance);
        Handles.DrawSolidArc(motor.transform.position, Vector3.up, motor.transform.forward, -motor.fieldOfView, motor.maxDetectDistance);
        Handles.color = new Color(1, 1, 1, 0.5f);
        Handles.DrawWireDisc(motor.transform.position, Vector3.up, motor.maxDetectDistance);		
        Handles.color = new Color(0, 1, 0, 0.1f);
        Handles.DrawSolidDisc(motor.transform.position, Vector3.up, motor.strafeDistance);

        Handles.color = new Color(1, 0, 0, 0.2f);
        Handles.DrawSolidDisc(motor.transform.position, Vector3.up, motor.minDetectDistance);
        Handles.color = new Color(0, 0, 1, 0.2f);
        Handles.DrawSolidDisc(motor.transform.position, Vector3.up, motor.distanceToAttack);
    }

    void CreateSensor(v_AIMotor motor)
    {
        if (Selection.activeGameObject != null && (PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.Prefab || !Selection.activeGameObject.activeSelf))
            return;

        motor.sphereSensor = motor.GetComponentInChildren<v_AISphereSensor>();
        if (motor.sphereSensor != null) return;

        var sensor = new GameObject("SphereSensor");
        var layer = LayerMask.NameToLayer("Triggers");
        sensor.layer = layer;
        sensor.tag = "Weapon";
        motor.sphereSensor = sensor.AddComponent<v_AISphereSensor>();
        sensor.transform.position = motor.transform.position;
        sensor.transform.parent = motor.transform;
        motor.sphereSensor.GetComponent<SphereCollider>().isTrigger = true;
        EditorUtility.SetDirty(motor);
    }

    public override void OnInspectorGUI()
    {
        var oldSkin = GUI.skin;
        if (!skin) skin = Resources.Load("skin") as GUISkin;
       
        GUI.skin = skin;
        v_AIMotor motor = (v_AIMotor) target;
        serializedObject.Update();
        GUILayout.BeginVertical("AI Controller by Invector", "window");

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (!motor) return;

        if (motor.sphereSensor == null)           
            CreateSensor(motor);
        else
            motor.sphereSensor.SetColliderRadius(motor.maxDetectDistance);

        if (motor.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Please assign the Layer of the Character to 'Enemy'", MessageType.Warning);
        }

        if (motor.groundLayer == 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Please assign the Ground Layer to 'Default' ", MessageType.Warning);
        }        
        
        EditorGUILayout.BeginVertical();

        if (Application.isPlaying)
            GUILayout.Box("Current Health: " + motor.currentHealth.ToString());


        DrawPropertiesExcluding(serializedObject, excludingProperties);
        GUILayout.BeginVertical("box");
        openEvents = GUILayout.Toggle(openEvents, (openEvents ? "Close " : "Open ") + "On Change State events ","button");
        GUI.skin = oldSkin;
        if (openEvents)
        {
            
            foreach(string propName in excludingProperties)
            {
                var prop = serializedObject.FindProperty(propName);
                if (prop!=null)
                    EditorGUILayout.PropertyField(prop);
            }
        }
        
        GUILayout.EndVertical();
        GUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
        GUI.skin = oldSkin;
    }

    //**********************************************************************************//
    // DEBUG RAYCASTS                                                                   //
    // draw the casts of the controller on play mode 							        //
    //**********************************************************************************//	
    [DrawGizmo(GizmoType.Selected)]
    private static void CustomDrawGizmos(Transform aTarget, GizmoType aGizmoType)
    {
        #if UNITY_EDITOR
        if (Application.isPlaying)
        {
            v_AIMotor motor = (v_AIMotor)aTarget.GetComponent<v_AIMotor>();
            if (Selection.activeGameObject != null && Selection.activeGameObject.activeSelf&& PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.Prefab || !Selection.activeGameObject.activeSelf)
                return;
            if (!motor || !motor.enabled) return;

            // debug auto crouch
            Vector3 posHead = motor.transform.position + Vector3.up * ((motor._capsuleCollider.height * 0.5f) - motor._capsuleCollider.radius);
            Ray ray1 = new Ray(posHead, Vector3.up);
            Gizmos.DrawWireSphere(ray1.GetPoint((motor.headDetect - (motor._capsuleCollider.radius * 0.1f))), motor._capsuleCollider.radius * 0.9f);
            Handles.Label(ray1.GetPoint((motor.headDetect + (motor._capsuleCollider.radius))), "Head Detection");

            // debug check trigger action
	        //Vector3 yOffSet = new Vector3(0f, -motor.actionRayHeight, 0f);
            //Ray ray2 = new Ray(motor.transform.position - yOffSet, motor.transform.forward);
            //Debug.DrawRay(ray2.origin, ray2.direction * motor.distanceOfRayActionTrigger, Color.white);
            //Handles.Label(ray2.GetPoint(motor.distanceOfRayActionTrigger), "Check for Trigger Actions");
        }
        #endif
    }
}

public class PopUpLayerInfoEditor : EditorWindow
{
    GUISkin skin;
    Vector2 rect = new Vector2(360, 100);

    void OnGUI()
    {
        this.titleContent = new GUIContent("Warning!");
        this.minSize = rect;

        EditorGUILayout.HelpBox("Please assign your EnemyAI to the Layer 'Enemy'.", MessageType.Warning);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("OK", GUILayout.Width(80), GUILayout.Height(20)))
            this.Close();
    }
}