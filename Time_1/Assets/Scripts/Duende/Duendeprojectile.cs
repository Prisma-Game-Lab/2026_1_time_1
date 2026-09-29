using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DundeProjectile : MonoBehaviour
{
    [Header("Configurações Padrão (sobrescritas pelo Duende)")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 5;
    [SerializeField] private float maxLifetime = 6f;
    [SerializeField] private LayerMask environmentLayers;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.freezeRotation = true;
        _rb.gravityScale = 0f; // trajetória reta, igual à linha mirada
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
        var parriedOrb = GetComponent<ParriedOrb>();
        if (parriedOrb != null && parriedOrb.EstaComoOrb) return;

        // Ignora outros projéteis.
        if (other.CompareTag("Projectile")) return;

        // Dano ao jogador — procura a vida no pai, pois o collider pode estar num filho.
        PlayerHealthController playerHealth = other.GetComponentInParent<PlayerHealthController>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Bateu no cenário.
        if (((1 << other.gameObject.layer) & environmentLayers) != 0)
            Destroy(gameObject);
    }
}