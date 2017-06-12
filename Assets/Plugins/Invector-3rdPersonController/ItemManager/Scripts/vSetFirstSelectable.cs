using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class vSetFirstSelectable : MonoBehaviour
{
    public GameObject firstSelectable;
    public void ApplyFirstSelectable(GameObject firstSelectable)
    {
        this.firstSelectable = firstSelectable;
    }
    void OnEnable()
    {
        StartCoroutine(SetSelectableHandle(firstSelectable));
    }

    IEnumerator SetSelectableHandle(GameObject target)
    {
        if (this.enabled)
        {
            yield return new WaitForEndOfFrame();
            SetSelectable(target);
        }
    }

    void SetSelectable(GameObject target)
    {
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
        EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
        ExecuteEvents.Execute(target, pointer, ExecuteEvents.selectHandler);
    }
}