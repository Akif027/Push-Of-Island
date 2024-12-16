using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; } // Singleton instance

    public AudioSource musicSource; // AudioSource for background music
    public AudioSource sfxSource; // AudioSource for sound effects

    // Sound Effect Clips
    public AudioClip BgPushIsland;
    public AudioClip CoinToss;
    public AudioClip CoinCollect;
    public AudioClip Score;
    public AudioClip CharacterChoose;
    public AudioClip Collision;
    public AudioClip DiscardFirstCard;
    public AudioClip TurnChange;
    public AudioClip TokenLeaving;
    public AudioClip PanelChanging;
    public AudioClip EndScreenOpen;
    public AudioClip ButtonTap;
    public AudioClip NotPossiblePlacement;

    private const string MUSIC_PREF = "MusicSetting"; // Key for saving music setting
    private const string SFX_PREF = "SFXSetting"; // Key for saving SFX setting

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved preferences
            LoadSoundSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    // Play background music
    public void PlayBackgroundMusic()
    {
        if (IsMusicOn() && BgPushIsland != null && musicSource != null)
        {
            musicSource.clip = BgPushIsland;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // Play individual sound effects
    public void PlayCoinToss() { PlaySFX(CoinToss); }
    public void PlayCoinCollect() { PlaySFX(CoinCollect); }
    public void PlayScore() { PlaySFX(Score); }
    public void PlayCharacterChoose() { PlaySFX(CharacterChoose); }
    public void PlayCollision() { PlaySFX(Collision); }
    public void PlayDiscardFirstCard() { PlaySFX(DiscardFirstCard); }
    public void PlayTurnChange() { PlaySFX(TurnChange); }
    public void PlayTokenLeaving() { PlaySFX(TokenLeaving); }
    public void PlayPanelChanging() { PlaySFX(PanelChanging); }
    public void PlayEndScreenOpen() { PlaySFX(EndScreenOpen); }
    public void PlayButtonTap() { PlaySFX(ButtonTap); }
    public void PlayNotPossiblePlacement() { PlaySFX(NotPossiblePlacement); }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null && IsSFXOn())
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Methods to toggle music and sound
    public void SetMusic(bool isOn)
    {
        PlayerPrefs.SetInt(MUSIC_PREF, isOn ? 1 : 0);
        musicSource.mute = !isOn;
    }

    public void SetSFX(bool isOn)
    {
        PlayerPrefs.SetInt(SFX_PREF, isOn ? 1 : 0);
        sfxSource.mute = !isOn;
    }

    public bool IsMusicOn()
    {
        return PlayerPrefs.GetInt(MUSIC_PREF, 1) == 1; // Default to music on
    }

    public bool IsSFXOn()
    {
        return PlayerPrefs.GetInt(SFX_PREF, 1) == 1; // Default to SFX on
    }

    private void LoadSoundSettings()
    {
        // Load settings when game starts
        musicSource.mute = !IsMusicOn();
        sfxSource.mute = !IsSFXOn();
    }
}
