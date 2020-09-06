using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISoundsController : MonoBehaviour
{
    AudioSource uiAudioSource;
    public AudioClip hideUISound;
    public AudioClip goNextSceneSound;

    // Start is called before the first frame update
    void Start()
    {
        uiAudioSource = GetComponent<AudioSource>();
    }

    public void PlayHideUISound()
    {
        uiAudioSource.clip = hideUISound;
        uiAudioSource.Play();
    
    }

    public void PlayGoNextSceneSound()
    {
        uiAudioSource.clip = goNextSceneSound;
        uiAudioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
