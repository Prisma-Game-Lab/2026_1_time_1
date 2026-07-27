using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NezhaHealthController : HealthController
{
    [Header("Morte")]
    [Tooltip("Objeto a desativar ao morrer. Vazio = este proprio (a raiz do Nezha).")]
    [SerializeField] private GameObject bossRoot;
    [Tooltip("VFX instanciado na posicao do boss ao morrer.")]
    [SerializeField] private GameObject vfxMortePrefab;
    [Tooltip("Nome do SFX de morte no SFXManager. Vazio = nao toca nada.")]
    [SerializeField] private string sfxMorteId = "";
    [Tooltip("Disparado ao morrer (fim de fase, cutscene, etc.).")]
    [SerializeField] private UnityEvent onMorte;

    [Header("Stagger (vulnerabilidade) - opcional")]
    [Tooltip("Multiplicador do dano RECEBIDO enquanto o boss esta vulneravel.")]
    [Range(1f, 5f)]
    [SerializeField] private float multiplicadorDanoStagger = 2f;

    private bool vulneravel;
    private bool morreu;

    // Chame isto de outro script (ex.: durante um stun/stagger do Nezha) para abrir janela de dano extra.
    public void SetVulneravel(bool v) => vulneravel = v;

    public bool EstaMorto => morreu;

    // Amplifica o dano durante o stagger; senao, dano normal.
    public override void TakeDamage(int dmg)
    {
        if (morreu) return;

        int final = vulneravel
            ? Mathf.RoundToInt(dmg * multiplicadorDanoStagger)
            : dmg;

        base.TakeDamage(final); // atualiza HP, dispara OnHealthChanged/OnDamageTaken, flash, e chama Die() se zerar
    }

    public override void Die()
    {
        if (morreu) return;
        morreu = true;

        if (vfxMortePrefab != null)
            Instantiate(vfxMortePrefab, transform.position, Quaternion.identity);

        if (!string.IsNullOrEmpty(sfxMorteId))
            SFXManager.PlaySFX(sfxMorteId);

        onMorte?.Invoke();

        GameObject alvo = bossRoot != null ? bossRoot : gameObject;
        alvo.SetActive(false);
    }
}