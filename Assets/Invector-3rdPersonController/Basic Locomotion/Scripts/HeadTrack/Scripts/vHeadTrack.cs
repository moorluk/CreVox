using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Invector.CharacterController;

public class vHeadTrack : MonoBehaviour
{
    #region variables

    [HideInInspector]
    public float minAngleX = -90f, maxAngleX = 90, minAngleY = -90f, maxAngleY = 90f;

    public Transform head;
    public float strafeHeadWeight = 0.8f;
    public float strafeBodyWeight = 0.8f;
    public float freeHeadWeight = 1f;
    public float freeBodyWeight = 0.4f;
    public float distanceToDetect = 10f;
    public float smooth = 12f;
    public float updateTargetInteration = 1;
    public LayerMask obstacleLayer = 1 << 0;

    [Header("--- Gameobjects Tags to detect ---")]
    public List<string> tagsToDetect = new List<string>() { "LookAt" };

    [Header("--- Animator State Tag to ignore the HeadTrack ---")]
    public List<string> animatorTags = new List<string>() { "Attack", "LockMovement", "CustomAction" };
    public bool followCamera = true;
    public bool useLimitAngle = true;

    [Tooltip("Head Track work with AnimatorIK of LateUpdate (using Invector logic)")]
    public bool useUnityAnimatorIK = false;

    [HideInInspector]
    public Vector2 offsetSpine;
    [HideInInspector]
    public bool updateIK;

    protected List<vLookTarget> targetsInArea = new List<vLookTarget>();

    private float yRotation, xRotation;
    private float _currentHeadWeight, _currentbodyWeight;
    private Animator animator;
    private float headHeight;
    private vLookTarget lookTarget;
    private Transform simpleTarget;
    private List<int> tagsHash;
    private vHeadTrackSensor sensor;
    private float interation;
    vCharacter vchar;
    Vector2 cameraAngle, targetAngle;

    List<Transform> spines;
    float yAngle, xAngle;
    float _yAngle, _xAngle;
    [HideInInspector]
    public UnityEvent onInitUpdate = new UnityEvent();
    [HideInInspector]
    public UnityEvent onFinishUpdate = new UnityEvent();

    #endregion

    void Start()
    {
        if (!sensor)
        {
            var sensorObj = new GameObject("HeadTrackSensor");
            sensor = sensorObj.AddComponent<vHeadTrackSensor>();
        }

        vchar = GetComponent<vCharacter>();

        sensor.headTrack = this;
        animator = GetComponentInParent<Animator>();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        var spine1 = animator.GetBoneTransform(HumanBodyBones.Spine);
        var spine2 = animator.GetBoneTransform(HumanBodyBones.Chest);
        spines = new List<Transform>();
        spines.Add(spine1);
        spines.Add(spine2);
        var neck = animator.GetBoneTransform(HumanBodyBones.Neck);

        if (neck.parent != spine2)
        {
            spines.Add(neck.parent);
        }

        if (head)
        {
            headHeight = Vector3.Distance(transform.position, head.position);
            sensor.transform.position = head.transform.position;
        }
        else
        {
            sensor.transform.position = transform.position;
        }

        var layer = LayerMask.NameToLayer("HeadTrack");
        sensor.transform.parent = transform;
        sensor.gameObject.layer = layer;
        sensor.gameObject.tag = transform.tag;
        tagsHash = new List<int>();

        for (int i = 0; i < animatorTags.Count; i++)
        {
            tagsHash.Add(Animator.StringToHash(animatorTags[i]));
        }
        GetLookPoint();
    }

    Vector3 headPoint { get { return transform.position + (transform.up * headHeight); } }

    void OnAnimatorIK()
    {
        if (!useUnityAnimatorIK) return;
        if (vchar != null && vchar.currentHealth > 0f)
        {
            onInitUpdate.Invoke();
            animator.SetLookAtWeight(_currentHeadWeight, _currentbodyWeight);
            animator.SetLookAtPosition(GetLookPoint());
            onFinishUpdate.Invoke();
        }
    }

    void FixedUpdate()
    {
        updateIK = true;
    }

    void LateUpdate()
    {
        if (animator == null) return;
        if (useUnityAnimatorIK || (!updateIK && animator.updateMode == AnimatorUpdateMode.AnimatePhysics)) return;
        updateIK = false;
        if (vchar != null && vchar.currentHealth > 0f && animator != null && animator.enabled)
        {
            onInitUpdate.Invoke();
            SetLookAtPosition(GetLookPoint(), _currentHeadWeight, _currentbodyWeight);
            onFinishUpdate.Invoke();
        }
    }

    public virtual void SetLookAtPosition(Vector3 point, float strafeHeadWeight, float spineWeight)
    {
        var lookRotation = Quaternion.LookRotation(GetLookPoint() - spines[spines.Count - 1].position);
        var euler = lookRotation.eulerAngles - transform.eulerAngles;

        var y = NormalizeAngle(euler.y);
        var x = NormalizeAngle(euler.x);

        xAngle = Mathf.Clamp(Mathf.Lerp(xAngle, (x), smooth * Time.fixedDeltaTime), minAngleX, maxAngleX);
        yAngle = Mathf.Clamp(Mathf.Lerp(yAngle, (y), smooth * Time.fixedDeltaTime), minAngleY, maxAngleY);

        xAngle = NormalizeAngle(xAngle + Quaternion.Euler(offsetSpine).eulerAngles.x);
        yAngle = NormalizeAngle(yAngle + Quaternion.Euler(offsetSpine).eulerAngles.y);

        foreach (Transform segment in spines)
        {
            var rotX = Quaternion.AngleAxis((xAngle * spineWeight) / spines.Count, segment.InverseTransformDirection(transform.right));
            var rotY = Quaternion.AngleAxis((yAngle * spineWeight) / spines.Count, segment.InverseTransformDirection(transform.up));
            segment.rotation *= rotX * rotY;
        }
        _yAngle = Mathf.Lerp(_yAngle, (yAngle - (yAngle * spineWeight)) * strafeHeadWeight, smooth * Time.fixedDeltaTime);
        _xAngle = Mathf.Lerp(_xAngle, (xAngle - (xAngle * spineWeight)) * strafeHeadWeight, smooth * Time.fixedDeltaTime);
        var _rotX = Quaternion.AngleAxis(_xAngle, head.InverseTransformDirection(transform.right));
        var _rotY = Quaternion.AngleAxis(_yAngle, head.InverseTransformDirection(transform.up));
        head.rotation *= _rotX * _rotY;
    }

    bool lookConditions { get { return head != null && (followCamera && Camera.main != null) || (!followCamera && (lookTarget || simpleTarget)); } }

    Vector3 GetLookPoint()
    {
        var distanceToLoock = 100;
        if (lookConditions && !IgnoreHeadTrack())
        {
            var lookPosition = headPoint + (transform.forward * distanceToLoock);
            if (followCamera)
            {
                lookPosition = (Camera.main.transform.position + (Camera.main.transform.forward * distanceToLoock));
            }

            var dir = lookPosition - headPoint;
            if (lookTarget != null && TargetIsOnRange(lookTarget.lookPoint - headPoint) && lookTarget.IsVisible(headPoint, obstacleLayer))
                dir = lookTarget.lookPoint - headPoint;
            else if (simpleTarget != null)
            {
                dir = simpleTarget.position - headPoint;
            }

            var angle = GetTargetAngle(dir);
            if (useLimitAngle)
            {
                if (TargetIsOnRange(dir))
                {
                    if (animator.GetBool("IsStrafing"))
                        SmoothValues(strafeHeadWeight, strafeBodyWeight, angle.x, angle.y);
                    else
                        SmoothValues(freeHeadWeight, freeBodyWeight, angle.x, angle.y);
                }
                else
                    SmoothValues();
            }
            else
            {
                if (animator.GetBool("IsStrafing"))
                    SmoothValues(strafeHeadWeight, strafeBodyWeight, angle.x, angle.y);
                else
                    SmoothValues(freeHeadWeight, freeBodyWeight, angle.x, angle.y);
            }
            if (targetsInArea.Count > 1)
                SortTargets();
        }
        else
        {
            SmoothValues();
            if (targetsInArea.Count > 1)
                SortTargets();
        }

        var rotA = Quaternion.AngleAxis(yRotation, transform.up);
        var rotB = Quaternion.AngleAxis(xRotation, transform.right);
        var finalRotation = (rotA * rotB);
        var lookDirection = finalRotation * transform.forward;
        return headPoint + (lookDirection * distanceToLoock);
    }

    Vector2 GetTargetAngle(Vector3 direction)
    {
        var lookRotation = Quaternion.LookRotation(direction, transform.up);        //rotation from head to camera point
        var angleResult = lookRotation.eulerAngles - transform.eulerAngles;         // diference between transform rotation and desiredRotation
        Quaternion desiredRotation = Quaternion.Euler(angleResult);                 // convert angleResult to Rotation
        var x = (float)System.Math.Round(NormalizeAngle(desiredRotation.eulerAngles.x), 2);
        var y = (float)System.Math.Round(NormalizeAngle(desiredRotation.eulerAngles.y), 2);
        return new Vector2(x, y);
    }

    bool TargetIsOnRange(Vector3 direction)
    {
        var angle = GetTargetAngle(direction);
        return (angle.x >= minAngleX && angle.x <= maxAngleX && angle.y >= minAngleY && angle.y <= maxAngleY);
    }

    /// <summary>
    /// Set vLookTarget
    /// </summary>
    /// <param name="target"></param>
    public void SetLookTarget(vLookTarget target, bool priority = false)
    {
        if (!targetsInArea.Contains(target)) targetsInArea.Add(target);
        if (priority)
            lookTarget = target;
    }

    /// <summary>
    /// Set Simple target
    /// </summary>
    /// <param name="target"></param>
    public void SetLookTarget(Transform target)
    {
        simpleTarget = target;
    }

    public void RemoveLookTarget(vLookTarget target)
    {
        if (targetsInArea.Contains(target)) targetsInArea.Remove(target);
        if (lookTarget == target) lookTarget = null;
    }

    public void RemoveLookTarget(Transform target)
    {
        if (simpleTarget == target) simpleTarget = null;
    }

    /// <summary>
    /// Make angle to work with -180 and 180 
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    float NormalizeAngle(float angle)
    {
        if (angle < -180)
            return angle + 360;
        else if (angle > 180)
            return angle - 360;
        else
            return angle;
    }

    void ResetValues()
    {
        _currentHeadWeight = 0;
        _currentbodyWeight = 0;
        yRotation = 0;
        xRotation = 0;
    }

    void SmoothValues(float _headWeight = 0, float _bodyWeight = 0, float _x = 0, float _y = 0)
    {
        _currentHeadWeight = Mathf.Lerp(_currentHeadWeight, _headWeight, smooth * Time.deltaTime);
        _currentbodyWeight = Mathf.Lerp(_currentbodyWeight, _bodyWeight, smooth * Time.deltaTime);
        yRotation = Mathf.Lerp(yRotation, _y, smooth * Time.deltaTime);
        xRotation = Mathf.Lerp(xRotation, _x, smooth * Time.deltaTime);
        yRotation = Mathf.Clamp(yRotation, minAngleY, maxAngleY);
        xRotation = Mathf.Clamp(xRotation, minAngleX, maxAngleX);
    }

    void SortTargets()
    {
        interation += Time.deltaTime;
        if (interation > updateTargetInteration)
        {
            interation -= updateTargetInteration;
            if (targetsInArea == null || targetsInArea.Count < 2)
            {
                if (targetsInArea != null && targetsInArea.Count > 0)
                    lookTarget = targetsInArea[0];
                return;
            }

            for (int i = targetsInArea.Count - 1; i >= 0; i--)
            {
                if (targetsInArea[i] == null)
                {
                    targetsInArea.RemoveAt(i);
                }
            }
            targetsInArea.Sort(delegate (vLookTarget c1, vLookTarget c2)
            {
                return Vector3.Distance(this.transform.position, c1 != null ? c1.transform.position : Vector3.one * Mathf.Infinity).CompareTo
                    ((Vector3.Distance(this.transform.position, c2 != null ? c2.transform.position : Vector3.one * Mathf.Infinity)));
            });
            if (targetsInArea.Count > 0)
            {
                lookTarget = targetsInArea[0];
            }
        }
    }

    public void OnDetect(Collider other)
    {
        if (tagsToDetect.Contains(other.gameObject.tag) && other.GetComponent<vLookTarget>() != null)
        {
            lookTarget = other.GetComponent<vLookTarget>();
            var headTrack = other.GetComponentInParent<vHeadTrack>();
            if (!targetsInArea.Contains(lookTarget) && (headTrack == null || headTrack != this))
            {
                targetsInArea.Add(lookTarget);
                SortTargets();
                lookTarget = targetsInArea[0];
            }
        }
    }

    public void OnLost(Collider other)
    {
        if (tagsToDetect.Contains(other.gameObject.tag) && other.GetComponentInParent<vLookTarget>() != null)
        {
            lookTarget = other.GetComponentInParent<vLookTarget>();
            if (targetsInArea.Contains(lookTarget))
            {
                targetsInArea.Remove(lookTarget);
            }
            SortTargets();
            if (targetsInArea.Count > 0)
                lookTarget = targetsInArea[0];
            else
                lookTarget = null;
        }
    }

    public bool IgnoreHeadTrack()
    {
        for (int index = 0; index < animator.layerCount; index++)
        {
            var info = animator.GetCurrentAnimatorStateInfo(index);
            if (tagsHash.Contains(info.tagHash))
            {
                return true;
            }
        }
        return false;
    }
}
