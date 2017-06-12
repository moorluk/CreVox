using UnityEngine;
using System.Collections;

namespace Invector.ItemManager
{
   
    public class vEquipmentDisplay : vItemSlot
    {
        public override void AddItem(vItem item)
        {
            if (this.item != item)
            {
                base.AddItem(item);
                if (item != null && item.amount > 1)
                    this.amountText.text = item.amount.ToString("00");
                else
                    this.amountText.text = "";
            }
        }      
        
    }

}

