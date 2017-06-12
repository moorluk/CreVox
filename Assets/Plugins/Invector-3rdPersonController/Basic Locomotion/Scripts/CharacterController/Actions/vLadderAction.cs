using UnityEngine;
using System.Collections;
using Invector.CharacterController;
using Invector.CharacterController.Actions;

namespace Invector.CharacterController.Actions
{
    [vClassHeader("Ladder Action", "Use the vTriggerLadderAction on your ladder mesh.")]
    public class vLadderAction : vActionListener
    {
        #region public variables

        [Tooltip("Tag of the object you want to access")]
        public string actionTag = "LadderTrigger";
        [Tooltip("Input to enter the ladder")]
        public GenericInput enterInput = new GenericInput("E", "A", "A");
        [Tooltip("Input to exit the ladder")]
        public GenericInput exitInput = new GenericInput("Space", "B", "B");

        public bool debugMode;

        #endregion

        #region protected variables    

        protected vThirdPersonInput tpInput;
        protected vTriggerLadderAction ladderAction;
        protected float speed;
        protected bool isUsingLadder;
        protected bool isExitingLadder;
        protected bool triggerEnterOnce;
        protected bool triggerExitOnce;

        #endregion

        private void Awake()
        {
            actionStay = true;
            actionExit = true;
        }

        void Start()
        {
            tpInput = GetComponent<vThirdPersonInput>();           
        }

        void LateUpdate()
        {
            AutoEnterLadder();
            EnterLadderInput();
            UseLadder();
            ExitLadderInput();
        }

        void OnAnimatorMove()
        {
            if (!isUsingLadder) return;
            // enable movement using root motion
            transform.rotation = tpInput.cc.animator.rootRotation;
            transform.position = tpInput.cc.animator.rootPosition;
        }

        void EnterLadderInput()
        {
            if (ladderAction == null) return;

            if (enterInput.GetButtonDown() && !isUsingLadder && !ladderAction.autoAction)
                TriggerEnterLadder();
        }

        void TriggerEnterLadder()
        {
            if (debugMode) Debug.Log("Enter Ladder");

            triggerEnterOnce = true;
            isUsingLadder = true;
            tpInput.cc.animator.SetInteger("ActionState", 1);     // set actionState 1 to avoid falling transitions            
            tpInput.enabled = false;                              // disable vThirdPersonInput
            tpInput.cc.enabled = false;                           // disable vThirdPersonController, Animator & Motor 
            tpInput.cc.DisableGravityAndCollision();              // disable gravity & turn collision trigger
            tpInput.cc._rigidbody.velocity = Vector3.zero;
            tpInput.cc._rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            ladderAction.OnDoAction.Invoke();

            if (!string.IsNullOrEmpty(ladderAction.playAnimation))
                tpInput.cc.animator.CrossFadeInFixedTime(ladderAction.playAnimation, 0.1f);     // trigger the action animation clip                           
        }

        void UseLadder()
        {
            if (!isUsingLadder) return;

            // update the base layer to know what animations are being played
            tpInput.cc.LayerControl();
            // update camera movement
            tpInput.CameraInput();

            // go up or down 
            tpInput.cc.input.y = tpInput.verticallInput.GetAxis();
            speed = Mathf.Clamp(tpInput.cc.input.y, -1f, 1f);
            tpInput.cc.animator.SetFloat("InputVertical", speed, 0.25f, Time.deltaTime);

            // enter ladder behaviour
            if (ladderAction != null && ladderAction.matchTarget != null)
            {
                if (tpInput.cc.baseLayerInfo.IsName("EnterLadderTop") || tpInput.cc.baseLayerInfo.IsName("EnterLadderBottom"))
                {                   
                    if (debugMode) Debug.Log("Match Target...");
                    // use match target to match the Y and Z target 
                    tpInput.cc.MatchTarget(ladderAction.matchTarget.transform.position, ladderAction.matchTarget.transform.rotation, AvatarTarget.Root, new MatchTargetWeightMask(new Vector3(1, 1, 1), 0), ladderAction.startMatchTarget, ladderAction.endMatchTarget);                 

                    if (ladderAction.useTriggerRotation)
                    {
                        // smoothly rotate the character to the target
                        transform.rotation = Quaternion.Lerp(transform.rotation, ladderAction.matchTarget.transform.rotation, tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                    }
                }
            }

            // exit ladder behaviour
            isExitingLadder = tpInput.cc.baseLayerInfo.IsName("ExitLadderTop") || tpInput.cc.baseLayerInfo.IsName("ExitLadderBottom");

            if (isExitingLadder && tpInput.cc.baseLayerInfo.normalizedTime >= 0.8f)
            {
                // after playing the animation we reset some values
                ResetPlayerSettings();
            }
        }

        void ExitLadderInput()
        {
            if (!isUsingLadder) return;

            if (ladderAction == null)
            {
                // exit ladder at any moment by pressing the cancelInput
                if (tpInput.cc.baseLayerInfo.IsName("ClimbLadder") && exitInput.GetButtonDown())
                {
                    if (debugMode) Debug.Log("Quick Exit");
                    ResetPlayerSettings();
                }
            }
            else
            {
                var animationClip = ladderAction.exitAnimation;
                if (animationClip == "ExitLadderBottom")
                {
                    // exit ladder when reach the bottom by pressing the cancelInput or pressing down at
                    if (exitInput.GetButtonDown() || (speed <= -0.05f && !triggerExitOnce))
                    {
                        if (debugMode) Debug.Log("Exit Bottom");
                        triggerExitOnce = true;
                        tpInput.cc.animator.CrossFadeInFixedTime(ladderAction.exitAnimation, 0.1f);             // trigger the animation clip        
                    }
                }
                else if (animationClip == "ExitLadderTop" && tpInput.cc.baseLayerInfo.IsName("ClimbLadder"))    // exit the ladder from the top
                {
                    if ((speed >= 0.05f) && !triggerExitOnce && !tpInput.cc.animator.IsInTransition(0))         // trigger the exit animation by pressing up
                    {
                        if (debugMode) Debug.Log("Exit Top");
                        triggerExitOnce = true;
                        tpInput.cc.animator.CrossFadeInFixedTime(ladderAction.exitAnimation, 0.1f);             // trigger the animation clip
                    }
                }
            }
        }

        void AutoEnterLadder()
        {
            if (ladderAction == null || !ladderAction.autoAction) return;
            if (isUsingLadder || tpInput.cc.animator.IsInTransition(0)) return;

            // enter the ladder automatically if checked with autoAction
            if (ladderAction.autoAction && tpInput.cc.input != Vector2.zero && !tpInput.cc.actions)
            {
                var inputDir = Camera.main.transform.TransformDirection(new Vector3(tpInput.cc.input.x, 0f, tpInput.cc.input.y));
                inputDir.y = 0f;
                var dist = Vector3.Distance(inputDir.normalized, ladderAction.transform.forward);
                if (dist < 0.8f)
                    TriggerEnterLadder();
            }
        }

        void ResetPlayerSettings()
        {
            if (debugMode) Debug.Log("Reset Player Settings");
            speed = 0f;
            ladderAction = null;
            isUsingLadder = false;
            triggerExitOnce = false;
            triggerEnterOnce = false;
            tpInput.cc._capsuleCollider.isTrigger = false;
            tpInput.cc._rigidbody.useGravity = true;
            tpInput.cc.animator.SetInteger("ActionState", 0);
            tpInput.cc._rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            tpInput.cc.enabled = true;
            tpInput.enabled = true;
        }

        public override void OnActionStay(Collider other)
        {
            if (other.gameObject.CompareTag(actionTag))
            {
                CheckForTriggerAction(other);
            }
        }

        public override void OnActionExit(Collider other)
        {
            if (other.gameObject.CompareTag(actionTag))
            {
                ladderAction = null;
            }
        }

        void CheckForTriggerAction(Collider other)
        {
            // assign the component - it will be null when he exit the trigger area
            var _ladderAction = other.GetComponent<vTriggerLadderAction>();
            if (!_ladderAction)
            {
                return;
            }
            // check the maxAngle too see if the character can do the action
            var dist = Vector3.Distance(transform.forward, _ladderAction.transform.forward);

            if (isUsingLadder && _ladderAction != null)
                ladderAction = _ladderAction;
            else if (dist <= 0.8f && !isUsingLadder)
            {
                ladderAction = _ladderAction;
                ladderAction.OnPlayerEnter.Invoke();
            }
            else
            {
                if (ladderAction != null) ladderAction.OnPlayerExit.Invoke();
                ladderAction = null;
            }
        }
    }
}