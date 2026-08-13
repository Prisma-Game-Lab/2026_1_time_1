using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnhangaHealthController : HealthController
{
    [Header("Morte")]
    [SerializeField] private GameObject bossRoot;
    [SerializeField] private GameObject vfxMortePrefab;
    [SerializeField] private AudioClip sfxMorte;
    [SerializeField] private GameObject portaParaAtivar;
    [SerializeField] private GameObject winScreenPrefab;
    [SerializeField] private UnityEvent onMorte;

    [Header("Stagger (vulnerabilidade)")]
    [Range(1f, 5f)]
    [SerializeField] private float multiplicadorDanoStagger = 2f;

    private bool vulneravel;
    private bool morreu;

    public void SetVulneravel(bool v) => vulneravel = v;

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

        if (WinScreenManager.Instance != null)
            WinScreenManager.Instance.ShowWinScreen(winScreenPrefab);

        if (portaParaAtivar != null)
            portaParaAtivar.SetActive(true);

        onMorte?.Invoke();

        GameObject alvo = bossRoot != null ? bossRoot : gameObject;
        alvo.SetActive(false);
    }
}