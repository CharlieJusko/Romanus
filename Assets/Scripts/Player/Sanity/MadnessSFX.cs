using System.Collections;
using UnityEngine;



[RequireComponent(typeof(AudioSource))]
public class MadnessSFX : MadnessEffect
{
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public bool random = true;
    [Range(0f, 1f)]
    public float targetVolume;
    public float delayBetweenClip = 0.25f;
    public float fadeInOutTime = 1f;

    private float delayClipTimer = 0f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
    }

    private void Update()
    {
        ControlVolume();
      
        if(active && delayClipTimer >= delayBetweenClip && !audioSource.isPlaying)
            NextClip();

        DelayBetweenClipTimer();

    }

    public override void Activate()
    {
        active = true;
        audioSource.volume = 0f;
    }

    public override void Deactivate()
    {
        active = false;
    }

    void NextClip()
    {
        var index = Random.Range(0, audioClips.Length);
        audioSource.clip = audioClips[index];
        audioSource.Play();
        delayClipTimer = 0f;
        //AudioSource.PlayClipAtPoint(audioClips[index], transform.position);
    }

    void ControlVolume()
    {
        if(active)
        {
            if(audioSource.volume < targetVolume)
                audioSource.volume += (Time.deltaTime / fadeInOutTime);
            else 
                audioSource.volume = targetVolume;
        } 
        else
        {
            if(audioSource.volume > 0f)
                audioSource.volume -= (Time.deltaTime / fadeInOutTime);
            else
                audioSource.volume = 0f;
        }
    }

    void DelayBetweenClipTimer()
    {
        if(delayClipTimer < delayBetweenClip)
            delayClipTimer += Time.deltaTime;
        else
            delayClipTimer = delayBetweenClip;
    }
}
