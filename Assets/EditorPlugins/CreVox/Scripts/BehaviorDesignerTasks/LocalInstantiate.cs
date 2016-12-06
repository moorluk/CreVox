using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityGameObject
{
    [TaskCategory("Decoration")]
    [TaskDescription("Instantiates a new GameObject.")]
	[TaskName("new Prefab")]
	public class LocalInstantiate : Action
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

        public override TaskStatus OnUpdate()
		{
			Random.seed = System.Guid.NewGuid().GetHashCode();
			if (Random.value < successProbability) {
				GameObject n;
				if (target.Value != null)
					n = GameObject.Instantiate (target.Value, position, Quaternion.Euler(rotation)) as GameObject;
				else
					n = new GameObject ();

				if (root.Value)
					n.transform.parent = root.Value.transform;
				n.transform.localPosition = position;
				n.transform.localRotation = Quaternion.Euler(rotation);
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
        }

        public override void OnReset()
        {
//            target = null;
//            position = Vector3.zero;
//            rotation = Quaternion.identity;
        }
    }
}