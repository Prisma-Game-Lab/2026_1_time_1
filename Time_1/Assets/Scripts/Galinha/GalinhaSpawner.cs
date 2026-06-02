using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GalinhaSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject galinhaPrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnWidth = 8f;
    [SerializeField] private int maxGalinhas = 10;

    private float timer;
    private void Update()
    {
        if (galinhaPrefab == null) return;
        if (maxGalinhas > 0 && ContarGalinhas() >= maxGalinhas) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Spawnar();
            timer = spawnInterval;
        }
    }
    private void Spawnar()
    {
        float randomX = transform.position.x + Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        Vector3 spawnPos = new Vector3(randomX, transform.position.y, 0f);
        Instantiate(galinhaPrefab, spawnPos, Quaternion.identity);
    }
    private int ContarGalinhas() =>
        FindObjectsOfType<Galinha>().Length;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnWidth, 0.2f, 0f));
    }
}