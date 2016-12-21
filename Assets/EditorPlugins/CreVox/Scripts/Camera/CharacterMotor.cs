using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class CharacterMotor : MonoBehaviour
{
	[System.Serializable]
	public class MoveSetting
	{
		public float forwardVel = 3f;
		public float rotateVel = 0.005f;
		public float jumpVel = 25f;
		public float distToGround = 0.1f;
		public LayerMask ground;
	}

	[System.Serializable]
	public class PhysicSetting
	{
		public float downAccel = 0.75f;
	}

	[System.Serializable]
	public class InputSetting
	{
		public float inputDelay = 0.1f;

		public string V_AXIS = "Vertical";
		public string H_AXIS = "Horizontal";

		public string JUMP_AXIS = "Jump";
	}

	public MoveSetting moveSetting = new MoveSetting ();
	public PhysicSetting physicSetting = new PhysicSetting ();
	public InputSetting inputSetting = new InputSetting ();

	Vector3 velocity = Vector3.zero;
	Quaternion targetRotation;
	Rigidbody rBody;
	float hInput, vInput, jumpInput;
	Vector3 forwardDir = Vector3.zero;

	public Quaternion TargetRotation {
		get { return targetRotation; }
	}

	bool Grounded ()
	{
		return Physics.Raycast (transform.position, Vector3.down, moveSetting.distToGround, moveSetting.ground);
	}

	// Use this for initialization
	void Start ()
	{
		targetRotation = transform.rotation;
		rBody = GetComponent<Rigidbody> ();
		if (rBody == null)
			Debug.LogError ("Character Motor need rigidbody component");

		jumpInput = 0f;
	}

	void GetInput ()
	{
		hInput = Input.GetAxisRaw (inputSetting.H_AXIS);
		vInput = Input.GetAxisRaw (inputSetting.V_AXIS);
		jumpInput = Input.GetAxisRaw (inputSetting.JUMP_AXIS);
	}

	// Update is called once per frame
	void Update ()
	{
		GetInput ();
	}

	void FixedUpdate ()
	{
        
		Run ();
		Turn ();
		Jump ();

		rBody.velocity = velocity;
	}

	public void Run ()
	{
		Vector3 moveInput = new Vector3 (hInput, 0, vInput).normalized;
		float speed = moveInput.sqrMagnitude;

		//Get Camera rotation
		Vector3 camDir = Camera.main.transform.forward;
		camDir.y = 0f;
		Quaternion camRot = Quaternion.FromToRotation (Vector3.forward, camDir);

		forwardDir = camRot * moveInput;
		forwardDir.Normalize ();

		forwardDir = forwardDir * (speed / 1.414f * moveSetting.forwardVel);
		velocity.x = forwardDir.x;
		velocity.z = forwardDir.z;
	}

	void Turn ()
	{
		if (Mathf.Abs (forwardDir.magnitude) > inputSetting.inputDelay) {
			Quaternion rot = Quaternion.LookRotation (forwardDir, Vector3.up);
			transform.rotation = Quaternion.Lerp (transform.rotation, rot, moveSetting.rotateVel * Time.fixedTime);
		}
	}

	void Jump ()
	{
		if (jumpInput > 0 && Grounded ()) {
			velocity.y = moveSetting.jumpVel;
		} else if (jumpInput == 0 && Grounded ()) {
			velocity.y = 0;
		} else {
			velocity.y -= physicSetting.downAccel;
		}
	}
}
