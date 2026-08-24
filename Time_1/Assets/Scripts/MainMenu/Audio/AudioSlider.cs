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

    public const string MIXER_MASTER = "MasterVolume";
    public const string MIXER_MUSIC = "MusicVolume";
    public const string MIXER_SFX = "SFXVolume";
    private const float MIN_VOLUME = 0.0001f;

    void Awake()
    {
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }
    void OnEnable()
    {
        LoadInto(masterSlider, AudioManager.MASTER_KEY, MIXER_MASTER);
        LoadInto(musicSlider, AudioManager.MUSIC_KEY, MIXER_MUSIC);
        LoadInto(sfxSlider, AudioManager.SFX_KEY, MIXER_SFX);
    }

    void LoadInto(Slider slider, string prefKey, string mixerParam)
    {
        float v = PlayerPrefs.GetFloat(prefKey, 1f);
        slider.SetValueWithoutNotify(v);
        mixer.SetFloat(mixerParam, VolumeToDecibels(v));
    }
    void SetMasterVolume(float value) => Apply(AudioManager.MASTER_KEY, MIXER_MASTER, value);
    void SetMusicVolume(float value) => Apply(AudioManager.MUSIC_KEY, MIXER_MUSIC, value);
    void SetSFXVolume(float value) => Apply(AudioManager.SFX_KEY, MIXER_SFX, value);

    void Apply(string prefKey, string mixerParam, float value)
    {
        mixer.SetFloat(mixerParam, VolumeToDecibels(value));
        PlayerPrefs.SetFloat(prefKey, value);
    }
    void OnDisable() => PlayerPrefs.Save();
    void OnApplicationQuit() => PlayerPrefs.Save();
    void OnApplicationPause(bool paused) { if (paused) PlayerPrefs.Save(); }
    private float VolumeToDecibels(float value)
    {
        return Mathf.Log10(Mathf.Max(value, MIN_VOLUME)) * 20f;
    }
}