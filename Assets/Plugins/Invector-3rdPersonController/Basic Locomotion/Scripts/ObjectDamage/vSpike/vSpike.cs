using UnityEngine;
using System.Collections;

public class vSpike : MonoBehaviour
{
    HingeJoint joint;
    [HideInInspector]
    public vSpikeControl control;

    void Start()
    {
        joint = GetComponent<HingeJoint>();
    }
    bool inConect;
    Transform impaled;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody != null && collision.collider.GetComponent<vCollisionMessage>() != null && !inConect)
        {
            bool condition = control == null ? true : !control.attachColliders.Contains(collision.collider.transform);
            if (control) control.attachColliders.Add(collision.collider.transform);
            if (condition)
            {
                inConect = true;
                if(joint && collision.rigidbody)
                    joint.connectedBody = collision.rigidbody;

                impaled = collision.transform;
                foreach (Rigidbody body in collision.transform.root.GetComponentsInChildren<Rigidbody>())
                {
                    body.velocity = Vector3.zero;
                }
                var ichar = collision.collider.GetComponent<vCollisionMessage>();
                if (ichar)
                    ichar.ragdoll.iChar.currentHealth = 0;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform != null && impaled != null && other.transform == impaled)
        {
            if (joint)
                joint.connectedBody = null;
            impaled = null;
            if (control != null && control.attachColliders.Contains(impaled))
                control.attachColliders.Remove(impaled);
            inConect = false;
        }
    }
}
