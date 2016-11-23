using UnityEngine;
using System.Collections;

[System.Serializable]
public class CameraCollisionHandler 
{
	public LayerMask collisionLayer;
	public float distance = 0f;
	public Vector3 targetPos = Vector3.zero;

	[HideInInspector]
	public Vector3[] cameraClipPoints;

	Camera camera;

	public void Initialize(Camera _cam)
	{
		camera = _cam;
		cameraClipPoints = new Vector3[5];
	}

	public bool Collide(Vector3 _lookPos, Vector3 _camPos) 
	{
		distance = 0f;

		UpdateCameraClipPoints (_lookPos, _camPos);

		if (CollisionDetectedAtClipPoints (_lookPos)) {
			distance = GetAdjustedDistanceWithRayFrom (_lookPos, _camPos);
			return true;
		}

		return false;
	}

	public void UpdateCameraClipPoints(Vector3 _lookPos, Vector3 _camPos)
	{
		if (!camera)
			return;

		float z = camera.nearClipPlane;
		float y = Mathf.Tan (camera.fieldOfView * 0.5f * Mathf.Deg2Rad ) * z;
		float x = y * camera.aspect;

		Vector3 forward = (_lookPos - _camPos).normalized;
		Quaternion rot = Quaternion.LookRotation(forward);
		cameraClipPoints [0] = (rot * new Vector3 (-x,  y,  z)) + _camPos;
		cameraClipPoints [1] = (rot * new Vector3 ( x,  y,  z)) + _camPos;
		cameraClipPoints [2] = (rot * new Vector3 (-x, -y,  z)) + _camPos;
		cameraClipPoints [3] = (rot * new Vector3 ( x, -y,  z)) + _camPos;
		cameraClipPoints [4] = _camPos + forward * z;
	}

	float GetAdjustedDistanceWithRayFrom(Vector3 _lookPos, Vector3 _camPos)
	{
		float distance = -1;
		int colID = 0;

		for (int i = 0; i < cameraClipPoints.Length; i++) {
			RaycastHit hit;
			if(Physics.Linecast(_lookPos, cameraClipPoints [i], out hit, collisionLayer)) {
				if(distance == -1) {
					distance = hit.distance;
				}else{
					if (hit.distance < distance) {
						distance = hit.distance;
						colID = i;
					}
				}
			}
		}

		for (int i = 0; i < cameraClipPoints.Length; i++) {
			if(colID == i)
				Debug.DrawLine (cameraClipPoints [i], _lookPos, Color.red);
			else
				Debug.DrawLine (cameraClipPoints [i], _lookPos, Color.blue);
		}

		if (distance == -1)
			return 0;

		if (colID != 4) {
			float originDist = Vector3.Distance(cameraClipPoints [0], _lookPos);
			float centerDist = Vector3.Distance(cameraClipPoints [4], _lookPos);
			distance = centerDist * distance / originDist;
		}

		targetPos = _lookPos + (_camPos -_lookPos).normalized * distance*0.99f;
		//Debug.DrawLine (targetPos, _lookPos, Color.yellow);
		return distance;
	}

	bool CollisionDetectedAtClipPoints(Vector3 _lookPos )
	{
		for (int i = 0; i < cameraClipPoints.Length; i++) {
			if(Physics.Linecast(_lookPos, cameraClipPoints [i], collisionLayer)) {
				return true;
			}
		}
		return false;
	}
}
