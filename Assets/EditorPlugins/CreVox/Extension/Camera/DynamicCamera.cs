using UnityEngine;
using System.Collections;

public class DynamicCamera : MonoBehaviour {
    
    public bool isEnable = false;
    [Range(-180f,180f)]
	public float minAngleP = 0;
    [Range(-180,180f)]
	public float maxAngleP = 0;
    [Range(-180f,0f)]
    public float minAngleY = 0;
    [Range(0,180f)]
	public float maxAngleY = 0;
	public float minDist;
	public float maxDist;
    
    public float minSafeFrameHorizon = 0.49f; 
    public float maxSafeFrameHorizon = 0.51f;
    public float minSafeFrameVertical = 0.49f;
    public float maxSafeFrameVertical = 0.51f;

    //[HideInInspector]
    public Camera cam;
    //[HideInInspector]
    public Transform target = null;
    [HideInInspector]
    public Vector3 oldRot
    {
        get
        {
            return transform.rotation.eulerAngles;
        }
        set
        {
            transform.rotation = Quaternion.Euler(value);
        }
    }
    [HideInInspector]
    public Vector3 oldPos
    {
        get
        {
            return transform.position;
        }
        set
        {
            transform.position = value;
        }
    }

    private Vector3 cachePos = Vector3.zero;
    private Vector3 cachEulers = Vector3.zero;
    public Vector3 resultPos
    {
        get
        {
            return oldPos + cachePos;
        }
    }
    public Vector3 resultEulers
    {
        get
        {
            return cachEulers;
        }
    }
        
    void Start()
    {
        m_lastUpdateTime = 0;
    }
    public void Update () 
    {
        if (isEnable == true)
        {
            UpdateCamera();
            ApplyToMainCamera();
        }
    }

    [HideInInspector]
    public float m_lastUpdateTime = 0;
    public void UpdateCamera()
    {
        if (target == null && GameObject.FindWithTag("Look") != null) 
        {
            target = GameObject.FindWithTag("Look").transform;
        }
        cachePos = Vector3.zero;
        cachEulers = Vector3.zero;

        Camera c = cam.GetComponent<Camera>();
        LookAtTargetInWorld(c);
        CheckEulerRange();
        MoveBySafeFrame(c);
        MoveDist();

        m_lastUpdateTime = Time.time;
    }

    void MoveDist()
    {
        Vector3 camPos = cam.transform.position + cachePos;
        Vector3 camForward = Quaternion.Euler(cachEulers) * Vector3.forward;

        Plane targetPlane = new Plane(camForward * -1, target.position);
        float camToTargetPlaneDis = (targetPlane.GetDistanceToPoint(camPos));

        Vector3 camToPlayerDir = target.position - camPos;
        float camToPlayerDis = camToPlayerDir.magnitude;

        float distancePct = 0;
        if (camToTargetPlaneDis > maxDist)
        {
            distancePct = (camToTargetPlaneDis - maxDist) / camToTargetPlaneDis;
        }
        else if (camToTargetPlaneDis < minDist)
        {
            distancePct = (camToTargetPlaneDis - minDist) / camToTargetPlaneDis;
        }
        if (distancePct == 0)
        {
            return;
        }
        //Debug.Log("MoveDist : " + result);

        cachePos += distancePct * camToPlayerDis * camToPlayerDir.normalized;
    }
    Vector3 IsInNewSafeFrame(Camera c)
    {
        Vector3 targetInViewPort = c.WorldToViewportPoint(target.position);
        Vector3 result = Vector3.zero;
        if (targetInViewPort.x >= maxSafeFrameHorizon)
        {
            result.x = targetInViewPort.x - maxSafeFrameHorizon;
        }
        else if (targetInViewPort.x <= minSafeFrameHorizon)
        {
            result.x = targetInViewPort.x - minSafeFrameHorizon;
        }
        if (targetInViewPort.y >= maxSafeFrameVertical)
        {
            result.y = targetInViewPort.y - maxSafeFrameVertical;
        }
        else if (targetInViewPort.y <= minSafeFrameVertical)
        {
            result.y = targetInViewPort.y - minSafeFrameVertical;
        }
        if (targetInViewPort.z >= 0)
        {
            result.z = 0;
        }
        else
        {
            result.z = 1;
        }
        /*if (result != Vector2.zero)
        {
            Debug.Log("Is Not In Safe Frame : (" + result.x + "," + result.y + ")");
        }
        /*if (result != Vector2.zero)
        {
            Debug.Log("Is Not In Safe Frame : (" + result.x + "," + result.y + ")");
        }*/

        return result;
    }
    void LookAtTargetInWorld(Camera c)
    {
        Vector3 a_safeFrameExceed = IsInNewSafeFrame(c);
        if (a_safeFrameExceed.z > 0)
        {
            a_safeFrameExceed.x *= -1;
            a_safeFrameExceed.y *= -1;
        }
        Plane nForward_pTarget = new Plane(c.transform.forward, target.position);
        float camToTargetPlaneDist = Mathf.Abs(nForward_pTarget.GetDistanceToPoint(c.transform.position));

        Vector3 oldMaxInSafeFrame;
        oldMaxInSafeFrame.x = (a_safeFrameExceed.x > 0 ? maxSafeFrameHorizon : minSafeFrameHorizon);
        oldMaxInSafeFrame.y = (a_safeFrameExceed.y > 0 ? maxSafeFrameVertical : minSafeFrameVertical);
        oldMaxInSafeFrame.z = camToTargetPlaneDist;
        Vector3 targetWorldPoint = c.WorldToViewportPoint(target.position);
        Vector3 maxInSafeFrame;
        maxInSafeFrame.x = Mathf.Clamp(targetWorldPoint.x, minSafeFrameHorizon, maxSafeFrameHorizon);
        maxInSafeFrame.y = Mathf.Clamp(targetWorldPoint.y, minSafeFrameVertical, maxSafeFrameVertical);
        maxInSafeFrame.z = camToTargetPlaneDist;
        maxInSafeFrame = oldMaxInSafeFrame;

        Vector3 viewPortHorizonVec = c.ViewportToWorldPoint(new Vector3(1f,0,camToTargetPlaneDist)) - c.ViewportToWorldPoint(new Vector3(0,0,camToTargetPlaneDist));
        Vector3 viewPortVerticalVec = c.ViewportToWorldPoint(new Vector3(0,1f,camToTargetPlaneDist)) - c.ViewportToWorldPoint(new Vector3(0,0,camToTargetPlaneDist));

        Vector3 diffEuler = Vector3.zero;
        Vector3 maxPointToWorldVec = (c.ViewportToWorldPoint(maxInSafeFrame) - c.transform.position);

        if (a_safeFrameExceed.x != 0)
        {
            Vector3 maxSafeFrameInWorld = c.ViewportToWorldPoint(maxInSafeFrame);
            Vector3 maxToTargetVec = target.position - maxSafeFrameInWorld;
            Vector3 exceedHorizonPoint = maxSafeFrameInWorld + Vector3.Project(maxToTargetVec, viewPortHorizonVec);
            Vector3 exceedPointToWorldVec = (exceedHorizonPoint - c.transform.position);
            //Debug.Log("old y : " + diffEuler.y);
            diffEuler.y = Vector3.Angle(maxPointToWorldVec, exceedPointToWorldVec) * (a_safeFrameExceed.x > 0 ? 1 : -1);

            //Debug.Log("new y : " + diffEuler.y);
        }
        if (a_safeFrameExceed.y != 0)
        {
            Vector3 maxSafeFrameInWorld = c.ViewportToWorldPoint(maxInSafeFrame);
            Vector3 maxToTargetVec = target.position - maxSafeFrameInWorld;
            Vector3 exceedVerticalPoint = maxSafeFrameInWorld + Vector3.Project(maxToTargetVec, viewPortVerticalVec);
            Vector3 exceedPointToWorldVec = (exceedVerticalPoint - c.transform.position);
            //Debug.Log("old x : " + diffEuler.x);
            diffEuler.x = Vector3.Angle(maxPointToWorldVec, exceedPointToWorldVec) * (a_safeFrameExceed.y > 0 ? -1 : 1);
            //Debug.Log("new x : " + diffEuler.x);
        }
        if (diffEuler != Vector3.zero)
        {
            //Debug.Log("LookAtTargetInWorld turn : " + (diffEuler));
            cachEulers += diffEuler;
        }
    }
    void CheckEulerRange()
    {
        Vector3 resultEuler = cachEulers + oldRot;
        //Debug.Log("min : " + (oldRot.x + minAngleP) + "   angle : " + resultEuler.x + "   min : " + (oldRot.x + maxAngleP));
        resultEuler.x = ClampAngle (resultEuler.x, oldRot.x + minAngleP, oldRot.x + maxAngleP);
        //Debug.Log("min : " + (oldRot.y + minAngleY) + "   angle : " + resultEuler.y + "   min : " + (oldRot.y + maxAngleY));
        resultEuler.y = ClampAngle (resultEuler.y, oldRot.y + minAngleY, oldRot.y + maxAngleY);
        resultEuler.z = 0;
        cachEulers = resultEuler;
    }

    Vector3 m_Left2Plane;
    Vector3 m_Right2Plane;
    Vector3 m_Up2Plane;
    Vector3 m_Down2Plane;
    void MoveBySafeFrame(Camera c)
    {
        Plane nForward_pTarget = new Plane(c.transform.forward, target.position);
        float camToTargetPlaneDist = Mathf.Abs(nForward_pTarget.GetDistanceToPoint(c.transform.position));
        
        Vector3 targetWorldPoint = c.WorldToViewportPoint(target.position);

        Vector3 worldLeftPos = c.ViewportToWorldPoint(new Vector3(minSafeFrameHorizon, 0.5f, camToTargetPlaneDist));
        Vector3 worldRightPos = c.ViewportToWorldPoint(new Vector3(maxSafeFrameHorizon, 0.5f, camToTargetPlaneDist));
        Vector3 worldUpPos = c.ViewportToWorldPoint(new Vector3(0.5f, maxSafeFrameVertical, camToTargetPlaneDist));
        Vector3 worldDownPos = c.ViewportToWorldPoint(new Vector3(0.5f, minSafeFrameVertical, camToTargetPlaneDist));

        Vector3 localLeftVector = Quaternion.Euler(0f, minAngleY, 0f) * c.transform.InverseTransformPoint(worldLeftPos);
        Vector3 localRightVector = Quaternion.Euler(0f, maxAngleY, 0f) * c.transform.InverseTransformPoint(worldRightPos);
        Vector3 localUpVector = Quaternion.Euler(minAngleP, 0f, 0f) * c.transform.InverseTransformPoint(worldUpPos);
        Vector3 localDownVector = Quaternion.Euler(maxAngleP, 0f, 0f) * c.transform.InverseTransformPoint(worldDownPos);

        if (89f < Vector3.Angle(Vector3.forward, localLeftVector))
        {
            localLeftVector = Quaternion.Euler(0f, -89f, 0f) * Vector3.forward;
        }
        if (89f < Vector3.Angle(Vector3.forward, localRightVector))
        {
            localRightVector = Quaternion.Euler(0f, 89f, 0f) * Vector3.forward;
        }
        if (89f < Vector3.Angle(Vector3.forward, localUpVector))
        {
            localUpVector = Quaternion.Euler(-89f, 0f, 0f) * Vector3.forward;
        }
        if (89f < Vector3.Angle(Vector3.forward, localDownVector))
        {
            localDownVector = Quaternion.Euler(89f, 0f, 0f) * Vector3.forward;
        }

        worldLeftPos = (c.transform.localToWorldMatrix * localLeftVector);
        worldRightPos = (c.transform.localToWorldMatrix * localRightVector);
        worldUpPos = (c.transform.localToWorldMatrix * localUpVector);
        worldDownPos = (c.transform.localToWorldMatrix * localDownVector);

        m_Left2Plane = worldLeftPos + c.transform.position;
        m_Right2Plane = worldRightPos + c.transform.position;
        m_Up2Plane = worldUpPos + c.transform.position;
        m_Down2Plane = worldDownPos + c.transform.position;

        Vector3 LeftPoint = c.WorldToViewportPoint(m_Left2Plane);
        Vector3 RightPoint = c.WorldToViewportPoint(m_Right2Plane);
        Vector3 UpPoint = c.WorldToViewportPoint(m_Up2Plane);
        Vector3 DownPoint = c.WorldToViewportPoint(m_Down2Plane);

        Vector3 maxInSafeFrame;
        maxInSafeFrame.x = Mathf.Clamp(targetWorldPoint.x, LeftPoint.x, RightPoint.x);
        maxInSafeFrame.y = Mathf.Clamp(targetWorldPoint.y, DownPoint.y, UpPoint.y);
        maxInSafeFrame.z = camToTargetPlaneDist;
        
        Vector3 maxPointToWorld = c.ViewportToWorldPoint(maxInSafeFrame);
        Plane xzPlane = new Plane(c.transform.forward, maxPointToWorld);
        Ray r = new Ray(target.position, c.transform.forward);
        
        float distFromTargetToXZ;
        xzPlane.Raycast(r, out distFromTargetToXZ);
        Vector3 newMaxPoint = target.position + (distFromTargetToXZ * c.transform.forward);
        Vector3 diffVec = newMaxPoint - maxPointToWorld;
        cachePos += diffVec;
        
        if (diffVec != Vector3.zero)
        {
            //Debug.Log("MoveBySafeFrame vec : " + (diffVec));
        }
    }
    void ApplyToMainCamera()
    {
        //Debug.Log("CamToMain   Old Pos: " + Camera.main.transform.position + "   Old Rotation: " + Camera.main.transform.rotation );
        Camera.main.transform.rotation = Quaternion.Euler (/*oldRot + */cachEulers);
        Camera.main.transform.position = oldPos + cachePos;
        //Debug.Log("CamToMain   New Pos: " + Camera.main.transform.position + "   New Rotation: " + Camera.main.transform.rotation );
    }

    void OnDrawGizmos() {
        if (!isEnable)
            return;

		if (target == null)
		{
			return;
		}

		if (cam == null)
		{
			return;
		}

        Camera c = cam.GetComponent<Camera>();
		if (c == null)
		{
			return;
		}

        Plane nForward_pTarget = new Plane(cam.transform.forward, target.position);
        float camToTargetPlaneDist = Mathf.Abs(nForward_pTarget.GetDistanceToPoint(cam.transform.position));

        Color old = Gizmos.color;
        Gizmos.color = Color.blue;
        DrawYawFrame(c, camToTargetPlaneDist);
        Gizmos.color = Color.green;
        DrawSafeFrame (c, camToTargetPlaneDist);
        Gizmos.color = old;
    }
    void DrawSafeFrame(Camera a_cam, float a_distance)
    {
        DrawRect(a_cam, maxSafeFrameHorizon, minSafeFrameHorizon, maxSafeFrameVertical, minSafeFrameVertical, a_distance);
        //DrawRect(a_cam, maxSafeFrameHorizon, minSafeFrameHorizon, maxSafeFrameVertical, minSafeFrameVertical, a_cam.nearClipPlane + 0.1f);
    }
    void DrawYawFrame(Camera a_cam, float a_distance)
    {
        Vector3 LeftViewPoint = a_cam.WorldToViewportPoint(m_Left2Plane);
        Vector3 RightViewPoint = a_cam.WorldToViewportPoint(m_Right2Plane);
        Vector3 UpViewPoint = a_cam.WorldToViewportPoint(m_Up2Plane);
        Vector3 DownViewPoint = a_cam.WorldToViewportPoint(m_Down2Plane);

        DrawRect(a_cam, RightViewPoint.x, LeftViewPoint.x, UpViewPoint.y, DownViewPoint.y, a_distance);
    }

    void DrawRect(Camera a_cam, float a_Right, float a_Left, float a_Up, float a_Down, float a_distance)
    {
        Vector3 rt = a_cam.ViewportToWorldPoint(new Vector3(a_Right, a_Up, a_distance));
        Vector3 lt = a_cam.ViewportToWorldPoint(new Vector3(a_Left, a_Up, a_distance));
        Vector3 rb = a_cam.ViewportToWorldPoint(new Vector3(a_Right, a_Down, a_distance));
        Vector3 lb = a_cam.ViewportToWorldPoint(new Vector3(a_Left, a_Down, a_distance));

        Gizmos.DrawLine (rt, lt);
        Gizmos.DrawLine (rt, rb);
        Gizmos.DrawLine (lb, lt);
        Gizmos.DrawLine (lb, rb);
    }

    public void EnableCam(bool result) {
        isEnable = result;
    }
    
    float ClampAngle(float angle, float min, float max) {
        angle = AngleNormalized(angle);
        if (min < 0)
        {
            min += 360f;
            max += 360f;
        }
        int over360 = (int)(min/360f) + (int)(max/360f);
        if (over360 > 2)
        {
            min -= ((over360 / 2) * 360f);
            max -= ((over360 / 2) * 360f);
        }
        if (((over360 % 2) == 1) && (angle < 180f))
        {
            angle += 360f;
        }

        float result = Mathf.Clamp(angle, min, max);
        if (( result != angle) && (((angle < min) || (angle > max))))
        {
            float distMin = Mathf.Abs(min - (angle - ((angle > max) ? 360f : 0)));
            float distMax = Mathf.Abs(max - (angle + ((angle < min) ? 360f : 0)));
            if (distMin != distMax)
            {
                angle = (distMin > distMax ? max : min);
            }
            else
            {
                angle = result;
            }
        }
        else
        {
            angle = result;
        }
        //angle = Mathf.Clamp(angle, min, max);
        
        return AngleNormalized(angle);
    }

    public static float AngleNormalized(float a_angle, float a_normailizeTo = 360f)
    {
        float result = a_angle % 360f;
        if (result >= a_normailizeTo)
            result -= 360f;
        else if (result < (a_normailizeTo - 360f))
            result += 360f;
        return result;
    }
	public void PlayerView(Transform a_player=null)
	{
		GameObject go = GameObject.FindWithTag("Look");
		if ( go != null )
		{
			target = GameObject.FindWithTag("Look").transform;
		}
		else
		{
			target = a_player;
		}

		if (Camera.main != null)
		{
			isEnable = true;
			Update();
		}
	}
}
