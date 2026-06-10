using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NPCFailureSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject galinhaPrefab;
    [SerializeField] private GameObject duendePrefab;

    [Header("Quantidades")]
    [SerializeField] private int qtdGalinhas = 3;
    [SerializeField] private int qtdDuendes = 2;

    [Header("Spawn")]
    [Tooltip("Ponto de origem do spawn. Se nulo, usa o transform deste GameObject.")]
    [SerializeField] private Transform pontoSpawn;
    [SerializeField] private float spawnWidth = 8f;
    public void DispararFalha()
    {
        Transform origem = pontoSpawn != null ? pontoSpawn : transform;

        for (int i = 0; i < qtdGalinhas; i++)
            Spawnar(galinhaPrefab, origem);

        for (int i = 0; i < qtdDuendes; i++)
            Spawnar(duendePrefab, origem);
    }
    private void Spawnar(GameObject prefab, Transform origem)
    {
        if (prefab == null) return;
        float x = origem.position.x + Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        Vector3 pos = new Vector3(x, origem.position.y, 0f);
        Instantiate(prefab, pos, Quaternion.identity);
    }
    void OnDrawGizmosSelected()
    {
        Transform origem = pontoSpawn != null ? pontoSpawn : transform;
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        Gizmos.DrawWireCube(origem.position, new Vector3(spawnWidth, 0.3f, 0f));
    }
}