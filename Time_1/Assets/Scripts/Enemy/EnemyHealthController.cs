using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthController : HealthController
{
    [SerializeField] GameObject EnemyReference;

    [Header("Porta pos-boss")]
    [SerializeField] private GameObject portaParaAtivar;

    [Header("Tela de vitoria")]
    [SerializeField] private GameObject winScreenPrefab;

    private bool morreu = false;

    public override void Die()
    {
        if (morreu) return;
        morreu = true;

        if (WinScreenManager.Instance != null)
            WinScreenManager.Instance.ShowWinScreen(winScreenPrefab);

        EnemyReference.SetActive(false);

        if (portaParaAtivar != null)
            portaParaAtivar.SetActive(true);
    }
}