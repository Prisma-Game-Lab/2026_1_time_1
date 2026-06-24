using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnhangaHealthController : HealthController
{
    [Header("Morte")]
    [Tooltip("Objeto a desativar ao morrer. Vazio = este próprio (a raiz do Anhangá).")]
    [SerializeField] private GameObject bossRoot;

    [Tooltip("VFX instanciado na posição do boss ao morrer. Opcional.")]
    [SerializeField] private GameObject vfxMortePrefab;

    [Tooltip("Som de morte. Opcional.")]
    [SerializeField] private AudioClip sfxMorte;

    [Tooltip("Disparado ao morrer — plugue aqui o que deve acontecer depois " +
             "(próxima cena, tela de vitória, GameManager.BossDerrotado, etc.). " +
             "Pode deixar vazio pra testar.")]
    [SerializeField] private UnityEvent onMorte;

    private bool morreu;

    public override void Die()
    {
        if (morreu) return;  
        morreu = true;

        if (vfxMortePrefab != null)
            Instantiate(vfxMortePrefab, transform.position, Quaternion.identity);

        if (sfxMorte != null)
            AudioManager.Instance?.TocaSFX(sfxMorte);

        onMorte?.Invoke();

        // Desativar a raiz para o boss: para a IA, os ataques (corrotinas),
        GameObject alvo = bossRoot != null ? bossRoot : gameObject;
        alvo.SetActive(false);
    }
}