using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnhangaHealthController : HealthController
{
    [Header("Morte")]
    [Tooltip("Objeto a desativar ao morrer. Vazio = este proprio (a raiz do Anhanga).")]
    [SerializeField] private GameObject bossRoot;
    [Tooltip("VFX instanciado na posicao do boss ao morrer.")]
    [SerializeField] private GameObject vfxMortePrefab;
    [Tooltip("Som de morte.")]
    [SerializeField] private AudioClip sfxMorte;
    [Tooltip("Disparado ao morrer.")]
    [SerializeField] private UnityEvent onMorte;

    [Header("Stagger (vulnerabilidade)")]
    [Tooltip("Multiplicador do dano que o boss RECEBE enquanto esta em stagger")]
    [Range(1f, 5f)]
    [SerializeField] private float multiplicadorDanoStagger = 2f;

    private bool vulneravel;
    private bool morreu;

    // Ligado/desligado pelo AnhangaInvestida durante o stagger.
    public void SetVulneravel(bool v) => vulneravel = v;

    // Amplifica o dano recebido durante o stagger; senao, dano normal.
    public override void TakeDamage(int dmg)
    {
        int final = vulneravel
            ? Mathf.RoundToInt(dmg * multiplicadorDanoStagger)
            : dmg;
        base.TakeDamage(final);
    }

    public override void Die()
    {
        if (morreu) return;
        morreu = true;

        if (vfxMortePrefab != null)
            Instantiate(vfxMortePrefab, transform.position, Quaternion.identity);
        if (sfxMorte != null)
            SFXManager.PlaySFX("anhanga_morte");

        onMorte?.Invoke();

        GameObject alvo = bossRoot != null ? bossRoot : gameObject;
        alvo.SetActive(false);
    }
}