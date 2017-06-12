using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Invector;

namespace Invector.ItemManager
{
    public class vItemListData : ScriptableObject
    {
        public List<vItem> items = new List<vItem>();       
       
        public bool inEdition;
       
        public bool itemsHidden = true;
        
    }

}
