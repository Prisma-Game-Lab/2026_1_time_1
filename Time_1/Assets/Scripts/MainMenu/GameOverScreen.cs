using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("Cena do Menu Principal")]
    [SerializeField] private string cenaMenuPrincipal = "MainMenu";

    private void OnEnable()
    {
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            Reiniciar();
    }
    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Desistir()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.IrParaMenu();
        }
        else
        {
            MusicManager.PlayMusic("menu");
            SceneManager.LoadScene(cenaMenuPrincipal);
        }
    }
}