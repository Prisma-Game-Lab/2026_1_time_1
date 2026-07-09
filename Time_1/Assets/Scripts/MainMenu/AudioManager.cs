using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("AudioManager");
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab);
                    go.name = "AudioManager";
                }
                else
                {
                    Debug.LogError("[AudioManager] Nenhuma instância em cena e nenhum prefab em Resources/AudioManager. Crie o prefab (veja instruções).");
                }
            }
            return _instance;
        }
    }

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Musica de Fundo")]
    public AudioClip MusicaDoMenu;
    public AudioClip MusicaDoJogo;

    [Header("Musica do Boss")]
    public AudioClip MusicaDoBoss;

    [Header("Efeitos Sonoros")]
    public AudioClip EfeitoDaLanca;
    public AudioClip EfeitoDePulo;
    public AudioClip EfeitoDeCura;
    public AudioClip EfeitoDeParry;

    [Header("Pitch Shifting")]
    [SerializeField] private float pitchMin = 0.92f;
    [SerializeField] private float pitchMax = 1.08f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        GarantirAudioSources();
        CarregarVolumes();
    }
    private void GarantirAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = CriarAudioSource("MusicSource_Auto", loop: true);
        }
        if (sfxSource == null)
        {
            sfxSource = CriarAudioSource("SFXSource_Auto", loop: false);
        }

        if (musicGroup != null) musicSource.outputAudioMixerGroup = musicGroup;
        if (sfxGroup != null) sfxSource.outputAudioMixerGroup = sfxGroup;
    }
    private AudioSource CriarAudioSource(string nomeObjeto, bool loop)
    {
        GameObject go = new GameObject(nomeObjeto);
        go.transform.SetParent(transform);
        AudioSource src = go.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = false;
        return src;
    }
    private void CarregarVolumes()
    {
        float volumeMusica = PlayerPrefs.GetFloat("VolumeMusica", 1f);
        float volumeSFX = PlayerPrefs.GetFloat("VolumeSFX", 1f);
        AjustaVolumeMusica(volumeMusica);
        AjustaVolumeSFX(volumeSFX);
    }
    // Musica
    public void TocaMusica(AudioClip clip, bool loop = true)
    {
        if (musicSource == null) return;
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }
    public void ParaMusica()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }
    public void AjustaVolumeMusica(float volume)
    {
        if (mixer == null) return;
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        mixer.SetFloat("VolumeMusica", dB);
    }
    // Efeitos Sonoros
    public void TocaSFX(AudioClip clip)
    {
        if (sfxSource == null) return;
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
    // SFX com pitch aleatório
    public void TocaSFXComPitch(AudioClip clip, float? pitchMinOverride = null, float? pitchMaxOverride = null)
    {
        if (clip == null) return;
        float min = pitchMinOverride ?? pitchMin;
        float max = pitchMaxOverride ?? pitchMax;
        GameObject temp = new GameObject("SFX_Pitched");
        temp.transform.SetParent(transform);
        AudioSource src = temp.AddComponent<AudioSource>();
        src.clip = clip;
        src.outputAudioMixerGroup = sfxGroup;
        src.volume = 1f;
        src.pitch = Random.Range(min, max);
        src.playOnAwake = false;
        src.Play();
        Destroy(temp, clip.length / src.pitch);
    }
    public void AjustaVolumeSFX(float volume)
    {
        if (mixer == null) return;
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        mixer.SetFloat("VolumeSFX", dB);
    }
}