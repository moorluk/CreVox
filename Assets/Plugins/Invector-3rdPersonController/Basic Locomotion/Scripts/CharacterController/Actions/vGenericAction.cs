using UnityEngine;
using System.Collections;
using Invector.CharacterController;
using UnityEngine.Events;

namespace Invector.CharacterController.Actions
{
    [vClassHeader("Generic Action", "Use the vTriggerGenericAction to trigger a simple animation.")]
    public class vGenericAction : vActionListener
    {
        #region Variables
        [Tooltip("Input to make the action")]
        public GenericInput actionInput = new GenericInput("E", "A", "A");
        [Tooltip("Tag of the object you want to access")]
        public string actionTag = "Action";    
        
        [Header("--- Debug Only ---")]
        public vTriggerGenericAction triggerAction;
        [Tooltip("Check this to enter the debug mode")]
        public bool debugMode;
        public bool canTriggerAction;
        public bool isPlayingAnimation;
        public bool triggerActionOnce;

        protected vThirdPersonInput tpInput;

        #endregion

        private void Awake()
        {
            actionStay = true;
            actionExit = true;
        }

        protected virtual void Start()
        {
            tpInput = GetComponent<vThirdPersonInput>();
        }

        protected virtual void LateUpdate()
        {
            AnimationBehaviour();
            TriggerActionInput();
        }

        protected virtual void TriggerActionInput()
        {
            if (triggerAction == null) return;

            if(canTriggerAction)
            {
                if ((triggerAction.autoAction && actionConditions) || (actionInput.GetButtonDown() && actionConditions))
                {
                    if (!triggerActionOnce)
                    {
                        OnDoAction.Invoke(triggerAction);
                        TriggerAnimation();
                    }                        
                }
            }
        }

        public virtual bool actionConditions
        {
            get
            {
                return !(tpInput.cc.isJumping || tpInput.cc.actions || !canTriggerAction || isPlayingAnimation) && !tpInput.cc.animator.IsInTransition(0);
            }
        }

        protected virtual void TriggerAnimation()
        {
            if (debugMode) Debug.Log("TriggerAnimation");           

            // trigger the animation behaviour & match target
            if (!string.IsNullOrEmpty(triggerAction.playAnimation))
            {
                isPlayingAnimation = true;                                                  
                tpInput.cc.animator.CrossFadeInFixedTime(triggerAction.playAnimation, 0.1f);    // trigger the action animation clip
            }

            // trigger OnDoAction Event, you can add a delay in the inspector   
            StartCoroutine(triggerAction.OnDoActionDelay(gameObject));

            // bool to limit the autoAction run just once
            if (triggerAction.autoAction) triggerActionOnce = true;

            // destroy the triggerAction if checked with destroyAfter
            if (triggerAction.destroyAfter)            
                StartCoroutine(DestroyDelay());
        }

        public virtual IEnumerator DestroyDelay()
        {
            yield return new WaitForSeconds(triggerAction.destroyDelay);            
            ResetPlayerSettings();
            Destroy(triggerAction.gameObject);
        }

        protected virtual void AnimationBehaviour()
        {
            if (playingAnimation)
            {
                if (triggerAction.matchTarget != null)
                {
                    if (debugMode) Debug.Log("Match Target...");
                    // use match target to match the Y and Z target 
                    tpInput.cc.MatchTarget(triggerAction.matchTarget.transform.position, triggerAction.matchTarget.transform.rotation, triggerAction.avatarTarget, 
                        new MatchTargetWeightMask(triggerAction.matchTargetMask, 0), triggerAction.startMatchTarget, triggerAction.endMatchTarget);
                }

                if (triggerAction.useTriggerRotation)
                {
                    if (debugMode) Debug.Log("Rotate to Target...");
                    // smoothly rotate the character to the target
                    transform.rotation = Quaternion.Lerp(transform.rotation, triggerAction.transform.rotation, tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                }

                if (triggerAction.resetPlayerSettings && tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= triggerAction.endExitTimeAnimation)
                {
                    if (debugMode) Debug.Log("Finish Animation");
                    // after playing the animation we reset some values
                    ResetPlayerSettings();
                }
            }           
        }

        protected virtual bool playingAnimation
        {
            get
            {
                if (triggerAction == null)
                {
                    isPlayingAnimation = false;
                    return false;
                }

                if (!isPlayingAnimation && !string.IsNullOrEmpty(triggerAction.playAnimation) && tpInput.cc.baseLayerInfo.IsName(triggerAction.playAnimation))
                {
                    isPlayingAnimation = true;
                    ApplyPlayerSettings();
                }                    
                else if(isPlayingAnimation && !string.IsNullOrEmpty(triggerAction.playAnimation) && !tpInput.cc.baseLayerInfo.IsName(triggerAction.playAnimation))
                    isPlayingAnimation = false;

                return isPlayingAnimation;
            }
        }

        public override void OnActionEnter(Collider other)
        {
            if (other.gameObject.CompareTag(actionTag))
            {
                if (triggerAction != null) triggerAction.OnPlayerEnter.Invoke();
            }
        }

        public override void OnActionStay(Collider other)
        {
            if (other.gameObject.CompareTag(actionTag) && !isPlayingAnimation)
            {
                CheckForTriggerAction(other);
            }
        }

        public override void OnActionExit(Collider other)
        {
            if (other.gameObject.CompareTag(actionTag))
            {
                if (debugMode) Debug.Log("Exit vTriggerAction");
                if (triggerAction != null) triggerAction.OnPlayerExit.Invoke();
                ResetPlayerSettings();
            }                
        }

        protected virtual void CheckForTriggerAction(Collider other)
        {
            var _triggerAction = other.GetComponent<vTriggerGenericAction>();
            if (!_triggerAction || canTriggerAction) return;
            var dist = Vector3.Distance(transform.forward, _triggerAction.transform.forward);
            if (!_triggerAction.activeFromForward || dist <= 0.8f)
            {
                triggerAction = _triggerAction;                
                canTriggerAction = true;
                triggerAction.OnPlayerEnter.Invoke();
            }
            else
            {             
                if (triggerAction != null) triggerAction.OnPlayerExit.Invoke();
                canTriggerAction = false;
            }
        }

        protected virtual void ApplyPlayerSettings()
        {
            if (debugMode) Debug.Log("ApplyPlayerSettings");

            if (triggerAction.disableGravity)
            {
                tpInput.cc._rigidbody.useGravity = false;               // disable gravity of the player
                tpInput.cc._rigidbody.velocity = Vector3.zero;
                tpInput.cc.isGrounded = true;                           // ground the character so that we can run the root motion without any issues
                tpInput.cc.animator.SetBool("IsGrounded", true);        // also ground the character on the animator so that he won't float after finishes the climb animation
                tpInput.cc.animator.SetInteger("ActionState", 1);       // set actionState 1 to avoid falling transitions     
            }
            if (triggerAction.disableCollision)
                tpInput.cc._capsuleCollider.isTrigger = true;           // disable the collision of the player if necessary 
        }

        protected virtual void ResetPlayerSettings()
        {
            if (debugMode) Debug.Log("Reset Player Settings");
            if(!playingAnimation || tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= triggerAction.endExitTimeAnimation)
            {
                tpInput.cc.EnableGravityAndCollision(0f);             // enable again the gravity and collision
                tpInput.cc.animator.SetInteger("ActionState", 0);     // set actionState 1 to avoid falling transitions
            }

            canTriggerAction = false;
            triggerActionOnce = false;
        }
    }
}