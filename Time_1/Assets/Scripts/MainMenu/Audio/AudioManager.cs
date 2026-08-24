using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }
    public static SFXManager sfxManager { get; private set; }
    public static MusicManager musicManager { get; private set; }

    [SerializeField] AudioMixer mixer;

    public const string MASTER_KEY = "masterVolume";
    public const string MUSIC_KEY = "musicVolume";
    public const string SFX_KEY = "sfxVolume";
    private const float MIN_VOLUME = 0.0001f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        sfxManager = GetComponentInChildren<SFXManager>();
        sfxManager.Initialization();
        musicManager = GetComponentInChildren<MusicManager>();
        musicManager.Initialization();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        ApplyVolumeSoon();
    }

    private void OnDestroy()
    {
        if (instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVolumeSoon();
    }

    private void ApplyVolumeSoon()
    {
        LoadVolume();                
        StartCoroutine(ApplyNextFrame()); 
    }

    private IEnumerator ApplyNextFrame()
    {
        yield return null;
        LoadVolume();
    }

    private void LoadVolume()
    {
        SetMixer(AudioSlider.MIXER_MASTER, PlayerPrefs.GetFloat(MASTER_KEY, 1f));
        SetMixer(AudioSlider.MIXER_MUSIC, PlayerPrefs.GetFloat(MUSIC_KEY, 1f));
        SetMixer(AudioSlider.MIXER_SFX, PlayerPrefs.GetFloat(SFX_KEY, 1f));
    }

    private void SetMixer(string param, float linear)
    {
        mixer.SetFloat(param, Mathf.Log10(Mathf.Max(linear, MIN_VOLUME)) * 20f);
    }
}