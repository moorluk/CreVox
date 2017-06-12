using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(vButtomAttribute))]
public class SingletonEditor : DecoratorDrawer
{
    public override void OnGUI(Rect position)
    {
        vButtomAttribute target = (vButtomAttribute)attribute;

        if (target != null)
        {
            Rect rect = position;
            rect.height = 20;
            if (GUI.Button(rect, target.label))
            {
                ExecuteFunction(target);
            }
        }
    }

    public override float GetHeight()
    {
        return 30f;
    }

    void ExecuteFunction(vButtomAttribute target)
    {
        MonoBehaviour[] sceneActive = Selection.activeGameObject.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour mono in sceneActive)
        {
            Type monoType = mono.GetType();

            // Retreive the fields from the mono instance
            MethodInfo method = monoType.GetMethod(target.function, BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo[] objectFields = monoType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < objectFields.Length; i++)
            {
                vButtomAttribute _attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(vButtomAttribute)) as vButtomAttribute;
                if (_attribute != null && _attribute.id == target.id && _attribute.function == target.function)
                {
                    method.Invoke(mono, null);
                }

            }
        }
    }
}
