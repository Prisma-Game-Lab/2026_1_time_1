using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MenuPausa : MonoBehaviour
{
    [Header("Paineis")]
    [SerializeField] private GameObject painelPausa;
    [SerializeField] private GameObject painelOpcoes;

    [Header("Sliders de Volume")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Cena do Menu Principal")]
    [SerializeField] private string cenaMenuPrincipal = "MainMenu";

    private bool pausado;

    private void Start()
    {
        if (painelPausa != null) painelPausa.SetActive(false);
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
        Time.timeScale = 1f;
        pausado = false;

        if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(PlayerPrefs.GetFloat(AudioManager.MUSIC_KEY, 1f));
        if (sliderSFX != null) sliderSFX.SetValueWithoutNotify(PlayerPrefs.GetFloat(AudioManager.SFX_KEY, 1f));
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Alternar();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed) Alternar();
    }

    public void Alternar()
    {
        if (pausado) Continuar();
        else Pausar();
    }

    public void Pausar()
    {
        pausado = true;
        Time.timeScale = 0f;
        if (painelPausa != null) painelPausa.SetActive(true);
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
    }

    public void Continuar()
    {
        pausado = false;
        Time.timeScale = 1f;
        if (painelPausa != null) painelPausa.SetActive(false);
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
    }

    public void AbrirOpcoes()
    {
        if (painelOpcoes != null) painelOpcoes.SetActive(true);
        if (painelPausa != null) painelPausa.SetActive(false);
    }
    public void FecharOpcoes()
    {
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
        if (painelPausa != null) painelPausa.SetActive(true);
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

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaMenuPrincipal);
    }
    public void Sair()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}