using UnityEngine;
using System.Collections;
using System;
using Invector;
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class vClassHeaderAttribute: Attribute
{
    public string header;
    public bool openClose;
    public string iconName;
    public bool useHelpBox;
    public string helpBoxText;
      
    public vClassHeaderAttribute(string header,bool openClose = true,string iconName= "icon_v2", bool useHelpBox = false,string helpBoxText="") 
    {
        this.header = header;
        this.openClose = openClose;
        this.iconName = iconName;
        this.useHelpBox = useHelpBox;
        this.helpBoxText = helpBoxText;
    }

    public vClassHeaderAttribute(string header, string helpBoxText)
    {
        this.header = header;
        this.openClose = true;
        this.iconName = "icon_v2";
        this.useHelpBox = true;
        this.helpBoxText = helpBoxText;
    }
}
//[AttributeUsage(AttributeTargets.Property,AllowMultiple =false,Inherited =true)]
//public sealed class vToolBarStartAttribute:PropertyAttribute
//{
//    public string header;
//    public bool useIcon;
//    public Texture2D icon;
//    public vToolBarStartAttribute(string header,string icon ="")
//    {
//        this.header = header;
//        if(string.IsNullOrEmpty(icon))
//        {
//            useIcon = false;            
//        }
//        else
//        {
//            this.icon = Resources.Load(icon) as Texture2D;
//        }
//    }
    
//}
//public sealed class vToolBarEndAttribute : PropertyAttribute
//{

//}