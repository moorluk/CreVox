using UnityEngine;
using System.Collections;
using Invector.CharacterController;
using Invector.EventSystems;
using System;

public partial class vCollisionMessage : MonoBehaviour,vIDamageReceiver
{
    [HideInInspector]
    public vRagdoll ragdoll;    

    void Start()
    {
        ragdoll = GetComponentInParent<vRagdoll>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            if (ragdoll)
            {
                ragdoll.OnRagdollCollisionEnter(new vRagdollCollision(this.gameObject, collision));
                if (!inAddDamage)
                {
                    float impactforce = collision.relativeVelocity.x + collision.relativeVelocity.y + collision.relativeVelocity.z;
                    if (impactforce > 10 || impactforce < -10)
                    {
                        inAddDamage = true;
                        vDamage damage = new vDamage((int)Mathf.Abs(impactforce) - 10);
                        damage.ignoreDefense = true;
                        damage.sender = collision.transform;
                        damage.hitPosition = collision.contacts[0].point;
                        ragdoll.ApplyDamage(damage);
                        Invoke("ResetAddDamage", 0.1f);
                    }
                }
            }
        }
    }

    bool inAddDamage;   

    void ResetAddDamage()
    {
        inAddDamage = false;
    }

    public void TakeDamage(vDamage damage, bool hitReaction = true)
    {

        if (!ragdoll) return;       
        if (!ragdoll.iChar.isDead)      
        {
            inAddDamage = true;
            ragdoll.ApplyDamage(damage);
            Invoke("ResetAddDamage", 0.1f);
        }
    }
}
