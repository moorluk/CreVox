using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if MOBILE_INPUT
using UnityStandardAssets.CrossPlatformInput;
#endif
using Invector;

public class vThirdPersonCamera : MonoBehaviour
{
    private static vThirdPersonCamera _instance;
    public static vThirdPersonCamera instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<vThirdPersonCamera>();

                //Tell unity not to destroy this object when loading a new scene!
                //DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

#region inspector properties    
    public Transform target;

    [Tooltip("Lerp speed between Camera States")]
    public float smoothBetweenState = 3f;
    public float smoothCameraRotation = 8f;
    public float scrollSpeed = 10f;

    [Tooltip("What layer will be culled")]
    public LayerMask cullingLayer = 1 << 0;
    [Tooltip("Change this value If the camera pass through the wall")]
    public float clipPlaneMargin;
    public float checkHeightRadius;
    public bool showGizmos;
    [Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
    public bool lockCamera;
#endregion

#region hide properties    
    [HideInInspector]
    public int indexList, indexLookPoint;
    [HideInInspector]
    public float offSetPlayerPivot;
    [HideInInspector]
    public float distance = 5f;
    [HideInInspector]
    public string currentStateName;
    [HideInInspector]
    public Transform currentTarget;
    [HideInInspector]
    public vThirdPersonCameraState currentState;    
    [HideInInspector]
    public vThirdPersonCameraListData CameraStateList;
    [HideInInspector]
    public Transform lockTarget;
    [HideInInspector]
    public Vector2 movementSpeed;
    [HideInInspector]
    public vThirdPersonCameraState lerpState;
    private vLockOnTargetControl lockOn;
    private Transform targetLookAt;
    private Vector3 currentTargetPos;
    private Vector3 lookPoint;
    private Vector3 current_cPos;
    private Vector3 desired_cPos;
    private Vector3 lookTargetOffSet;
    private Camera _camera;    
    private float mouseY = 0f;
    private float mouseX = 0f;
    private float currentHeight;    
    private float currentZoom;
    private float cullingHeight;
    private float cullingDistance;
    private float switchRight, currentSwitchRight;
    private bool useSmooth;
    private bool isNewTarget;    

#endregion

    void OnDrawGizmos()
    {
        if(showGizmos)
        {
            if(currentTarget)
            {

                var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
                Gizmos.DrawWireSphere(targetPos + Vector3.up * cullingHeight, checkHeightRadius);
                Gizmos.DrawLine(targetPos, targetPos + Vector3.up * cullingHeight);
            }
           
        }
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        if (target == null)
            return;

        _camera = GetComponent<Camera>();
        currentTarget = target;
        currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
        targetLookAt = new GameObject("targetLookAt").transform;
        targetLookAt.position = currentTarget.position;
        targetLookAt.hideFlags = HideFlags.HideInHierarchy;
        targetLookAt.rotation = currentTarget.rotation;
        // initialize the first camera state
        mouseY = currentTarget.eulerAngles.x;
        mouseX = currentTarget.eulerAngles.y;
        switchRight = 1f;
        currentSwitchRight = 1f;
        lockOn = GetComponent<vLockOnTargetControl>();
        ChangeState("Default", false);
        currentZoom = currentState.defaultDistance;
        distance = currentState.defaultDistance;
        currentHeight = currentState.height;
        useSmooth = true;
    }

    void FixedUpdate()
    {
        if (target == null || targetLookAt == null || currentState == null || lerpState == null) return;

        switch (currentState.cameraMode)
        {
            case TPCameraMode.FreeDirectional:
                CameraMovement();
                break;
            case TPCameraMode.FixedAngle:
                CameraMovement();
                break;
            case TPCameraMode.FixedPoint:
                CameraFixed();
                break;
        }
    }

    public void SetTargetLockOn(Transform _lockTarget)
    {
        if (_lockTarget != null)
            currentTarget.SendMessage("FindTargetLockOn", _lockTarget, SendMessageOptions.DontRequireReceiver);
        lockTarget = _lockTarget;

        isNewTarget = _lockTarget != null;
    }

    public void ClearTargetLockOn()
    {
        lockTarget = null;
        currentTarget.SendMessage("LostTargetLockOn", SendMessageOptions.DontRequireReceiver);
        var lockOn = GetComponent<vLockOnTargetControl>();
        if (lockOn != null)
            lockOn.StopLockOn();
    }

    /// <summary>
    /// Set the target for the camera
    /// </summary>
    /// <param name="New cursorObject"></param>
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget ? newTarget : target;
    }

    public void SetMainTarget(Transform newTarget)
    {
        target = newTarget;
        currentTarget = newTarget;
        mouseY = currentTarget.rotation.eulerAngles.x;
        mouseX = currentTarget.rotation.eulerAngles.y;
        Init();
    }

    public void UpdateLockOn(bool value)
    {
        if (lockOn)
            lockOn.UpdateLockOn(value);
    }

    /// <summary>    
    /// Convert a point in the screen in a Ray for the world
    /// </summary>
    /// <param name="Point"></param>
    /// <returns></returns>
    public Ray ScreenPointToRay(Vector3 Point)
    {
        return this.GetComponent<Camera>().ScreenPointToRay(Point);
    }

    /// <summary>
    /// Change CameraState
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="Use smoth"></param>
    public void ChangeState(string stateName, bool hasSmooth)
    {
        if (currentState != null && currentState.Name.Equals(stateName)) return;
        // search for the camera state string name
        vThirdPersonCameraState state = CameraStateList != null ? CameraStateList.tpCameraStates.Find(delegate (vThirdPersonCameraState obj) { return obj.Name.Equals(stateName); }) : new vThirdPersonCameraState("Default");

        if (state != null)
        {
            currentStateName = stateName;
            currentState.cameraMode = state.cameraMode;
            lerpState = state; // set the state of transition (lerpstate) to the state finded on the list

            // in case there is no smooth, a copy will be make without the transition values
            if (currentState != null && !hasSmooth)
                currentState.CopyState(state);
        }
        else
        {
            // if the state choosed if not real, the first state will be set up as default
            if (CameraStateList != null && CameraStateList.tpCameraStates.Count > 0)
            {
                state = CameraStateList.tpCameraStates[0];
                currentStateName = state.Name;
                currentState.cameraMode = state.cameraMode;
                lerpState = state;

                if (currentState != null && !hasSmooth)
                    currentState.CopyState(state);
            }
        }
        // in case a list of states does not exist, a default state will be created
        if (currentState == null)
        {
            currentState = new vThirdPersonCameraState("Null");
            currentStateName = currentState.Name;
        }
        if (CameraStateList != null)
            indexList = CameraStateList.tpCameraStates.IndexOf(state);
        currentZoom = state.defaultDistance;
        currentState.fixedAngle = new Vector3(mouseX, mouseY);
        useSmooth = hasSmooth;
        indexLookPoint = 0;

    }

    /// <summary>
    /// Change State using look at point if the cameraMode is FixedPoint  
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="pointName"></param>
    /// <param name="hasSmooth"></param>
    public void ChangeState(string stateName, string pointName, bool hasSmooth)
    {
        useSmooth = hasSmooth;
        if (!currentState.Name.Equals(stateName))
        {
            // search for the camera state string name
            var state = CameraStateList.tpCameraStates.Find(delegate (vThirdPersonCameraState obj)
           {
               return obj.Name.Equals(stateName);
           });

            if (state != null)
            {
                currentStateName = stateName;
                currentState.cameraMode = state.cameraMode;
                lerpState = state; // set the state of transition (lerpstate) to the state finded on the list
                                   // in case there is no smooth, a copy will be make without the transition values
                if (currentState != null && !hasSmooth)
                    currentState.CopyState(state);
            }
            else
            {
                // if the state choosed if not real, the first state will be set up as default
                if (CameraStateList.tpCameraStates.Count > 0)
                {
                    state = CameraStateList.tpCameraStates[0];
                    currentStateName = state.Name;
                    currentState.cameraMode = state.cameraMode;
                    lerpState = state;
                    if (currentState != null && !hasSmooth)
                        currentState.CopyState(state);
                }
            }
            // in case a list of states does not exist, a default state will be created
            if (currentState == null)
            {
                currentState = new vThirdPersonCameraState("Null");
                currentStateName = currentState.Name;
            }

            indexList = CameraStateList.tpCameraStates.IndexOf(state);
            currentZoom = state.defaultDistance;
            currentState.fixedAngle = new Vector3(mouseX, mouseY);
            indexLookPoint = 0;
        }

        if (currentState.cameraMode == TPCameraMode.FixedPoint)
        {
            var point = currentState.lookPoints.Find(delegate (LookPoint obj)
           {
               return obj.pointName.Equals(pointName);
           });
            if (point != null)
            {
                indexLookPoint = currentState.lookPoints.IndexOf(point);
            }
            else
            {
                indexLookPoint = 0;
            }
        }
    }

    /// <summary>
    /// Change the lookAtPoint of current state if cameraMode is FixedPoint
    /// </summary>
    /// <param name="pointName"></param>
    public void ChangePoint(string pointName)
    {
        if (currentState == null || currentState.cameraMode != TPCameraMode.FixedPoint || currentState.lookPoints == null) return;
        var point = currentState.lookPoints.Find(delegate (LookPoint obj) { return obj.pointName.Equals(pointName); });
        if (point != null) indexLookPoint = currentState.lookPoints.IndexOf(point); else indexLookPoint = 0;
    }

    /// <summary>    
    /// Zoom baheviour 
    /// </summary>
    /// <param name="scroolValue"></param>
    /// <param name="zoomSpeed"></param>
    public void Zoom(float scroolValue)
    {
        currentZoom -= scroolValue * scrollSpeed;
    }

    /// <summary>
    /// Camera Rotation behaviour
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void RotateCamera(float x, float y)
    {
        if (currentState.cameraMode.Equals(TPCameraMode.FixedPoint)) return;
        if (!currentState.cameraMode.Equals(TPCameraMode.FixedAngle))
        {
            // lock into a target
            if (lockTarget)
            {
                CalculeLockOnPoint();
            }
            else
            {
                // free rotation 
                mouseX += x * currentState.xMouseSensitivity;
                mouseY -= y * currentState.yMouseSensitivity;

                movementSpeed.x = x;
                movementSpeed.y = -y;
                if (!lockCamera)
                {
                    mouseY = vExtensions.ClampAngle(mouseY, currentState.yMinLimit, currentState.yMaxLimit);
                    mouseX = vExtensions.ClampAngle(mouseX, currentState.xMinLimit, currentState.xMaxLimit);
                }
                else
                {
                    mouseY = currentTarget.root.localEulerAngles.x;
                    mouseX = currentTarget.root.localEulerAngles.y;
                }
            }
        }
        else
        {
            // fixed rotation
            mouseX = currentState.fixedAngle.x;
            mouseY = currentState.fixedAngle.y;
        }
    }

    /// <summary>
    /// Switch Camera Right 
    /// </summary>
    /// <param name="value"></param>
    public void SwitchRight(bool value = false)
    {
        switchRight = value ? -1 : 1;
    }

    void CalculeLockOnPoint()
    {
        if (currentState.cameraMode.Equals(TPCameraMode.FixedAngle) && lockTarget) return;   // check if angle of camera is fixed         
        var collider = lockTarget.GetComponent<Collider>();                                  // collider to get center of bounds

        if (collider == null)
        {
            return;
        }

        var _point = collider.bounds.center;
        Vector3 relativePos = _point - (current_cPos);                      // get position relative to transform
        Quaternion rotation = Quaternion.LookRotation(relativePos);         // convert to rotation

        //convert angle (360 to 180)
        var y = 0f;
        var x = rotation.eulerAngles.y;
        if (rotation.eulerAngles.x < -180)
            y = rotation.eulerAngles.x + 360;
        else if (rotation.eulerAngles.x > 180)
            y = rotation.eulerAngles.x - 360;
        else
            y = rotation.eulerAngles.x;

        mouseY = vExtensions.ClampAngle(y, currentState.yMinLimit, currentState.yMaxLimit);
        mouseX = vExtensions.ClampAngle(x, currentState.xMinLimit, currentState.xMaxLimit);
    }
    
    void CameraMovement()
    {
        if (currentTarget == null)
            return;

        if (useSmooth)
        {
            currentState.Slerp(lerpState, smoothBetweenState * Time.fixedDeltaTime);
        }
        else
            currentState.CopyState(lerpState);

        if (currentState.useZoom)
        {
            currentZoom = Mathf.Clamp(currentZoom, currentState.minDistance, currentState.maxDistance);
            distance = useSmooth ? Mathf.Lerp(distance, currentZoom, lerpState.smoothFollow * Time.fixedDeltaTime) : currentZoom;
        }
        else
        {
            distance = useSmooth ? Mathf.Lerp(distance, currentState.defaultDistance, lerpState.smoothFollow * Time.fixedDeltaTime) : currentState.defaultDistance;
            currentZoom = currentState.defaultDistance;
        }

        _camera.fieldOfView = currentState.fov;
        cullingDistance = Mathf.Lerp(cullingDistance, currentZoom, smoothBetweenState * Time.fixedDeltaTime);
        currentSwitchRight = Mathf.Lerp(currentSwitchRight, switchRight, smoothBetweenState * Time.fixedDeltaTime);
        var camDir = (currentState.forward * targetLookAt.forward) + ((currentState.right * currentSwitchRight) * targetLookAt.right);

        camDir = camDir.normalized;
     
        var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
        currentTargetPos = useSmooth ? Vector3.Lerp(currentTargetPos, targetPos, lerpState.smoothFollow * Time.fixedDeltaTime) : targetPos;
        desired_cPos = targetPos + new Vector3(0, currentState.height, 0);
        current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
        RaycastHit hitInfo;
       
        ClipPlanePoints planePoints = _camera.NearClipPlanePoints(current_cPos + (camDir * (distance)), clipPlaneMargin);
        ClipPlanePoints oldPoints = _camera.NearClipPlanePoints(desired_cPos + (camDir * currentZoom), clipPlaneMargin);
        //Check if Height is not blocked 
        if(Physics.SphereCast(targetPos,checkHeightRadius,Vector3.up,out hitInfo,currentState.cullingHeight + 0.2f, cullingLayer))
        {
            var t = hitInfo.distance - 0.2f;
            t -= currentState.height;
            t /= (currentState.cullingHeight - currentState.height);
            cullingHeight = Mathf.Lerp(currentState.height, currentState.cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));          
        }
        else
        {
            cullingHeight = useSmooth ? Mathf.Lerp(cullingHeight, currentState.cullingHeight, smoothBetweenState * Time.fixedDeltaTime) : currentState.cullingHeight;
        }
        //Check if desired target position is not blocked       
        if (CullingRayCast(desired_cPos, oldPoints, out hitInfo, currentZoom + 0.2f, cullingLayer,Color.blue))
        {
            distance = hitInfo.distance - 0.2f;
            if(distance < currentState.defaultDistance)
            {
                var t = hitInfo.distance;
                t -= currentState.cullingMinDist;
                t /= (currentZoom - currentState.cullingMinDist);
                currentHeight = Mathf.Lerp(cullingHeight, currentState.height, Mathf.Clamp(t, 0.0f, 1.0f));
                current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
            }           
        }
        else
        {          
            currentHeight = useSmooth ? Mathf.Lerp(currentHeight, currentState.height, smoothBetweenState * Time.fixedDeltaTime) : currentState.height;            
        }
        //Check if target position with culling height applied is not blocked
        if (CullingRayCast(current_cPos, planePoints, out hitInfo, distance, cullingLayer, Color.cyan)) distance = Mathf.Clamp(cullingDistance, 0.0f, currentState.defaultDistance);
        var lookPoint = current_cPos + targetLookAt.forward * 2f;
        lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));
        targetLookAt.position = current_cPos;

        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
        targetLookAt.rotation = useSmooth ? Quaternion.Slerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.fixedDeltaTime) : newRot;
        transform.position = current_cPos + (camDir * (distance));
        var rotation = Quaternion.LookRotation((lookPoint) - transform.position);
        if (lockTarget)
        {
            if (!(currentState.cameraMode.Equals(TPCameraMode.FixedAngle)))
            {
                var collider = lockTarget.GetComponent<Collider>();
                if (collider != null)
                {
                    var point = collider.bounds.center - transform.position;
                    var euler = Quaternion.LookRotation(point).eulerAngles - rotation.eulerAngles;
                    if (isNewTarget)
                    {
                        lookTargetOffSet = Vector3.MoveTowards(lookTargetOffSet, euler, currentState.smoothFollow * Time.fixedDeltaTime);
                        if (Vector3.Distance(lookTargetOffSet, euler) < 1f)
                            isNewTarget = false;
                    }
                    else
                        lookTargetOffSet = euler;
                }
            }
        }
        else
        {
            lookTargetOffSet = Vector3.Lerp(lookTargetOffSet, Vector3.zero, 1 * Time.fixedDeltaTime);
        }
        rotation.eulerAngles += currentState.rotationOffSet ;       
        var _rot = Quaternion.Euler(rotation.eulerAngles.x+lookTargetOffSet.x, rotation.eulerAngles.y+ lookTargetOffSet.y, lookTargetOffSet.z);
        transform.rotation = _rot;
        movementSpeed = Vector2.zero;
    }
    
    void CameraFixed()
    {
        if (useSmooth) currentState.Slerp(lerpState, smoothBetweenState);
        else currentState.CopyState(lerpState);

        var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot + currentState.height, currentTarget.position.z);
        currentTargetPos = useSmooth ? Vector3.MoveTowards(currentTargetPos, targetPos, currentState.smoothFollow * Time.fixedDeltaTime) : targetPos;
        current_cPos = currentTargetPos;
        var pos = isValidFixedPoint ? currentState.lookPoints[indexLookPoint].positionPoint : transform.position;
        transform.position = useSmooth ? Vector3.Lerp(transform.position, pos, currentState.smoothFollow * Time.fixedDeltaTime) : pos;
        targetLookAt.position = current_cPos;
        if (isValidFixedPoint && currentState.lookPoints[indexLookPoint].freeRotation)
        {
            var rot = Quaternion.Euler(currentState.lookPoints[indexLookPoint].eulerAngle);          
            transform.rotation = useSmooth ? Quaternion.Slerp(transform.rotation, rot, (currentState.smoothFollow * 0.5f) * Time.fixedDeltaTime) : rot;
        }
        else if (isValidFixedPoint)
        {            
            var rot = Quaternion.LookRotation(currentTargetPos - transform.position);          
            transform.rotation = useSmooth ? Quaternion.Slerp(transform.rotation, rot, (currentState.smoothFollow) * Time.fixedDeltaTime) : rot;
        }
        _camera.fieldOfView = currentState.fov;
    }
    
    bool isValidFixedPoint
    {
        get
        {
            return (currentState.lookPoints != null && currentState.cameraMode.Equals(TPCameraMode.FixedPoint) && (indexLookPoint < currentState.lookPoints.Count || currentState.lookPoints.Count > 0));
        }
    }
    
    bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer,Color color)
    {
        bool value = false;
        if (showGizmos)
        {
            Debug.DrawRay(from, _to.LowerLeft - from,color);
            Debug.DrawLine(_to.LowerLeft, _to.LowerRight, color);
            Debug.DrawLine(_to.UpperLeft, _to.UpperRight, color);
            Debug.DrawLine(_to.UpperLeft, _to.LowerLeft, color);
            Debug.DrawLine(_to.UpperRight, _to.LowerRight, color);
            Debug.DrawRay(from, _to.LowerRight - from, color);
            Debug.DrawRay(from, _to.UpperLeft - from, color);
            Debug.DrawRay(from, _to.UpperRight - from, color);
        }
        if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            cullingDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        return value;
    }
}
