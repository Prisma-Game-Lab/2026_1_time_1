using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Prefab da Orb física. Deve conter: SpriteRenderer, Rigidbody2D, Collider2D, OrbFisica.cs")]
    [SerializeField] private GameObject orbPrefab;

    [Header("Quantidade")]
    [Tooltip("Quantas Orbs são geradas por Parry bem-sucedido")]
    [SerializeField] private int orbsPorParry = 1;

    [Header("Posição de Spawn")]
    [Tooltip("Distância do player onde a Orb aparece")]
    [SerializeField] private float distanciaDeSpawn = 0.5f;

    [Tooltip("Spawna no local do Parry (posição da lança) em vez de no player")]
    [SerializeField] private bool spawnarNaLanca = true;

    [Header("Velocidade Inicial por Orb (opcional)")]
    [Tooltip("Velocidade inicial aplicada a cada Orb. (0,0) = sem impulso")]
    [SerializeField] private Vector2 velocidadeInicial = new Vector2(0f, 3f);

    [Tooltip("Espalha as Orbs em direções aleatórias quando mais de 1 é gerada")]
    [SerializeField] private bool espalhamentoAleatorio = true;

    [Tooltip("Ângulo máximo de espalhamento em graus (usado quando espalhamentoAleatorio = true)")]
    [SerializeField] private float anguloEspalhamento = 45f;


    private Transform spearTransform;

    public void RegistrarLanca(Transform lanca)
    {
        spearTransform = lanca;
    }
    public void NotificarParry()
    {
        if (orbPrefab == null)
        {
            Debug.LogWarning("[OrbSpawner] orbPrefab não atribuído no Inspector!", this);
            return;
        }

        Vector3 origemSpawn = CalcularOrigemSpawn();

        for (int i = 0; i < orbsPorParry; i++)
        {
            SpawnarOrb(origemSpawn, i);
        }
    }
    private Vector3 CalcularOrigemSpawn()
    {
        // Prefere spawnar na lança se disponível e configurado
        if (spawnarNaLanca && spearTransform != null)
            return spearTransform.position;

        // Fallback: próximo ao player com distância configurada
        return transform.position + (Vector3)(Vector2.up * distanciaDeSpawn);
    }
    private void SpawnarOrb(Vector3 origem, int indice)
    {
        GameObject orbGO = Instantiate(orbPrefab, origem, Quaternion.identity);

        Rigidbody2D orbRb = orbGO.GetComponent<Rigidbody2D>();
        if (orbRb == null) return;

        Vector2 direcao = velocidadeInicial;

        // Aplica espalhamento se houver mais de uma Orb
        if (espalhamentoAleatorio && orbsPorParry > 1)
        {
            float angulo = Random.Range(-anguloEspalhamento, anguloEspalhamento);
            direcao = RotacionarVetor(velocidadeInicial, angulo);
        }
        orbRb.velocity = direcao;
    }
    private Vector2 RotacionarVetor(Vector2 vetor, float graus)
    {
        float rad = graus * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            vetor.x * cos - vetor.y * sin,
            vetor.x * sin + vetor.y * cos
        );
    }
}