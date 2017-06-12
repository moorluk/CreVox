using UnityEngine;
using System.Collections;
using Invector.CharacterController;
using System.Collections.Generic;

namespace Invector
{
    [vClassHeader("MoveSet Speed", "Use this to add extra speed into a specific MoveSet")]
    public class vMoveSetSpeed : vMonoBehaviour
    {
        vThirdPersonMotor cc;
        private vMoveSetControlSpeed defaultFree = new vMoveSetControlSpeed();
        private vMoveSetControlSpeed defaultStrafe = new vMoveSetControlSpeed();

        public List<vMoveSetControlSpeed> listFree;
        public List<vMoveSetControlSpeed> listStrafe;

        private int currentMoveSet;

        void Start()
        {
            cc = GetComponent<vThirdPersonMotor>();

            defaultFree.walkSpeed = cc.freeWalkSpeed;
            defaultFree.runningSpeed = cc.freeRunningSpeed;
            defaultFree.sprintSpeed = cc.freeSprintSpeed;

            defaultStrafe.walkSpeed = cc.strafeWalkSpeed;
            defaultStrafe.runningSpeed = cc.strafeRunningSpeed;
            defaultStrafe.sprintSpeed = cc.strafeRunningSpeed;

            StartCoroutine(UpdateMoveSetSpeed());
        }

        IEnumerator UpdateMoveSetSpeed()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                ChangeSpeed();
            }
        }

        void ChangeSpeed()
        {
            currentMoveSet = (int)Mathf.Round(cc.animator.GetFloat("MoveSet_ID"));
            var strafing = cc.isStrafing;
            if (strafing)
            {
                var extraSpeed = listStrafe.Find(l => l.moveset == currentMoveSet);
                if (extraSpeed != null)
                {
                    cc.strafeWalkSpeed = extraSpeed.walkSpeed;
                    cc.strafeRunningSpeed = extraSpeed.runningSpeed;
                    cc.strafeSprintSpeed = extraSpeed.sprintSpeed;
                    cc.strafeCrouchSpeed = extraSpeed.crouchSpeed;
                }
                else
                {
                    cc.strafeWalkSpeed = defaultStrafe.walkSpeed;
                    cc.strafeRunningSpeed = defaultStrafe.runningSpeed;
                    cc.strafeRunningSpeed = defaultStrafe.sprintSpeed;
                    cc.strafeCrouchSpeed = defaultStrafe.crouchSpeed;
                }
            }
            else
            {
                var extraSpeed = listFree.Find(l => l.moveset == currentMoveSet);
                if (extraSpeed != null)
                {
                    cc.freeWalkSpeed = extraSpeed.walkSpeed;
                    cc.freeRunningSpeed = extraSpeed.runningSpeed;
                    cc.freeSprintSpeed = extraSpeed.sprintSpeed;
                    cc.freeCrouchSpeed = extraSpeed.crouchSpeed;
                }
                else
                {
                    cc.freeWalkSpeed = defaultFree.walkSpeed;
                    cc.freeRunningSpeed = defaultFree.runningSpeed;
                    cc.freeSprintSpeed = defaultFree.sprintSpeed;
                    cc.freeCrouchSpeed = defaultFree.crouchSpeed;
                }
            }
        }

        [System.Serializable]
        public class vMoveSetControlSpeed
        {
            public int moveset;
            public float walkSpeed = 1.5f;
            public float runningSpeed = 1.5f;
            public float sprintSpeed = 1.5f;
            public float crouchSpeed = 1.5f;
        }
    }
}

