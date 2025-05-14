using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager audioManager;
    public static AudioManager instance
    {
        get
        {
            return RequestAudioManager();
        }
    }

    private AudioSource bgmSource;
    private List<AudioSource> sfxSource = new List<AudioSource>();
    private const int SFX_POOL_SIZE = 5;

    void Awake()
    {
        if (audioManager)
        {
            Destroy(gameObject);
        }
        else
        {
            RequestAudioManager();
        }

        bgmSource = transform.Find("BGM_source").GetComponent<AudioSource>();
        
        for(int i = 0; i < SFX_POOL_SIZE; i++) // Creamos el "pool" de SFX Sources
        {
            GameObject sfxObj = new GameObject("SFX_Source_" + (i+1));
            sfxObj.isStatic = true;
            sfxObj.transform.parent = gameObject.transform;
            AudioSource src = sfxObj.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0;
            sfxSource.Add(src);
        }

        bgmSource.loop = true;
    }

    public void PlayBGM(AudioClip clip, bool activateLoop)
    {
        bgmSource.clip = clip;
        bgmSource.loop = activateLoop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        foreach(AudioSource i in sfxSource)
        {
            if (!i.isPlaying)
            {
                i.PlayOneShot(clip);
                return; // Una vez encuentra un AudioSource para reproducirlo termina el metodo
            }
        }
    }

    private static AudioManager RequestAudioManager()
    {
        if (!audioManager)
        {
            audioManager = FindObjectOfType<AudioManager>();
            DontDestroyOnLoad(audioManager);
        }
        return audioManager;
    }
}
