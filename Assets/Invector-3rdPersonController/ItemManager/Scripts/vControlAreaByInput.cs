using UnityEngine;
using System.Collections;
using Invector.CharacterController;
using System.Collections.Generic;

namespace Invector.ItemManager
{
    public class vControlAreaByInput : MonoBehaviour
    {
        public List<SlotsSelector> slotsSelectors;
        public vEquipArea equipArea;
        public vInventory inventory;

        void Start()
        {
            inventory = GetComponentInParent<vInventory>();
        }

        void Update()
        {
            if (!inventory || !equipArea || inventory.lockInput) return;

            for (int i = 0; i < slotsSelectors.Count; i++)
            {
                if(slotsSelectors[i].input.GetButtonDown() && (inventory && !inventory.isOpen && inventory.canEquip))
                {
                    if(slotsSelectors[i].indexOfSlot < equipArea.equipSlots.Count && slotsSelectors[i].indexOfSlot >= 0)
                    {                        
                        equipArea.SetEquipSlot(slotsSelectors[i].indexOfSlot);
                    }
                }

                if (slotsSelectors[i].equipDisplay != null && slotsSelectors[i].indexOfSlot < equipArea.equipSlots.Count && slotsSelectors[i].indexOfSlot >= 0)
                {
                    if(equipArea.equipSlots[slotsSelectors[i].indexOfSlot].item != slotsSelectors[i].equipDisplay.item)
                    {
                        slotsSelectors[i].equipDisplay.AddItem(equipArea.equipSlots[slotsSelectors[i].indexOfSlot].item);
                    }
                }
            }
        }

        [System.Serializable]
        public class SlotsSelector
        {
            public GenericInput input;
            public int indexOfSlot;
            public vEquipmentDisplay equipDisplay;
        }
    }
}

