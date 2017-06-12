using UnityEngine;
using System.Collections;
using Invector.ItemManager;

public class vLeaveDropItemExample : MonoBehaviour
{
    vItemManager itemManager;
    private Rect windowRect;
    private Vector2 scroll;

    void OnGUI()
    {
        windowRect = GUILayout.Window(0, windowRect, vLeaveDropItensWindow, "Leave and Drop Items test by Invector:");
    }

    private void vLeaveDropItensWindow(int windowID)
    {
        GUILayout.BeginVertical();
        itemManager = FindObjectOfType<vItemManager>();

        if (itemManager)
        {
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(300), GUILayout.MinHeight(300));
            for (int i = 0; i < itemManager.items.Count; i++)
            {
                GUILayout.BeginHorizontal("box");

                GUILayout.Label(new GUIContent("Name:" + itemManager.items[i].name + "\nAmount :" + itemManager.items[i].amount.ToString()), GUILayout.Width(200), GUILayout.Height(40));
                GUILayout.BeginVertical("box");
                if (GUILayout.Button("Leave"))
                {
                    itemManager.inventory.isOpen = true;
                    itemManager.inventory.OnLeaveItem(itemManager.items[i], 1);
                    itemManager.inventory.isOpen = false;
                    break;
                }
                if (GUILayout.Button("Drop"))
                {
                    itemManager.inventory.isOpen = true;
                    itemManager.inventory.OnDropItem(itemManager.items[i], 1);
                    itemManager.inventory.isOpen = false;
                    break;
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
        GUI.DragWindow();
    }
}
