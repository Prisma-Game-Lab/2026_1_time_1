using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnhangaAI : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private AnhangaMovement movement;
    [SerializeField] private AnhangaCorrida corrida;
    [SerializeField] private AnhangaRaizes raizes;
    [SerializeField] private AnhangaInvestida investida;

    [Header("Estado Neutro")]
    [SerializeField] private float velocidadeNeutro = 2.5f;
    [SerializeField] private float neutroTempoMin = 1.5f;
    [SerializeField] private float neutroTempoMax = 2f;

    [Header("Pesos dos Ataques (relativos)")]
    [Tooltip("Quanto maior o peso, mais frequente o ataque. 0 = nunca.")]
    [SerializeField] private float pesoCorrida = 1f;
    [SerializeField] private float pesoRaizes = 1f;
    [SerializeField] private float pesoInvestida = 1f;

    private void Start()
    {
        if (movement == null) movement = GetComponent<AnhangaMovement>();
        if (corrida == null) corrida = GetComponent<AnhangaCorrida>();
        if (raizes == null) raizes = GetComponent<AnhangaRaizes>();
        if (investida == null) investida = GetComponent<AnhangaInvestida>();

        if (movement == null || corrida == null || raizes == null || investida == null)
        {
            return;
        }
        StartCoroutine(LoopPrincipal());
    }

    private IEnumerator LoopPrincipal()
    {
        while (true)
        {
            // --- NEUTRO ---
            float tempoNeutro = Random.Range(neutroTempoMin, neutroTempoMax);
            float t = 0f;
            while (t < tempoNeutro)
            {
                movement.AndarParaPlayer(velocidadeNeutro);
                t += Time.deltaTime;
                yield return null;
            }

            // --- ESCOLHE POR PESO E EXECUTA ---
            switch (SortearAtaque())
            {
                case 0:
                    corrida.Iniciar();
                    yield return new WaitUntil(() => !corrida.IsAttacking);
                    break;
                case 1:
                    raizes.Iniciar();
                    yield return new WaitUntil(() => !raizes.IsAttacking);
                    break;
                default:
                    investida.Iniciar();
                    yield return new WaitUntil(() => !investida.IsAttacking);
                    break;
            }
        }
    }

    // 0 = corrida, 1 = raízes, 2 = investida
    private int SortearAtaque()
    {
        float total = pesoCorrida + pesoRaizes + pesoInvestida;
        if (total <= 0f) return 0; // proteção: se tudo zero, faz corrida

        float r = Random.value * total;
        if (r < pesoCorrida) return 0;
        if (r < pesoCorrida + pesoRaizes) return 1;
        return 2;
    }
}