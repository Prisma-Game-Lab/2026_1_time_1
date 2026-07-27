using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
public class MenuPrincipal : MonoBehaviour
{
    [Header("Paineis")]
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelPrincipal;

    [Header("Sliders de Volume")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Cutscene")]
    [SerializeField] private CutsceneController cutsceneController;

    private void Start()
    {
        painelOpcoes.SetActive(false);

        float volumeMusica = PlayerPrefs.GetFloat(AudioManager.MUSIC_KEY, 1f);
        float volumeSFX = PlayerPrefs.GetFloat(AudioManager.SFX_KEY, 1f);

        sliderMusica.SetValueWithoutNotify(volumeMusica);
        sliderSFX.SetValueWithoutNotify(volumeSFX);

        if (mixer != null)
        {
            mixer.SetFloat(AudioSlider.MIXER_MUSIC, Mathf.Log10(Mathf.Max(0.0001f, volumeMusica)) * 20);
            mixer.SetFloat(AudioSlider.MIXER_SFX, Mathf.Log10(Mathf.Max(0.0001f, volumeSFX)) * 20);
        }
        MusicManager.PlayMusic("menu");
    }
    public void AoBotaoIniciar()
    {
        if (cutsceneController != null)
            cutsceneController.IniciarCutscene();
        else
            SceneManager.LoadScene("AreaInicial");
    }
    public void AoBotaoOpcoes()
    {
        painelOpcoes.SetActive(true);
        painelPrincipal.SetActive(false);
    }
    public void AoMudarVolumeMusica(float value)
    {
        if (mixer != null) mixer.SetFloat(AudioSlider.MIXER_MUSIC, Mathf.Log10(Mathf.Max(0.0001f, value)) * 20);
        PlayerPrefs.SetFloat(AudioManager.MUSIC_KEY, value);
    }
    public void AoMudarVolumeSFX(float value)
    {
        if (mixer != null) mixer.SetFloat(AudioSlider.MIXER_SFX, Mathf.Log10(Mathf.Max(0.0001f, value)) * 20);
        PlayerPrefs.SetFloat(AudioManager.SFX_KEY, value);
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