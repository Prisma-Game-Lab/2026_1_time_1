using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [HideInInspector] public const string MIXER_MASTER = "MasterVolume";
    [HideInInspector] public const string MIXER_MUSIC = "MusicVolume";
    [HideInInspector] public const string MIXER_SFX = "SFXVolume";
    private const float MIN_VOLUME = 0.0001f;

    void Awake()
    {
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    void Start()
    {
        masterSlider.value = PlayerPrefs.GetFloat(AudioManager.MASTER_KEY, 1f);
        musicSlider.value = PlayerPrefs.GetFloat(AudioManager.MUSIC_KEY, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(AudioManager.SFX_KEY, 1f);
    }
    void OnDisable()
    {
        PlayerPrefs.SetFloat(AudioManager.MASTER_KEY, masterSlider.value);
        PlayerPrefs.SetFloat(AudioManager.MUSIC_KEY, musicSlider.value);
        PlayerPrefs.SetFloat(AudioManager.SFX_KEY, sfxSlider.value);
    }
    void SetMasterVolume(float value)
    {
        mixer.SetFloat(MIXER_MASTER, VolumeToDecibels(value));
    }
    void SetMusicVolume(float value)
    {
        mixer.SetFloat(MIXER_MUSIC, VolumeToDecibels(value));
    }
    void SetSFXVolume(float value)
    {
        mixer.SetFloat(MIXER_SFX, VolumeToDecibels(value));
    }
    private float VolumeToDecibels(float value)
    {
        return Mathf.Log10(Mathf.Max(value, MIN_VOLUME)) * 20f;
    }
}