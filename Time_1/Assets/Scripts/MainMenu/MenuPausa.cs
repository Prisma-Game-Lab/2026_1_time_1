using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PainelOpcoes : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject painelOpcoes;

    [Header("Sliders")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    private void Start()
    {
        painelOpcoes.SetActive(false);
        sliderMusica.SetValueWithoutNotify(PlayerPrefs.GetFloat("VolumeMusica", 1f));
        sliderSFX.SetValueWithoutNotify(PlayerPrefs.GetFloat("VolumeSFX", 1f));
    }

    public void AbrirOpcoes()
    {
        painelOpcoes.SetActive(true);
        Time.timeScale = 0f;
    }
    public void FecharOpcoes()
    {
        painelOpcoes.SetActive(false);
        Time.timeScale = 1f;
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
}