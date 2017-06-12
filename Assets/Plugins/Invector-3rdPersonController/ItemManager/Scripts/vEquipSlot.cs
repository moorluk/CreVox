using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Invector.ItemManager
{
    public class vEquipSlot : vItemSlot
    {
        [Header("--- Item Type ---")]
        public List<vItemType> itemType;
       
        public override void AddItem(vItem item)
        {
            if (item) item.isInEquipArea = true;
            base.AddItem(item);
            onAddItem.Invoke(item);
        }

        public override void RemoveItem()
        {
            onRemoveItem.Invoke(item);     
            if (item != null) item.isInEquipArea = false;
            base.RemoveItem();
        }
    }
}