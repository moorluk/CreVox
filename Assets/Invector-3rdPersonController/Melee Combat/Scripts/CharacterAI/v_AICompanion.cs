using UnityEngine;
using System.Collections;

namespace Invector
{
    public class v_AICompanion : v_AIController
    {
        [Header("--- Companion ---")]
        public string companionTag = "Player";
        public float companionMaxDistance = 10f;
        [Range(0f, 1.5f)]
        public float followSpeed = 1f;
        public float followStopDistance = 2f;
        [Range(0f, 1.5f)]
        public float moveToSpeed = 1f;
        public float moveToStopDistance = 0.5f;
        public Transform moveToTarget;

        public CompanionState companionState = CompanionState.Follow;
        public Transform companion;
        public bool debug = true;
        public UnityEngine.UI.Text debugUIText;

        public enum CompanionState
        {
            None, // this state works with AiController normal rotine
            Follow,
            MoveTo,
            Stay
        }

        protected virtual void LateUpdate()
        {

            CompanionInputs();
        }

        void CompanionInputs()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                companionState = CompanionState.Stay;
                agressiveAtFirstSight = false;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                companionState = CompanionState.Follow;
                agressiveAtFirstSight = false;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                agressiveAtFirstSight = !agressiveAtFirstSight;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) && moveToTarget != null)
            {
                SetMoveTo(moveToTarget);
                companionState = CompanionState.MoveTo;
                agressiveAtFirstSight = false;
            }
        }

        /// <summary>
        /// Gets the companion distance.
        /// </summary>
        /// <value>The companion distance.</value>
        float companionDistance
        {
            get { return companion != null ? Vector3.Distance(transform.position, companion.transform.position) : 0f; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="v_AICompanion"/> is near of companion. Relative to <see cref="companionMaxDistance"/>
        /// </summary>
        /// <value><c>true</c> if near of companion; otherwise, <c>false</c>.</value>
        bool nearOfCompanion
        {
            get
            {
                var value = ((companion != null && companion.gameObject.activeSelf && companionDistance < companionMaxDistance) || (companion == null || !companion.gameObject.activeSelf));
                return value;
            }
        }

        /// <summary>
        /// Sets the target Move to.
        /// </summary>
        /// <param name="_target">Target.</param>
        public void SetMoveTo(Transform _target)
        {
            companionState = CompanionState.MoveTo;
            moveToTarget = _target;
        }

        #region Override Ai Controller rotine
        protected override void Start()
        {
            try
            {
                var comp = GameObject.FindGameObjectWithTag(companionTag);
                if (comp != null)
                    companion = comp.transform;
                else
                {
                    companionState = CompanionState.None;
                    Debug.LogWarning("Cant find the " + companionTag);
                }
            }
            catch (UnityException e)
            {
                companionState = CompanionState.None;
                Debug.LogWarning("AICompanion Cant find the " + companionTag);
                Debug.LogWarning("AICompanion " + e.Message);
            }
            Init();
            agent.enabled = true;
            StartCoroutine(CompanionStateRoutine());
            StartCoroutine(FindTarget());
            StartCoroutine(DestinationBehaviour());
        }

        /// <summary>
        /// override <see cref="v_AICompanion.StateRoutine()"/>
        /// ps: this rotine work with internal while loop
        /// </summary>
        /// <returns></returns>
        protected IEnumerator CompanionStateRoutine()
        {
            while (this.enabled)
            {
                yield return new WaitForEndOfFrame();
                System.Text.StringBuilder debugString = new System.Text.StringBuilder();
                debugString.AppendLine("----DEBUG----");
                debugString.AppendLine("Agressive : " + agressiveAtFirstSight);

                CheckIsOnNavMesh();
                CheckAutoCrouch();
                SetTarget();

                //Companion Behavior (override Aicontroller Behavior)
                switch (companionState)
                {
                    #region Companion rotine
                    case CompanionState.Follow:
                        if (canSeeTarget && nearOfCompanion)
                            yield return StartCoroutine(base.Chase());
                        else
                        {
                            yield return StartCoroutine(FollowCompanion());
                        }

                        debugString.AppendLine(canSeeTarget && nearOfCompanion ? "Chase/Follow" : "Follow");

                        break;
                    case CompanionState.MoveTo:
                        if (canSeeTarget)
                            yield return StartCoroutine(base.Chase());
                        else
                        {
                            yield return StartCoroutine(MoveTo());
                        }

                        debugString.AppendLine(canSeeTarget ? "Chase/MoveTo" : "MoveTo");
                        break;
                    case CompanionState.Stay:
                        if (canSeeTarget)
                            yield return StartCoroutine(base.Chase());
                        else
                            yield return StartCoroutine(Stay());
                        debugString.AppendLine(canSeeTarget ? "Chase/Stay" : "Stay");
                        break;
                    #endregion
                    case CompanionState.None:
                        //Aicontroller Behavior
                        #region Ai controller Normal Rotine
                        debugString.AppendLine("None : using normal AI routine");
                        switch (currentState)
                        {
                            case AIStates.Idle:
                                debugString.AppendLine("idle");
                                yield return StartCoroutine(base.Idle());
                                break;
                            case AIStates.Chase:
                                yield return StartCoroutine(base.Chase());
                                break;
                            case AIStates.PatrolSubPoints:
                                yield return StartCoroutine(base.PatrolSubPoints());
                                break;
                            case AIStates.PatrolWaypoints:
                                yield return StartCoroutine(base.PatrolWaypoints());
                                break;
                        }
                        break;
                        #endregion
                }
                if (debugUIText != null && debug)
                {
                    debugUIText.text = debugString.ToString();
                }
            }
        }

        /// <summary>
        /// override <see cref="v_AICompanion.Idle()"/>
        /// </summary>
        /// <returns></returns>
        protected IEnumerator Stay()
        {
            if (companion != null)
            {
                agent.speed = Mathf.Lerp(agent.speed, 0, 2f * Time.deltaTime);
            }
            else
            {
                yield return StartCoroutine(Idle());
            }
        }

        protected override void SetAgressive(bool value)
        {
            if (companionState != CompanionState.Follow)
                base.SetAgressive(value);
        }
        #endregion

        #region Companion rotine
        /// <summary>
        /// Follows the companion.
        /// </summary>
        /// <returns>The companion.</returns>
        IEnumerator FollowCompanion()
        {
            while (!agent.enabled || currentHealth <= 0)
                yield return null;

            // check if companion exist in Scene to work follow rotine
            if (companion != null && companion.gameObject.activeSelf)
            {
                agent.speed = Mathf.Lerp(agent.speed, followSpeed, 10f * Time.deltaTime);
                agent.stoppingDistance = followStopDistance;
                UpdateDestination(companion.position);
            }
            else // go to start position case companion dont exist
            {
                agent.speed = Mathf.Lerp(agent.speed, moveToSpeed, 10f * Time.deltaTime);
                agent.stoppingDistance = moveToStopDistance;
                UpdateDestination(startPosition);
            }
        }

        /// <summary>
        /// Moves to target applied from <see cref="SetMoveTo"/>
        /// </summary>
        /// <returns>The to.</returns>
        IEnumerator MoveTo()
        {
            while (!agent.enabled || currentHealth <= 0)
                yield return null;

            agent.speed = Mathf.Lerp(agent.speed, moveToSpeed, 2f * Time.deltaTime);
            agent.stoppingDistance = moveToStopDistance;
            // update destination to moveTo target position
            UpdateDestination(moveToTarget.position);
            // check if can see some target (included from SetUpTarget method)
            if (canSeeTarget && nearOfCompanion)
                currentState = AIStates.Chase;
        }

        protected override float maxSpeed
        {
            get
            {
                if (companionState != CompanionState.None)
                {
                    return companionState == CompanionState.Follow ? followSpeed : companionState == CompanionState.MoveTo ? moveToSpeed : 0;
                }
                return base.maxSpeed;
            }
        }
        #endregion
    }
}