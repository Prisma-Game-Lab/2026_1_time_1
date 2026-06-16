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
    private float originalSpeed;
    private float timer;
    private bool collected;
    private bool activated;
    public bool EstaComoOrb => activated;

    private readonly List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
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
        Debug.Log("[ORB] Parry realizado → projétil convertido em Orb.");
        originalSpeed = projectileSpeed;
        timer = orbDuration;

        disabledScripts.Clear();
        foreach (MonoBehaviour mb in GetComponents<MonoBehaviour>())
        {
            if (mb == this || !mb.enabled) continue;
            mb.enabled = false;
            disabledScripts.Add(mb);
        }

        // Troca sprite e escala
        if (sr != null)
        {
            originalSprite = sr.sprite;
            if (orbSprite != null)
                sr.sprite = orbSprite;
        }

        originalScale = transform.localScale;
        transform.localScale = originalScale * orbScale;

        // Reduz velocidade mantendo direção atual
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
        // Restaura sprite e escala
        if (sr != null && originalSprite != null)
            sr.sprite = originalSprite;

        transform.localScale = originalScale;

        if (rb != null)
        {
            Vector2 currentDir = rb.velocity.sqrMagnitude > 0.001f
                ? rb.velocity.normalized
                : orbVelocity.normalized;
            rb.velocity = currentDir * originalSpeed;
        }
        // Reativa os scripts de comportamento original
        foreach (MonoBehaviour mb in disabledScripts)
        {
            if (mb != null) mb.enabled = true;
        }
        disabledScripts.Clear();

        activated = false;
        Destroy(this);
    }
}