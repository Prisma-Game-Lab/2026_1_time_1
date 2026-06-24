using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnhangaAI : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private AnhangaMovement movement;
    [SerializeField] private AnhangaCorrida corrida;
    [SerializeField] private AnhangaRaizes raizes;

    [Header("Estado Neutro")]
    [Tooltip("Velocidade (lenta) ao caminhar pro player no neutro")]
    [SerializeField] private float velocidadeNeutro = 2.5f;
    [Tooltip("Tempo MÍNIMO no neutro antes de escolher um ataque")]
    [SerializeField] private float neutroTempoMin = 1.5f;
    [Tooltip("Tempo MÁXIMO no neutro antes de escolher um ataque")]
    [SerializeField] private float neutroTempoMax = 2f;

    [Header("Escolha de Ataque")]
    [Tooltip("Chance de escolher a CORRIDA (o resto vai pras RAÍZES)")]
    [Range(0f, 1f)]
    [SerializeField] private float chanceCorrida = 0.5f;

    private void Start()
    {
        if (movement == null) movement = GetComponent<AnhangaMovement>();
        if (corrida == null) corrida = GetComponent<AnhangaCorrida>();
        if (raizes == null) raizes = GetComponent<AnhangaRaizes>();

        if (movement == null || corrida == null || raizes == null)
        {
            Debug.LogError("[AnhangaAI] Faltam componentes (movement/corrida/raizes).", this);
            return;
        }

        StartCoroutine(LoopPrincipal());
    }

    private IEnumerator LoopPrincipal()
    {
        while (true)
        {
            float tempoNeutro = Random.Range(neutroTempoMin, neutroTempoMax);
            float t = 0f;
            while (t < tempoNeutro)
            {
                movement.AndarParaPlayer(velocidadeNeutro);
                t += Time.deltaTime;
                yield return null;
            }

            // --- ESCOLHE E EXECUTA ---
            if (Random.value < chanceCorrida)
            {
                corrida.Iniciar();
                yield return new WaitUntil(() => !corrida.IsAttacking);
            }
            else
            {
                raizes.Iniciar();
                yield return new WaitUntil(() => !raizes.IsAttacking);
            }
            // volta pro neutro
        }
    }
}