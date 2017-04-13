using UnityEngine;
using System;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityGameObject
{
    [TaskCategory("Decoration")]
    [TaskDescription("Instantiates a new GameObject, and random offset & Rotate.")]
	[TaskName("new Prefab(R)")]
	public class LocalInstantiateRandom : Action
	{
		[Tooltip("The Virtual blockAir GameObject.")]
		public SharedGameObject root;
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
		public SharedGameObject target;
		[Tooltip("The local instantiate position")]
		public Vector3 position;
		[Tooltip("The local instantiate rotation")]
		public Vector3 rotation;
		[Tooltip("The chance that the task will return success")]
		public float successProbability = 1.0f;
		[Space]

		[Tooltip("The chance that the Gameobject is mess up")]
		public float messProbability = 0.5f;
		[Tooltip("random offset range")]
		public Vector3 randomOffset;
		[Tooltip("random turn range")]
		public Vector3 randomRotate;

        public override TaskStatus OnUpdate()
		{
			UnityEngine.Random.InitState(System.Guid.NewGuid().GetHashCode());
			if (UnityEngine.Random.value < successProbability) {
				GameObject n;
				if (target.Value != null)
					n = GameObject.Instantiate (target.Value, position, Quaternion.Euler(rotation)) as GameObject;
				else
					n = new GameObject ();

				if (root.Value)
					n.transform.parent = root.Value.transform;
				n.transform.localPosition = position;
				n.transform.localRotation = Quaternion.Euler(rotation);

				UnityEngine.Random.InitState(System.Guid.NewGuid().GetHashCode());
				if (UnityEngine.Random.value < messProbability) {
					UnityEngine.Random.InitState(System.Guid.NewGuid().GetHashCode());
					float posR = UnityEngine.Random.Range (-1.0f, 1.0f);
					n.transform.localPosition += new Vector3 (randomOffset.x * posR, randomOffset.y * posR, randomOffset.z * posR);
					UnityEngine.Random.InitState(System.Guid.NewGuid().GetHashCode());
					float rotR = UnityEngine.Random.Range (-1.0f, 1.0f);
					n.transform.Rotate (randomRotate.x * rotR, randomRotate.y * rotR, randomRotate.z * rotR, Space.Self);
				}
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
        }
    }
}

//Change all the random with unity engine random