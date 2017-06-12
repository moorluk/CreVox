using UnityEngine;
using System.Collections;

public class vLookAtCamera : MonoBehaviour
{
    [Tooltip("Align position to stay always on top of parent")]
    public bool alignUp = false;
    [Tooltip("Height of alignment on top of parent \n!!(Check alignUp to work)!!")]
    public float height = 1;
    [Tooltip("Detach of the parent on start \n!!(if alignUp not is checked, the object not follow the parent)!!")]
    public bool detachOnStart;
    [Tooltip("use smoth to look at camera")]
    public bool useSmothRotation = true;
    protected Transform parent;

    void Start()
    {
        if (detachOnStart)
        {
            parent = transform.parent;
            transform.SetParent(null);
        }
    }

    void FixedUpdate()
    {
        if (alignUp && parent)
            transform.position = parent.position + Vector3.up * height;

        var lookPos = Camera.main.transform.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        if (useSmothRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 4f);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
        }
    }
}
