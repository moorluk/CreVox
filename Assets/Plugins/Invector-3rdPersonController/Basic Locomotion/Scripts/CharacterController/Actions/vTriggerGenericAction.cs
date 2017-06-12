using UnityEngine;
using System.Collections;
using Invector.CharacterController;
using UnityEngine.Events;
using Invector;

[vClassHeader("Trigger Generic Action")]
public class vTriggerGenericAction : vMonoBehaviour 
{    
    [HideInInspector]
    [Header("Trigger Action Options")]
    [Tooltip("Automatically execute the action without the need to press a Button")]
    public bool autoAction;   
    [Tooltip("Disable the the Capsule Collider Collision of the Player")]
    public bool disableCollision = true;
    [Tooltip("Disable the Rigibody Gravity of the Player")]
    public bool disableGravity = true;
    [Tooltip("Reset Player Gravity and Collision at the end of the animation")]
    public bool resetPlayerSettings = true;
    [Tooltip("Trigger an Animation - Use the exactly same name of the AnimationState you want to trigger")]
    public string playAnimation;
    [Tooltip("Check the Exit Time of your animation and insert here")]
    public float endExitTimeAnimation = 0.8f;
    [Tooltip("Select the transform you want to use as reference to the Match Target")]
    public AvatarTarget avatarTarget;
    [Tooltip("Check what position XYZ you want the matchTarget to work")]
    public Vector3 matchTargetMask;
    [Tooltip("Use a transform to help the character climb any height, take a look at the Example Scene ClimbUp, StepUp, JumpOver objects.")]
    public Transform matchTarget;
    [Tooltip("Start the match target of the animation")]
    public float startMatchTarget;
    [Tooltip("End the match target of the animation")]
    public float endMatchTarget;    
    [Tooltip("Use this to limit the trigger to active if forward of character is close to this forward")]
    public bool activeFromForward;
    [Tooltip("Rotate Character for this rotation when active")]
    public bool useTriggerRotation;
    [Tooltip("Destroy this TriggerAction after press the input or do the auto action")]
    public bool destroyAfter = false;
    [Tooltip("Delay to destroy the TriggerAction")]
    public float destroyDelay = 0f;
    [Tooltip("Delay to run the OnDoAction Event")]
    public float onDoActionDelay;

    public UnityEvent OnDoAction;
    public UnityEvent OnPlayerEnter;
    public UnityEvent OnPlayerStay;
    public UnityEvent OnPlayerExit;

    protected void Start()
    {
        this.gameObject.tag = "Action";
        this.gameObject.layer = LayerMask.NameToLayer("Action");
        GetComponent<Collider>().isTrigger = true;
    }

    public virtual IEnumerator OnDoActionDelay(GameObject obj)
    {
        yield return new WaitForSeconds(onDoActionDelay);
        OnDoAction.Invoke();
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            OnPlayerExit.Invoke();
        }
    }
}
