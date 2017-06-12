using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Invector.EventSystems;
using Invector.CharacterController;
#if UNITY_5_5_OR_NEWER
using UnityEngine.AI;
#endif

namespace Invector
{
    public class v_AIMotor : vCharacter
    {
        #region Variables

        #region Layers
        [Header("---! Layers !---")]
        [Tooltip("Layers that the character can walk on")]
        public LayerMask groundLayer = 1 << 0;
        [Tooltip("Distance to became not grounded")]
        [SerializeField]
        protected float groundCheckDistance = 0.5f;

        [Tooltip("What objects can make the character auto crouch")]
        public LayerMask autoCrouchLayer = 1 << 0;
        [Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
        public float headDetect = 0.95f;
        #endregion

        #region AI variables
        [Header("--- Locomotion Speed ---")]
        [Tooltip("Use to limit your locomotion animation, if you want to patrol walking set this value to 0.5f")]
        [Range(0f, 1.5f)]
        public float patrolSpeed = 0.5f;
        [Tooltip("Use to limit your locomotion animation, if you want to chase the target walking set this value to 0.5f")]
        [Range(0f, 1.5f)]
        public float chaseSpeed = 1f;
        [Tooltip("Use to limit your locomotion animation, if you want to strafe the target walking set this value to 0.5f")]
        [Range(0f, 1.5f)]
        public float strafeSpeed = 1f;

	    [Header("--- Sensors - Who is your Target? ---")]        
	    public v_AISphereSensor sphereSensor;
	    public List<string> tagsToDetect = new List<string>() { "Player" };
        public AIStates currentState = AIStates.PatrolWaypoints;
        public bool drawAgentPath = false;
        public bool displayGizmos;
        [Range(0f, 180f)]
        public float fieldOfView = 95f;
        [Tooltip("Max Distance to detect the Target with FOV")]
        public float maxDetectDistance = 5f;
        [Tooltip("Min Distance to noticed the Target without FOV")]
        public float minDetectDistance = 2f;
        [Tooltip("Distance to lost the Target")]
        public float distanceToLostTarget = 20f;
        [Tooltip("Distance to stop when chasing the Player")]
        public float chaseStopDistance = 1f;      

        [Header("--- Strafe ---")]
        [Tooltip("Strafe around the target")]
        public bool strafeSideways = true;
        [Tooltip("Strafe a few steps backwards")]
        public bool strafeBackward = true;
        [Tooltip("Distance to switch to the strafe locomotion, leave with 0 if you don't want your character to strafe")]
        public float strafeDistance = 3f;
        [Tooltip("Min time to change the strafe direction")]
        public float minStrafeSwape = 2f;
        [Tooltip("Max time to change the strafe direction")]
        public float maxStrafeSwape = 5f;
        [Tooltip("Velocity to rotate the character while strafing")]
        public float strafeRotationSpeed = 5f;

        [Header("--- Combat ---")]
	    
        [Tooltip("Check if you want the Enemy to be passive even if you attack him")]
	    public bool passiveToDamage = false;
        [Tooltip("Check if you want the Enemy to chase the Target at first sight")]
        public bool agressiveAtFirstSight = true;
        [Tooltip("Velocity to rotate the character while attacking")]
        public float attackRotationSpeed = 0.5f;
        [Tooltip("Delay to trigger the first attack when close to the target")]
        public float firstAttackDelay = 0f;
        [Tooltip("Min frequency to attack")]
        public float minTimeToAttack = 4f;
        [Tooltip("Max frequency to attack")]
        public float maxTimeToAttack = 6f;
        [Tooltip("How many attacks the AI will make on a combo")]
        public int maxAttackCount = 3;
        [Tooltip("Randomly attacks based on the maxAttackCount")]
        public bool randomAttackCount = true;
        [Range(0f, 1f)]
        public float chanceToRoll = .1f;
        [Range(0f, 1f)]
        public float chanceToBlockInStrafe = .1f;
        [Range(0f, 1f)]
        public float chanceToBlockAttack = 0f;
        [Tooltip("How much time the character will stand up the shield")]
        public float raiseShield = 4f;
        [Tooltip("How much time the character will lower the shield")]
        public float lowerShield = 2f;

        [Header("--- Waypoint ---")]
        [Tooltip("Max Distance to change waypoint")]
        [Range(0.5f, 2f)]
        public float distanceToChangeWaypoint = 1f;
        [Tooltip("Min Distance to stop when Patrolling through waypoints")]
        [Range(0.5f, 2f)]
        public float patrollingStopDistance = 0.5f;
        public vWaypointArea pathArea;
        public bool randomWaypoints;

        public vFisherYatesRandom randomWaypoint = new vFisherYatesRandom();
        public vFisherYatesRandom randomPatrolPoint = new vFisherYatesRandom();
        [HideInInspector]
        public CapsuleCollider _capsuleCollider;
        // there is a prefab of health hud example that you can drag and drop into the head bone of your character
        [HideInInspector]
        public v_SpriteHealth healthSlider;
        // attach a meleeManager component to create new hitboxs and set up different weapons
        [HideInInspector]
        public vMeleeManager meleeManager;
        // check your MeleeWeapon Inspector, each weapon can set up different distances to attack        
        public OnSetAgressiveEvent onSetAgressive = new OnSetAgressiveEvent();
        public class OnSetAgressiveEvent : UnityEngine.Events.UnityEvent<bool> { }
        [HideInInspector]
        public bool lockMovement;
        public enum AIStates
        {
            Idle,
            PatrolSubPoints,
            PatrolWaypoints,
            Chase
        }
        #endregion

        #region Protected Variables
    
        protected Transform target;
        protected Vector3 targetPos;
        protected bool canSeeTarget;
        protected Vector3 destination;
        protected Vector3 fwd;
        protected bool isGrounded;
        protected bool isStrafing;
        [HideInInspector]
        public bool isAttacking;
        protected bool inResetAttack;
        protected bool firstAttack = true;
        protected int attackCount;
        protected int currentWaypoint;
        protected int currentPatrolPoint;
        protected float direction;
        protected float timer, wait;
        protected float fovAngle;
        protected float sideMovement, fwdMovement = 0;
        protected float strafeSwapeFrequency;
        protected float groundDistance;     
        protected Vector3 startPosition;        
        protected RaycastHit groundHit;
        protected NavMeshAgent agent;
        protected NavMeshPath agentPath;
        protected Quaternion freeRotation;
        protected Quaternion desiredRotation;
        protected Vector3 oldPosition;
        protected Vector3 combatMovement;
        protected Vector3 rollDirection;
        protected Rigidbody _rigidbody;
        protected PhysicMaterial frictionPhysics;
        protected Transform head;
        protected Collider colliderTarget;
        protected vWaypoint targetWaypoint;
        protected vPoint targetPatrolPoint;
        protected System.Collections.Generic.List<vPoint> visitedPatrolPoint = new System.Collections.Generic.List<vPoint>();
        protected System.Collections.Generic.List<vWaypoint> visitedWaypoint = new System.Collections.Generic.List<vWaypoint>();
       
        #endregion

        #region Actions
        protected bool 
           isCrouched,
           canAttack,
           isBlocking,
           tryingBlock,
           isRolling;

        // one bool to rule then all
        public bool actions { get { return lockMovement || isRolling; } }

        #endregion

        #endregion

        public override void Init()
        {
            base.Init();
            if (randomWaypoint == null)
                randomWaypoint = new vFisherYatesRandom();
            if (randomPatrolPoint == null)
	            randomPatrolPoint = new vFisherYatesRandom();          
	        
            currentWaypoint = -1;
            currentPatrolPoint = -1;
            fwd = transform.forward;
            destination = transform.position;
            agent = GetComponent<NavMeshAgent>();
           
            agentPath = new NavMeshPath();
            sphereSensor = GetComponentInChildren<v_AISphereSensor>();
	        if (sphereSensor)
	        {
	        	sphereSensor.SetColliderRadius(maxDetectDistance);
	        	sphereSensor.tagsToDetect = tagsToDetect;
	        	sphereSensor.passiveToDamage = passiveToDamage;
	        }
                
            animator = GetComponent<Animator>();
            meleeManager = GetComponent<vMeleeManager>();
            canAttack = true;
            attackCount = 0;
            sideMovement = GetRandonSide();
            destination = transform.position;

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = true;
            _rigidbody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.enabled = false;
            _capsuleCollider = GetComponent<CapsuleCollider>();

            // avoid collision detection with inside colliders 
            Collider[] AllColliders = this.GetComponentsInChildren<Collider>();
            Collider thisCollider = GetComponent<Collider>();
            for (int i = 0; i < AllColliders.Length; i++)
            {
                Physics.IgnoreCollision(thisCollider, AllColliders[i]);
            }

            healthSlider = GetComponentInChildren<v_SpriteHealth>();
            head = animator.GetBoneTransform(HumanBodyBones.Head);
            oldPosition = transform.position;
            currentHealth = maxHealth;
            startPosition = transform.position;
        }

        #region AI Locomotion
        public float distanceToAttack
        {
            get { if (meleeManager) return meleeManager.GetAttackDistance(); return 1f; }
        }
        public bool OnCombatArea
        {
            get
            {
                if (target == null) return false;
                var inFloor = Vector3.Distance(new Vector3(0, transform.position.y, 0), new Vector3(0, target.position.y, 0)) < distanceToAttack;
                return (inFloor && agressiveAtFirstSight && TargetDistance <= strafeDistance && !agent.isOnOffMeshLink);
            }
        }

        public bool OnStrafeArea
        {
            get
            {
                if (!canSeeTarget)
                {
                    isStrafing = false;
                    return false;
                }

                if (target == null || !agressiveAtFirstSight) return false;
                if (currentState.Equals(AIStates.PatrolSubPoints)) return false;

                var inFloor = Vector3.Distance(new Vector3(0, transform.position.y, 0), new Vector3(0, target.position.y, 0)) < 1.5f;
                // exit strafe 
                if (isStrafing)
                {                    
                    isStrafing = TargetDistance < (strafeDistance + 2f);
                }
                // enter strafe                 
                else
                    isStrafing = OnCombatArea;

                return inFloor ? isStrafing : false;
            }
        }

        public bool AgentDone()
        {
            if (!agent.enabled && agent.updatePosition)
                return true;
            return !agent.pathPending && AgentStopping() && agent.updatePosition;
        }

        public bool AgentStopping()
        {
            if (!agent.enabled || !agent.isOnNavMesh)
                return true;
            return agent.remainingDistance <= agent.stoppingDistance;
        }

        public int GetRandonSide()
        {
            var side = UnityEngine.Random.Range(-1, 1);
            if (side < 0)
            {
                side = -1;
            }
            else side = 1;
            return side;
        }

        protected void CheckGroundDistance()
        {
            if (_capsuleCollider != null)
            {
                var dist = 10f;
                Ray ray1 = new Ray(transform.position + new Vector3(0, _capsuleCollider.height / 2, 0), Vector3.down);

                if (Physics.Raycast(ray1, out groundHit, _capsuleCollider.height * 0.75f, groundLayer))
                    dist = transform.position.y - groundHit.point.y;

                groundDistance = dist;

                if (!actions && !isRolling && groundDistance < 0.3f)
                {
                    if (currentHealth > 0)
                    {
                        NavMeshHit navHit;
                        if (NavMesh.SamplePosition(transform.position, out navHit, 5f, NavMesh.AllAreas))
                        {
                            _rigidbody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                            _rigidbody.useGravity = false;
                            agent.updatePosition = false;
                            agent.enabled = true;
                        }
                    }

                    if (agent.enabled && agent.isOnNavMesh && Vector3.Distance(agent.nextPosition, transform.position) <= 0.1f)
                    {
                        agent.updatePosition = true;
                    }
                    else if (agent.enabled && agent.isOnNavMesh && !agent.updatePosition)
                        agent.nextPosition = transform.position;
                }

                if (!agent.isOnNavMesh && groundDistance > 0.3f && !ragdolled)
                {
                    _rigidbody.useGravity = true;
                    _rigidbody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
                    agent.enabled = false;
                    agent.updatePosition = false;
                }
            }
        }

        public void CheckAutoCrouch()
        {
            // radius of SphereCast
            float radius = _capsuleCollider.radius * 0.9f;
            // Position of SphereCast origin stating in base of capsule
            Vector3 pos = transform.position + Vector3.up * ((_capsuleCollider.height * 0.5f) - _capsuleCollider.radius);
            // ray for SphereCast
            Ray ray2 = new Ray(pos, Vector3.up);
            RaycastHit groundHit;
            // sphere cast around the base of capsule for check ground distance
            //if (Physics.SphereCast(ray2, radius, out groundHit, _capsuleCollider.bounds.max.y - (_capsuleCollider.radius * 0.1f), groundLayer))
            if (Physics.SphereCast(ray2, radius, out groundHit, headDetect - (_capsuleCollider.radius * 0.1f), autoCrouchLayer))
                isCrouched = true;
            else
                isCrouched = false;
        }
        
        /// <summary>
        /// Waypoint WaitTime
        /// </summary>
        /// <param name="waitTime"></param>
        public void ReturnWaitTime(float waitTime)
        {
            wait = waitTime;
        }

        #endregion

        #region Check Target       

        /// <summary>
        /// Calculate Fov Angle
        /// </summary>
        /// <returns></returns>
        public bool onFovAngle()
        {
            if (target == null) return false;
            var freeRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
            var newAngle = freeRotation.eulerAngles - transform.eulerAngles;
            fovAngle = newAngle.NormalizeAngle().y;

            if (fovAngle < fieldOfView && fovAngle > -fieldOfView)            
                return true;                            

            return false;
        }

        int canSeeTargetIteration;
        /// <summary>
        /// Target Detection
        /// </summary>
        /// <param name="_target"></param>
        /// <returns></returns>
        public void CheckTarget()
        {
            if (target == null || !agressiveAtFirstSight)
            {
                canSeeTarget = false;
                canSeeTargetIteration = 0;
                return;
            }

            if (TargetDistance > maxDetectDistance)
            {
                canSeeTarget = false;
                canSeeTargetIteration = 0;
                return;
            }
            if (colliderTarget == null || colliderTarget.transform != target ) colliderTarget = target.GetComponent<Collider>();
            if (colliderTarget == null)
            {
                canSeeTarget = false;
                canSeeTargetIteration = 0;
                return;
            }
            var top = new Vector3(colliderTarget.bounds.center.x, colliderTarget.bounds.max.y, colliderTarget.bounds.center.z);
            var bottom = new Vector3(colliderTarget.bounds.center.x, colliderTarget.bounds.min.y, colliderTarget.bounds.center.z);
            var offset = Vector3.Distance(top, bottom) * 0.15f;
            top.y -= offset;
            bottom.y += offset;

            if (!onFovAngle() && TargetDistance > minDetectDistance)
            {
                canSeeTarget = false;
                canSeeTargetIteration = 0;
                return;
            }

            //Debug.DrawLine(head.position, top, Color.blue, .1f);
            //Debug.DrawLine(head.position, bottom, Color.blue, .1f);
            //Debug.DrawLine(head.position, colliderTarget.bounds.center, Color.blue, .1f);

            RaycastHit hit;
            if (canSeeTargetIteration ==0 && !Physics.Linecast(head.position, top, out hit, sphereSensor.obstacleLayer))
            {
                canSeeTarget = true;
                canSeeTargetIteration = 0;
                return;
            }

            else if (canSeeTargetIteration == 1 && !Physics.Linecast(head.position, bottom, out hit, sphereSensor.obstacleLayer))
            {
                canSeeTarget = true;
                canSeeTargetIteration = 0;
                return;
            }
            else if (canSeeTargetIteration == 2 && !Physics.Linecast(head.position, colliderTarget.bounds.center, out hit, sphereSensor.obstacleLayer))
            {
                canSeeTarget = true;
                canSeeTargetIteration = 0;
                return;
            }
            else
                canSeeTargetIteration++;
            if (canSeeTargetIteration > 1)
                canSeeTargetIteration = 0;
            canSeeTarget = false;
        }               

        public float TargetDistance
        {
            get
            {
                if (target != null)
                    return Vector3.Distance(transform.position, target.position);
                return maxDetectDistance + 1f;
            }
        }

        public Transform headTarget
        {
            get
            {
                if (target != null && target.GetComponent<Animator>() != null)
                    return target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                else
                    return null;
            }
        }

        #endregion       

        #region AI Health
        
        public void HealthRecovery()
        {            
            // If the player has lost all it's health and the death flag hasn't been set yet...
            if (currentHealth <= 0 && !isDead)
            {
                isDead = true;
                onDead.Invoke(gameObject);
                return;
            }
            if (currentHealthRecoveryDelay > 0)
            {
                currentHealthRecoveryDelay -= Time.deltaTime;
            }
            else
            {
                if (currentHealth > maxHealth)
                    currentHealth = maxHealth;
                if (currentHealth < maxHealth)
                    currentHealth = Mathf.Lerp(currentHealth, maxHealth, healthRecovery * Time.deltaTime);
            }
        }

        protected void RemoveComponents()
        {
            if (_capsuleCollider != null) Destroy(_capsuleCollider);
            if (_rigidbody != null) Destroy(_rigidbody);
            if (animator != null) Destroy(animator);
            if (agent != null) Destroy(agent);
            var comps = GetComponents<MonoBehaviour>();
            for(int i=0;i<comps.Length;i++)
            {
                Destroy(comps[i]);
            }
          
        }

        /// <summary>
        /// APPLY DAMAGE - call this method by a SendMessage with the damage value
        /// </summary>
        /// <param name="damage"> damage to apply </param>
        public override void TakeDamage(vDamage damage,bool hitReaction)
        {
            // ignore damage if the character is rolling, dead or the animator is disable
	        if (isRolling || currentHealth <= 0 || !animator.enabled) return;
            if (!damage.ignoreDefense && !actions && CheckChanceToRoll()) return;

            // instantiate hit particle 
            var hitRotation = Quaternion.LookRotation(new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z) - damage.hitPosition);
            if(damage.damageValue > 0)
                SendMessage("TriggerHitParticle", new vHittEffectInfo(new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z), hitRotation, damage.attackName), SendMessageOptions.DontRequireReceiver);
            // apply damage to the health
            currentHealth -= damage.damageValue;
            currentHealthRecoveryDelay = healthRecoveryDelay;            
          
            if (healthSlider != null)
                healthSlider.Damage(damage.damageValue);
           
            if (animator != null && animator.enabled && !damage.activeRagdoll && !isRolling)
            {
                animator.SetInteger("HitDirection", (int)transform.HitAngle(damage.sender.position));
                // trigger hitReaction animation
                if (hitReaction)
                {
                    // set the ID of the reaction based on the attack animation state of the attacker - Check the MeleeAttackBehaviour script
                    animator.SetInteger("ReactionID", damage.reaction_id);
                    animator.SetTrigger("TriggerReaction");
                    animator.SetTrigger("ResetState");
                }
                else
                {
                    animator.SetInteger("RecoilID", damage.recoil_id);
                    animator.SetTrigger("TriggerRecoil");
                    animator.SetTrigger("ResetState");
                }
            }

            onReceiveDamage.Invoke(damage);           
            if (damage.activeRagdoll)
                onActiveRagdoll.Invoke();
        }

        #endregion

        #region AI Melee Combat Methods
         
        protected bool CheckChanceToRoll()
        {
            if (isAttacking || actions) return false;
            
            float randomRoll = UnityEngine.Random.value;
            if (randomRoll < chanceToRoll && randomRoll > 0 && target != null)
            {
                animator.SetTrigger("ResetState");
                sideMovement = GetRandonSide();
                Ray ray = new Ray(target.position, target.right * sideMovement);
                rollDirection = ray.direction;               
                animator.CrossFadeInFixedTime("Roll", 0.1f);
               
                return true;
            }
            return false;
        }

        protected IEnumerator CheckChanceToBlock(float chance, float timeToEnter)
        {
            tryingBlock = true;
            float randomBlock = UnityEngine.Random.value;
            if (randomBlock < chance && randomBlock > 0 && !isBlocking)
            {
                if (timeToEnter > 0)
                    yield return new WaitForSeconds(timeToEnter);
                isBlocking = target == null || (actions) || isAttacking ? false : true;
                StartCoroutine(ResetBlock());
                tryingBlock = false;
            }
            else
            {
                tryingBlock = false;
            }
        }

        protected IEnumerator ResetBlock()
        {
            yield return new WaitForSeconds(target == null ? 0 : raiseShield);
            isBlocking = false;
        }
        
        protected virtual void SetAgressive(bool value)
        {
            agressiveAtFirstSight = value;//
            onSetAgressive.Invoke(value);
        }
	    
        int GetDamageResult(int damage, float defenseRate)
        {
            int result = (int)(damage - ((damage * defenseRate) / 100));
            return result;
        }

        #endregion

        #region Ragdoll

        public override void ResetRagdoll()
        {
            oldPosition = transform.position;
            ragdolled = false;
            _capsuleCollider.isTrigger = false;
            _rigidbody.isKinematic = false;
            agent.updatePosition = false;
            agent.enabled = true;
        }

        public override void EnableRagdoll()
        {
            agent.enabled = false;
            agent.updatePosition = false;
            ragdolled = true;
            _rigidbody.isKinematic = true;
            _capsuleCollider.isTrigger = true;
        }

        #endregion
    }
}
