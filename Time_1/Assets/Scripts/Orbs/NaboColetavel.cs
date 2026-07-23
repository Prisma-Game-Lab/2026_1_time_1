using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NaboColetavel : MonoBehaviour
{
    [Tooltip("Quantos orbs este nabo vale ao ser coletado.")]
    [SerializeField] private int valor = 1;

    [Tooltip("Destrói sozinho após este tempo se não for coletado (0 = nunca).")]
    [SerializeField] private float tempoDeVida = 8f;

    private bool coletado;

    private void Start()
    {
        if (tempoDeVida > 0f) Destroy(gameObject, tempoDeVida);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (coletado) return;
        if (!other.CompareTag("Player")) return;

        coletado = true;
        if (OrbManager.Instance != null) OrbManager.Instance.AddOrb(valor);
        Destroy(gameObject);
    }
}