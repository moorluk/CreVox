using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Invector;
using Invector.CharacterController;
using Invector.CharacterController.Actions;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace Invector.ItemManager
{
    public class vItemManager : MonoBehaviour
    {
        public string collectableTag = "Collectable";
        public bool dropItemsWhenDead;
        public GenericInput actionInput = new GenericInput("E", "A", "A");
        public vInventory inventoryPrefab;
        [HideInInspector]
        public vInventory inventory;
        public vItemListData itemListData;

        [Header("---Items Filter---")]      
        public List<vItemType> itemsFilter = new List<vItemType>() { 0 };

        #region SerializedProperties in Custom Editor
        [SerializeField]
        public List<ItemReference> startItems = new List<ItemReference>();

        public List<vItem> items;
        public OnHandleItemEvent onUseItem, onAddItem;
        public OnChangeItemAmount onLeaveItem, onDropItem;
        public OnOpenCloseInventory onOpenCloseInventory;
        public OnChangeEquipmentEvent onEquipItem, onUnequipItem;
        [SerializeField]
        public List<EquipPoint> equipPoints;
        [SerializeField]
        public List<ApplyAttributeEvent> applyAttributeEvents;
        #endregion
        [HideInInspector]
        public bool inEquip;
        private float equipTimer;
        private Animator animator;
        private vMeleeCombatInput tpInput;        
        private static vItemManager instance;
        
        void Start()
        {
            if (instance != null) return;
            inventory = FindObjectOfType<vInventory>();

            instance = this;
            if (!inventory && inventoryPrefab)
            {
                inventory = Instantiate(inventoryPrefab);
            }

            if (!inventory) Debug.LogError("No vInventory assigned!");

            if (inventory)
            {
                inventory.GetItemsHandler = GetItems;
                inventory.onEquipItem.AddListener(EquipItem);
                inventory.onUnequipItem.AddListener(UnequipItem);
                inventory.onDropItem.AddListener(DropItem);
                inventory.onLeaveItem.AddListener(LeaveItem);
                inventory.onUseItem.AddListener(UseItem);
                inventory.onOpenCloseInventory.AddListener(OnOpenCloseInventory);
            }

            if (dropItemsWhenDead)
            {
                var character = GetComponent<vCharacter>();
                if (character)
                    character.onDead.AddListener(DropAllItens);
            }

            items = new List<vItem>();
            if (itemListData)
            {
                for (int i = 0; i < startItems.Count; i++)
                {
                    AddItem(startItems[i]);
                }
            }          

            animator = GetComponent<Animator>();
            tpInput = GetComponent<vMeleeCombatInput>();            
            if (tpInput)
            {
                tpInput.onUpdateInput.AddListener(UpdateInput);
            }

            var basicAction = GetComponent<vGenericAction>();
            if (basicAction != null)
            {
                basicAction.OnDoAction.AddListener(CollectItem);
            }
        }

        public List<vItem> GetItems()
        {
            return items;
        }

        /// <summary>
        /// Add new Instance of Item to itemList
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(ItemReference itemReference)
        {
            if (itemReference != null && itemListData != null && itemListData.items.Count > 0)
            {
                var item = itemListData.items.Find(t => t.id.Equals(itemReference.id));
                if (item)
                {
                    var sameItems = items.FindAll(i => i.stackable && i.id == item.id && i.amount < i.maxStack);
                    if (sameItems.Count == 0)
                    {
                        var _item = Instantiate(item);
                        _item.name = _item.name.Replace("(Clone)", string.Empty);
                        if (itemReference.attributes != null && _item.attributes != null && item.attributes.Count == itemReference.attributes.Count)
                            _item.attributes = new List<vItemAttribute>(itemReference.attributes);
                        _item.amount = 0;
                        for (int i = 0; i < item.maxStack && _item.amount < _item.maxStack && itemReference.amount > 0; i++)
                        {
                            _item.amount++;
                            itemReference.amount--;
                        }
                        items.Add(_item);
                        onAddItem.Invoke(_item);
                        if (itemReference.amount > 0)
                        {
                            AddItem(itemReference);
                        }
                    }
                    else
                    {
                        var indexOffItem = items.IndexOf(sameItems[0]);

                        for (int i = 0; i < items[indexOffItem].maxStack && items[indexOffItem].amount < items[indexOffItem].maxStack && itemReference.amount > 0; i++)
                        {
                            items[indexOffItem].amount++;
                            itemReference.amount--;
                        }
                        if (itemReference.amount > 0)
                        {
                            AddItem(itemReference);
                        }
                    }
                }
            }
        }

        public void UseItem(vItem item)
        {
            if (item)
            {
                onUseItem.Invoke(item);
                if (item.attributes != null && item.attributes.Count > 0 && applyAttributeEvents.Count > 0)
                {
                    foreach (ApplyAttributeEvent attributeEvent in applyAttributeEvents)
                    {
                        var attributes = item.attributes.FindAll(a => a.name.Equals(attributeEvent.attribute));
                        foreach (vItemAttribute attribute in attributes)
                            attributeEvent.onApplyAttribute.Invoke(attribute.value);
                    }
                }
                if (item.amount <= 0 && items.Contains(item)) items.Remove(item);
            }
        }

        public void LeaveItem(vItem item, int amount)
        {
            onLeaveItem.Invoke(item, amount);
            item.amount -= amount;
            if (item.amount <= 0 && items.Contains(item))
            {               
                if (item.type != vItemType.Consumable)
                {
                    var equipPoint = equipPoints.Find(ep => ep.equipmentReference.item == item || ep.area != null && ep.area.ValidSlots.Find(slot => slot.item == item));
                    if (equipPoint != null)                   
                        if (equipPoint.area) equipPoint.area.RemoveItem(item);                                         
                }
                items.Remove(item);
                Destroy(item);
            }
        }

        public void DropItem(vItem item, int amount)
        {

            item.amount -= amount;
            if (item.dropObject != null)
            {
                var dropObject = Instantiate(item.dropObject, transform.position, transform.rotation) as GameObject;
                vItemCollection collection = dropObject.GetComponent<vItemCollection>();
                if (collection != null)
                {
                    collection.items.Clear();
                    var itemReference = new ItemReference(item.id);
                    itemReference.amount = amount;
                    itemReference.attributes = new List<vItemAttribute>(item.attributes);
                    collection.items.Add(itemReference);
                }
            }
            onDropItem.Invoke(item, amount);
            if (item.amount <= 0 && items.Contains(item))
            {                
                if (item.type != vItemType.Consumable)
                {
                    var equipPoint = equipPoints.Find(ep => ep.equipmentReference.item == item || ep.area!=null && ep.area.ValidSlots.Find(slot =>slot.item == item));                 
                    if (equipPoint != null)                  
                        if (equipPoint.area) equipPoint.area.RemoveItem(item);
                }                
                items.Remove(item);
                Destroy(item);
            }
        }

        public void DropAllItens(GameObject target = null)
        {
            if (target != null && target != gameObject) return;
            List<ItemReference> itemReferences = new List<ItemReference>();
            for (int i = 0; i < items.Count; i++)
            {
                if (itemReferences.Find(_item => _item.id == items[i].id) == null)
                {
                    var sameItens = items.FindAll(_item => _item.id == items[i].id);
                    ItemReference itemReference = new ItemReference(items[i].id);
                    for (int a = 0; a < sameItens.Count; a++)
                    {
                        if (sameItens[a].type != vItemType.Consumable)
                        {
                            var equipPoint = equipPoints.Find(ep => ep.equipmentReference.item == sameItens[a]);
                            if (equipPoint != null && equipPoint.equipmentReference.equipedObject != null)
                                UnequipItem(equipPoint.area, equipPoint.equipmentReference.item);
                        }

                        itemReference.amount += sameItens[a].amount;
                        Destroy(sameItens[a]);
                    }
                    itemReferences.Add(itemReference);
                    if (equipPoints != null)
                    {
                        var equipPoint = equipPoints.Find(e => e.equipmentReference != null && e.equipmentReference.item != null && e.equipmentReference.item.id == itemReference.id && e.equipmentReference.equipedObject != null);
                        if (equipPoint != null)
                        {
                            Destroy(equipPoint.equipmentReference.equipedObject);
                            equipPoint.equipmentReference = null;
                        }
                    }
                    if (items[i].dropObject)
                    {
                        var dropObject = Instantiate(items[i].dropObject, transform.position, transform.rotation) as GameObject;
                        vItemCollection collection = dropObject.GetComponent<vItemCollection>();
                        if (collection != null)
                        {
                            collection.items.Clear();
                            collection.items.Add(itemReference);
                        }
                    }
                }
            }
            items.Clear();
        }

        #region Check Item in List
        /// <summary>
        /// Check if Item List contains a  Item
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns></returns>
        public bool ContainItem(int id)
        {
            return items.Exists(i => i.id == id);
        }

        /// <summary>
        /// Check if the list contains a item with certain amount, or more
        /// </summary>
        /// <param name="id">Item id</param>
        /// <param name="amount">Item amount</param>
        /// <returns></returns>
        public bool ContainItem(int id, int amount)
        {
            var item = items.Find(i => i.id == id && i.amount >= amount);
            return item != null;
        }

        /// <summary>
        /// Check if the list contains a certain count of items, or more
        /// </summary>
        /// <param name="id">Item id</param>
        /// <param name="count">Item count</param>
        /// <returns></returns>
        public bool ContainItems(int id, int count)
        {
            var _items = items.FindAll(i => i.id == id);
            return _items != null && _items.Count >= count;
        }

        #endregion

        #region Get Item in List
        /// <summary>
        /// Get a single Item with same id
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns></returns>
        public vItem GetItem(int id)
        {
            return items.Find(i => i.id == id);
        }

        /// <summary>
        /// Get All Items with same id
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns></returns>
        public List<vItem> GetItems(int id)
        {
            var _items = items.FindAll(i => i.id == id);
            return _items;
        }

        /// <summary>
        /// Ask if the Item is currently equipped
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsItemEquipped(int id)
        {
            return equipPoints.Exists(ep => ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.id.Equals(id));
        }

        /// <summary>
        /// Get a specific Item on a specific EquipmentPoint
        /// </summary>
        /// <param name="equipPointName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsItemEquippedOnSpecificEquipPoint(string equipPointName, int id)
        {
            return equipPoints.Exists(ep => ep.equipPointName.Equals(equipPointName) && ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.id.Equals(id));
        }

        #endregion

        public void EquipItem(vEquipArea equipArea, vItem item)
        {
            onEquipItem.Invoke(equipArea, item);            
            if (item != equipArea.currentEquipedItem) return;
            var equipPoint = equipPoints.Find(ep => ep.equipPointName == equipArea.equipPointName);
            if (equipPoint != null && item != null && equipPoint.equipmentReference.item != item)
            {
                equipTimer = item.equipDelayTime;

                var type = item.type;
                if (type != vItemType.Consumable)
                {
                    if (!inventory.isOpen)
                    {
                        animator.SetInteger("EquipItemID", equipArea.equipPointName.Contains("Right") ? item.EquipID : -item.EquipID);
                        animator.SetTrigger("EquipItem");
                    }
                    equipPoint.area = equipArea;
                    StartCoroutine(EquipItemRoutine(equipPoint, item));
                }
            }
        }

        public void UnequipItem(vEquipArea equipArea, vItem item)
        {
            onUnequipItem.Invoke(equipArea, item);
            //if (item != equipArea.lastEquipedItem) return;
            var equipPoint = equipPoints.Find(ep => ep.equipPointName == equipArea.equipPointName && ep.equipmentReference.item != null && ep.equipmentReference.item == item);
            if (equipPoint != null && item != null)
            {
                equipTimer = item.equipDelayTime;
                var type = item.type;
                if (type != vItemType.Consumable)
                {
                    if (!inventory.isOpen && !inEquip)
                    {
                        animator.SetInteger("EquipItemID", equipArea.equipPointName.Contains("Right") ? item.EquipID : -item.EquipID);
                        animator.SetTrigger("EquipItem");
                    }
                    StartCoroutine(UnequipItemRoutine(equipPoint, item));
                }
            }
        }

        IEnumerator EquipItemRoutine(EquipPoint equipPoint, vItem item)
        {
            if (!inEquip)
            {
                inventory.canEquip = false;
                inEquip = true;
                if (!inventory.isOpen)
                {
                    while (equipTimer > 0)
                    {
                        equipTimer -= Time.deltaTime;
                        if (item == null) break;
                        yield return new WaitForEndOfFrame();
                    }
                }
                if (equipPoint != null)
                {
                    if (item.originalObject)
                    {
                        if (equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null)
                        {
                            var _equipment = equipPoint.equipmentReference.equipedObject.GetComponent<vIEquipment>();
                            if (_equipment != null) _equipment.OnUnequip(equipPoint.equipmentReference.item);
                            Destroy(equipPoint.equipmentReference.equipedObject);
                        }                            

                        var point = equipPoint.handler.customHandlers.Find(p => p.name == item.customEquipPoint);
                        var equipTransform = point != null ? point : equipPoint.handler.defaultHandler;
                        var equipedObject = Instantiate(item.originalObject, equipTransform.position, equipTransform.rotation) as GameObject;
                        equipedObject.transform.parent = equipTransform;

                        if (equipPoint.equipPointName.Contains("Left"))
                        {
                            var scale = equipedObject.transform.localScale;
                            scale.x *= -1;
                            equipedObject.transform.localScale = scale;
                        }

                        equipPoint.equipmentReference.item = item;
                        equipPoint.equipmentReference.equipedObject = equipedObject;
                        var equipment = equipedObject.GetComponent<vIEquipment>();
                        if (equipment != null) equipment.OnEquip(item);
                        equipPoint.onInstantiateEquiment.Invoke(equipedObject);
                    }
                    else if (equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null)
                    {
                        var _equipment = equipPoint.equipmentReference.equipedObject.GetComponent<vIEquipment>();
                        if (_equipment != null) _equipment.OnUnequip(equipPoint.equipmentReference.item);
                        Destroy(equipPoint.equipmentReference.equipedObject);
                        equipPoint.equipmentReference.item = null;
                    }
                }
                inEquip = false;
                inventory.canEquip = true;
                if (equipPoint != null)
                    CheckTwoHandItem(equipPoint, item);
            }
        }

        void CheckTwoHandItem(EquipPoint equipPoint, vItem item)
        {
            if (item == null) return;
            var opposite = equipPoints.Find(ePoint => ePoint.area != null && ePoint.equipPointName.Equals("LeftArm") && ePoint.area.currentEquipedItem != null);
            if (equipPoint.equipPointName.Equals("LeftArm"))
                opposite = equipPoints.Find(ePoint => ePoint.area != null && ePoint.equipPointName.Equals("RightArm") && ePoint.area.currentEquipedItem != null);
            else if (!equipPoint.equipPointName.Equals("RightArm"))
            {
                return;
            }
            if (opposite != null && (item.twoHandWeapon || opposite.area.currentEquipedItem.twoHandWeapon))
            {
                opposite.area.RemoveCurrentItem();
            }
        }

        IEnumerator UnequipItemRoutine(EquipPoint equipPoint, vItem item)
        {
            if (!inEquip)
            {
                inventory.canEquip = false;
                inEquip = true;
                if (equipPoint != null && equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null)
                {
                    var equipment = equipPoint.equipmentReference.equipedObject.GetComponent<vIEquipment>();
                    if (equipment != null) equipment.OnUnequip(item);
                    if (!inventory.isOpen)
                    {
                        while (equipTimer > 0)
                        {
                            equipTimer -= Time.deltaTime;
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    Destroy(equipPoint.equipmentReference.equipedObject);
                    equipPoint.equipmentReference.item = null;
                }
                inEquip = false;
                inventory.canEquip = true;
            }
        }

        void OnOpenCloseInventory(bool value)
        {
            if(value)           
                animator.SetTrigger("ResetState");
           
            onOpenCloseInventory.Invoke(value);
        }

        public void SetEquipmmentToArea(vItem item, string equipPointName)
        {
            if (inventory)
            {
                var changeEquipmentController = inventory.changeEquipmentControllers.Find(c => c.equipArea != null && c.equipArea.equipPointName.Equals(equipPointName));
                if (changeEquipmentController != null)
                {
                    changeEquipmentController.equipArea.EquiItemToSlot(item);
                }
            }
        }

        public void UpdateInput(vMeleeCombatInput tpInput)
        {
            inventory.lockInput = tpInput.lockInventory;
            tpInput.lockInputByItemManager = inventory.isOpen || inEquip;
        }

        #region Item Collector    

        public virtual void CollectItem(vItemCollection collection)
        {
            foreach (ItemReference reference in collection.items)
            {
                AddItem(reference);
            }
            
            collection.OnCollectItems(gameObject);
        }

        public virtual void CollectItem(vTriggerGenericAction action)
        {
            var collection = action.GetComponentInChildren<vItemCollection>();
            if(collection != null)
            {
                CollectItem(collection);
            }
        }

        #endregion

    }


    [System.Serializable]
    public class ItemReference
    {
        public int id;
        public int amount;
        public ItemReference(int id)
        {
            this.id = id;
        }
        public List<vItemAttribute> attributes;
        public bool changeAttributes;
    }

    [System.Serializable]
    public class EquipPoint
    {
        #region SeralizedProperties in CustomEditor

        [SerializeField]
        public string equipPointName;
        public EquipmentReference equipmentReference = new EquipmentReference();
        [HideInInspector]
        public vEquipArea area;
        public vHandler handler = new vHandler();
        //public Transform defaultPoint;
        //public List<Transform> customPoints = new List<Transform>();
        public OnInstantiateItemObjectEvent onInstantiateEquiment = new OnInstantiateItemObjectEvent();

        #endregion
    }

    public class EquipmentReference
    {
        public GameObject equipedObject;
        public vItem item;
    }

    [System.Serializable]
    public class ApplyAttributeEvent
    {
        [SerializeField]
        public vItemAttributes attribute;
        [SerializeField]
        public OnApplyAttribute onApplyAttribute;
    }

}

