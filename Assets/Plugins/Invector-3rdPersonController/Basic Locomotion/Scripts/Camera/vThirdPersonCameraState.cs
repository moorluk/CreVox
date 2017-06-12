using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class vThirdPersonCameraState
{
	public string Name;
	public float forward;
	public float right;
    public float defaultDistance;
	public float maxDistance;
	public float minDistance;
	public float height;
    public float smoothFollow;
    public float xMouseSensitivity;
    public float yMouseSensitivity ;
    public float yMinLimit;
    public float yMaxLimit;
    public float xMinLimit;
    public float xMaxLimit;

    public Vector3 rotationOffSet;
    public float cullingHeight;
    public float cullingMinDist;
    public float fov;
    public bool useZoom;   
    public Vector2 fixedAngle;
    public List<LookPoint> lookPoints;
    public TPCameraMode cameraMode;
	public vThirdPersonCameraState(string name)
	{
		this.Name = name;
		this.forward = -1f;
		this.right = 0f;
        this.defaultDistance = 1.5f;
		this.maxDistance = 3f;
		this.minDistance = 0.5f;
		this.height = 0f;
        this.smoothFollow = 10f;
        this.xMouseSensitivity = 3f;
        this.yMouseSensitivity = 3f;
        this.yMinLimit = -40f;
        this.yMaxLimit = 80f;
        this.xMinLimit = -360f;
        this.xMaxLimit = 360f;
        this.cullingHeight = 0.2f;
        this.cullingMinDist = 0.1f;
        this.useZoom = false;
        this.forward = 60;    
        this.fixedAngle = Vector2.zero;       
        this.cameraMode = TPCameraMode.FreeDirectional;
	}
}
[System.Serializable]
public class LookPoint
{
    public string pointName;
    public Vector3 positionPoint;
    public Vector3 eulerAngle;
    public bool freeRotation;
}
public enum TPCameraMode
{
    FreeDirectional,
    FixedAngle,
    FixedPoint
}