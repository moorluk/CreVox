using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CamSys : MonoBehaviour {

	public List<DynamicCameraZone> submissionList;
	public List<DynamicCameraZone> timerList;
	public List<float> blendTimer;
		
	public Transform target;

	public bool isBlending = false;

	// Use this for initialization
	void Start () {
		submissionList = new List<DynamicCameraZone> (10);
        timerList = new List<DynamicCameraZone> (10);
	}

	// Update is called once per frame
	void Update () {
		if (!isBlending)
            return;
        ClearNull();

        if (m_ShowDebug == true)
        {
            m_enableCameraList.Clear();
        }

        if (timerList.Count > 1)
        {
            for(int i = 0 ; i < timerList.Count ; ++i)
            {
                if ((timerList[i] != null) && (timerList[i].dCam.m_lastUpdateTime < Time.time))
                {
                    timerList[i].dCam.UpdateCamera();
                    if (m_ShowDebug == true)
                    {
                        if (m_enableCameraList.Contains(timerList[i].dCam) == false)
                        {
                            m_enableCameraList.Add(timerList[i].dCam);
                        }
                    }
                }
            }

            CameraBlend(timerList [timerList.Count - 1].dCam, 1);

    		// blend the camera
    		for (int i = timerList.Count-2; i >= 0; i--) {
    			float curTime = blendTimer [i];
    			float blendTime = timerList [i].blendTime;
    			float ratio = ((blendTime <= 0) ? 1f :(curTime / blendTime));
    			ratio = timerList [i].curve.Evaluate(ratio);
    			if (ratio > 1)
    				ratio = 1;

                CameraBlend(timerList [i].dCam, ratio);
    		}

    		int found = -1;
    		//update timer and reset overtimed camera's timer
    		for (int i = 0; i < timerList.Count; i++) {
    			blendTimer [i] += Time.deltaTime;
    			
    			if (blendTimer [i] >= timerList [i].blendTime) {
    				blendTimer [i] = timerList [i].blendTime; 
    				if (found == -1)
    					found = i;
    				if (i > found)
    					blendTimer [i] = 0f;
    			}
    			
    		}

    		//remove overtimed cameras
    		if (found > -1) {
    			timerList.RemoveRange (found + 1, timerList.Count - found - 1);
    			blendTimer.RemoveRange(found + 1, blendTimer.Count - found - 1);
    		}
        }
		//stop blend camera if there is only one camera in timerlist
		else if (timerList.Count == 1) {
			isBlending = false;
		    timerList [0].dCam.EnableCam (true);// Switch to zone cam
            Camera.main.fieldOfView = timerList [0].dCam.cam.fieldOfView;
		}
        else if (timerList.Count == 0)
        {
            isBlending = false;
        }

        if (m_ShowDebug == true)
        {
            ShowDebugBlendingCamera();
        }
	}

    void CameraBlend(DynamicCamera a_cam, float a_camWeight)
    {
        float blendCamWeight = 1f - a_camWeight;

        transform.position = (a_cam.resultPos * a_camWeight) + (transform.position * blendCamWeight);
        transform.rotation = Quaternion.Euler(BlendEuler(a_cam.resultEulers , transform.rotation.eulerAngles , blendCamWeight));
        Camera.main.fieldOfView = (a_cam.cam.fieldOfView * a_camWeight) + (Camera.main.fieldOfView * blendCamWeight);
    }
    Vector3 BlendEuler(Vector3 a_eulerA, Vector3 a_eulerB, float a_weightB)
    {
        Vector3 eulerDiff = a_eulerB - a_eulerA;
        eulerDiff.x = DynamicCamera.AngleNormalized(eulerDiff.x, 180f);
        eulerDiff.y = DynamicCamera.AngleNormalized(eulerDiff.y, 180f);
        eulerDiff.z = DynamicCamera.AngleNormalized(eulerDiff.z, 180f);
        Vector3 result = a_eulerA + (eulerDiff * a_weightB);
        return result;
    }

    void AddTimerList(DynamicCameraZone newZone) {
        if (newZone == null)
        {
            return;
        }
        ClearNull();
		DynamicCameraZone oldZone = null;
		float curTime = 0;
		if (timerList.Count == 0) {
			curTime = newZone.blendTime;
			newZone.dCam.EnableCam (true);
		} else if (timerList.Count == 1) {
			oldZone = timerList [0];
		}

		timerList.Insert (0, newZone);
		blendTimer.Insert (0, curTime);

		if (timerList.Count > 1 && !isBlending) {
			isBlending = true;
			if(oldZone) {
				oldZone.dCam.EnableCam(false);
			}
			newZone.dCam.EnableCam(false);
		}
	}

	public void AddCameraZone(DynamicCameraZone zone) {
		submissionList.Insert(0, zone);
		AddTimerList(submissionList [0]);
	}
	
	public void RemoveCameraZone(DynamicCameraZone zone) {
		submissionList.Remove (zone);
		if (submissionList.Count > 0)
		{
			if ((timerList.Count > 0) && (submissionList [0] == timerList [0]))
			{
				return;
			}
			else
			{
				AddTimerList(submissionList [0]);
			}
		}
	}

    public void ClearNull()
    {
        for(int i = (timerList.Count - 1) ; i >= 0 ; --i)
        {
            DynamicCameraZone dcz = timerList[i];
            if ((dcz == null) && (blendTimer.Count > i))
            {
                blendTimer.RemoveAt(i);
            }
        }
        submissionList.RemoveAll(dcz => dcz == null);
        timerList.RemoveAll(dcz => dcz == null);
    }

    public bool m_ShowDebug = false;
    List<Transform> m_blendCamera = new List<Transform>(4);
    List<DynamicCamera> m_enableCameraList = new List<DynamicCamera>(4);
    void ShowDebugBlendingCamera()
    {
        for(int i = 0 ; ((i < m_blendCamera.Count) || (i < m_enableCameraList.Count)) ; ++i)
        {
            if (m_enableCameraList[i] == null)
            {
                continue;
            }
            if (m_enableCameraList.Count > m_blendCamera.Count)
            {
                GameObject newCamera = new GameObject();
                newCamera.AddComponent<Camera>().enabled = false;
                newCamera.transform.parent = transform;
                m_blendCamera.Add(newCamera.transform);
            }

            if ((m_enableCameraList.Count <= i) || (m_enableCameraList[i] == null))
            {
                m_blendCamera[i].transform.position = Vector3.zero;
                m_blendCamera[i].transform.rotation = Quaternion.identity;
            }
            else
            {
                m_blendCamera[i].transform.position = m_enableCameraList[i].resultPos;
                m_blendCamera[i].transform.rotation = Quaternion.Euler(m_enableCameraList[i].resultEulers);
            }
        }
    }
}
