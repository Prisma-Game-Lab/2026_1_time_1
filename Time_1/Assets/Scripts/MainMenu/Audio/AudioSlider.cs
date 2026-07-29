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
    void Awake()
    {
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    void Start()
    {
        if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat(AudioManager.MASTER_KEY, 1f);
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat(AudioManager.MUSIC_KEY, 1f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat(AudioManager.SFX_KEY, 1f);
    }
    void OnDisable()
    {
        if (masterSlider != null) PlayerPrefs.SetFloat(AudioManager.MASTER_KEY, masterSlider.value);
        if (musicSlider != null) PlayerPrefs.SetFloat(AudioManager.MUSIC_KEY, musicSlider.value);
        if (sfxSlider != null) PlayerPrefs.SetFloat(AudioManager.SFX_KEY, sfxSlider.value);
    }
    void SetMasterVolume(float value)
    {
        if (mixer != null) mixer.SetFloat(MIXER_MASTER, Mathf.Log10(Mathf.Max(0.0001f, value)) * 20);
    }
    void SetMusicVolume(float value)
    {
        if (mixer != null) mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(Mathf.Max(0.0001f, value)) * 20);
    }
    void SetSFXVolume(float value)
    {
        if (mixer != null) mixer.SetFloat(MIXER_SFX, Mathf.Log10(Mathf.Max(0.0001f, value)) * 20);
    }
}