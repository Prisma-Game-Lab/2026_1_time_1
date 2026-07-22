using System.Collections.Generic;
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

    [Tooltip("Escala do sprite enquanto é orb (ex: 2 = dobro do tamanho)")]
    [SerializeField] private float orbScale = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Sprite originalSprite;
    private Vector3 originalScale;
    private Vector2 orbVelocity;
    private float   originalSpeed;
    private float   timer;
    private bool    collected;
    private bool    activated;
    private bool    wasKinematic;

    public bool EstaComoOrb => activated;

    private readonly List<MonoBehaviour> disabledScripts = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        activated = false;
        collected = false;
        timer     = 0f;
    }

    public void Init(float projectileSpeed)
    {
        if (activated) return;
        activated     = true;
        originalSpeed = projectileSpeed;
        timer         = orbDuration;

        disabledScripts.Clear();
        foreach (MonoBehaviour mb in GetComponents<MonoBehaviour>())
        {
            if (mb == this || !mb.enabled) continue;
            mb.enabled = false;
            disabledScripts.Add(mb);
        }

        if (sr != null)
        {
            originalSprite = sr.sprite;
            if (orbSprite != null) sr.sprite = orbSprite;
        }

        originalScale        = transform.localScale;
        transform.localScale = originalScale * orbScale;

        // Determine movement direction:
        //   1. Rigidbody velocity (physics-moved projectiles)
        //   2. BasicProjectile.GetMovementDirection() (kinematic/script-moved: Arrow uses
        //      transform.up, Fireball uses its internal velocity vector, etc.)
        Vector2 movDir;
        if (rb != null && rb.velocity.sqrMagnitude >= 0.01f)
        {
            movDir = rb.velocity.normalized;
        }
        else
        {
            var bp = GetComponent<BasicProjectile>();
            movDir = bp != null ? bp.GetMovementDirection() : (Vector2)transform.right;
        }

        orbVelocity = movDir * (originalSpeed * orbSpeedMultiplier);

        if (rb != null)
        {
            wasKinematic   = rb.isKinematic;
            rb.isKinematic = false;
            rb.velocity    = orbVelocity;
        }
    }

    private void Update()
    {
        if (!activated || collected) return;

        // Projectiles without a Rigidbody (e.g. Fireball) need manual movement
        if (rb == null)
            transform.position += (Vector3)(orbVelocity * Time.deltaTime);

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
        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;

        transform.localScale = originalScale;

        if (rb != null)
        {
            if (wasKinematic)
            {
                // Restore kinematic so the original script handles movement (e.g. Arrow)
                rb.isKinematic = true;
                rb.velocity    = Vector2.zero;
            }
            else
            {
                Vector2 currentDir = rb.velocity.sqrMagnitude > 0.001f
                    ? rb.velocity.normalized
                    : orbVelocity.normalized;
                rb.velocity = currentDir * originalSpeed;
            }
        }

        foreach (MonoBehaviour mb in disabledScripts)
        {
            if (mb != null) mb.enabled = true;
        }
        disabledScripts.Clear();

        activated = false;
        Destroy(this);
    }
}
