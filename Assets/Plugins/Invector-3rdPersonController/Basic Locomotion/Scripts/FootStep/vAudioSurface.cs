using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class vAudioSurface : ScriptableObject
{
    public AudioSource audioSource;  
    public AudioMixerGroup audioMixerGroup;                 // The AudioSource that will play the clips.   
    public List<string> TextureOrMaterialNames;             // The tag on the surfaces that play these sounds.
    public List<AudioClip> audioClips;                      // The different clips that can be played on this surface.    
    public GameObject particleObject;
    
    private vFisherYatesRandom randomSource = new vFisherYatesRandom();       // For randomly reordering clips.   
    public vAudioSurface()
    {
        audioClips = new List<AudioClip>();
        TextureOrMaterialNames = new List<string>();
    }

    public void PlayRandomClip(FootStepObject footStepObject)
    {
        // If there are no clips to play return.
        if (audioClips == null || audioClips.Count == 0)
            return;

        // initialize variable if not already started
        if (randomSource == null)
            randomSource = new vFisherYatesRandom();
        // Find a random clip and play it.
        GameObject audioObject = null;
        if (audioSource != null)
            audioObject = Instantiate(audioSource.gameObject, footStepObject.sender.position,Quaternion.identity) as GameObject;
        else
        {
            audioObject = new GameObject("audioObject");
            audioObject.transform.position = footStepObject.sender.position;
        }
           
        var source = audioObject.AddComponent<vAudioSurfaceControl>();
        if (audioMixerGroup != null)
        {
            source.outputAudioMixerGroup = audioMixerGroup;
        }
        int index = randomSource.Next(audioClips.Count);
        if (particleObject)
        {
            var particle = Instantiate(particleObject, footStepObject.sender.position, footStepObject.sender.rotation) as GameObject;
            
            particle.SendMessage("StepMark", footStepObject, SendMessageOptions.DontRequireReceiver);
        }
        source.PlayOneShot(audioClips[index]);

    }
}