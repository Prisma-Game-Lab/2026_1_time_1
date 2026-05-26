using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
// Musica
    public void TocaMusica(AudioClip clip, bool loop = true)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }
    public void ParaMusica()
    {
        musicSource.Stop();
    }
    public void AjustaVolumeMusica(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }
// Efeitos Sonoros
    public void TocaSFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
    public void AjustaVolumeSFX(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }
}