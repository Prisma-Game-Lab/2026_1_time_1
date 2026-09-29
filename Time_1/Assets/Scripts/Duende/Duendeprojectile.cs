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
        _rb.gravityScale = 0f;
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
        if (other.GetComponent<SpearCollisionRelay>() != null) return;

        if (other.GetComponentInParent<Duende>() != null) return;

        var parriedOrb = GetComponent<ParriedOrb>();
        if (parriedOrb != null && parriedOrb.EstaComoOrb) return;

        if (other.CompareTag("Projectile")) return;

        PlayerHealthController playerHealth = other.GetComponentInParent<PlayerHealthController>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        if (((1 << other.gameObject.layer) & environmentLayers) != 0)
            Destroy(gameObject);
    }
}