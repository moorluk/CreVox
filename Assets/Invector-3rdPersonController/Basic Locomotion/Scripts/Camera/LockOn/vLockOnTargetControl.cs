using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Invector;

public class vLockOnTargetControl : vLockOnTarget
{
    [Tooltip("Create a Image inside the UI and assign here")]
    public RectTransform aimImage;
    [Tooltip("Assign the UI here")]
    public Canvas aimCanvas;
    [Tooltip("True: Hide the sprite when not Lock On, False: Always show the Sprite")]
    public bool hideSprite;
    private bool inTarget;

    void Start()
    {      
        Init();
    }
    
    void Update()
    {
        CheckForCharacterAlive();
        UpdateAimImage();
    }

    void CheckForCharacterAlive()
    {
        if (currentTarget && !isCharacterAlive() && inTarget ||(inTarget && !isCharacterAlive()))
        {           
            ResetLockOn();
            inTarget = false;
            UpdateLockOn(true);
            if(currentTarget==null)
                SendMessage("ClearTargetLockOn", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void StopLockOn()
    {
        inTarget = false;
        ResetLockOn();
    }

    /// <summary>
    /// Override of base Update (turn On or Off the LockOnTarget)
    /// Call This function using SendMessage("UpdateLockOn",bool);
    /// </summary>
    /// <param name="value"></param>
    public override void UpdateLockOn(bool value)
    {
        base.UpdateLockOn(value);

        if (!inTarget && currentTarget)
        {
            inTarget = true;
            // send current target if inTarget           
            SetTarget();
        }
        else if (inTarget && !currentTarget)
        {
            inTarget = false;
            // send message to clear current target
            SendMessage("ClearTargetLockOn", SendMessageOptions.DontRequireReceiver); 
        }         
    }

    public override void SetTarget()
    {
        SendMessage("SetTargetLockOn", currentTarget.transform, SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Override of base ChangeTarget
    /// Use this to change the current target for the next.
    /// Call this function using SendMessage("ChangeTarget");
    /// </summary>
    public override void ChangeTarget(int value)
    {       
        base.ChangeTarget(value);        
    }

    void UpdateAimImage()
    {
        if(hideSprite)
        {            
            if (currentTarget && !aimImage.transform.gameObject.activeSelf && isCharacterAlive())
                aimImage.transform.gameObject.SetActive(true);
            else if (!currentTarget && aimImage.transform.gameObject.activeSelf)
                aimImage.transform.gameObject.SetActive(false);
            else if(aimImage.transform.gameObject.activeSelf  && !isCharacterAlive())
                aimImage.transform.gameObject.SetActive(false);
        }
        if (currentTarget && aimImage && aimCanvas)
            aimImage.anchoredPosition = currentTarget.GetScreenPointOffBoundsCenter(aimCanvas, cam, spriteHeight);
        else if (aimCanvas)
            aimImage.anchoredPosition = Vector2.zero;
    }	
}
