using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DundeProjectile : MonoBehaviour
{
    [Header("Configurações Padrão (sobrescritas pelo Duende)")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 5;
    [SerializeField] private float maxLifetime = 6f;
    private Rigidbody2D _rb;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.freezeRotation = true;
        Destroy(gameObject, maxLifetime);
    }

    public void Initialize(Vector2 direction, float projectileSpeed, int projectileDamage)
    {
        speed = projectileSpeed;
        damage = projectileDamage;
        _rb.velocity = direction.normalized * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignora outros projéteis (tag "Projectile" já existe no projeto — usada pelo parry da lança)
        if (other.CompareTag("Projectile")) return;

        // Dano ao jogador
        PlayerHealthController playerHealth = other.GetComponent<PlayerHealthController>();
        if (playerHealth != null)
            playerHealth.TakeDamage(damage);

        Destroy(gameObject);
    }
}