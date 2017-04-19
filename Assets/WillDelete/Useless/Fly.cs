using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Fly : MonoBehaviour {
	private FirstPersonController firstPersonController;
	private CharacterController characterController;
	private float originGravity;
	private float currentGravity;
	private float delta;

	float mass = 3.0F; // defines the character mass
	Vector3 impact = Vector3.zero;

	// Use this for initialization
	void Start () {
		firstPersonController = gameObject.GetComponent<FirstPersonController>();
		characterController = gameObject.GetComponent<CharacterController>();
		originGravity = firstPersonController.m_GravityMultiplier;
		currentGravity = originGravity;
		delta = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
		delta -= 0.5f * Time.deltaTime;
		if (delta < 0) delta = 0;
		if (!characterController.isGrounded && Input.GetKeyDown(KeyCode.F)) {
			delta += 0.33333f;
			if (delta > 1.0f) {
				delta = 1;
			}
		}
		currentGravity = Mathf.Lerp(originGravity, -1.0f, delta);
		firstPersonController.m_GravityMultiplier = currentGravity;
	}
}
