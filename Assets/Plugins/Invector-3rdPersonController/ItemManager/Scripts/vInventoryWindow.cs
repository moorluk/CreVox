using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Invector.ItemManager
{
    public class vInventoryWindow : MonoBehaviour
    {
        [SerializeField]
        private System.Action<string, object> myCallback;
        public vInventory inventory;
        public List<vWindowPop_up> pop_ups = new List<vWindowPop_up>();
        GameObject lastSelected;
        public bool isOpen;

        public bool IsOpen
        {
            get { if (pop_ups != null && pop_ups.Count > 0) return false; return isOpen; }
        }

        void Start()
        {
            inventory = GetComponentInParent<vInventory>();
        }

        void OnEnable()
        {
            try
            {
                pop_ups = new List<vWindowPop_up>();
                if (inventory == null)
                    inventory = GetComponentInParent<vInventory>();

                if (lastSelected)
                    StartCoroutine(SetSelectableHandle(lastSelected));
                inventory.SetCurrentWindow(this);
                isOpen = true;
            }
            catch { }
        }

        void OnDisable()
        {
            try
            {
                lastSelected = EventSystem.current.currentSelectedGameObject;
                RemoveAllPop_up();
                isOpen = false;
            }
            catch { }
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

        public void AddPop_up(vWindowPop_up pop_up)
        {
            if (!pop_ups.Contains(pop_up))
            {
                pop_ups.Add(pop_up);
                if (!pop_up.gameObject.activeSelf)
                    pop_up.gameObject.SetActive(true);
            }
        }

        public void RemovePop_up(vWindowPop_up pop_up)
        {
            try
            {
                if (pop_ups.Contains(pop_up))
                {
                    pop_ups.Remove(pop_up);
                    if (pop_up.gameObject.activeSelf)
                        pop_up.gameObject.SetActive(false);

                    //if (pop_ups.Count > 0)
                    //{
                    //    if (pop_ups[pop_ups.Count - 1]!=null &&!pop_ups[pop_ups.Count - 1].gameObject.activeSelf)
                    //        pop_ups[pop_ups.Count - 1].gameObject.SetActive(true);
                    //}
                }
            }
            catch { }

        }

        public void RemoveLastPop_up()
        {
            if (pop_ups.Count > 0)
            {
                var popup = pop_ups[pop_ups.Count - 1];
                pop_ups.Remove(popup);
                if (popup.gameObject.activeSelf)
                    popup.gameObject.SetActive(false);

                if (pop_ups.Count > 0)
                {
                    if (pop_ups.Count > 0)
                        if (!pop_ups[pop_ups.Count - 1].gameObject.activeSelf)
                            pop_ups[pop_ups.Count - 1].gameObject.SetActive(true);
                }
            }
        }

        public void RemoveAllPop_up()
        {
            foreach (vWindowPop_up popup in pop_ups)
            {
                popup.gameObject.SetActive(false);
            }
            pop_ups.Clear();
        }

        public bool ContainsPop_up()
        {
            return pop_ups.Count > 0;
        }
    }
}
