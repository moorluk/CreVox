using UnityEngine;
using System.Collections;

public class vRandomAttackBehaviour : StateMachineBehaviour
{
    public int attackCount;

    //OnStateMachineEnter is called when entering a statemachine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        animator.SetInteger("RandomAttack", Random.Range(0, attackCount));
    }
}