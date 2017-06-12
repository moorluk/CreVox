using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vTriggerSoundByState: StateMachineBehaviour
{
	public GameObject audioSource;
	public List<AudioClip> sounds;
	private vFisherYatesRandom _random;
	
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
		if(_random ==null)
		_random = new vFisherYatesRandom ();
		GameObject audioObject = null;
		if (audioSource != null)
			audioObject = Instantiate(audioSource.gameObject,animator.transform.position,Quaternion.identity) as GameObject;
		else
		{
			audioObject = new GameObject("audioObject");
			audioObject.transform.position = animator.transform.position;
		}
		if (audioObject != null) 
		{
			var source = audioObject.gameObject.GetComponent<AudioSource> ();
			var clip =sounds[_random.Next (sounds.Count)];
			source.PlayOneShot (clip);
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
