﻿using UnityEngine;
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
	float curAway, curUp, curDis, curYRot;
	Vector3 desiredRigPos, vecolity;
	
	[Header ("Rotation Setting")]
	public float yRotateSpeed = 500f;
	public float xRotateSpeed = 15f;
	public float smooth = 5;
	float xInput, yInput;
	Vector3 lookDir = Vector3.zero;
	public float targetPitchMax = 30f;
	float curTargetPitch;
	public float xAngleMin = -10f;
	public float xAngleMax = 60f;
	float curXAngle;

	[Header ("Target Effect Setting")]
	public GameObject targetEffect;
	[Range (0f, 1f)] public float effectHigh = 0.66f;
	GameObject effInstance;
	Vector3 targetPos, effectPos, smoothedLookPos;

	[Space]
	public CameraCollisionHandler camCol = new CameraCollisionHandler ();
//	private InputAct act;

	[Header ("Infomation Only")]
	[SerializeField] CamState camState;
	[SerializeField] Transform m_look, m_camera, m_target;
	Vector3 sVel1, sVel2, sVel3; 

	[Header("Test Camera Shake")]
	public float duration = 0.3f;
	public float speed = 20f;
	public float magnitude = 0.01f;
	public AnimationCurve damper = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.9f, .33f, -2f, -2f), new Keyframe(1f, 0f, -5.65f, -5.65f));
	public bool testCameraShake;


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
        if (m_look == null) {
            var g = GameObject.FindWithTag ("Look");
            if (g != null)
                m_look = g.transform;
        }

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

		if (Input.GetButton ("Fire1") && testCameraShake) {
			StopAllCoroutines ();
			StartCoroutine (ShakeCamera (Camera.main, duration, speed, magnitude, damper));
		}
	}

	void LateUpdate ()
	{
        if (!m_look)
            return;
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

		transform.position = camState == CamState.Reset ? 
            Vector3.SmoothDamp (transform.position, desiredRigPos, ref sVel1, smooth * 0.12f) : 
            desiredRigPos; 

		m_camera.position = camCol.Collide (smoothedLookPos, desiredRigPos) ? 
            Vector3.SmoothDamp (m_camera.position, camCol.targetPos, ref sVel3, smooth * 0.01f) : 
            transform.position;

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
		curTargetPitch = 90f - Vector3.Angle (Vector3.up, delta);
		if (curTargetPitch > targetPitchMax)
			curTargetPitch = targetPitchMax;

		float disTarget = curDis + delta.magnitude;

		Vector3 axis = Vector3.Cross (delta.normalized, Vector3.up);
		delta = Quaternion.AngleAxis (curTargetPitch - 90f, axis) * Vector3.up;

		desiredRigPos = m_target.position + delta.normalized * disTarget;

		effectPos = m_target.position + Vector3.up * (m_target.GetComponentInChildren<Renderer> ().bounds.size.y * effectHigh);
		effInstance.transform.position = effectPos;
	}

	void CameraCollision ()
	{
        RaycastHit hit;
		if (Physics.Linecast (m_look.position, desiredRigPos, out hit, camCol.collisionLayer))
			desiredRigPos = hit.point;
	}

	void OnDrawGizmos() {
		Gizmos.DrawSphere (smoothedLookPos, 0.3f);
		Gizmos.DrawSphere (desiredRigPos, 0.3f);
		Gizmos.DrawSphere (camCol.targetPos, 0.3f);
	}

	static IEnumerator ShakeCamera(Camera camera, float duration, float speed, float magnitude, AnimationCurve damper = null)
	{
		float elapsed = 0f;
		while (elapsed < duration) 
		{
			elapsed += Time.deltaTime;			
			float damperedMag = (damper != null) ? (damper.Evaluate(elapsed / duration) * magnitude) : magnitude;
			float x = (Mathf.PerlinNoise(Time.time * speed, 0f) * damperedMag) - (damperedMag / 2f);
			float y = (Mathf.PerlinNoise(0f, Time.time * speed) * damperedMag) - (damperedMag / 2f);
			// offset camera obliqueness - http://answers.unity3d.com/questions/774164/is-it-possible-to-shake-the-screen-rather-than-sha.html
			float frustrumHeight = 2 * camera.nearClipPlane * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
			float frustrumWidth = frustrumHeight * camera.aspect;
			Matrix4x4 mat = camera.projectionMatrix;
			mat[0, 2] = 2 * x / frustrumWidth;
			mat[1, 2] = 2 * y / frustrumHeight;
			camera.projectionMatrix = mat;
			yield return null;
		}
		camera.ResetProjectionMatrix();
	}
}