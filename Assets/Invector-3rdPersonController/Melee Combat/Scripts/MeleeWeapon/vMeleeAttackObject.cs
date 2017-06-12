using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Invector.EventSystems;
using Invector;

public class vMeleeAttackObject : MonoBehaviour
{
    public vDamage damage;
    public List<vHitBox> hitBoxes;
    public int damageModifier;
    [HideInInspector]
    public bool canApplyDamage;
    [HideInInspector]
    public OnHitEnter onDamageHit;
    [HideInInspector]
    public OnHitEnter onRecoilHit;
    [HideInInspector]
    public UnityEvent onEnableDamage, onDisableDamage;
    private Dictionary<vHitBox, List<GameObject>> targetColliders;
    [HideInInspector]
    public vMeleeManager meleeManager;

    protected virtual void Start()
    {
        targetColliders = new Dictionary<vHitBox, List<GameObject>>();// init list of targetColliders
        if (hitBoxes.Count > 0)
        {
            /// inicialize the hitBox properties
            foreach (vHitBox hitBox in hitBoxes)
            {
                hitBox.attackObject = this;
                targetColliders.Add(hitBox, new List<GameObject>());
            }
        }
        else
        {
            this.enabled = false;
        }
    }

    /// <summary>
    /// Set Active all hitBoxes of the MeleeAttackObject
    /// </summary>
    /// <param name="value"> active value</param>  
    public virtual void SetActiveDamage(bool value)
    {
        canApplyDamage = value;
        for (int i = 0; i < hitBoxes.Count; i++)
        {
            var hitCollider = hitBoxes[i];
            hitCollider.trigger.enabled = value;
            if (value == false && targetColliders != null)
                targetColliders[hitCollider].Clear();
        }
        if (value)
            onEnableDamage.Invoke();
        else onDisableDamage.Invoke();
    }

    /// <summary>
    /// Call Back of hitboxes
    /// </summary>
    /// <param name="hitBox">vHitBox object</param>
    /// <param name="other">target Collider</param>
    public virtual void OnHit(vHitBox hitBox, Collider other)
    {
        //Check  first contition for hit 
        if (canApplyDamage && !targetColliders[hitBox].Contains(other.gameObject) && (meleeManager != null && other.gameObject != meleeManager.gameObject))
        {
            var inDamage = false;
            var inRecoil = false;
            if (meleeManager == null) meleeManager = GetComponentInParent<vMeleeManager>();
            //check if meleeManager exist and apply  his hitProperties  to this
            HitProperties _hitProperties = meleeManager.hitProperties;
          
            /// Damage Conditions
            if (((hitBox.triggerType & vHitBoxType.Damage) != 0) && _hitProperties.hitDamageTags == null || _hitProperties.hitDamageTags.Count == 0)
                inDamage = true;
            else if (((hitBox.triggerType & vHitBoxType.Damage) != 0) && _hitProperties.hitDamageTags.Contains(other.tag))
                inDamage = true;
            else   ///Recoil Conditions  
            if (((hitBox.triggerType & vHitBoxType.Recoil) != 0) && (_hitProperties.hitRecoilLayer == (_hitProperties.hitRecoilLayer | (1 << other.gameObject.layer))))
                inRecoil = true;
            if (inDamage || inRecoil)
            {
                ///add target collider in list to control frequency of hit him
                targetColliders[hitBox].Add(other.gameObject);
                vHitInfo hitInfo = new vHitInfo(this, hitBox, other, hitBox.transform.position);
                if (inDamage == true)
                {
                    /// If meleeManager 
                    /// call onDamageHit to control damage values
                    /// and  meleemanager will call the ApplyDamage after to filter the damage
                    /// if meleeManager is null
                    /// The damage will be  directly applied
                    /// Finally the OnDamageHit event is called
	                if (meleeManager) 
		                meleeManager.OnDamageHit(hitInfo);
                    else
                    {
                        damage.sender = transform;
                        ApplyDamage(hitBox, other, damage);
                    }
                    onDamageHit.Invoke(hitInfo);
                }
                /// Recoil just work with OnRecoilHit event and meleeManger
                if (inRecoil == true)
                {
                    if (meleeManager) meleeManager.OnRecoilHit(hitInfo);
                    onRecoilHit.Invoke(hitInfo);
                }
            }
        }
    }
	
    /// <summary>
    /// Apply damage to target collider (SendMessage(TakeDamage,damage))
    /// </summary>
    /// <param name="hitBox">vHitBox object</param>
    /// <param name="other">collider target</param>
    /// <param name="damage"> damage</param>
    public void ApplyDamage(vHitBox hitBox, Collider other, vDamage damage)
	{
        vDamage _damage = new vDamage(damage);
        _damage.damageValue = (int)Mathf.RoundToInt(((float)(damage.damageValue + damageModifier) * (((float)hitBox.damagePercentage) * 0.01f)));
        //_damage.sender = transform;
        _damage.hitPosition = hitBox.transform.position;
        if (other.gameObject.IsAMeleeFighter())
        {
            other.gameObject.GetMeleeFighter().OnReceiveAttack(_damage, meleeManager.fighter);
        }
        else if (other.gameObject.CanReceiveDamage())
            other.gameObject.ApplyDamage(_damage);
    }
}

#region Secundary Class
[System.Serializable]
public class OnHitEnter : UnityEvent<vHitInfo> { }

public class vHitInfo
{
    public vMeleeAttackObject attackObject;
    public vHitBox hitBox;
    public Vector3 hitPoint;
    public Collider targetCollider;
    public vHitInfo(vMeleeAttackObject attackObject, vHitBox hitBox, Collider targetCollider, Vector3 hitPoint)
    {
        this.attackObject = attackObject;
        this.hitBox = hitBox;
        this.targetCollider = targetCollider;
        this.hitPoint = hitPoint;
    }
}

[System.Serializable]
public class HitProperties
{
    [Tooltip("Tag to receiver Damage")]
    public List<string> hitDamageTags = new List<string>() { "Enemy" };
    [Tooltip("Trigger a HitRecoil animation if the character attacks a obstacle")]
    public bool useRecoil = true;
    public bool drawRecoilGizmos;
    [Range(0, 180f)]
    public float recoilRange = 90f;
    [Tooltip("layer to Recoil Damage")]
    public LayerMask hitRecoilLayer = 1 << 0;
}
#endregion
