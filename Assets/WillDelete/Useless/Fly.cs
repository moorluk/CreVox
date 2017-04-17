using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Fly : MonoBehaviour {
	public FirstPersonController firstPersonController;
	public CharacterController characterController;
	public float originGravity;
	public float currentGravity;
	public float delta;
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
