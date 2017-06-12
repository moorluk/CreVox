using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Invector.CharacterController;

[RequireComponent(typeof(SphereCollider))]
public class v_AISphereSensor : MonoBehaviour
{
    [Header("Who the AI can chase")]
    [Tooltip("Character won't hit back when receive damage, check false and it will automatically add the Tag of the attacker")]
    [HideInInspector]
    public bool passiveToDamage = false;    
	[HideInInspector]
    public List<string> tagsToDetect = new List<string>() { "Player" };
    public LayerMask obstacleLayer = 1 << 0;
    [HideInInspector]
    public bool getFromDistance;

    public List<Transform> targetsInArea;

    void Start()
    {
        targetsInArea = new List<Transform>();
    }

    public void SetTagToDetect(Transform _transform)
    {
        if (_transform!=null && tagsToDetect != null && !tagsToDetect.Contains(_transform.tag))
        {
            tagsToDetect.Add(_transform.tag);
            targetsInArea.Add(_transform);
        }
    }

    public void RemoveTag(Transform _transform)
    {
        if (tagsToDetect != null && tagsToDetect.Contains(_transform.tag))
        {
            tagsToDetect.Remove(_transform.tag);
            if (targetsInArea.Contains(_transform))
                targetsInArea.Remove(_transform);
        }
    }

    public void SetColliderRadius(float radius)
    {
        var collider = GetComponent<SphereCollider>();
        if (collider)
            collider.radius = radius;
    }

    public Transform GetTargetTransform()
    {
        if (targetsInArea.Count > 0)
        {
            SortTargets();
            if (targetsInArea.Count > 0)
                return targetsInArea[0];
        }
        return null;
    }

	public vCharacter GetTargetvCharacter()
    {
        if (targetsInArea.Count > 0)
        {
            SortCharacters();
            if (targetsInArea.Count > 0)
            {
	            var vChar = targetsInArea[0].GetComponent<vCharacter>();
                if (vChar != null && vChar.currentHealth > 0)
                    return vChar;
            }
        }

        return null;
    }

    void SortCharacters()
    {
        for (int i = targetsInArea.Count-1; i >=0; i--)
        {
            var t = targetsInArea[i];
            if (t == null || t.GetComponent<vCharacter>() == null)
            {
                targetsInArea.RemoveAt(i);  
            }
        } 
           

        if (getFromDistance)
            targetsInArea.Sort(delegate (Transform c1, Transform c2)
            {
                return Vector3.Distance(this.transform.position, c1 != null ? c1.transform.position : Vector3.one * Mathf.Infinity).CompareTo
                    ((Vector3.Distance(this.transform.position, c2 != null ? c2.transform.position : Vector3.one * Mathf.Infinity)));
            });
    }

    void SortTargets()
    {
        for (int i = targetsInArea.Count-1; i >=0; i--)
        {
            var t = targetsInArea[i];
            if (t == null)
            {
                targetsInArea.RemoveAt(i);               
            }
        }
        if (getFromDistance)
            targetsInArea.Sort(delegate (Transform c1, Transform c2)
            {
                return Vector3.Distance(this.transform.position, c1 != null ? c1.transform.position : Vector3.one * Mathf.Infinity).CompareTo
                    ((Vector3.Distance(this.transform.position, c2 != null ? c2.transform.position : Vector3.one * Mathf.Infinity)));
            });
    }

    void OnTriggerEnter(Collider other)
    {
        if (tagsToDetect.Contains(other.gameObject.tag) && !targetsInArea.Contains(other.transform))        
            targetsInArea.Add(other.transform);        
    }

    void OnTriggerExit(Collider other)
    {
        if (tagsToDetect.Contains(other.gameObject.tag) && targetsInArea.Contains(other.transform))
            targetsInArea.Remove(other.transform);
    }
}
