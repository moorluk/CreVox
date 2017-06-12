using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Invector;
using Invector.EventSystems;

public class vMeleeManager : vMonoBehaviour
{
    #region SeralizedProperties in CustomEditor
    public List<vBodyMember> Members = new List<vBodyMember>();
    public vDamage defaultDamage = new vDamage(10);
    public HitProperties hitProperties;
    public vMeleeWeapon leftWeapon, rightWeapon;
    public vOnHitEvent onDamageHit, onRecoilHit;
    #endregion
    
    [Tooltip("NPC ONLY- Ideal distance for the attack")]
    public float defaultAttackDistance = 1f;
    [Tooltip("Default cost for stamina when attack")]
    public float defaultStaminaCost = 20f;
    [Tooltip("Default recovery delay for stamina when attack")]
    public float defaultStaminaRecoveryDelay = 1f;
    [Range(0, 100)]
    public int defaultDefenseRate = 50;
    [Range(0, 180)]
    public float defaultDefenseRange = 90;

    [HideInInspector]
    public vIMeleeFighter fighter;
    private int damageMultiplier;
    private int currentRecoilID;
    private int currentReactionID;
    private bool ignoreDefense;
    private bool activeRagdoll;
    private bool inRecoil;
    private string attackName;

    protected virtual void Start()
    {
        Init();
    }

    /// <summary>
    /// Init properties
    /// </summary>
    protected virtual void Init()
    {
        fighter = gameObject.GetMeleeFighter();
        ///Initialize all bodyMembers and weapons
        foreach (vBodyMember member in Members)
        {
            ///damage of member will be always the defaultDamage
            //member.attackObject.damage = defaultDamage;
            member.attackObject.damage.damageValue = defaultDamage.damageValue;
            if (member.bodyPart == HumanBodyBones.LeftLowerArm.ToString())
            {
                var weapon = member.attackObject.GetComponentInChildren<vMeleeWeapon>();
                leftWeapon = weapon;
            }
            if (member.bodyPart == HumanBodyBones.RightLowerArm.ToString())
            {
                var weapon = member.attackObject.GetComponentInChildren<vMeleeWeapon>();
                rightWeapon = weapon;
            }
            member.attackObject.meleeManager = this;
            member.SetActiveDamage(false);
        }

        if (leftWeapon != null)
        {
            leftWeapon.SetActiveDamage(false);
            leftWeapon.meleeManager = this;
        }
        if (rightWeapon != null)
        {
            rightWeapon.meleeManager = this;
            rightWeapon.SetActiveDamage(false);
        }
    }

    /// <summary>
    /// Set active Multiple Parts to attack
    /// </summary>
    /// <param name="bodyParts"></param>
    /// <param name="type"></param>
    /// <param name="active"></param>
    /// <param name="damageMultiplier"></param>
    public virtual void SetActiveAttack(List<string> bodyParts, vAttackType type, bool active = true, int damageMultiplier = 0, int recoilID = 0, int reactionID = 0, bool ignoreDefense = false, bool activeRagdoll = false, string attackName = "")
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            var bodyPart = bodyParts[i];
            SetActiveAttack(bodyPart, type, active, damageMultiplier, recoilID, reactionID, ignoreDefense, activeRagdoll, attackName);
        }
    }

    /// <summary>
    /// Set active Single Part to attack
    /// </summary>
    /// <param name="bodyPart"></param>
    /// <param name="type"></param>
    /// <param name="active"></param>
    /// <param name="damageMultiplier"></param>
    public virtual void SetActiveAttack(string bodyPart, vAttackType type, bool active = true, int damageMultiplier = 0, int recoilID = 0, int reactionID = 0, bool ignoreDefense = false, bool activeRagdoll = false, string attackName = "")
    {
        this.damageMultiplier = damageMultiplier;
        currentRecoilID = recoilID;
        currentReactionID = reactionID;
        this.ignoreDefense = ignoreDefense;
        this.activeRagdoll = activeRagdoll;
        this.attackName = attackName;

        if (type == vAttackType.Unarmed)
        {
            /// find attackObject by bodyPart
            var attackObject = Members.Find(member => member.bodyPart == bodyPart);
            if (attackObject != null)
            {
                attackObject.SetActiveDamage(active);
            }
        }
        else
        {   ///if AttackType == MeleeWeapon
            ///this work just for Right and Left Lower Arm         
            if (bodyPart == "RightLowerArm" && rightWeapon != null)
            {
                rightWeapon.meleeManager = this;
                rightWeapon.SetActiveDamage(active);
            }
            else if (bodyPart == "LeftLowerArm" && leftWeapon != null)
            {
                leftWeapon.meleeManager = this;
                leftWeapon.SetActiveDamage(active);
            }
        }
    }

    /// <summary>
    /// Listener of Damage Event
    /// </summary>
    /// <param name="hitInfo"></param>
    public virtual void OnDamageHit(vHitInfo hitInfo)
    {
        vDamage damage = new vDamage(hitInfo.attackObject.damage);
        damage.sender = transform;
        damage.reaction_id = currentReactionID;
        damage.recoil_id = currentRecoilID;
        if (this.activeRagdoll) damage.activeRagdoll = this.activeRagdoll;
        if (this.attackName != string.Empty) damage.attackName = this.attackName;
        if (this.ignoreDefense) damage.ignoreDefense = this.ignoreDefense;
        /// Calc damage with multiplier 
        /// and Call ApplyDamage of attackObject 
        damage.damageValue *= damageMultiplier > 1 ? damageMultiplier : 1;
        hitInfo.attackObject.ApplyDamage(hitInfo.hitBox, hitInfo.targetCollider, damage);
        onDamageHit.Invoke(hitInfo);
    }

    /// <summary>
    /// Listener of Recoil Event
    /// </summary>
    /// <param name="hitInfo"></param>
    public virtual void OnRecoilHit(vHitInfo hitInfo)
    {
        if (hitProperties.useRecoil && InRecoilRange(hitInfo) && !inRecoil)
        {
            inRecoil = true;
            var id = currentRecoilID;
            fighter.OnRecoil(id);
            onRecoilHit.Invoke(hitInfo);
            Invoke("ResetRecoil", 1f);
        }
    }

    /// <summary>
    /// Call Weapon Defense Events.
    /// </summary>
    public virtual void OnDefense()
    {
        if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            leftWeapon.OnDefense();
        }
        if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            rightWeapon.OnDefense();
        }
    }

    /// <summary>
    /// Get Current Attack ID
    /// </summary>
    /// <returns></returns>
    public virtual int GetAttackID()
    {
        if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyDefense && rightWeapon.gameObject.activeSelf) return rightWeapon.attackID;
        return 0;
    }

    /// <summary>
    /// Get StaminaCost
    /// </summary>
    /// <returns></returns>
    public virtual float GetAttackStaminaCost()
    {
        if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyDefense && rightWeapon.gameObject.activeSelf) return rightWeapon.staminaCost;
        return defaultStaminaCost;
    }

    /// <summary>
    /// Get StaminaCost
    /// </summary>
    /// <returns></returns>
    public virtual float GetAttackStaminaRecoveryDelay()
    {
        if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyDefense && rightWeapon.gameObject.activeSelf) return rightWeapon.staminaRecoveryDelay;
        return defaultStaminaRecoveryDelay;
    }

    /// <summary>
    /// Get ideal distance for the attack
    /// </summary>
    /// <returns></returns>
    public virtual float GetAttackDistance()
    {
        if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyDefense && rightWeapon.gameObject.activeSelf) return rightWeapon.distanceToAttack;
        return defaultAttackDistance;
    }

    /// <summary>
    /// Get Current Defense ID
    /// </summary>
    /// <returns></returns>
    public virtual int GetDefenseID()
    {
        if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            GetComponent<Animator>().SetBool("FlipAnimation", false);
            return leftWeapon.defenseID;
        }
        if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            GetComponent<Animator>().SetBool("FlipAnimation", true);
            return rightWeapon.defenseID;
        }
        return 0;
    }

    /// <summary>
    /// Get Defense Rate of Melee Defense 
    /// </summary>
    /// <returns></returns>
    public int GetDefenseRate()
    {
        if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            return leftWeapon.defenseRate;
        }
        if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            return rightWeapon.defenseRate;
        }
        return defaultDefenseRate;
    }

    /// <summary>
    /// Get Current MoveSet ID
    /// </summary>
    /// <returns></returns>
    public virtual int GetMoveSetID()
    {
        if (rightWeapon != null && rightWeapon.gameObject.activeSelf) return rightWeapon.movesetID;
        // if (leftWeapon != null && leftWeapon.gameObject.activeSelf) return leftWeapon.MovesetID;
        return 0;
    }

    /// <summary>
    /// Check if defence can break Attack
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBreakAttack()
    {
        if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            return leftWeapon.breakAttack;
        }
        if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            return rightWeapon.breakAttack;
        }
        return false;
    }

    /// <summary>
    /// Check if attack can be blocked
    /// </summary>
    /// <param name="attackPoint"></param>
    /// <returns></returns>
    public virtual bool CanBlockAttack(Vector3 attackPoint)
    {
        if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            return Math.Abs(transform.HitAngle(attackPoint)) <= leftWeapon.defenseRange;
        }
        if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            return Math.Abs(transform.HitAngle(attackPoint)) <= rightWeapon.defenseRange;
        }
        return Math.Abs(transform.HitAngle(attackPoint)) <= defaultDefenseRange;
    }

    /// <summary>
    /// Get defense recoilID for break attack
    /// </summary>
    /// <returns></returns>
    public virtual int GetDefenseRecoilID()
    {
        if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            return leftWeapon.recoilID;
        }
        if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            return rightWeapon.recoilID;
        }
        return 0;
    }

    /// <summary>
    /// Check if angle of hit point is in range of recoil
    /// </summary>
    /// <param name="hitInfo"></param>
    /// <returns></returns>
    protected virtual bool InRecoilRange(vHitInfo hitInfo)
    {
        var center = new Vector3(transform.position.x, hitInfo.hitPoint.y, transform.position.z);
        var euler = (Quaternion.LookRotation(hitInfo.hitPoint - center).eulerAngles - transform.eulerAngles).NormalizeAngle();

        return euler.y <= hitProperties.recoilRange;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weaponObject"></param>
    public void SetLeftWeapon(GameObject weaponObject)
    {
        if (weaponObject)
        {
            leftWeapon = weaponObject.GetComponent<vMeleeWeapon>();
            if(leftWeapon)
            {
                leftWeapon.SetActiveDamage(false);
                leftWeapon.meleeManager = this;
            }            
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weaponObject"></param>
    public void SetRightWeapon(GameObject weaponObject)
    {
        if (weaponObject)
        {            
            rightWeapon = weaponObject.GetComponent<vMeleeWeapon>();
            if (rightWeapon)
            {
                rightWeapon.meleeManager = this;
                rightWeapon.SetActiveDamage(false);
            }                
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weaponObject"></param>
    public void SetLeftWeapon(vMeleeWeapon weapon)
    {
        if (weapon)
        {
            leftWeapon = weapon;
            leftWeapon.SetActiveDamage(false);
            leftWeapon.meleeManager = this;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weaponObject"></param>
    public void SetRightWeapon(vMeleeWeapon weapon)
    {
        if (weapon)
        {
            rightWeapon = weapon;
            rightWeapon.meleeManager = this;
            rightWeapon.SetActiveDamage(false);
        }
    }

    void ResetRecoil()
    {
        inRecoil = false;
    }
}

#region Secundary Classes
[Serializable]
public class vOnHitEvent : UnityEngine.Events.UnityEvent<vHitInfo> { }

[Serializable]
public class vBodyMember
{
    public Transform transform;
    public string bodyPart;

    public vMeleeAttackObject attackObject;
    public bool isHuman;
    public void SetActiveDamage(bool active)
    {
        attackObject.SetActiveDamage(active);
    }
}

public enum vHumanBones
{
    RightHand, RightLowerArm, RightUpperArm, RightShoulder,
    LeftHand, LeftLowerArm, LetfUpperArm, LeftShoulder,
    RightFoot, RightLowerLeg, RightUpperLeg,
    LeftFoot, LeftLowerLeg, LeftUpperLeg,
    Chest,
    Head
}

public enum vAttackType
{
    Unarmed, MeleeWeapon
}
#endregion