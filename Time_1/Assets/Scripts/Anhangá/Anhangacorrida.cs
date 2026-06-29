using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnhangaCorrida : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private AnhangaMovement movement;

    [Header("Corrida")]
    [Tooltip("Velocidade horizontal da chifrada")]
    [SerializeField] private float velocidade = 8f;
    [Tooltip("Número MÍNIMO de travessias por ataque")]
    [SerializeField] private int numeroDeCorridasMin = 5;
    [Tooltip("Número MÁXIMO de travessias (= min para valor fixo)")]
    [SerializeField] private int numeroDeCorridasMax = 6;

    [Header("Tempos")]
    [Tooltip("Pausa de aviso, parado, antes de correr")]
    [SerializeField] private float telegraphDuration = 0.6f;
    [Tooltip("Pausa ao bater na parede antes de virar e voltar")]
    [SerializeField] private float pausaNaBorda = 0.25f;

    [Header("Áudio")]
    [SerializeField] private AudioClip sfxChifrada;

    private Coroutine routine;
    public bool IsAttacking => routine != null;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<AnhangaMovement>();
    }
    public void Iniciar()
    {
        if (routine != null) return;
        if (movement == null)
        {
            Debug.LogError("[AnhangaCorrida] AnhangaMovement não encontrado.", this);
            return;
        }
        routine = StartCoroutine(CorridaRoutine());
    }
    private IEnumerator CorridaRoutine()
    {
        // Direção inicial: na direção do player.
        int direcao = 1;
        if (movement.Player != null)
            direcao = movement.Player.position.x >= transform.position.x ? 1 : -1;

        movement.Encarar(direcao);

        // Telegraph (parado).
        if (telegraphDuration > 0f)
            yield return new WaitForSeconds(telegraphDuration);

        if (sfxChifrada != null) AudioManager.Instance?.TocaSFX(sfxChifrada);

        int corridas = Random.Range(numeroDeCorridasMin, numeroDeCorridasMax + 1);
        while (corridas > 0)
        {
            float alvoX = direcao > 0 ? movement.MaxX : movement.MinX;

            bool chegou = false;
            while (!chegou)
            {
                chegou = movement.IrParaX(alvoX, velocidade);
                yield return null;
            }
            corridas--;
            if (corridas > 0)
            {
                if (pausaNaBorda > 0f)
                    yield return new WaitForSeconds(pausaNaBorda);
                direcao = -direcao;
            }
        }
        routine = null;
    }
}