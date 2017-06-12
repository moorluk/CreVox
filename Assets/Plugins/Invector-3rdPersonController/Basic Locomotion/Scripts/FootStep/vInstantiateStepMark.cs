using UnityEngine;

public class vInstantiateStepMark : MonoBehaviour
{
    public GameObject stepMark;
    public LayerMask stepLayer;
    public float timeToDestroy = 5f;

    void StepMark(FootStepObject footStep)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), -footStep.sender.up, out hit, 1f, stepLayer))
        {
            var angle = Quaternion.FromToRotation(footStep.sender.up, hit.normal);
            if (stepMark != null)
            {
                var step = Instantiate(stepMark, hit.point, angle * footStep.sender.rotation) as GameObject;
                //if (footStep.ground != null)
                //    step.transform.SetParent(footStep.ground);
                Destroy(step, timeToDestroy);
                //Destroy(gameObject, timeToDestroy);
            }
            else
                Destroy(gameObject, timeToDestroy);
        }
    }
}
