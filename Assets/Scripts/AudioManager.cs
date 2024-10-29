using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip mainMenuBackgroundMusic;
    [SerializeField] private AudioClip[] levelBackgroundMusic;
    [SerializeField] private AudioSource SFXAudioSource;
    public AudioClip clickSFX, swapSFX, matchSFX, superMatchSFX, dropSFX, winSFX, loseSFX, selectSFX;
    public static AudioManager Instance { get; private set; }

    private AudioSource backgroundAudioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        backgroundAudioSource = GetComponent<AudioSource>();
    }

    public void PlayMainMenuMusic()
    {
        backgroundAudioSource.clip = mainMenuBackgroundMusic;
        backgroundAudioSource.Play();
    }

    public void PlayLevelMusic()
    {
        int randomIndex = Random.Range(0, levelBackgroundMusic.Length);
        backgroundAudioSource.clip = levelBackgroundMusic[randomIndex];
        backgroundAudioSource.Play();
    }

    public void PlaySFX(AudioClip sfx)
    {
        if (sfx != null)
            SFXAudioSource.PlayOneShot(sfx);
    }

    public void StopMusic() => backgroundAudioSource.Stop();
}