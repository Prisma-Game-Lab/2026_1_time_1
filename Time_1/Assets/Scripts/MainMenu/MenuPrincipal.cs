using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelPrincipal;

    [Header("Sliders de Volume")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    private void Start()
    {
        painelOpcoes.SetActive(false);

        float volumeMusica = PlayerPrefs.GetFloat("VolumeMusica", 1f);
        float volumeSFX = PlayerPrefs.GetFloat("VolumeSFX", 1f);

        sliderMusica.SetValueWithoutNotify(volumeMusica);
        sliderSFX.SetValueWithoutNotify(volumeSFX);

        AudioManager.Instance?.AjustaVolumeMusica(volumeMusica);
        AudioManager.Instance?.AjustaVolumeSFX(volumeSFX);
        AudioManager.Instance?.TocaMusica(AudioManager.Instance.MusicaDoMenu);
    }

    public void AoBotaoIniciar()
    {
        SceneManager.LoadScene("ea");
    }
    public void AoBotaoOpcoes()
    {
        painelOpcoes.SetActive(true);
        painelPrincipal.SetActive(false);
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
    public void AoFecharOpcoes()
    {
        painelOpcoes.SetActive(false);
        painelPrincipal.SetActive(true);
    }
    public void AoBotaoSair()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}