using UnityEngine;
using System.Collections;
using Invector;
using Invector.CharacterController;
public class vCharacterStandalone : vCharacter
{
    /// <summary>
    /// 
    /// vCharacter Example - You can assign this script into non-Invector Third Person Characters to still use the AI and apply damage
    /// 
    /// </summary>
    
    [HideInInspector] public v_SpriteHealth healthSlider;

    void Start ()
    {
        // health info
        isDead = false;
        currentHealth = maxHealth;
        currentHealthRecoveryDelay = healthRecoveryDelay;
        currentStamina = maxStamina;
        // health slider hud - prefab located into Prefabs/AI/enemyHealthUI
        healthSlider = GetComponentInChildren<v_SpriteHealth>();
    }
	

    /// <summary>
    /// APPLY DAMAGE - call this method by a SendMessage with the damage value
    /// </summary>
    /// <param name="damage"> damage to apply </param>
    public override void TakeDamage(vDamage damage,bool hitReaction)
    {       
        // don't apply damage if the character is rolling, you can add more conditions here
        if (isDead)
            return;

        // instantiate the hitDamage particle - check if your character has a HitDamageParticle component
        var hitrotation = Quaternion.LookRotation(new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z) - damage.hitPosition);
        SendMessage("TriggerHitParticle", new vHittEffectInfo(new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z), hitrotation, damage.attackName), SendMessageOptions.DontRequireReceiver);

        // reduce the current health by the damage amount.
        currentHealth -= damage.damageValue;
        currentHealthRecoveryDelay = healthRecoveryDelay;

        // update the HUD display
        if (healthSlider != null) healthSlider.Damage(damage.damageValue);

        // trigger the hit sound 
        if(damage.sender!=null)
        damage.sender.SendMessage("PlayHitSound", SendMessageOptions.DontRequireReceiver);

        // apply vibration on the gamepad             
        transform.SendMessage("GamepadVibration", 0.25f, SendMessageOptions.DontRequireReceiver);
        onReceiveDamage.Invoke(damage);
    }   
}
