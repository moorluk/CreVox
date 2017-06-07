using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour {
	public Transform character;
	public float smoothTime = 0.01f;
	private Vector3 cameraVelocity = Vector3.zero;

	void Awake() {
	}

	void Update() {
		transform.position = Vector3.SmoothDamp(transform.position, character.position + new Vector3(0, 7.63f, -4.55f), ref cameraVelocity, smoothTime);
	}

}
