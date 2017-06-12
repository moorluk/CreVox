using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Invector.ItemManager
{
    [System.Serializable]
    public class vItemAttribute
    {
        public vItemAttributes name = 0;        
        public int value;
        public bool isBool;
        public vItemAttribute(vItemAttributes name, int value)
        {
            this.name = name;
            this.value = value;
        }
    }

    public static class vItemAttributeHelper
    {
        public static bool Contains(this List<vItemAttribute> attributes, vItemAttributes name)
        {
            var attribute = attributes.Find(at => at.name == name);
            return attribute != null;
        }
        public static vItemAttribute GetAttributeByType(this List<vItemAttribute> attributes, vItemAttributes name)
        {
            var attribute = attributes.Find(at => at.name == name);
            return attribute;
        }
        public static bool Equals(this vItemAttribute attributeA, vItemAttribute attributeB)
        {
            return attributeA.name == attributeB.name;
        }

        public static List<vItemAttribute> CopyAsNew(this List<vItemAttribute> copy)
        {
            var target = new List<vItemAttribute>();

            if (copy != null)
            {
                for (int i = 0; i < copy.Count; i++)
                {
                    vItemAttribute attribute = new vItemAttribute(copy[i].name, copy[i].value);                  
                    target.Add(attribute);
                }
            }
            return target;
        }
    }

}
