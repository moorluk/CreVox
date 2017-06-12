using UnityEngine;
using System.Collections;
#if MOBILE_INPUT
using UnityStandardAssets.CrossPlatformInput;
#endif
using Invector.EventSystems;
using System;

namespace Invector.CharacterController
{
    // here you can modify the Melee Combat inputs
    // if you want to modify the Basic Locomotion inputs, go to the vThirdPersonInput
    [vClassHeader("Melee Input Manager")]
    public class vMeleeCombatInput : vThirdPersonInput, vIMeleeFighter
    {
        [System.Serializable]
        public class OnUpdateEvent : UnityEngine.Events.UnityEvent<vMeleeCombatInput> { }

        #region Variables                

        protected vMeleeManager meleeManager;
        protected bool isAttacking;
        protected bool isBlocking;
        protected bool isLockingOn;

        [Header("MeleeCombat Inputs")]
        public GenericInput weakAttackInput = new GenericInput("Mouse0", "RB", "RB");
        public GenericInput strongAttackInput = new GenericInput("Alpha1", false, "RT", true, "RT", false);
        public GenericInput blockInput = new GenericInput("Mouse1", "LB", "LB");
        public bool strafeWhileLockOn = true;
        public GenericInput lockOnInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");

        [HideInInspector]
        public OnUpdateEvent onUpdateInput = new OnUpdateEvent();
        [HideInInspector]
        public bool lockInputByItemManager;

        #endregion

        protected override void Start()
        {
            base.Start();
        }

        public virtual bool lockInventory
        {
            get
            {
                return isAttacking || cc.isDead;
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            onUpdateInput.Invoke(this);
        }

        protected override void InputHandle()
        {
            if (cc == null) return;

            if (MeleeAttackConditions && !lockInputByItemManager)
            {
                MeleeWeakAttackInput();
                MeleeStrongAttackInput();
                BlockingInput();
            }
            else
            {
                isBlocking = false;
            }

            if (!isAttacking)
            {
                base.InputHandle();
                UpdateMeleeAnimations();
            }

            LockOnInput();
        }

        #region MeleeCombat Input Methods

        /// <summary>
        /// WEAK ATK INPUT
        /// </summary>
        protected virtual void MeleeWeakAttackInput()
        {
            if (cc.animator == null) return;

            if (weakAttackInput.GetButtonDown() && MeleeAttackStaminaConditions())
            {
                cc.animator.SetInteger("AttackID", meleeManager.GetAttackID());
                cc.animator.SetTrigger("WeakAttack");
            }
        }

        /// <summary>
        /// STRONG ATK INPUT
        /// </summary>
        protected virtual void MeleeStrongAttackInput()
        {
            if (cc.animator == null) return;

            if (strongAttackInput.GetButtonDown() && MeleeAttackStaminaConditions())
            {
                cc.animator.SetInteger("AttackID", meleeManager.GetAttackID());
                cc.animator.SetTrigger("StrongAttack");
            }
        }

        /// <summary>
        /// BLOCK INPUT
        /// </summary>
        protected virtual void BlockingInput()
        {
            if (cc.animator == null) return;

            isBlocking = blockInput.GetButton() && cc.currentStamina > 0;
        }

        /// <summary>
        /// LOCK ON INPUT
        /// </summary>
        protected void LockOnInput()
        {
            // lock the camera into a target, if there is any around
            if (lockOnInput.GetButtonDown() && !cc.actions)
            {
                isLockingOn = !isLockingOn;
                tpCamera.UpdateLockOn(isLockingOn);
            }
            // unlock the camera if the target is null
            else if (isLockingOn && tpCamera.lockTarget == null)
            {
                isLockingOn = false;
                tpCamera.UpdateLockOn(false);
            }

            // choose to use lock-on with strafe of free movement
            if (!cc.locomotionType.Equals(vThirdPersonMotor.LocomotionType.OnlyStrafe))
            {
                if (strafeWhileLockOn && isLockingOn && tpCamera.lockTarget != null)
                    cc.isStrafing = true;
                else
                    cc.isStrafing = false;
            }

            // switch targets using inputs
            SwitchTargetsInput();
        }

        /// <summary>
        /// SWITCH TARGETS INPUT
        /// </summary>
        void SwitchTargetsInput()
        {
            if (tpCamera == null) return;

            if (tpCamera.lockTarget)
            {
                // switch between targets using Keyboard
                if (inputDevice == InputDevice.MouseKeyboard)
                {
                    if (Input.GetKey(KeyCode.X))
                        tpCamera.gameObject.SendMessage("ChangeTarget", 1, SendMessageOptions.DontRequireReceiver);
                    else if (Input.GetKey(KeyCode.Z))
                        tpCamera.gameObject.SendMessage("ChangeTarget", -1, SendMessageOptions.DontRequireReceiver);
                }
                // switch between targets using GamePad
                else if (inputDevice == InputDevice.Joystick)
                {
                    var value = Input.GetAxisRaw("RightAnalogHorizontal");
                    if (value == 1)
                        tpCamera.gameObject.SendMessage("ChangeTarget", 1, SendMessageOptions.DontRequireReceiver);
                    else if (value == -1f)
                        tpCamera.gameObject.SendMessage("ChangeTarget", -1, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        #endregion

        #region Conditions

        protected virtual bool MeleeAttackStaminaConditions()
        {
            var result = cc.currentStamina - meleeManager.GetAttackStaminaCost();
            return result >= 0;
        }

        protected virtual bool MeleeAttackConditions
        {
            get
            {
                if (meleeManager == null) meleeManager = GetComponent<vMeleeManager>();
                return meleeManager != null && !cc.customAction && !cc.lockMovement && !cc.isCrouching;
            }
        }

        #endregion

        #region Update Animations

        protected virtual void UpdateMeleeAnimations()
        {
            if (cc.animator == null || meleeManager == null) return;
            cc.animator.SetInteger("AttackID", meleeManager.GetAttackID());
            cc.animator.SetInteger("DefenseID", meleeManager.GetDefenseID());
            cc.animator.SetBool("IsBlocking", isBlocking);
            cc.animator.SetFloat("MoveSet_ID", meleeManager.GetMoveSetID(), .2f, Time.deltaTime);
        }

        #endregion

        #region Melee Methods

        public void OnEnableAttack()
        {
            cc.currentStaminaRecoveryDelay = meleeManager.GetAttackStaminaRecoveryDelay();
            cc.currentStamina -= meleeManager.GetAttackStaminaCost();
            isAttacking = true;
        }

        public void OnDisableAttack()
        {
            isAttacking = false;
        }

        public void ResetAttackTriggers()
        {
            cc.animator.ResetTrigger("WeakAttack");
            cc.animator.ResetTrigger("StrongAttack");
        }

        public void BreakAttack(int breakAtkID)
        {
            ResetAttackTriggers();
            OnRecoil(breakAtkID);
        }

        public void OnRecoil(int recoilID)
        {
            cc.animator.SetInteger("RecoilID", recoilID);
            cc.animator.SetTrigger("TriggerRecoil");
            cc.animator.SetTrigger("ResetState");
            cc.animator.ResetTrigger("WeakAttack");
            cc.animator.ResetTrigger("StrongAttack");
        }

        public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
        {
            // character is blocking
            if (!damage.ignoreDefense && isBlocking && meleeManager != null && meleeManager.CanBlockAttack(attacker.Character().transform.position))
            {
                var damageReduction = meleeManager.GetDefenseRate();
                if (damageReduction > 0)
                    damage.ReduceDamage(damageReduction);
                if (attacker != null && meleeManager != null && meleeManager.CanBreakAttack())
                    attacker.OnRecoil(meleeManager.GetDefenseRecoilID());
                meleeManager.OnDefense();
                cc.currentStaminaRecoveryDelay = damage.staminaRecoveryDelay;
                cc.currentStamina -= damage.staminaBlockCost;
            }
            // apply damage
            cc.TakeDamage(damage, !isBlocking);
        }

        public vCharacter Character()
        {
            return cc;
        }

        #endregion

    }
}

