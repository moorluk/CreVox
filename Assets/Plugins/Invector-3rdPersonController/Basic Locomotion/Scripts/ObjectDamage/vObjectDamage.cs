using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Invector.EventSystems;

public class vObjectDamage : MonoBehaviour
{
    public vDamage damage;
    [Tooltip("List of tags that can be hit")]
    public List<string> tags;
    [HideInInspector]
    public bool useCollision;
    [HideInInspector]
    [Tooltip("Check to use the damage Frequence")]
    public bool continuousDamage;
    [HideInInspector]
    [Tooltip("Apply damage to each end of the frequency in seconds ")]
    public float damageFrequency = 0.5f;
    private List<Collider> targets;
    private List<Collider> disabledTarget;
    private float currentTime;

    protected virtual void Start()
    {
        targets = new List<Collider>();
        disabledTarget = new List<Collider>();
    }

    protected virtual void Update()
    {
        if (continuousDamage && targets != null && targets.Count > 0)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
            }
            else
            {
                currentTime = damageFrequency;
                foreach (Collider collider in targets)
                    if (collider != null)
                    {
                        if (collider.enabled)
                            ApplyDamage(collider.transform, transform.position);// apply damage to enabled collider
                        else
                            disabledTarget.Add(collider);// add disabled collider to list of disabled
                    }
                //remove all disabled colliders of target list
                if(disabledTarget.Count > 0)
                {
                    for (int i = disabledTarget.Count; i >= 0; i--)
                    {
                        if (disabledTarget.Count == 0) break;
                        try
                        {
                            if (targets.Contains(disabledTarget[i]))
                                targets.Remove(disabledTarget[i]);
                        }
                        catch
                        {
                            break;
                        }                        
                    }
                }
                
                if (disabledTarget.Count > 0) disabledTarget.Clear();
            }
        }
    }

    protected virtual void OnCollisionEnter(Collision hit)
    {
        if (!useCollision || continuousDamage) return;
        
        if (tags.Contains(hit.gameObject.tag))
            ApplyDamage(hit.transform, hit.contacts[0].point);
    }

    protected virtual void OnTriggerEnter(Collider hit)
	{
        if (useCollision) return;       
        if (continuousDamage && tags.Contains(hit.transform.tag) && !targets.Contains(hit))
        {
            targets.Add(hit);
        }

        else if (tags.Contains(hit.gameObject.tag))
            ApplyDamage(hit.transform, transform.position);
    }

    protected virtual void OnTriggerExit(Collider hit)
    {
        if (useCollision && !continuousDamage) return;

        if (tags.Contains(hit.gameObject.tag) && targets.Contains(hit))
            targets.Remove(hit);
    }

    protected virtual void ApplyDamage(Transform target, Vector3 hitPoint)
    {
        damage.sender = transform;
        damage.hitPosition = hitPoint;
        target.gameObject.ApplyDamage(damage);
    }
}


[System.Serializable]
public class vDamage
{
    [Tooltip("Apply damage to the Character Health")]
    public int damageValue = 15;
    [Tooltip("How much stamina the target will lost when blocking this attack")]
    public float staminaBlockCost = 5;
    [Tooltip("How much time the stamina of the target will wait to recovery")]
    public float staminaRecoveryDelay = 1;
    [Tooltip("Apply damage even if the Character is blocking")]
    public bool ignoreDefense;
    [Tooltip("Activated Ragdoll when hit the Character")]
    public bool activeRagdoll;
    [HideInInspector]
    public Transform sender;
    [HideInInspector]
    public Transform receiver;
    [HideInInspector]
    public Vector3 hitPosition;
    [HideInInspector]
    public int recoil_id = 0;
    [HideInInspector]
    public int reaction_id = 0;
    public string attackName;

    public vDamage(int value)
    {
        this.damageValue = value;
    }

    public vDamage(vDamage damage)
    {
        this.damageValue = damage.damageValue;
        this.staminaBlockCost = damage.staminaBlockCost;
        this.staminaRecoveryDelay = damage.staminaRecoveryDelay;
        this.ignoreDefense = damage.ignoreDefense;
        this.activeRagdoll = damage.activeRagdoll;
        this.sender = damage.sender;
        this.receiver = damage.receiver;
        this.recoil_id = damage.recoil_id;
        this.reaction_id = damage.reaction_id;
        this.attackName = damage.attackName;
        this.hitPosition = damage.hitPosition;
    }
    /// <summary>
    /// Calc damage Resuction percentage
    /// </summary>
    /// <param name="damageReduction"></param>
    public void ReduceDamage( float damageReduction)
    {
        int result = (int)(this.damageValue - ((this.damageValue * damageReduction) / 100));
        this.damageValue = result;
    }
}