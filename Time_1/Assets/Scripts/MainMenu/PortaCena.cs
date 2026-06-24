using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class PortaCena : MonoBehaviour
{
    [Header("Cena de Destino")]
    [Tooltip("Nome exato da cena a carregar (precisa estar nas Build Settings)")]
    [SerializeField] private string nomeDaCena;

    [Tooltip("Se marcado, ignora o nome acima e carrega a próxima cena pelo índice das Build Settings")]
    [SerializeField] private bool usarProximaCenaNoBuild = false;

    [Header("Configuração")]
    [Tooltip("Tag do objeto que ativa a porta")]
    [SerializeField] private string tagDoJogador = "Player";

    private bool jaAtivou = false;
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (jaAtivou) return;
        if (!other.CompareTag(tagDoJogador)) return;

        jaAtivou = true;
        CarregarCena();
    }
    private void CarregarCena()
    {
        if (usarProximaCenaNoBuild)
        {
            int proximoIndice = SceneManager.GetActiveScene().buildIndex + 1;
            if (proximoIndice < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(proximoIndice);
            }
            else
            {
            }
            return;
        }
        if (string.IsNullOrEmpty(nomeDaCena))
        {
            return;
        }
        SceneManager.LoadScene(nomeDaCena);
    }
}