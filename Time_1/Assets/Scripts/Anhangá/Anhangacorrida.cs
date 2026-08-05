using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnhangaCorrida : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AnhangaMovement movement;

    [Header("Corrida")]
    [SerializeField] private float velocidade = 8f;
    [SerializeField] private int numeroDeCorridasMin = 5;
    [SerializeField] private int numeroDeCorridasMax = 6;

    [Header("Tempos")]
    [SerializeField] private float telegraphDuration = 0.6f;
    [SerializeField] private float pausaNaBorda = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioClip sfxAviso;
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
            Debug.LogError("[AnhangaCorrida] AnhangaMovement nao encontrado.", this);
            return;
        }
        routine = StartCoroutine(CorridaRoutine());
    }
    private IEnumerator CorridaRoutine()
    {
        int direcao = 1;
        if (movement.Player != null)
            direcao = movement.Player.position.x >= transform.position.x ? 1 : -1;

        movement.Encarar(direcao);

        if (sfxAviso != null) SFXManager.PlaySFX("anhanga_corrida_aviso");

        if (telegraphDuration > 0f)
            yield return new WaitForSeconds(telegraphDuration);

        if (sfxChifrada != null) SFXManager.PlaySFX("anhanga_chifrada");

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