using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Galinha : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float lateralIntensity = 1.5f;
    [SerializeField] private float lateralFrequency = 0.8f;
    [SerializeField] private float maxFallSpeed = 6f;
    [SerializeField] private float gravityMultiplier = 0.5f;
    [SerializeField] private float lifetime = 0f;

    [Header("Configurações da Explosão")]
    [SerializeField] private int explosionDamage = 20;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float minDamageDistance = 0f;
    [SerializeField] private float maxDamageDistance = 0f;
    [SerializeField] private LayerMask damageableLayers;

    [Header("Configurações do Jogador Afetado")]
    [SerializeField] private float knockbackMultiplier = 1f;
    [SerializeField] private bool ignoreKnockback = false;

    [Header("Visual e Áudio")]
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private AudioClip explosionSFX;
    [SerializeField] private GameObject fallVFXPrefab;
    [SerializeField] private Transform explosionPoint;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasExploded;
    private float timeAlive;
    private float lateralPhaseOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }
    void Start()
    {
        lateralPhaseOffset = Random.Range(0f, Mathf.PI * 2f);

        if (fallVFXPrefab != null)
            Instantiate(fallVFXPrefab, transform.position, Quaternion.identity, transform);
    }
    private void Update()
    {
        if (hasExploded) return;

        float lateralVelocity = lateralIntensity * lateralFrequency
                                   * Mathf.Cos(Time.time * lateralFrequency + lateralPhaseOffset)
                                   * (2f * Mathf.PI);

        float gravityExtra = Physics2D.gravity.y * gravityMultiplier * Time.deltaTime;
        float targetFallVelocity = rb.velocity.y + (-fallSpeed * Time.deltaTime) + gravityExtra;
        float clampedY = Mathf.Clamp(targetFallVelocity, -maxFallSpeed, 0f);

        rb.velocity = new Vector2(lateralVelocity, clampedY);

        if (lifetime > 0f)
        {
            timeAlive += Time.deltaTime;
            if (timeAlive >= lifetime) Explode();
        }
    }
    public void Explodir()
    {
        Explode();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;
        if (other.GetComponent<PlayerHealthController>() != null) { Explode(); return; }
        if (other.GetComponent<Galinha>() != null) { 
            //Explode(); 
            return; }
        if (other.CompareTag("Ground") || IsInLayerMask(other.gameObject.layer, damageableLayers))
            Explode();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;
        GameObject o = collision.gameObject;
        if (o.GetComponent<PlayerHealthController>() != null ||
            o.GetComponent<Galinha>() != null ||
            o.CompareTag("Ground") ||
            IsInLayerMask(o.layer, damageableLayers))
            Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        Vector2 center = explosionPoint != null
            ? (Vector2)explosionPoint.position
            : (Vector2)transform.position;
        float raioEfetivo = maxDamageDistance > 0f ? maxDamageDistance : explosionRadius;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, raioEfetivo, damageableLayers);
        Debug.Log($"Explode chamado | Hits encontrados: {hits.Length} | Raio: {raioEfetivo} | Layers: {damageableLayers.value}");
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("VFX")) continue;
            Debug.Log($"Hit detectado: {hit.gameObject.name} | Tag: {hit.tag} | Layer: {hit.gameObject.layer}");

            PlayerHealthController playerHealth = hit.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                Debug.Log($"Dano aplicado ao player! HP antes: {playerHealth.currentHealth}");
                playerHealth.TakeDamage(explosionDamage);
                Debug.Log($"HP depois: {playerHealth.currentHealth}");
            }

            if (!ignoreKnockback)
            {
                PlayerMovement pm = hit.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    Vector2 dir = ((Vector2)hit.transform.position - center).normalized;
                    if (dir == Vector2.zero) dir = Vector2.up;
                    pm.Knockback(dir * knockbackForce * knockbackMultiplier);
                    Debug.Log($"Knockback aplicado ao player!");
                }
            }
        }
        if (explosionVFXPrefab != null)
            Instantiate(explosionVFXPrefab, center, Quaternion.identity);

        AudioManager.Instance?.TocaSFX(explosionSFX);
        Destroy(gameObject);
    }
    private static bool IsInLayerMask(int layer, LayerMask mask) =>
        (mask.value & (1 << layer)) != 0;

    private void OnDrawGizmosSelected()
    {
        Vector2 center = explosionPoint != null
            ? (Vector2)explosionPoint.position
            : (Vector2)transform.position;

        Gizmos.color = new Color(1f, 0.3f, 0f, 0.35f);
        Gizmos.DrawSphere(center, maxDamageDistance > 0f ? maxDamageDistance : explosionRadius);

        if (minDamageDistance > 0f)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawSphere(center, minDamageDistance);
        }
    }
}