using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class vBoxTrigger : MonoBehaviour
{
    public List<string> tagsToIgnore = new List<string>() { "Player" };
    public LayerMask mask = 1;
    // [HideInInspector]
    public bool inCollision;
    private bool triggerStay;
    // Use this for initialization
    private bool inResetMove;
    private Rigidbody rgb;
    void OnDrawGizmos()
    {

        Color red = new Color(1, 0, 0, 0.5f);
        Color green = new Color(0, 1, 0, 0.5f);
        // Forward Gizmo;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, (transform.lossyScale));
        Gizmos.color = inCollision && Application.isPlaying ? red : green;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

    }
    void Start()
    {
        inCollision = false;
        gameObject.GetComponent<BoxCollider>().isTrigger = true;
        rgb = gameObject.GetComponent<Rigidbody>();
        rgb.isKinematic = true;
        rgb.useGravity = false;
        rgb.constraints = RigidbodyConstraints.FreezeAll;
    }
    void Update()
    {
        if (rgb != null && rgb.IsSleeping()) rgb.WakeUp();
    }
    void OnTriggerStay(Collider Other)
    {
        if (!tagsToIgnore.Contains(Other.gameObject.tag) && IsInLayerMask(Other.gameObject, mask))
        {
            inCollision = true;
            triggerStay = true;
        }
    }
    void OnTriggerExit(Collider Other)
    {
        if (!tagsToIgnore.Contains(Other.gameObject.tag) && IsInLayerMask(Other.gameObject, mask))
        {
            triggerStay = false;
            if (!inResetMove)
            {
                inResetMove = true;
                StartCoroutine(ResetMove());
            }
        }
    }
    IEnumerator ResetMove()
    {
        yield return new WaitForSeconds(0.2f);
        if (!triggerStay)
            inCollision = false;
        inResetMove = false;
    }
    bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return ((mask.value & (1 << obj.layer)) > 0);
    }
}
