using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Invector;
using UnityEngine.Events;
using System;

namespace Invector.ItemManager
{
    public class vItemCollection : vTriggerGenericAction
    {               
        [SerializeField]
        private OnCollectItems onCollectItems;

        [Header("---Items List Data---")]
        public vItemListData itemListData;

        [Header("---Items Filter---")]      
        public List<vItemType> itemsFilter = new List<vItemType>() { 0 };

        [HideInInspector]
        public List<ItemReference> items = new List<ItemReference>();
        public float onCollectDelay;

        public void OnCollectItems(GameObject target)
        {            
            if (items.Count > 0)
            {
                items.Clear();
                StartCoroutine(OnCollect(target));
            }
        }

        IEnumerator OnCollect(GameObject target)
        {
            yield return new WaitForSeconds(onCollectDelay);
            onCollectItems.Invoke(target);
        }        
    }
}

