using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class BossMusic : MonoBehaviour
{
    [Header("Música do boss")]
    [Tooltip("Arraste o AudioClip da música desta cena de boss.")]
    [SerializeField] private AudioClip musica;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool tocarNoStart = true;

    [Header("Mixer (opcional, mas recomendado)")]
    [Tooltip("Grupo 'Music' do AudioMixer, pra o volume seguir os sliders. Deixe vazio para tocar direto.")]
    [SerializeField] private AudioMixerGroup grupoMusica;

    [Header("Integração")]
    [Tooltip("Silencia a música do MusicManager antes de tocar .")]
    [SerializeField] private bool pararMusicaGlobal = true;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.clip = musica;
        source.volume = volume;
        if (grupoMusica != null) source.outputAudioMixerGroup = grupoMusica;
    }

    private void Start()
    {
        if (tocarNoStart) Tocar();
    }

    public void Tocar()
    {
        if (musica == null)
        {
            Debug.LogWarning("[BossMusic] 'musica' não atribuída no Inspector.", this);
            return;
        }

        if (pararMusicaGlobal) MusicManager.StartFadeOut();

        source.clip = musica;
        source.loop = loop;
        source.volume = volume;
        source.Play();
    }

    public void Parar()
    {
        if (source != null) source.Stop();
    }
}