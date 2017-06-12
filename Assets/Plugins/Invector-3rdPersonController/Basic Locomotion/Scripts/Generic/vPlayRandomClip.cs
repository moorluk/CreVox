using UnityEngine;
using System.Collections;
[RequireComponent(typeof(AudioSource))]
public class vPlayRandomClip : MonoBehaviour {

    public AudioClip[] clips;
    public AudioSource audioSource;
    #if !UNITY_5_4_OR_NEWER
    protected System.Random random;
    #endif
    void Start () {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if(audioSource)
        {
            var index = 0;
            #if UNITY_5_4_OR_NEWER
            Random.InitState(Random.Range(0, System.DateTime.Now.Millisecond));
            index = Random.Range(0, clips.Length - 1);
            #else
            random = new System.Random(Random.Range(0, System.DateTime.Now.Millisecond));
            index = random.Next(0, clips.Length - 1);
            #endif
            if (clips.Length > 0)
                audioSource.PlayOneShot(clips[index]);
        }
	}
}
