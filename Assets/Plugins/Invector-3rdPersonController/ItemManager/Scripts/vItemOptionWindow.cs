using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
namespace Invector.ItemManager
{
    public class vItemOptionWindow : MonoBehaviour
    {
        public Button useItemButton;
        public List<vItemType> itemsCanBeUsed = new List<vItemType>() { vItemType.Consumable };
       
        public void EnableOptions(vItemSlot slot)
        {
            if (slot ==null || slot.item==null) return;
            useItemButton.interactable = itemsCanBeUsed.Contains(slot.item.type);
        }        
    }
}

