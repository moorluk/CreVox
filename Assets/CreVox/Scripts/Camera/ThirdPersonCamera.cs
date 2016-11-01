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
	public LayerMask collisionMask;
	private float curAway, curUp, dis;
	private Vector3 camPos;
	
	[Header ("Rotation Setting")]
	public float xRotate = 150f;
	public float yRotate = 10f;
	public float smooth = 12;
	private float xInput = 0f, yInput = 0f;
	private Vector3 lookDir = Vector3.zero;

	[Header ("Target Effect Setting")]
	public GameObject targetEffect;
	[Range (0f, 1f)] public float effectHigh = 0.66f;
	private GameObject effInstance;
	private Vector3 targetPos, effectPos;

	[Header ("Infomation Only")]
	[SerializeField] private CamState camState;
	[SerializeField] private Transform m_player, m_camera, m_target;

	void Start ()
	{
		m_camera = this.transform;
		curAway = distanceAway;
		curUp = distanceUp;
		dis = new Vector2 (distanceAway, distanceUp).magnitude;
		effInstance = Instantiate (targetEffect);
		effInstance.SetActive (false);
	}

	void Update ()
	{
		if (m_player == null)
			m_player = GameObject.FindWithTag ("Look").transform;
		
//		m_target = GameObject.FindWithTag ("Player").transform.GetComponent<InputAct> ().m_target;

		if (Input.GetButtonDown ("Snap")) {
			if (m_target == null) {
				camState = CamState.Reset;
			} else {
				camState = (camState == CamState.Behind) ? CamState.Target : CamState.Behind;
			}
		}

		if ((camState == CamState.Target) != effInstance.activeSelf)
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
			lookDir = m_player.position - m_camera.position;
			lookDir.y = 0;
			lookDir.Normalize ();
			CameraRotate ();
			break;

		case CamState.Target:
			lookDir = m_target.position - m_player.position;
			lookDir.y = 0;
			lookDir.Normalize ();
			CameraRotateTarget ();
			break;

		case CamState.Reset:
			curAway = distanceAway;
//			curUp = distanceUp;
			lookDir = m_player.transform.forward;
			CameraRotate ();
			break;
		}

		CameraCollision ();

		if (camState == CamState.Reset) {
			if ((transform.position - camPos).magnitude < 0.05f
			    || Mathf.Abs (Input.GetAxisRaw ("Horizontal")) > 0.1f
			    || Mathf.Abs (Input.GetAxisRaw ("Vertical")) > 0.1f)
				camState = CamState.Behind;
		}

		transform.position = Vector3.Lerp (transform.position, camPos, Time.deltaTime * smooth);
		transform.LookAt ((camState == CamState.Target) ? targetPos : m_player.position);
	}

	void CameraRotate ()
	{
		if (Mathf.Abs (yInput) > 0.1f)
			curUp += yInput * yRotate * Time.deltaTime;

		curUp = Mathf.Clamp (curUp, dis * -0.7f, dis * 0.95f);
		curAway = Mathf.Pow (Mathf.Pow (dis, 2.0f) - Mathf.Pow (curUp, 2.0f), 0.5f);

		camPos = m_player.position + m_player.up * curUp - lookDir * curAway;

		if (Mathf.Abs (xInput) > 0.1f)
			m_camera.RotateAround (m_player.position + m_player.up * distanceUp, m_player.up, xInput * xRotate * Time.deltaTime);
	}

	void CameraRotateTarget ()
	{
		targetPos = m_target.position + (Vector3.up * -0.2f);
		Vector3 delta = (m_player.position - targetPos);
		float disTarget = dis + delta.magnitude;

		camPos = m_target.position + delta.normalized * disTarget;

		effectPos = m_target.position + Vector3.up * (m_target.GetComponentInChildren<Renderer> ().bounds.size.y * effectHigh);
		effInstance.transform.position = effectPos;
	}

	void CameraCollision ()
	{
		RaycastHit hit = new RaycastHit ();
		if (Physics.Linecast (m_player.position, camPos, out hit, collisionMask))
			camPos = hit.point;
	}
}