using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BossCutscene : MonoBehaviour
{
    [Header("Identidade")]
    [Tooltip("ID unico desta cutscene.")]
    [SerializeField] private string cutsceneId = "Coalemos";

    [Header("Referencias")]
    [Tooltip("Painel/tela que aparece durante a cutscene.")]
    [SerializeField] private GameObject painel;
    [Tooltip("VideoPlayer da cutscene. Se vazio, a cutscene 'termina' na hora.")]
    [SerializeField] private VideoPlayer video;

    [Header("Segurar durante a cutscene")]
    [Tooltip("Componentes DESLIGADOS durante a cutscene e religados no fim. Arraste o CoalemosAI aqui.")]
    [SerializeField] private MonoBehaviour[] pausarDurante;

    [Header("Pular")]
    [SerializeField] private bool permitirPular = true;

    [Header("Eventos")]
    [Tooltip("Disparado quando a cutscene comeca a tocar.")]
    [SerializeField] private UnityEvent aoIniciar;
    [Tooltip("Disparado quando a cutscene termina e E TAMBeM quando e pulada por ja ter sido vista. " +
             "Ligue aqui o BossMusic.Tocar() pra musica comecar so depois da cutscene.")]
    [SerializeField] private UnityEvent aoTerminar;

    private bool vaiTocar;
    private bool tocando;

    private void Awake()
    {
        if (painel != null) painel.SetActive(false);

        vaiTocar = !CutsceneManager.Instance.JaViu(cutsceneId);
        if (vaiTocar) SetPausado(true);
    }

    private void OnEnable()
    {
        if (video != null) video.loopPointReached += AoFimDoVideo;
    }
    private void OnDisable()
    {
        if (video != null) video.loopPointReached -= AoFimDoVideo;
    }
    private void Start()
    {
        if (!vaiTocar)
        {
            aoTerminar?.Invoke();
            return;
        }
        Tocar();
    }
    private void Tocar()
    {
        tocando = true;
        CutsceneManager.Instance.MarcarVisto(cutsceneId);

        if (painel != null) painel.SetActive(true);
        MusicManager.StartFadeOut();
        aoIniciar?.Invoke();

        if (video != null) { video.Stop(); video.Play(); }
        else Terminar(); 
    }

    private void Update()
    {
        if (!tocando || !permitirPular) return;
        if (Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.enterKey.wasPressedThisFrame)
            Pular();
    }

    public void Pular()
    {
        if (!tocando) return;
        if (video != null) video.Stop();
        Terminar();
    }
    private void AoFimDoVideo(VideoPlayer vp) => Terminar();
    private void Terminar()
    {
        if (!tocando) return;
        tocando = false;

        if (painel != null) painel.SetActive(false);
        SetPausado(false);
        aoTerminar?.Invoke();
    }
    private void SetPausado(bool pausado)
    {
        if (pausarDurante == null) return;
        foreach (MonoBehaviour mb in pausarDurante)
            if (mb != null) mb.enabled = !pausado;
    }
}