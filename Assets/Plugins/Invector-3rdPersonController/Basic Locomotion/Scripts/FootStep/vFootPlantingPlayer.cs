using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public abstract class vFootPlantingPlayer : MonoBehaviour
{
    // The different surfaces and their sounds.
    public vAudioSurface defaultSurface;
    public List<vAudioSurface> customSurfaces;

    public void PlayFootFallSound(FootStepObject footStepObject)
    {
        for (int i = 0; i < customSurfaces.Count; i++)
            if (customSurfaces[i] != null && ContainsTexture(footStepObject.name, customSurfaces[i]))
            {
                customSurfaces[i].PlayRandomClip(footStepObject);
                return;
            }
        if (defaultSurface != null)
            defaultSurface.PlayRandomClip(footStepObject);
    }

    // check if AudioSurface Contains texture in TextureName List
    private bool ContainsTexture(string name, vAudioSurface surface)
    {
        for (int i = 0; i < surface.TextureOrMaterialNames.Count; i++)          
            if (name.Contains(surface.TextureOrMaterialNames[i]))
                return true;

        return false;
    }
}

