using UnityEngine;
using UnityEditor;

public static class MenuItems
{
    /*[MenuItem("Tools/Level Creator/New Level Scene")]
    private static void NewLevel()
    {
        EditorUtils.NewLevel();
    }*/

    [MenuItem("Tools/Level Creator/Show Palette _&p")]
    private static void ShowPalette()
    {
        PaletteWindow.ShowPalette();
    }

}
