using UnityEngine;
//using UnityEditor;
using System.Collections;

public class DynamicCameraZone : MonoBehaviour {
    [HideInInspector]
	public DynamicCamera dCam;
    [HideInInspector]
	public CamSys camSys;
	public AnimationCurve curve;
	public float blendTime = 0.5f;

	void Awake () {
		camSys = Camera.main.gameObject.GetComponent<CamSys>();
		dCam = GetComponentInChildren<DynamicCamera> ();
	}

	void OnTriggerEnter(Collider other) {
		if (camSys != null && other.tag == "Look") {
			camSys.AddCameraZone(this);
		}
	}
	
	void OnTriggerExit(Collider other) {
        if (other.tag == "Look")
        {
			camSys.RemoveCameraZone(this);
		}
	}
}
