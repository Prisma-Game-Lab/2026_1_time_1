using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnhangaInvestida : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private AnhangaMovement movement;
    [SerializeField] private AnhangaHealthController health;
    [Tooltip("SpriteRenderer do corpo do boss — escondido enquanto na fumaça")]
    [SerializeField] private SpriteRenderer bossSprite;
    [Tooltip("Collider do chifre (corpo). Desligado enquanto invisível/dash; religado no stagger.")]
    [SerializeField] private Collider2D chifreCollider;
    [Tooltip("Collider de DANO ALTO da investida (com seu próprio ChifreCollider). Ligado só no dash.")]
    [SerializeField] private Collider2D colliderInvestida;
    [Tooltip("Fumaça.")]
    [SerializeField] private GameObject fumaca;

    [Header("Recuo")]
    [SerializeField] private float velocidadeRecuo = 4f;
    [Tooltip("Quanto tempo recua antes de sumir")]
    [SerializeField] private float tempoRecuo = 0.5f;

    [Header("Sumiço / Surgimento")]
    [Tooltip("Tempo invisível na fumaça antes de reaparecer")]
    [SerializeField] private float tempoNaFumaca = 1f;
    [Tooltip("Pausa após surgir, antes do dash (avisa de que lado vem)")]
    [SerializeField] private float tempoSurgir = 0.4f;

    [Header("Investida (dash)")]
    [Tooltip("Velocidade do dash atravessando a tela (alta)")]
    [SerializeField] private float velocidadeInvestida = 22f;

    [Header("Stagger")]
    [Tooltip("Tempo parado e vulnerável no lado oposto")]
    [SerializeField] private float tempoStagger = 1.5f;

    [Header("Áudio")]
    [SerializeField] private AudioClip sfxInvestida;

    private Coroutine routine;
    public bool IsAttacking => routine != null;
    private void Awake()
    {
        if (movement == null) movement = GetComponent<AnhangaMovement>();
        if (health == null) health = GetComponent<AnhangaHealthController>();
        if (colliderInvestida != null) colliderInvestida.enabled = false;
    }
    public void Iniciar()
    {
        if (routine != null) return;
        if (movement == null)
        {
            Debug.LogError("[AnhangaInvestida] AnhangaMovement não encontrado.", this);
            return;
        }
        routine = StartCoroutine(InvestidaRoutine());
    }

    private IEnumerator InvestidaRoutine()
    {
        // 1) RECUO — anda pra trás (longe do player), encarando o player.
        if (fumaca != null) fumaca.SetActive(true);
        int dirPlayer = movement.DirecaoParaPlayer();
        float t = 0f;
        while (t < tempoRecuo)
        {
            movement.PassoHorizontal(-dirPlayer, dirPlayer, velocidadeRecuo);
            t += Time.deltaTime;
            yield return null;
        }

        // 2) SUMIÇO — invisível, sem dano.
        if (bossSprite != null) bossSprite.enabled = false;
        if (chifreCollider != null) chifreCollider.enabled = false;
        if (fumaca != null) fumaca.SetActive(false);

        yield return new WaitForSeconds(tempoNaFumaca);

        // 3) SURGE num lado sorteado.
        bool surgeNaEsquerda = Random.value < 0.5f;
        float xSurge = surgeNaEsquerda ? movement.MinX : movement.MaxX;
        float xAlvo = surgeNaEsquerda ? movement.MaxX : movement.MinX;
        int dirDash = surgeNaEsquerda ? 1 : -1;

        movement.PosicionarEm(xSurge);
        movement.Encarar(dirDash);
        if (bossSprite != null) bossSprite.enabled = true;
        if (fumaca != null) fumaca.SetActive(true);

        yield return new WaitForSeconds(tempoSurgir);

        // 4) INVESTIDA — dash atravessando a tela com dano alto.
        if (fumaca != null) fumaca.SetActive(false);
        if (colliderInvestida != null) colliderInvestida.enabled = true;
        if (sfxInvestida != null) AudioManager.Instance?.TocaSFX(sfxInvestida);

        bool chegou = false;
        while (!chegou)
        {
            chegou = movement.IrParaX(xAlvo, velocidadeInvestida);
            yield return null;
        }

        // 5) STAGGER — para no lado oposto, vulnerável.
        if (colliderInvestida != null) colliderInvestida.enabled = false;
        if (chifreCollider != null) chifreCollider.enabled = true; // volta a poder ser atingido pela lança
        if (health != null) health.SetVulneravel(true);

        yield return new WaitForSeconds(tempoStagger);

        // 6) FIM — restaura.
        if (health != null) health.SetVulneravel(false);
        routine = null;
    }
}