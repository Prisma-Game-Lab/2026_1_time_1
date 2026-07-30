using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PortaCenaInteragivel : Interactable
{
    [Header("Cena de Destino")]
    [Tooltip("Nome exato da cena a carregar (precisa estar nas Build Settings)")]
    [SerializeField] private string nomeDaCena;

    [Tooltip("Se marcado, ignora o nome acima e carrega a proxima cena pelo indice das Build Settings")]
    [SerializeField] private bool usarProximaCenaNoBuild = false;

    private bool jaAtivou = false;
    public override bool PodeInteragir() => !jaAtivou;

    public override void Interagir()
    {
        if (jaAtivou) return;
        jaAtivou = true;
        CarregarCena();
    }
    private void CarregarCena()
    {
        if (usarProximaCenaNoBuild)
        {
            int proximoIndice = SceneManager.GetActiveScene().buildIndex + 1;
            if (proximoIndice < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(proximoIndice);
            return;
        }

        if (string.IsNullOrEmpty(nomeDaCena))
            return;

        SceneManager.LoadScene(nomeDaCena);
    }
}