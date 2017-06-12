using Invector.EventSystems;
using System.Collections;
using UnityEngine;
using Invector.CharacterController;
namespace Invector
{
    public class v_AIController : v_AIAnimator, vIMeleeFighter
    {
        [Header("--- Iterations ---")]
        public float stateRoutineIteration = 0.15f;
	    public float destinationRoutineIteration = 0.25f;
	    public float findTargetIteration = 0.25f;
        public float smoothSpeed = 5f;
        [Header("--- On Change State Events ---")]
        public UnityEngine.Events.UnityEvent onIdle,onChase,onPatrol;
        protected AIStates oldState;

        protected virtual void Start()
        {
            Init();
            StartCoroutine(StateRoutine());
            StartCoroutine(FindTarget());
            StartCoroutine(DestinationBehaviour());
        }

        protected virtual void FixedUpdate()
        {
            ControlLocomotion();
            HealthRecovery();
        }

        #region AI Target

        protected void SetTarget()
        {
            if (currentHealth > 0 && sphereSensor != null)
            {
                if (target == null || (sphereSensor.getFromDistance))
                {
                    var vChar = sphereSensor.GetTargetvCharacter();
                    if (vChar != null && vChar.currentHealth > 0)
                        target = vChar.GetTransform;
                }

                if (!CheckTargetIsAlive() || TargetDistance > distanceToLostTarget)
                {
                    target = null;
                }
            }
            else if (currentHealth <= 0f)
            {
                destination = transform.position;
                target = null;
            }
        }

        bool CheckTargetIsAlive()
        {
            if (target == null) return false;

            var vChar = target.GetComponent<vCharacter>();
            if (vChar == null) return false;
            else if (vChar.currentHealth > 0)
                return true;

            return false;
        }

        protected IEnumerator FindTarget()
        {
            while (true)
            {
                yield return new WaitForSeconds(findTargetIteration);
                SetTarget();
                CheckTarget();
            }
        }

        #endregion

        #region AI Locomotion

        void ControlLocomotion()
        {
            if (AgentDone() && agent.updatePosition || lockMovement)
            {
                agent.speed = 0f;
                combatMovement = Vector3.zero;
            }
            if (agent.isOnOffMeshLink)
            {
                float speed = agent.desiredVelocity.magnitude;
                UpdateAnimator(AgentDone() ? 0f : speed, direction);
            }
            else
            {
                var desiredVelocity = agent.enabled ? agent.updatePosition ? agent.desiredVelocity : (agent.nextPosition - transform.position) : (destination - transform.position);
                if (OnStrafeArea)
                {
                    var destin = transform.InverseTransformDirection(desiredVelocity).normalized;
                    combatMovement = Vector3.Lerp(combatMovement, destin, 2f * Time.deltaTime);
                    UpdateAnimator(AgentDone() ? 0f : combatMovement.z, combatMovement.x);
                }
                else
                {
                    float speed = desiredVelocity.magnitude;
                    combatMovement = Vector3.zero;
                    UpdateAnimator(AgentDone() ? 0f : speed, 0f);
                }
            }
        }

        Vector3 AgentDirection()
        {
            var forward = AgentDone() ? (target != null && OnStrafeArea && canSeeTarget ?
                         (new Vector3(destination.x, transform.position.y, destination.z) - transform.position) :
                         transform.forward) : agent.desiredVelocity;

            fwd = Vector3.Lerp(fwd, forward, 20 * Time.deltaTime);
            return fwd;
        }

        protected virtual IEnumerator DestinationBehaviour()
        {
            while (true)
            {
                yield return new WaitForSeconds(destinationRoutineIteration);
                CheckGroundDistance();
                if (agent.updatePosition)
                    UpdateDestination(destination);
            }
        }

        protected virtual void UpdateDestination(Vector3 position)
        {
            #region destination routine

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(position);
            }              

            #region debug Path
            if (agent.enabled && agent.hasPath)
            {
                if (drawAgentPath)
                {
                    Debug.DrawLine(transform.position, position, Color.red, 0.5f);
                    var oldPos = transform.position;
                    for (int i = 0; i < agent.path.corners.Length; i++)
                    {
                        var pos = agent.path.corners[i];
                        Debug.DrawLine(oldPos, pos, Color.green, 0.5f);
                        oldPos = pos;
                    }
                }
            }
            #endregion
            #endregion
        }

        protected void CheckIsOnNavMesh()
        {
            // check if the AI is on a valid Navmesh, if not he dies
            if (!agent.isOnNavMesh && agent.enabled && !ragdolled)
            {
                Debug.LogWarning("Missing NavMesh Bake, character will die - Please Bake your navmesh again!");
                currentHealth = 0;
            }
        }

        #endregion

        #region AI States

        protected IEnumerator StateRoutine()
        {
            while (this.enabled)
            {
                CheckIsOnNavMesh();
                CheckAutoCrouch();
                yield return new WaitForSeconds(stateRoutineIteration);
                if (!lockMovement)
                {
                   
                   
                    switch (currentState)
                    {
                        case AIStates.Idle:
                            if (currentState != oldState) { onIdle.Invoke(); oldState = currentState; }
                            yield return StartCoroutine(Idle());
                            break;
                        case AIStates.Chase:
                            if (currentState != oldState) { onChase.Invoke(); oldState = currentState; }
                            yield return StartCoroutine(Chase());
                            break;
                        case AIStates.PatrolSubPoints:
                            if(currentState != oldState) { onPatrol.Invoke(); oldState = currentState; }
                            yield return StartCoroutine(PatrolSubPoints());
                            break;
                        case AIStates.PatrolWaypoints:
                            if (currentState != oldState) { onPatrol.Invoke(); oldState = currentState; }
                            yield return StartCoroutine(PatrolWaypoints());
                            break;
                    }
                   
                }
             
            }
        }

        protected IEnumerator Idle()
        {
            while (currentHealth <= 0) yield return null;
            if (canSeeTarget)
                currentState = AIStates.Chase;
            if (agent.enabled && Vector3.Distance(transform.position, startPosition) > agent.stoppingDistance && !((pathArea && pathArea.waypoints.Count > 0)))
                currentState = AIStates.PatrolWaypoints;
            else if ((pathArea && pathArea.waypoints.Count > 0))
                currentState = AIStates.PatrolWaypoints;
            else
                agent.speed = Mathf.Lerp(agent.speed, 0f, smoothSpeed * Time.deltaTime);

        }

        protected IEnumerator Chase()
        {
            while (currentHealth <= 0) yield return null;
            agent.speed = Mathf.Lerp(agent.speed, chaseSpeed, smoothSpeed * Time.deltaTime);
            agent.stoppingDistance = chaseStopDistance;

            if (!isBlocking && !tryingBlock) StartCoroutine(CheckChanceToBlock(chanceToBlockInStrafe, lowerShield));

	        if (target == null || !agressiveAtFirstSight)
                currentState = AIStates.Idle;

            // begin the Attack Routine when close to the Target
            if (TargetDistance <= distanceToAttack && meleeManager != null && canAttack && !actions)
            {
                canAttack = false;

                yield return StartCoroutine(MeleeAttackRotine());
            }
            if (attackCount <= 0 && !inResetAttack && !isAttacking)
            {
                StartCoroutine(ResetAttackCount());
                yield return null;
            }
            // strafing while close to the Target
            if (OnStrafeArea && strafeSideways)
            {
                //Debug.DrawRay(transform.position, dir * 2, Color.red, 0.2f);
                if (strafeSwapeFrequency <= 0)
                {
                    sideMovement = GetRandonSide();
                    strafeSwapeFrequency = UnityEngine.Random.Range(minStrafeSwape, maxStrafeSwape);
                }
                else
                {
                    strafeSwapeFrequency -= Time.deltaTime;
                }
                fwdMovement = (TargetDistance < distanceToAttack) ? (strafeBackward ? -1 : 0) : TargetDistance > distanceToAttack ? 1 : 0;
                var dir = ((transform.right * sideMovement) + (transform.forward * fwdMovement));
                Ray ray = new Ray(new Vector3(transform.position.x, target != null ? target.position.y : transform.position.y, transform.position.z), dir);
                if (TargetDistance < strafeDistance - 0.5f)
                    destination = OnStrafeArea ? ray.GetPoint(agent.stoppingDistance + 0.5f) : target.position;
                else if (target != null)
                    destination = target.position;
            }
            // chase Target
            else
            {
                if (!OnStrafeArea && target != null)
                    destination = target.position;
                else
                {
                    fwdMovement = (TargetDistance < distanceToAttack) ? (strafeBackward ? -1 : 0) : TargetDistance > distanceToAttack ? 1 : 0;
                    Ray ray = new Ray(transform.position, transform.forward * fwdMovement);
                    if (TargetDistance < strafeDistance - 0.5f)
                        destination = (fwdMovement != 0) ? ray.GetPoint(agent.stoppingDistance + ((fwdMovement > 0) ? TargetDistance : 1f)) : transform.position;
                    else if (target != null)
                        destination = target.position;
                }
            }
        }

        protected IEnumerator PatrolSubPoints()
        {
            while (!agent.enabled) yield return null;

            if (targetWaypoint)
            {
                if (targetPatrolPoint == null || !targetPatrolPoint.isValid)
                {
                    targetPatrolPoint = GetPatrolPoint(targetWaypoint);
                }
                else
                {
                    agent.speed = Mathf.Lerp(agent.speed, (agent.hasPath && targetPatrolPoint.isValid) ? patrolSpeed : 0, smoothSpeed * Time.deltaTime);
                    agent.stoppingDistance = patrollingStopDistance;
                    destination = targetPatrolPoint.isValid ? targetPatrolPoint.position : transform.position;
                    if (Vector3.Distance(transform.position, destination) < targetPatrolPoint.areaRadius && targetPatrolPoint.CanEnter(transform) && !targetPatrolPoint.IsOnWay(transform))
                    {
                        targetPatrolPoint.Enter(transform);
                        wait = targetPatrolPoint.timeToStay;
                        visitedPatrolPoint.Add(targetPatrolPoint);
                    }
                    else if (Vector3.Distance(transform.position, destination) < targetPatrolPoint.areaRadius && (!targetPatrolPoint.CanEnter(transform) || !targetPatrolPoint.isValid)) targetPatrolPoint = GetPatrolPoint(targetWaypoint);

                    if (targetPatrolPoint != null && (targetPatrolPoint.IsOnWay(transform) && Vector3.Distance(transform.position, destination) < distanceToChangeWaypoint))
                    {
                        if (wait <= 0 || !targetPatrolPoint.isValid)
                        {
                            wait = 0;
                            if (visitedPatrolPoint.Count == pathArea.GetValidSubPoints(targetWaypoint).Count)
                            {
                                currentState = AIStates.PatrolWaypoints;
                                targetWaypoint.Exit(transform);
                                targetPatrolPoint.Exit(transform);
                                targetWaypoint = null;
                                targetPatrolPoint = null;
                                visitedPatrolPoint.Clear();
                            }
                            else
                            {
                                targetPatrolPoint.Exit(transform);
                                targetPatrolPoint = GetPatrolPoint(targetWaypoint);
                            }
                        }
                        else if (wait > 0)
                        {
                            if (agent.desiredVelocity.magnitude == 0)
                                wait -= Time.deltaTime;
                        }
                    }
                }
            }
            if (canSeeTarget)
                currentState = AIStates.Chase;
        }

        protected IEnumerator PatrolWaypoints()
        {
            while (!agent.enabled) yield return null;

            if (pathArea != null && pathArea.waypoints.Count > 0)
            {
                if (targetWaypoint == null || !targetWaypoint.isValid)
                {
                    targetWaypoint = GetWaypoint();
                }
                else
                {
                    agent.speed = Mathf.Lerp(agent.speed, (agent.hasPath && targetWaypoint.isValid) ? patrolSpeed : 0, smoothSpeed * Time.deltaTime);

                    agent.stoppingDistance = patrollingStopDistance;

                    destination = targetWaypoint.position;
                    if (Vector3.Distance(transform.position, destination) < targetWaypoint.areaRadius && targetWaypoint.CanEnter(transform) && !targetWaypoint.IsOnWay(transform))
                    {
                        targetWaypoint.Enter(transform);
                        wait = targetWaypoint.timeToStay;
                    }
                    else if (Vector3.Distance(transform.position, destination) < targetWaypoint.areaRadius && (!targetWaypoint.CanEnter(transform) || !targetWaypoint.isValid))
                        targetWaypoint = GetWaypoint();

                    if (targetWaypoint != null && targetWaypoint.IsOnWay(transform) && Vector3.Distance(transform.position, destination) < distanceToChangeWaypoint)
                    {
                        if (wait <= 0 || !targetWaypoint.isValid)
                        {
                            wait = 0;
                            if (targetWaypoint.subPoints.Count > 0)
                                currentState = AIStates.PatrolSubPoints;
                            else
                            {
                                targetWaypoint.Exit(transform);
                                visitedPatrolPoint.Clear();
                                targetWaypoint = GetWaypoint();
                            }
                        }
                        else if (wait > 0)
                        {
                            if (agent.desiredVelocity.magnitude == 0)
                                wait -= Time.deltaTime;
                        }
                    }
                }
            }
            else if (Vector3.Distance(transform.position, startPosition) > patrollingStopDistance)
            {
                agent.speed = Mathf.Lerp(agent.speed, patrolSpeed, smoothSpeed * Time.deltaTime);
                agent.stoppingDistance = patrollingStopDistance;
                destination = startPosition;
            }
            if (canSeeTarget)
                currentState = AIStates.Chase;
        }

        #endregion

        #region AI Waypoint & PatrolPoint

        vWaypoint GetWaypoint()
        {
            var waypoints = pathArea.GetValidPoints();

            if (randomWaypoints) currentWaypoint = randomWaypoint.Next(waypoints.Count);
            else currentWaypoint++;

            if (currentWaypoint >= waypoints.Count) currentWaypoint = 0;
            if (waypoints.Count == 0)
            {
                agent.Stop();
                return null;
            }
            if (visitedWaypoint.Count == waypoints.Count) visitedWaypoint.Clear();

            if (visitedWaypoint.Contains(waypoints[currentWaypoint])) return null;

            agent.Resume();
            return waypoints[currentWaypoint];
        }

        vPoint GetPatrolPoint(vWaypoint waypoint)
        {
            var subPoints = pathArea.GetValidSubPoints(waypoint);
            if (waypoint.randomPatrolPoint) currentPatrolPoint = randomPatrolPoint.Next(subPoints.Count);
            else currentPatrolPoint++;

            if (currentPatrolPoint >= subPoints.Count) currentPatrolPoint = 0;
            if (subPoints.Count == 0)
            {
                agent.Stop();
                return null;
            }
            if (visitedPatrolPoint.Contains(subPoints[currentPatrolPoint])) return null;
            agent.Resume();
            return subPoints[currentPatrolPoint];
        }

        #endregion

        #region AI Melee Combat        

        protected IEnumerator MeleeAttackRotine()
        {
            if (!isAttacking && !actions && attackCount > 0 && !lockMovement)
            {
                sideMovement = GetRandonSide();
                agent.stoppingDistance = distanceToAttack;
                attackCount--;
                MeleeAttack();
                yield return null;
            }
            //else if (!actions && attackCount > 0) canAttack = true;
        }

        public void FinishAttack()
        {
            //  if(attackCount > 0)
            canAttack = true;
        }

        IEnumerator ResetAttackCount()
        {
            inResetAttack = true;
            canAttack = false;
            var value = 0f;
            if (firstAttack)
            {
                firstAttack = false;
                value = firstAttackDelay;
            }
            else value = UnityEngine.Random.Range(minTimeToAttack, maxTimeToAttack);
            yield return new WaitForSeconds(value);
            attackCount = randomAttackCount ? UnityEngine.Random.Range(1, maxAttackCount + 1) : maxAttackCount;
            canAttack = true;
            inResetAttack = false;
        }

        public void OnEnableAttack()
        {
            isAttacking = true;            
        }

        public void OnDisableAttack()
        {
            isAttacking = false;
            canAttack = true;
        }

        public void ResetAttackTriggers()
        {
            animator.ResetTrigger("WeakAttack");
        }

        public void BreakAttack(int breakAtkID)
        {
            ResetAttackCount();
            ResetAttackTriggers();
            OnRecoil(breakAtkID);
        }

        public void OnRecoil(int recoilID)
        {
            TriggerRecoil(recoilID);
        }

        public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
        {
            StartCoroutine(CheckChanceToBlock(chanceToBlockAttack, 0));

            var attackPos = (attacker != null && attacker.Character() != null) ? attacker.Character().transform.position : damage.hitPosition;
            if (!damage.ignoreDefense && isBlocking && meleeManager != null && meleeManager.CanBlockAttack(attackPos))
            {
                var damageReduction = meleeManager != null ? meleeManager.GetDefenseRate() : 0;
                if (damageReduction > 0)
                    damage.ReduceDamage(damageReduction);
                if (attacker != null && meleeManager != null && meleeManager.CanBreakAttack())
                    attacker.OnRecoil(meleeManager.GetDefenseRecoilID());
                meleeManager.OnDefense();
            }
            // apply tag from the character that hit you and start chase
            if (!sphereSensor.passiveToDamage && damage.sender != null)
            {
                target = damage.sender;
                currentState = AIStates.Chase;
                sphereSensor.SetTagToDetect(damage.sender);
            }
	        if(!passiveToDamage)
            	SetAgressive(true);
            TakeDamage(damage, !isBlocking);
        }

        public vCharacter Character()
        {
            return this;
        }
        #endregion
    }
}
