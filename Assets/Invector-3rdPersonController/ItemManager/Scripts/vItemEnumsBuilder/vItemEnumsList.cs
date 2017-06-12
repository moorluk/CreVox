using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vItemEnumsList : ScriptableObject
{
    [SerializeField]
    public List<string> itemTypeEnumValues = new List<string>();
    [SerializeField]
    public List<string> itemAttributesEnumValues = new List<string>();

}
