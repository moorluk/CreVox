using UnityEngine;
using System.Collections;
using Invector.CharacterController;
using UnityEngine.Events;

[vClassHeader("vCollectableStandalone")]
public class vCollectableStandalone : vTriggerGenericAction
{
    public string targetEquipPoint;
    public GameObject weapon;
    public Sprite weaponIcon;
    public string weaponText;
    public UnityEvent OnEquip;
    public UnityEvent OnDrop;
  
    private vCollectMeleeControl manager;   

    public override IEnumerator OnDoActionDelay(GameObject cc)
    {
        yield return StartCoroutine(base.OnDoActionDelay(cc));

        manager = cc.GetComponent<vCollectMeleeControl>();

        if (manager != null)
        {
            manager.HandleCollectableInput(this);
        }
    }
}