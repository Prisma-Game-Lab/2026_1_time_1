using UnityEngine;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Elementos da UI")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    private void Start()
    {
        float volumeMusica = PlayerPrefs.GetFloat("VolumeMusica", 1f);
        float volumeSFX = PlayerPrefs.GetFloat("VolumeSFX", 1f);
        if (sliderMusica != null)
        {
            sliderMusica.value = volumeMusica;
            AudioManager.Instance?.AjustaVolumeMusica(volumeMusica);
        }
        if (sliderSFX != null)
        {
            sliderSFX.value = volumeSFX;
            AudioManager.Instance?.AjustaVolumeSFX(volumeSFX);
        }
        AudioManager.Instance?.TocaMusica(AudioManager.Instance.MusicaDoMenu);
    }
    public void AoBotaoJogar()
    {
        AudioManager.Instance?.TocaSFX(AudioManager.Instance.EfeitoDoBoss);
        GameManager.Instance?.IniciarJogo();
    }
    public void AoMudarVolumeMusica(float value)
    {
        AudioManager.Instance?.AjustaVolumeMusica(value);
        PlayerPrefs.SetFloat("VolumeMusica", value);
    }
    public void AoMudarVolumeSFX(float value)
    {
        AudioManager.Instance?.AjustaVolumeSFX(value);
        PlayerPrefs.SetFloat("VolumeSFX", value);
    }
    public void AoBotaoSair()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}