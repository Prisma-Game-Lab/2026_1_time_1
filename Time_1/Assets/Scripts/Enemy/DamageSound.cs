using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSound : MonoBehaviour
{
    [Tooltip("Som tocado ao levar dano. Cada entidade tem o seu.")]
    [SerializeField] private AudioClip clipDano;

    [Tooltip("HealthController alvo. Vazio = o deste GameObject.")]
    [SerializeField] private HealthController alvo;

    private void Awake()
    {
        if (alvo == null) alvo = GetComponent<HealthController>();
        if (alvo == null)
            Debug.LogError("[DamageSound] Nenhum HealthController encontrado. Atribua no Inspector.", this);
    }
    private void OnEnable()
    {
        if (alvo != null) alvo.OnDamageTaken += AoLevarDano;
    }
    private void OnDisable()
    {
        if (alvo != null) alvo.OnDamageTaken -= AoLevarDano;
    }

    private void AoLevarDano(int dano)
    {
        if (clipDano == null) return;
        AudioManager.Instance?.TocaSFX(clipDano);
    }
}