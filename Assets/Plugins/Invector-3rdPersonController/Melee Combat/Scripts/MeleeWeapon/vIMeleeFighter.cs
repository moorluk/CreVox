using UnityEngine;
using System.Collections;
using Invector.CharacterController;
namespace Invector.EventSystems
{
    public interface vIMeleeFighter
    {
        void OnEnableAttack();

        void OnDisableAttack();

        void ResetAttackTriggers();

        void BreakAttack(int breakAtkID);

        void OnRecoil(int recoilID);

        void OnReceiveAttack(vDamage damage,vIMeleeFighter attacker);

        vCharacter Character();        
    }

    public static class vIMeeleFighterHelper
    {

        /// <summary>
        /// check if gameObject has a <see cref="vIMeleeFighter"/> Component
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns>return true if gameObject contains a <see cref="vIMeleeFighter"/></returns>
        public static bool IsAMeleeFighter(this GameObject receiver)
        {
            return receiver.GetComponent<vIMeleeFighter>() != null;
        }

        /// <summary>
        /// Get <see cref="vIMeleeFighter"/> of gameObject
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns>the <see cref="vIMeleeFighter"/> component</returns>
        public static vIMeleeFighter GetMeleeFighter(this GameObject receiver)
        {
            return receiver.GetComponent<vIMeleeFighter>();
        }
    }
}
