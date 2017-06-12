using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Invector;
using Invector.CharacterController;
using Invector.ItemManager;

[vClassHeader("CollectMeleeControl")]
public class vCollectMeleeControl : vMonoBehaviour
{
    [HideInInspector]
    public vMeleeManager meleeManager;
    [Header("Handlers")]
    public vHandler rightHandler = new vHandler();
    public vHandler leftHandler = new vHandler();
    [Header("Unequip Inputs")]
    public GenericInput unequipRightInput;
    public GenericInput unequipLeftInput;
    [HideInInspector]
    public GameObject leftWeapon,rightWeapon;
    public vControlDisplayWeaponStandalone controlDisplayPrefab;
    protected vControlDisplayWeaponStandalone currentDisplay;

    protected virtual void Start()
    {
        meleeManager = GetComponent<vMeleeManager>();       
        if (controlDisplayPrefab)
          currentDisplay = Instantiate(controlDisplayPrefab) as vControlDisplayWeaponStandalone;        
    }
    
    protected virtual void Update()
    {
        UnequipWeaponHandle();
    }

    public virtual void HandleCollectableInput(vCollectableStandalone collectableStandAlone)
    {
        if (!meleeManager) return;
        if (collectableStandAlone != null && collectableStandAlone.weapon != null )
        {
            var weapon = collectableStandAlone.weapon.GetComponent<vMeleeWeapon>();
            if (!weapon) return;
            if (weapon.meleeType != vMeleeType.OnlyDefense)
            {
                var p = GetEquipPoint(rightHandler, collectableStandAlone.targetEquipPoint);
                if (!p) return;
                collectableStandAlone.weapon.transform.SetParent(p);
                collectableStandAlone.weapon.transform.localPosition = Vector3.zero;
                collectableStandAlone.weapon.transform.localEulerAngles = Vector3.zero;
                if (rightWeapon && rightWeapon!=weapon.gameObject) 
                    RemoveRightWeapon();
               
                meleeManager.SetRightWeapon(weapon);
                collectableStandAlone.OnEquip.Invoke();
                rightWeapon = weapon.gameObject;
                UpdateRightDisplay(collectableStandAlone);
            }
            if (weapon.meleeType != vMeleeType.OnlyAttack && weapon.meleeType != vMeleeType.AttackAndDefense)
            {
                var p = GetEquipPoint(leftHandler, collectableStandAlone.targetEquipPoint);
                if (!p) return;
                collectableStandAlone.weapon.transform.SetParent(p);
                collectableStandAlone.weapon.transform.localPosition = Vector3.zero;
                collectableStandAlone.weapon.transform.localEulerAngles = Vector3.zero;
                if (leftWeapon && leftWeapon!=weapon.gameObject)
                    RemoveLeftWeapon();
               
                meleeManager.SetLeftWeapon(weapon);
                collectableStandAlone.OnEquip.Invoke();
                leftWeapon = weapon.gameObject;
                UpdateLeftDisplay(collectableStandAlone);                
            }
        }
    }

    protected virtual Transform GetEquipPoint(vHandler point, string name)
    {
        Transform p = point.defaultHandler;
        var customP = point.customHandlers.Find(_p => _p.name.Equals(name));
        if (customP) p = customP;
        return p;
    }

    protected virtual void UnequipWeaponHandle()
    {
        if (rightWeapon)        
            if (unequipRightInput.GetButtonDown())            
                RemoveRightWeapon();            
        
        if (leftWeapon)        
            if (unequipLeftInput.GetButtonDown())            
                RemoveLeftWeapon();
    }

    protected virtual void RemoveLeftWeapon()
    {
        if (leftWeapon)
        {
            leftWeapon.transform.parent = null;
            var _collectable = leftWeapon.GetComponentInChildren<vCollectableStandalone>();
            if (_collectable) _collectable.OnDrop.Invoke();
        }       
        if(meleeManager)
            meleeManager.leftWeapon = null;
        UpdateLeftDisplay();
    }

    protected virtual void RemoveRightWeapon()
    {
        if(rightWeapon)
        {
            rightWeapon.transform.parent = null;
            var _collectable = rightWeapon.GetComponentInChildren<vCollectableStandalone>();
            if (_collectable) _collectable.OnDrop.Invoke();
        }       
        if(meleeManager)
            meleeManager.rightWeapon = null;
        UpdateRightDisplay();
    }

    protected virtual void UpdateLeftDisplay(vCollectableStandalone collectable = null)
    {
        if (!currentDisplay) return;
        if (collectable)
        {
            currentDisplay.SetLeftWeaponIcon(collectable.weaponIcon);
            currentDisplay.SetLeftWeaponText(collectable.weaponText);
        }
        else
        {
            currentDisplay.RemoveLeftWeaponIcon();
            currentDisplay.RemoveLeftWeaponText();
        }           

    }
    protected virtual void UpdateRightDisplay(vCollectableStandalone collectable = null)
    {
        if (!currentDisplay) return;
        if (collectable)
        {
            currentDisplay.SetRightWeaponIcon(collectable.weaponIcon);
            currentDisplay.SetRightWeaponText(collectable.weaponText);
        }
        else
        {
            currentDisplay.RemoveRightWeaponIcon();
            currentDisplay.RemoveRightWeaponText();
        }
    }
}
