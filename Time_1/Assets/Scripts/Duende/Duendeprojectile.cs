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
        // Se colidiu com a lança, o PlayerShooting trata o parry — NÃO destrói.
        if (other.GetComponent<SpearCollisionRelay>() != null) return;
        // Ignora qualquer Duende (incluindo o que disparou) — tiro não acerta Duende.
        if (other.GetComponentInParent<Duende>() != null) return;
        // Se já virou orb, não faz mais nada (a lógica de orb controla o objeto).
        if (GetComponent<ParriedOrb>() != null && !enabled) return;
        // Ignora outros projéteis.
        if (other.CompareTag("Projectile")) return;
        // Dano ao jogador.
        PlayerHealthController playerHealth = other.GetComponent<PlayerHealthController>();
        if (playerHealth != null)
            playerHealth.TakeDamage(damage);
        Destroy(gameObject);
    }
}