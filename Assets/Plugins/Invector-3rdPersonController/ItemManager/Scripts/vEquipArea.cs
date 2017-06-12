using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Invector.ItemManager
{
    public class vEquipArea : MonoBehaviour
    {
        public delegate void OnPickUpItem(vEquipArea area, vItemSlot slot);
        public OnPickUpItem onPickUpItemCallBack;

        public vInventory inventory;
        public vInventoryWindow rootWindow;
        public vItemWindow itemPicker;
        public List<vEquipSlot> equipSlots;
        public string equipPointName;
        public OnChangeEquipmentEvent onEquipItem;
        public OnChangeEquipmentEvent onUnequipItem;
        [HideInInspector]
        public vEquipSlot currentSelectedSlot;
        private int indexOfEquipedItem;
        public vItem lastEquipedItem;

        void Start()
        {
            inventory = GetComponentInParent<vInventory>();
            if (equipSlots.Count == 0)
            {
                var equipSlotsArray = GetComponentsInChildren<vEquipSlot>();
                equipSlots = equipSlotsArray.vToList();
            }
            rootWindow = GetComponentInParent<vInventoryWindow>();
            foreach (vEquipSlot slot in equipSlots)
            {
                slot.onSubmitSlotCallBack = OnSubmitSlot;
                slot.onSelectSlotCallBack = OnSelectSlot;
                slot.onDeselectSlotCallBack = OnDeselect;
                slot.amountText.text = "";
            }          
        }

        public void OnSubmitSlot(vItemSlot slot)
        {
            if (itemPicker != null)
            {
                currentSelectedSlot = slot as vEquipSlot;
                itemPicker.gameObject.SetActive(true);
                itemPicker.CreateEquipmentWindow(inventory.items, currentSelectedSlot.itemType, slot.item, OnPickItem);
            }
        }

        public void RemoveItem(vEquipSlot slot)
        {
            if (slot)
            {
                vItem item = slot.item;
                slot.RemoveItem();
                onUnequipItem.Invoke(this, item);
            }
        }
        public void RemoveItem(vItem item)
        {
            var slot = ValidSlots.Find(_slot => _slot.item == item);
            if (slot)
            {
                slot.RemoveItem();
                onUnequipItem.Invoke(this, item);
            }
        }
        public void RemoveItem()
        {
            if (currentSelectedSlot)
            {
                {
                    var _item = currentSelectedSlot.item;
                    currentSelectedSlot.RemoveItem();
                    onUnequipItem.Invoke(this, _item);
                    
                }
            }
        }
        public void RemoveCurrentItem()
        {
            if (!currentEquipedItem) return;
            lastEquipedItem = currentEquipedItem;
            ValidSlots[indexOfEquipedItem].RemoveItem();
            onUnequipItem.Invoke(this, lastEquipedItem);

        }
        public void OnSelectSlot(vItemSlot slot)
        {
            currentSelectedSlot = slot as vEquipSlot;
        }

        public void OnDeselect(vItemSlot slot)
        {
            currentSelectedSlot = null;
        }

        public void OnPickItem(vItemSlot slot)
        {
            if (currentSelectedSlot.item != null && slot.item != currentSelectedSlot.item)
            {
                currentSelectedSlot.item.isInEquipArea = false;            
                onUnequipItem.Invoke(this, currentSelectedSlot.item);
            }
            if (slot.item != currentSelectedSlot.item)
            {
                if (onPickUpItemCallBack != null && slot)
                    onPickUpItemCallBack(this, slot);

                if (currentSelectedSlot.item != null && currentSelectedSlot.item != slot.item)
                    lastEquipedItem = slot.item;

                currentSelectedSlot.AddItem(slot.item);
                onEquipItem.Invoke(this, slot.item);
            }

            itemPicker.gameObject.SetActive(false);
        }

        public vItem currentEquipedItem
        {
            get
            {
                var validEquipSlots = ValidSlots;
                return validEquipSlots[indexOfEquipedItem].item;
            }
        }

        public void NextEquipSlot()
        {
            if (equipSlots == null || equipSlots.Count == 0) return;
            lastEquipedItem = currentEquipedItem;
            var validEquipSlots = ValidSlots;
            if (indexOfEquipedItem + 1 < validEquipSlots.Count)
                indexOfEquipedItem++;
            else
                indexOfEquipedItem = 0;

            onEquipItem.Invoke(this, currentEquipedItem);
            onUnequipItem.Invoke(this, lastEquipedItem);
        }

        public void PreviousEquipSlot()
        {
            if (equipSlots == null || equipSlots.Count == 0) return;
            lastEquipedItem = currentEquipedItem;
            var validEquipSlots = ValidSlots;

            if (indexOfEquipedItem - 1 >= 0)
                indexOfEquipedItem--;
            else
                indexOfEquipedItem = validEquipSlots.Count - 1;
            onEquipItem.Invoke(this, currentEquipedItem);
            onUnequipItem.Invoke(this, lastEquipedItem);

        }

        public void SetEquipSlot(int index)
        {
            if (equipSlots == null || equipSlots.Count == 0) return;
           

            if (index < equipSlots.Count /*&& equipSlots[index].isValid*/ && equipSlots[index].item!=currentEquipedItem)
            {
                lastEquipedItem = currentEquipedItem;
                indexOfEquipedItem = index;
                onEquipItem.Invoke(this, currentEquipedItem);
                onUnequipItem.Invoke(this, lastEquipedItem);
            }
        }

        public List<vEquipSlot> ValidSlots
        {
            get { return equipSlots.FindAll(slot => slot.isValid); }
        }

        public bool ContainsItem(vItem item)
        {
            return ValidSlots.Find(slot => slot.item == item) != null;
        }

        public void EquiItemToSlot(vItem item, vEquipSlot slot = null)
        {
            if (slot == null && indexOfEquipedItem < equipSlots.Count)
                currentSelectedSlot = equipSlots[indexOfEquipedItem];
            else if (equipSlots.Contains(currentSelectedSlot))
                currentSelectedSlot = slot;

            if (currentSelectedSlot)
            {
                if (item != currentSelectedSlot.item && currentSelectedSlot.item != null)
                {
                    onUnequipItem.Invoke(this, currentSelectedSlot.item);
                }
                if (currentSelectedSlot.item != item)
                {
                    if (onPickUpItemCallBack != null && slot)
                        onPickUpItemCallBack(this, slot);

                    if (currentSelectedSlot.item != item)
                        lastEquipedItem = slot.item;

                    currentSelectedSlot.AddItem(item);
                    onEquipItem.Invoke(this, item);

                    if (currentSelectedSlot.item != null)
                        currentSelectedSlot.item.isInEquipArea = false;
                }
                //itemPicker.gameObject.SetActive(false);
            }
        }
    }
}
