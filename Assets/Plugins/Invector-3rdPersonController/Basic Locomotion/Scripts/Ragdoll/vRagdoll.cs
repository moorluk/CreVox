using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Invector.CharacterController
{
    [vClassHeader("Ragdoll System", true, "icon_v2", true, "Every gameobject children of the character must have their tag added in the IgnoreTag List.")]
    public class vRagdoll : vMonoBehaviour
    {
        //Declare a class that will hold useful information for each body part
      
        #region public variables
        public bool removePhysicsAfterDie;
        [Tooltip("Press the B key to enable the ragdoll")]
        public bool debug;
        [Tooltip("SHOOTER: Keep false to use body detection or true if you're making a MELEE only")]
        public bool disableColliders = false;
        public AudioSource collisionSource;
        public AudioClip collisionClip;        

        [Header("Add Tags for Weapons or Itens here:")]
        public List<string> ignoreTags = new List<string>() { "Weapon", "Ignore Ragdoll" };

        public AnimatorStateInfo stateInfo;
        #endregion

        #region private variables
        [HideInInspector]
        public vCharacter iChar;
        Animator animator;
        [HideInInspector] public Transform characterChest, characterHips;
      
        bool inStabilize, isActive,updateBeharivour;

        bool ragdolled
        {
            get
            {
                return state != RagdollState.animated;
            }
            set
            {
                if (value == true)
                {
                    if (state == RagdollState.animated)
                    {
                        //Transition from animated to ragdolled
                        setKinematic(false); //allow the ragdoll RigidBodies to react to the environment
                        setCollider(false);
                        animator.enabled = false; //disable animation
                        state = RagdollState.ragdolled;
                    }
                }
                else
                {
                    characterHips.parent = hipsParent;
                    if (state == RagdollState.ragdolled)
                    {
                        //Transition from ragdolled to animated through the blendToAnim state
                        
                        setKinematic(true); //disable gravity etc.
                        setCollider(true);
                        ragdollingEndTime = Time.time; //store the state change time
                       
                        animator.enabled = true; //enable animation
                        state = RagdollState.blendToAnim;

                        //Store the ragdolled position for blending
                        foreach (BodyPart b in bodyParts)
                        {
                            b.storedRotation = b.transform.rotation;
                            b.storedPosition = b.transform.position;
                        }

                        //Remember some key positions
                        ragdolledFeetPosition = 0.5f * (animator.GetBoneTransform(HumanBodyBones.LeftToes).position + animator.GetBoneTransform(HumanBodyBones.RightToes).position);
                        ragdolledHeadPosition = animator.GetBoneTransform(HumanBodyBones.Head).position;
                        ragdolledHipPosition = animator.GetBoneTransform(HumanBodyBones.Hips).position;

                        //Initiate the get up animation
                        //hip hips forward vector pointing upwards, initiate the get up from back animation
                        if (animator.GetBoneTransform(HumanBodyBones.Hips).forward.y > 0)
                            animator.Play("StandUp@FromBack");
                        else
                            animator.Play("StandUp@FromBelly");
                    }
                }
            }
        }

        //Possible states of the ragdoll
        enum RagdollState
        {
            animated,    //Mecanim is fully in control
            ragdolled,   //Mecanim turned off, physics controls the ragdoll
            blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
        }

        //The current state
        RagdollState state = RagdollState.animated;
        //How long do we blend when transitioning from ragdolled to animated
        float ragdollToMecanimBlendTime = 0.5f;
        float mecanimToGetUpTransitionTime = 0.05f;
        //A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
        float ragdollingEndTime = -100;
        //Additional vectores for storing the pose the ragdoll ended up in.
        Vector3 ragdolledHipPosition, ragdolledHeadPosition, ragdolledFeetPosition;
        //Declare a list of body parts, initialized in Start()
        List<BodyPart> bodyParts = new List<BodyPart>();
        // usage to reset parent of hips
        Transform hipsParent;
        //usage to controll damage frequency
        bool inApplyDamage;

        class BodyPart
        {
            public Transform transform;
            public Vector3 storedPosition;
            public Quaternion storedRotation;
        }
        #endregion

        void Start()
        {
            // store the Animator component
            animator = GetComponent<Animator>();
            iChar = GetComponent<vCharacter>();
            if (iChar)
                iChar.onActiveRagdoll.AddListener(ActivateRagdoll);

            // find character chest and hips
            characterChest = animator.GetBoneTransform(HumanBodyBones.Chest);
            characterHips = animator.GetBoneTransform(HumanBodyBones.Hips);
            hipsParent = characterHips.parent;
            // set all RigidBodies to kinematic so that they can be controlled with Mecanim
            // and there will be no glitches when transitioning to a ragdoll
            setKinematic(true);
            setCollider(true);

            // find all the transforms in the character, assuming that this script is attached to the root
            Component[] components = GetComponentsInChildren(typeof(Transform));

            // for each of the transforms, create a BodyPart instance and store the transform 
            foreach (Component c in components)
            {
                if (!ignoreTags.Contains(c.tag))
                {
                    BodyPart bodyPart = new BodyPart();
                    bodyPart.transform = c as Transform;
                    if (c.GetComponent<Rigidbody>() != null)
                        c.tag = gameObject.tag;
                    bodyParts.Add(bodyPart);
                }
            }
        }

        void LateUpdate()
        {
            if (animator == null) return;
            if (!updateBeharivour && animator.updateMode == AnimatorUpdateMode.AnimatePhysics) return;
            updateBeharivour = false;
            RagdollBehaviour();            
            if (debug && Input.GetKeyDown(KeyCode.B)) ActivateRagdoll();    // debug purposes
            
        }
       
        void FixedUpdate()
        {
            updateBeharivour = true;
            if (iChar.currentHealth > 0)
            {
                if (inStabilize) StartCoroutine(RagdollStabilizer(2f));
                if (animator != null && !animator.isActiveAndEnabled && ragdolled || (animator == null && ragdolled)) transform.position = characterHips.position;
            }
        }
        /// <summary>
        /// Reset the inApplyDamage variable. Set to false;
        /// </summary>
        void ResetDamage()
        {
            inApplyDamage = false;
        }

        /// <summary>
        /// Add Damage to vCharacter every 0.1 seconds
        /// </summary>
        /// <param name="damage"></param>
        public void ApplyDamage(vDamage damage)
        {
            if (isActive && ragdolled && !inApplyDamage && iChar)
            {
                inApplyDamage = true;
                iChar.TakeDamage(damage);
                Invoke("ResetDamage", 0.2f);
            }
        }

        // active ragdoll - call this method to turn the ragdoll on      
        public void ActivateRagdoll()
        {
            if (isActive)
	            return;
	        
            inApplyDamage = true;
	        isActive = true;
	        
	        if (transform.parent != null) transform.parent = null;            

            iChar.EnableRagdoll();

            var isDead = !(iChar.currentHealth > 0);
            if (isDead)
            {
                Destroy(animator);
            }
            // turn ragdoll on
            ragdolled = true;

            // start to check if the ragdoll is stable
	        inStabilize = true;
	        
	        if(!isDead)
                characterHips.parent = null;
            Invoke("ResetDamage", 0.2f);
        }

        // ragdoll collision sound        
        public void OnRagdollCollisionEnter(vRagdollCollision ragdolCollision)
        {
            if (ragdolCollision.ImpactForce > 1)
            {
                collisionSource.clip = collisionClip;
                collisionSource.volume = ragdolCollision.ImpactForce * 0.05f;
                if (!collisionSource.isPlaying)
                    collisionSource.Play();
            }
        }

        // ragdoll stabilizer - wait until the ragdoll became stable based on the chest velocity.magnitude
        IEnumerator RagdollStabilizer(float delay)
        {
            inStabilize = false;
            float rdStabilize = Mathf.Infinity;
            yield return new WaitForSeconds(delay);
            var isDead = !(iChar.currentHealth > 0);

            while (rdStabilize > (isDead ? 0.0001f : 0.1f))
            {
                if (animator != null && !animator.isActiveAndEnabled)
                {
                    rdStabilize = characterChest.GetComponent<Rigidbody>().velocity.magnitude;
                   
                }
                else
                    break;
                yield return new WaitForEndOfFrame();
            }

            if (!isDead)
            {               
                ragdolled = false;             
               
                // reset original setup on tpController
	            StartCoroutine(ResetPlayer(1f));
            }
            else
            {
                Destroy(iChar as Component);
                yield return new WaitForEndOfFrame();
                DestroyComponents();
            }
        }

        // reset player - restore control to the character	
        IEnumerator ResetPlayer(float waitTime)
        {          
            yield return new WaitForSeconds(waitTime);
            //Debug.Log("Ragdoll OFF");        
            isActive = false;
            iChar.ResetRagdoll();
        }

        // ragdoll blend - code based on the script by Perttu Hämäläinen with modifications to work with this Controller        
        void RagdollBehaviour()
        {
            var isDead = !(iChar.currentHealth > 0);
            if (isDead) return;
            if (!iChar.ragdolled) return;            
            
            //Blending from ragdoll back to animated
            if (state == RagdollState.blendToAnim)
            {
                if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime)
                {
                    //If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim
                    //character to the best match with the ragdoll
                    Vector3 animatedToRagdolled = ragdolledHipPosition - animator.GetBoneTransform(HumanBodyBones.Hips).position;
                    Vector3 newRootPosition = transform.position + animatedToRagdolled;

                    //Now cast a ray from the computed position downwards and find the highest hit that does not belong to the character 
                    RaycastHit[] hits = Physics.RaycastAll(new Ray(newRootPosition + Vector3.up, Vector3.down));
                    //newRootPosition.y = 0;

                    foreach (RaycastHit hit in hits)
                    {
                        if (!hit.transform.IsChildOf(transform))
                        {                            
                            newRootPosition.y = Mathf.Max(newRootPosition.y, hit.point.y);
                        }
                    }
                    transform.position = newRootPosition;

                    //Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
                    Vector3 ragdolledDirection = ragdolledHeadPosition - ragdolledFeetPosition;
                    ragdolledDirection.y = 0;

                    Vector3 meanFeetPosition = 0.5f * (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                    Vector3 animatedDirection = animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
                    animatedDirection.y = 0;

                    //Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
                    //hence setting the y components of the vectors to zero. 
                    transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdolledDirection.normalized);
                }
                //compute the ragdoll blend amount in the range 0...1
                float ragdollBlendAmount = 1.0f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
                ragdollBlendAmount = Mathf.Clamp01(ragdollBlendAmount);

                //In LateUpdate(), Mecanim has already updated the body pose according to the animations. 
                //To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips 
                //and slerp all the rotations towards the ones stored when ending the ragdolling
                foreach (BodyPart b in bodyParts)
                {
                    if (b.transform != transform)
                    { //this if is to prevent us from modifying the root of the character, only the actual body parts
                      //position is only interpolated for the hips
                        if (b.transform == animator.GetBoneTransform(HumanBodyBones.Hips))
                            b.transform.position = Vector3.Lerp(b.transform.position, b.storedPosition, ragdollBlendAmount);
                        //rotation is interpolated for all body parts
                        b.transform.rotation = Quaternion.Slerp(b.transform.rotation, b.storedRotation, ragdollBlendAmount);
                    }
                }

                //if the ragdoll blend amount has decreased to zero, move to animated state
                if (ragdollBlendAmount == 0)
                {
                    state = RagdollState.animated;
                    return;
                }
            }
        }

        // set all rigidbodies to kinematic
        void setKinematic(bool newValue)
        {
            var _hips = characterHips.GetComponent<Rigidbody>();
            _hips.isKinematic = newValue;
            Component[] components = _hips.transform.GetComponentsInChildren(typeof(Rigidbody));

            foreach (Component c in components)
            {
                if (!ignoreTags.Contains(c.transform.tag))
                    (c as Rigidbody).isKinematic = newValue;
            }
        }

        // set all colliders to trigger
        void setCollider(bool newValue)
        {
            if (!disableColliders) return;

            var _hips = characterHips.GetComponent<Collider>();
            _hips.enabled = !newValue;
            Component[] components = _hips.transform.GetComponentsInChildren(typeof(Collider));

            foreach (Component c in components)
            {
                if (!ignoreTags.Contains(c.transform.tag))
                    if (!c.transform.Equals(transform)) (c as Collider).enabled = !newValue;
            }
        }

        // destroy the components if the character is dead
        void DestroyComponents()
        {
            if (removePhysicsAfterDie)
            {
                var joints = GetComponentsInChildren<CharacterJoint>();
                if (joints != null)
                {
                    foreach (CharacterJoint comp in joints)
                        if (!ignoreTags.Contains(comp.gameObject.tag))
                            DestroyObject(comp);
                }

                var rigidbodys = GetComponentsInChildren<Rigidbody>();
                if (rigidbodys != null)
                {
                    foreach (Rigidbody comp in rigidbodys)
                        if (!ignoreTags.Contains(comp.gameObject.tag))
                            DestroyObject(comp);
                }

                var colliders = GetComponentsInChildren<Collider>();
                if (colliders != null)
                {
                    foreach (Collider comp in colliders)
                        if (!ignoreTags.Contains(comp.gameObject.tag))
                            DestroyObject(comp);
                }
            }
            else
            {
                var collider = GetComponent<Collider>();
                var rigidbody = GetComponent<Rigidbody>();
                Destroy(rigidbody);
                Destroy(collider);
            }

            var scripts = GetComponentsInChildren<MonoBehaviour>();
            if (scripts != null)
            {
                foreach (MonoBehaviour comp in scripts)
                    if (!ignoreTags.Contains(comp.gameObject.tag))
                        DestroyObject(comp);
            }
        }
    }
}