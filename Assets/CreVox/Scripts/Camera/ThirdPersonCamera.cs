using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
	public enum CamState
	{
		Behind,
		Reset,
		Target
	}

	[Header ("Position Setting")]
	public float distanceAway = 4.3f;
	public float distanceUp = 1.2f;
	private float curAway, curUp, curDis, curYRot;
	private Vector3 desiredRigPos, vecolity;
	
	[Header ("Rotation Setting")]
	public float yRotateSpeed = 500f;
	public float xRotateSpeed = 15f;
	public float smooth = 5;
	private float xInput = 0f, yInput = 0f;
	private Vector3 lookDir = Vector3.zero;

	[Header ("Target Effect Setting")]
	public GameObject targetEffect;
	[Range (0f, 1f)] public float effectHigh = 0.66f;
	private GameObject effInstance;
	private Vector3 targetPos, effectPos, smoothedLookPos;

	[Space]
	public CameraCollisionHandler camCol = new CameraCollisionHandler ();
//	private InputAct act;

	[Header ("Infomation Only")]
	[SerializeField] private CamState camState;
	[SerializeField] private Transform m_look, m_camera, m_target;
	private Vector3 sVel1, sVel2, sVel3; 

	public float pitchMax = 30f;
	public float currentPitch = 0f;
	public float xAngleMin = -10f;
	public float xAngleMax = 60f;
	public float curXAngle = 0f;

	void Start ()
	{
		Application.targetFrameRate = 60;
		Camera cam = transform.GetComponentInChildren<Camera> ();
		m_camera = cam.transform; //
		curAway = distanceAway;
		curUp = distanceUp;
		curDis = new Vector2 (distanceAway, distanceUp).magnitude;
		if(targetEffect) {
			effInstance = Instantiate (targetEffect);
			effInstance.SetActive (false);
		}

		camCol.Initialize (cam);
//		act = GameObject.FindWithTag ("Player").transform.GetComponent<InputAct> ();
		m_target = null;
	}

	void Update ()
	{
		if (m_look == null)
			m_look = GameObject.FindWithTag ("Look").transform;

//		if(act != null)
//			m_target = act.m_target;

		if (Input.GetButtonDown ("Snap")) {
			if (m_target == null) {
				camState = CamState.Reset;
			} else {
				camState = (camState == CamState.Behind) ? CamState.Target : CamState.Behind;
			}
		}

		if (effInstance != null && ((camState == CamState.Target) != effInstance.activeSelf))
			effInstance.SetActive (!effInstance.activeSelf);

		if (m_target == null && camState == CamState.Target)
			camState = CamState.Behind;

		xInput = Input.GetAxis ("CamH");
		yInput = Input.GetAxis ("CamV");
	}

	void LateUpdate ()
	{
		switch (camState) {
		case CamState.Behind:
			lookDir = m_look.position - transform.position;
			lookDir.y = 0;
			lookDir.Normalize ();
			CameraRotate ();
			break;

		case CamState.Target:
			lookDir = m_target.position - m_look.position;
			lookDir.y = 0;
			lookDir.Normalize ();
			CameraRotateTarget ();
			break;

		case CamState.Reset:
			curAway = distanceAway;
//			curUp = distanceUp;
			lookDir = m_look.transform.forward;
			CameraRotate ();
			break;
		}

		if (camState == CamState.Reset) {
			if ((transform.position - desiredRigPos).magnitude < 0.05f
			    || Mathf.Abs (Input.GetAxisRaw ("Horizontal")) > 0.1f
			    || Mathf.Abs (Input.GetAxisRaw ("Vertical")) > 0.1f)
				camState = CamState.Behind;
		}



		//Vector3 lookPos = (camState == CamState.Target) ? targetPos : m_look.position;
		Vector3 lookPos = m_look.position;

		Vector3 localLookPos = m_camera.InverseTransformPoint (lookPos);
		Vector3 localSmoothedLookPos = m_camera.InverseTransformPoint (smoothedLookPos);
		localSmoothedLookPos.y = localLookPos.y;
		localSmoothedLookPos.z = localLookPos.z;
		smoothedLookPos = m_camera.TransformPoint (localSmoothedLookPos);

		smoothedLookPos = Vector3.SmoothDamp (smoothedLookPos, lookPos, ref sVel2, smooth * 1.3f);

		if(camState == CamState.Reset)
			transform.position = Vector3.SmoothDamp (transform.position, desiredRigPos, ref sVel1, smooth*0.12f);
		else
			transform.position = desiredRigPos; 

		if (camCol.Collide (smoothedLookPos, desiredRigPos))
			m_camera.position = Vector3.SmoothDamp (m_camera.position, camCol.targetPos, ref sVel3, smooth*0.01f);
		else
			m_camera.position = transform.position;

		//transform.LookAt (smoothedLookPos);
		if(camState == CamState.Target)
			m_camera.LookAt (m_target.position);
		else
			m_camera.LookAt (smoothedLookPos);
	}

	void CameraRotate ()
	{
		if (Mathf.Abs (yInput) > 0.1f)
			curXAngle += yInput * xRotateSpeed * Time.deltaTime;
			//curUp += yInput * xRotateSpeed * Time.deltaTime;

		if (Mathf.Abs (xInput) > 0.1f) {
			curYRot = xInput * yRotateSpeed * Time.deltaTime;
		} else
			curYRot = Mathf.SmoothStep(curYRot, 0f, Time.deltaTime * 10f);

		//curUp = Mathf.Clamp (curUp, curDis * -0.7f, curDis * 0.95f);
		//curAway = Mathf.Pow (Mathf.Pow (curDis, 2.0f) - Mathf.Pow (curUp, 2.0f), 0.5f);
		curXAngle = Mathf.Clamp(curXAngle, xAngleMin, xAngleMax);
		curUp = curDis * Mathf.Sin (curXAngle * Mathf.Deg2Rad);
		curAway = curDis * Mathf.Cos (curXAngle * Mathf.Deg2Rad);

		Quaternion yRot = Quaternion.Euler (0f, curYRot, 0f);

		desiredRigPos = m_look.position + yRot *(m_look.up * curUp - lookDir * curAway);
	}

	void CameraRotateTarget ()
	{
		targetPos = m_target.position + (Vector3.up * -0.2f);
		Vector3 delta = (smoothedLookPos - targetPos);
		currentPitch = 90f - Vector3.Angle (Vector3.up, delta);
		if (currentPitch > pitchMax)
			currentPitch = pitchMax;

		float disTarget = curDis + delta.magnitude;

		Vector3 axis = Vector3.Cross (delta.normalized, Vector3.up);
		delta = Quaternion.AngleAxis (currentPitch - 90f, axis) * Vector3.up;

		desiredRigPos = m_target.position + delta.normalized * disTarget;

		effectPos = m_target.position + Vector3.up * (m_target.GetComponentInChildren<Renderer> ().bounds.size.y * effectHigh);
		effInstance.transform.position = effectPos;
	}

	void CameraCollision ()
	{
		RaycastHit hit = new RaycastHit ();
		if (Physics.Linecast (m_look.position, desiredRigPos, out hit, camCol.collisionLayer))
			desiredRigPos = hit.point;
	}

	void OnDrawGizmos() {
		Gizmos.DrawSphere (smoothedLookPos, 0.3f);
		Gizmos.DrawSphere (desiredRigPos, 0.3f);
		Gizmos.DrawSphere (camCol.targetPos, 0.3f);
	}
}