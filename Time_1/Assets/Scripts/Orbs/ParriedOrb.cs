using UnityEngine;

public class ParriedOrb : MonoBehaviour
{
    [Header("Orb Settings")]
    [Tooltip("Duração em segundos que o projétil permanece como orb")]
    [SerializeField] private float orbDuration = 3f;

    [Tooltip("Multiplicador de velocidade enquanto é orb (ex: 0.3 = 30% da velocidade original)")]
    [SerializeField] private float orbSpeedMultiplier = 0.3f;

    [Tooltip("Sprite exibido enquanto é orb (opcional — se null mantém o sprite original)")]
    [SerializeField] private Sprite orbSprite;

    private Rigidbody2D rb;
    private BasicProjectile basicProjectile;
    private SpriteRenderer sr;

    // Comportamentos do inimigo que precisam ser pausados enquanto é orb
    private Galinha galinha;
    private DundeProjectile dundeProjectile;

    private Sprite originalSprite;
    private Vector2 orbVelocity;
    private float originalSpeed;
    private float timer;
    private bool collected;
    private bool activated;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        basicProjectile = GetComponent<BasicProjectile>();
        sr = GetComponent<SpriteRenderer>();
        galinha = GetComponent<Galinha>();
        dundeProjectile = GetComponent<DundeProjectile>();
    }
    private void OnEnable()
    {
        activated = false;
        collected = false;
        timer = 0f;
    }
    public void Init(float projectileSpeed)
    {
        if (activated) return;
        activated = true;
        originalSpeed = projectileSpeed;
        timer = orbDuration;

        // Pausa todos os comportamentos de "projétil/inimigo" enquanto é orb
        if (basicProjectile != null) basicProjectile.enabled = false;
        if (dundeProjectile != null) dundeProjectile.enabled = false;
        if (galinha != null)
        {
            galinha.SetComoOrb(true);  
            galinha.enabled = false;   
        }

        // Troca sprite
        if (sr != null)
        {
            originalSprite = sr.sprite;
            if (orbSprite != null)
                sr.sprite = orbSprite;
        }

        // Mantém direção, reduz velocidade
        if (rb != null)
        {
            Vector2 currentVel = rb.velocity;
            if (currentVel.sqrMagnitude < 0.01f)
                currentVel = transform.right * originalSpeed;

            orbVelocity = currentVel.normalized * (currentVel.magnitude * orbSpeedMultiplier);
            rb.velocity = orbVelocity;
            rb.isKinematic = false;
        }
    }
    private void Update()
    {
        if (!activated || collected) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            RevertToProjectile();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!activated || collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        OrbManager.Instance?.AddOrb();
        Destroy(gameObject);
    }
    private void RevertToProjectile()
    {
        // Restaura sprite
        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;

        // Restaura velocidade na direção atual
        if (rb != null)
        {
            Vector2 currentDir = rb.velocity.sqrMagnitude > 0.001f
                ? rb.velocity.normalized
                : orbVelocity.normalized;
            rb.velocity = currentDir * originalSpeed;
        }

        // Reativa os comportamentos originais
        if (basicProjectile != null) basicProjectile.enabled = true;
        if (dundeProjectile != null) dundeProjectile.enabled = true;
        if (galinha != null)
        {
            galinha.enabled = true;
            galinha.SetComoOrb(false);  // libera a explosão de novo
        }
        activated = false;
        Destroy(this);
    }
}