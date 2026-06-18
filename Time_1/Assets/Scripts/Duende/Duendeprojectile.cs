using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DundeProjectile : MonoBehaviour
{
    [Header("Configura��es Padr�o (sobrescritas pelo Duende)")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 5;
    [SerializeField] private float maxLifetime = 6f;
    [SerializeField] private LayerMask environmentLayers;

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
        // Se colidiu com a lan�a, o PlayerShooting trata o parry � N�O destr�i.
        if (other.GetComponent<SpearCollisionRelay>() != null) return;
        // Ignora qualquer Duende (incluindo o que disparou) � tiro n�o acerta Duende.
        if (other.GetComponentInParent<Duende>() != null) return;
        // Se j� virou orb, n�o faz mais nada (a l�gica de orb controla o objeto).
        if (GetComponent<ParriedOrb>() != null && !enabled) return;
        // Ignora outros proj�teis.
        if (other.CompareTag("Projectile")) return;
        // Dano ao jogador.
        if (other.TryGetComponent(out PlayerHealthController playerHealth))
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        
        if (((1 << other.gameObject.layer) & environmentLayers) != 0)
            Destroy(gameObject);
    }
}