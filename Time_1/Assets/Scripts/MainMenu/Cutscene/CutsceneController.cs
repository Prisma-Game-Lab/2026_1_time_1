using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
public class CutsceneController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject painelCutscene;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Cena de Destino")]
    [SerializeField] private string cenaDestino = "ea";

    private bool cenaJaCarregada = false;

    private void Awake()
    {
        if (painelCutscene != null)
            painelCutscene.SetActive(false);
    }
    private void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += AoFimDoVideo;
    }
    private void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= AoFimDoVideo;
    }
    public void IniciarCutscene()
    {
        cenaJaCarregada = false;

        if (painelCutscene != null)
            painelCutscene.SetActive(true);

        MusicManager.StartFadeOut();

        if (videoPlayer != null)
            StartCoroutine(PrepareAndPlay());
        else
            CarregarCenaDestino();
    }

    private IEnumerator PrepareAndPlay()
    {
        videoPlayer.Stop();
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);
        videoPlayer.Play();
    }

    public void PularCutscene()
    {
        StopAllCoroutines();
        if (videoPlayer != null)
            videoPlayer.Stop();

        CarregarCenaDestino();
    }
    private void AoFimDoVideo(VideoPlayer vp)
    {
        CarregarCenaDestino();
    }
    private void CarregarCenaDestino()
    {
        if (cenaJaCarregada) return;
        cenaJaCarregada = true;

        SceneManager.LoadScene(cenaDestino);
    }
}