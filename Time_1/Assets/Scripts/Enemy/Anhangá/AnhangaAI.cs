using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnhangaAI : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private AnhangaMovement  movement;
    [SerializeField] private AnhangaCorrida   corrida;
    [SerializeField] private AnhangaRaizes    raizes;
    [SerializeField] private AnhangaInvestida investida;
    [SerializeField] private AnhangaArrow     anhangaArrow;
    [SerializeField] private AnhangaScream    anhangaScream;

    [Header("Estado Neutro")]
    [SerializeField] private float velocidadeNeutro = 2.5f;
    [SerializeField] private float neutroTempoMin   = 1.5f;
    [SerializeField] private float neutroTempoMax   = 2f;

    [Header("Pesos dos Ataques (relativos)")]
    [Tooltip("Quanto maior o peso, mais frequente o ataque. 0 = nunca.")]
    [SerializeField] private float pesoCorrida  = 1f;
    [SerializeField] private float pesoRaizes   = 1f;
    [SerializeField] private float pesoInvestida = 1f;
    [SerializeField] private float pesoArrow    = 3f;
    [SerializeField] private float pesoScream   = 0f;

    [Header("Flechas")]
    [SerializeField] private int   arrowCount    = 5;
    [SerializeField] private float arrowInterval = 0.2f;

    [Header("Grito")]
    [SerializeField] private int screamDuration = 3;

    private void Start()
    {
        if (movement     == null) movement     = GetComponent<AnhangaMovement>();
        if (corrida      == null) corrida      = GetComponent<AnhangaCorrida>();
        if (raizes       == null) raizes       = GetComponent<AnhangaRaizes>();
        if (investida    == null) investida    = GetComponent<AnhangaInvestida>();
        if (anhangaArrow == null) anhangaArrow = GetComponent<AnhangaArrow>();
        if (anhangaScream == null) anhangaScream = GetComponent<AnhangaScream>();

        if (movement == null || corrida == null || raizes == null || investida == null || anhangaArrow == null)
            return;

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
            int ataque = SortearAtaque();
            switch (ataque)
            {
                case 0:
                    corrida.Iniciar();
                    yield return new WaitUntil(() => !corrida.IsAttacking);
                    break;
                case 1:
                    raizes.Iniciar();
                    yield return new WaitUntil(() => !raizes.IsAttacking);
                    break;
                case 2:
                    investida.Iniciar();
                    yield return new WaitUntil(() => !investida.IsAttacking);
                    break;
                case 3:
                    anhangaArrow.HomingArrows(arrowCount, arrowInterval);
                    yield return new WaitUntil(() => !anhangaArrow.IsAttacking);
                    break;
                default:
                    if (anhangaScream != null) anhangaScream.Scream(screamDuration);
                    yield return new WaitUntil(() => anhangaScream == null || !anhangaScream.IsAttacking);
                    break;
            }
            // Tick confused duration counter after every non-scream attack.
            if (ataque != 4 && anhangaScream != null)
                anhangaScream.OnMoveCompleted();
        }
    }

    // 0 = corrida, 1 = raízes, 2 = investida, 3 = flechas, 4 = grito
    private int SortearAtaque()
    {
        // Don't scream again while the player is already confused.
        float effectivePesoScream = (anhangaScream != null && anhangaScream.IsConfused) ? 0f : pesoScream;
        float total = pesoCorrida + pesoRaizes + pesoInvestida + pesoArrow + effectivePesoScream;
        if (total <= 0f) return 0;

        float r = Random.value * total;
        if (r < pesoCorrida) return 0;
        r -= pesoCorrida;
        if (r < pesoRaizes) return 1;
        r -= pesoRaizes;
        if (r < pesoInvestida) return 2;
        r -= pesoInvestida;
        if (r < pesoArrow) return 3;
        return 4;
    }
}
