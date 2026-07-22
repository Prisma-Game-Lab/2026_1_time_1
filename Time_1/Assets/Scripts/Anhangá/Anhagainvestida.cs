using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnhangaInvestida : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AnhangaMovement movement;
    [SerializeField] private AnhangaHealthController health;
    [Tooltip("SpriteRenderer do corpo do boss escondido  na fumaca")]
    [SerializeField] private SpriteRenderer bossSprite;
    [Tooltip("Collider do chifre (corpo). Desligado enquanto invisivel/dash; religado no stagger.")]
    [SerializeField] private Collider2D chifreCollider;
    [Tooltip("Collider de DANO ALTO da investida (com seu proprio ChifreCollider). Ligado so no dash.")]
    [SerializeField] private Collider2D colliderInvestida;
    [Tooltip("Fumaca.")]
    [SerializeField] private GameObject fumaca;

    [Header("Recuo")]
    [SerializeField] private float velocidadeRecuo = 4f;
    [Tooltip("Quanto tempo recua antes de sumir")]
    [SerializeField] private float tempoRecuo = 0.5f;

    [Header("Sumico / Surgimento")]
    [Tooltip("Tempo invisivel na fumaca antes de reaparecer")]
    [SerializeField] private float tempoNaFumaca = 1f;
    [Tooltip("Pausa apos surgir, antes do dash (avisa de que lado vem)")]
    [SerializeField] private float tempoSurgir = 0.4f;

    [Header("Investida (dash)")]
    [Tooltip("Velocidade do dash atravessando a tela (alta)")]
    [SerializeField] private float velocidadeInvestida = 22f;

    [Header("Stagger")]
    [Tooltip("Tempo parado e vulneravel no lado oposto")]
    [SerializeField] private float tempoStagger = 1.5f;

    [Header("audio")]
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
            Debug.LogError("[AnhangaInvestida] AnhangaMovement nao encontrado.", this);
            return;
        }
        routine = StartCoroutine(InvestidaRoutine());
    }

    private IEnumerator InvestidaRoutine()
    {
        if (fumaca != null) fumaca.SetActive(true);
        int dirPlayer = movement.DirecaoParaPlayer();
        float t = 0f;
        while (t < tempoRecuo)
        {
            movement.PassoHorizontal(-dirPlayer, dirPlayer, velocidadeRecuo);
            t += Time.deltaTime;
            yield return null;
        }

        if (bossSprite != null) bossSprite.enabled = false;
        if (chifreCollider != null) chifreCollider.enabled = false;
        if (fumaca != null) fumaca.SetActive(false);

        yield return new WaitForSeconds(tempoNaFumaca);

        bool surgeNaEsquerda = Random.value < 0.5f;
        float xSurge = surgeNaEsquerda ? movement.MinX : movement.MaxX;
        float xAlvo = surgeNaEsquerda ? movement.MaxX : movement.MinX;
        int dirDash = surgeNaEsquerda ? 1 : -1;

        movement.PosicionarEm(xSurge);
        movement.Encarar(dirDash);
        if (bossSprite != null) bossSprite.enabled = true;
        if (fumaca != null) fumaca.SetActive(true);

        yield return new WaitForSeconds(tempoSurgir);

        if (fumaca != null) fumaca.SetActive(false);
        if (colliderInvestida != null) colliderInvestida.enabled = true;
        if (sfxInvestida != null) SFXManager.PlaySFX("anhanga_investida");

        bool chegou = false;
        while (!chegou)
        {
            chegou = movement.IrParaX(xAlvo, velocidadeInvestida);
            yield return null;
        }

        if (colliderInvestida != null) colliderInvestida.enabled = false;
        if (chifreCollider != null) chifreCollider.enabled = true; // volta a poder ser atingido pela lanca
        if (health != null) health.SetVulneravel(true);

        yield return new WaitForSeconds(tempoStagger);

        if (health != null) health.SetVulneravel(false);
        routine = null;
    }
}