using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbPickup : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int orbValue = 1;
    private float timer;
    void Start() => timer = lifetime;
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        OrbManager.Instance?.AddOrb(orbValue);
        Destroy(gameObject);
    }
}