using UnityEngine;
using System.Collections;

public enum SwitchState
{
	On,
	On2Off,
	Off,
	Off2On,
	Count,
}

public class SwitchProperty : PropProperty {
	public SwitchState state = SwitchState.Off;
	public GameObject onObj;
	public GameObject offObj;
	public GameObject switchingObj;

	void Start() {
//		onObj = transform.FindChild ("on").gameObject;
//		offObj = transform.FindChild ("off").gameObject;
//		switchingObj = transform.FindChild ("switching").gameObject;
//		onObj.SetActive(false);
//		switchingObj.SetActive(false);
//		offObj.SetActive(true);
	}

	void Switch() {
//		if (state == SwitchState.On) {
//			state = SwitchState.On2Off;
//			OnMessage(OnEvent.TurnOff);
//			onObj.SetActive(false);
//			switchingObj.SetActive(true);
//			Debug.Log ("Turn Off");
//		}
//		if (state == SwitchState.Off) {
//			state = SwitchState.Off2On;
//			OnMessage(OnEvent.TurnOn);
//			switchingObj.SetActive(true);
//			offObj.SetActive(false);
//			Debug.Log ("Turn On!");
//		}
	}

	void Switched() {
//		if (state == SwitchState.On2Off) {
//			state = SwitchState.Off;
//			offObj.SetActive(true);
//			switchingObj.SetActive(false);
//		}
//		if (state == SwitchState.Off2On) {
//			state = SwitchState.On;
//			switchingObj.SetActive(false);
//			onObj.SetActive(true);
//		}
	}
}
