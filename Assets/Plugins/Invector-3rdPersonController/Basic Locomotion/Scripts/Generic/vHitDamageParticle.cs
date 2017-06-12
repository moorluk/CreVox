using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Invector
{
    [vClassHeader("HitDamage Particle", "Default hit Particle to instantiate every time you receive damage and Custom hit Particle to instantiate based on a custom AttackName from a Attack Animation State")]
    public class vHitDamageParticle : vMonoBehaviour
    {
        public GameObject defaultHitEffect;
        public List<vHitEffect> customHitEffects = new List<vHitEffect>();

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            var character = GetComponent<CharacterController.vCharacter>();
            if (character != null)
            {
                character.onReceiveDamage.AddListener(OnReceiveDamage);
            }
        }

        public void OnReceiveDamage(vDamage damage)
        {
            // instantiate the hitDamage particle - check if your character has a HitDamageParticle component
            var damageDirection = new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z) - damage.hitPosition;
            var hitrotation = damageDirection != Vector3.zero ? Quaternion.LookRotation(damageDirection) : transform.rotation;

            if (damage.damageValue > 0)
                TriggerHitParticle(new vHittEffectInfo(new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z), hitrotation, damage.attackName, damage.receiver));
        }

        /// <summary>
        /// Raises the hit event.
        /// </summary>
        /// <param name="hitEffectInfo">Hit effect info.</param>
        void TriggerHitParticle(vHittEffectInfo hitEffectInfo)
        {
            var hitEffect = customHitEffects.Find(effect => effect.hitName.Equals(hitEffectInfo.hitName));

            if (hitEffect != null)
            {
                if (hitEffect.hitPrefab != null)
                {
                    var prefab = Instantiate(hitEffect.hitPrefab, hitEffectInfo.position, hitEffect.rotateToHitDirection ? hitEffectInfo.rotation : hitEffect.hitPrefab.transform.rotation) as GameObject;
                    if (hitEffect.attachInReceiver && hitEffectInfo.receiver)
                        prefab.transform.SetParent(hitEffectInfo.receiver);
                }
            }
            else if (defaultHitEffect != null)
                Instantiate(defaultHitEffect, hitEffectInfo.position, hitEffectInfo.rotation);
        }

    }

    public class vHittEffectInfo
    {
        public Transform receiver;
        public Vector3 position;
        public Quaternion rotation;
        public string hitName;
        public vHittEffectInfo(Vector3 position, Quaternion rotation, string hitName = "", Transform receiver = null)
        {
            this.receiver = receiver;
            this.position = position;
            this.rotation = rotation;
            this.hitName = hitName;
        }
    }

    [System.Serializable]
    public class vHitEffect
    {
        public string hitName = "";
        public GameObject hitPrefab;
        public bool rotateToHitDirection = true;
        [Tooltip("Attach prefab in Damage Receiver transform")]
        public bool attachInReceiver = false;
    }
}
