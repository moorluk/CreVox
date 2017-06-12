using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
[RequireComponent(typeof(AudioSource))]

public class vAudioSurfaceControl : MonoBehaviour
{
    AudioSource source;
    bool isWorking;

    /// <summary>
    /// Play One Shot in Audio Source Component
    /// </summary>
    /// <param name="clip"></param>
    public void PlayOneShot(AudioClip clip)
    {
        if (!source) source = GetComponent<AudioSource>();
        source.PlayOneShot(clip);
        isWorking = true;
    }
    void Update()
    {
        if (isWorking && !source.isPlaying)
        {
            Destroy(gameObject);
        }
    }
    public AudioMixerGroup outputAudioMixerGroup
    {
        set
        {
            if (!source) source = GetComponent<AudioSource>();
            source.outputAudioMixerGroup = value;
        }
    }
}
