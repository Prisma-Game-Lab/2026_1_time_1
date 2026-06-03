using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GalinhaSpawner : MonoBehaviour
{
    [Header("Prefab - Galinha")]
    [SerializeField] private GameObject galinhaPrefab;

    [Header("Prefab - Duende")]
    [SerializeField] private GameObject duendePrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnWidth = 8f;

    [Header("Limites")]
    [SerializeField] private int maxGalinhas = 10;
    [SerializeField] private int maxDuendes = 5;

    private float timer;
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = spawnInterval;
        TentarSpawnar();
    }
    private void TentarSpawnar()
    {
        bool podeSpawnarGalinha = galinhaPrefab != null && ContarGalinhas() < maxGalinhas;
        bool podeSpawnarDuende = duendePrefab != null && ContarDuendes() < maxDuendes;

        if (!podeSpawnarGalinha && !podeSpawnarDuende) return;

        // Se ambos disponíveis, sorteia qual spawnar
        if (podeSpawnarGalinha && podeSpawnarDuende)
        {
            if (Random.value < 0.5f)
                Spawnar(galinhaPrefab);
            else
                Spawnar(duendePrefab);
        }
        else if (podeSpawnarGalinha)
            Spawnar(galinhaPrefab);
        else
            Spawnar(duendePrefab);
    }
    private void Spawnar(GameObject prefab)
    {
        float randomX = transform.position.x + Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        Vector3 spawnPos = new Vector3(randomX, transform.position.y, 0f);
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
    private int ContarGalinhas() => FindObjectsOfType<Galinha>().Length;
    private int ContarDuendes() => FindObjectsOfType<Duende>().Length;
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnWidth, 0.2f, 0f));
    }
}