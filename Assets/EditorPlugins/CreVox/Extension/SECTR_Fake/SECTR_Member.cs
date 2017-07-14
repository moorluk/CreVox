using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SECTR_Member : MonoBehaviour {
	public enum BoundsUpdateModes
	{
		Start,			
		Movement,		
		Always,			
		Static,
		SelfOnly,
	};

	public enum ChildCullModes
	{
		Default,
		Group,
		Individual
	};
	public BoundsUpdateModes BoundsUpdateMode = BoundsUpdateModes.Always;
}
