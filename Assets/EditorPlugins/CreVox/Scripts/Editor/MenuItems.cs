using UnityEngine;
using UnityEditor;

namespace CreVox
{

    public static class MenuItems
    {

        [MenuItem ("Tools/CreVox/Show Palette _&p")]
        static void ShowPalette ()
        {
            PaletteWindow.ShowPalette ();
        }

        [MenuItem ("Tools/CreVox/ArtPack Check")]
        static void ShowArtPack ()
        {
            ArtPackWindow.ShowPalette ();
        }

        [MenuItem ("GameObject/3D Object/Volume (CreVox)")]
        static void AddVolume ()
        {
            GameObject _parent = Selection.activeGameObject;
            if (_parent == null)
                _parent = AddVolumeManager ();
            else if (_parent.GetComponent<VolumeManager> () == null)
                _parent = AddVolumeManager ();
            GameObject newVol = new GameObject ("New Volume", new []{ typeof(Volume) });
            newVol.transform.parent = _parent.transform;
        }

        [MenuItem ("GameObject/3D Object/Volume Manager (CreVox)")]
        static GameObject AddVolumeManager ()
        {
            GameObject newVol = new GameObject ("VolumeManager", new []{ typeof(VolumeManager) });
            return newVol;
        }

    }
}