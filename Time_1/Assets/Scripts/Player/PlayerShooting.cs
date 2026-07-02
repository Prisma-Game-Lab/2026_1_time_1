using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject spear;
    [SerializeField] private float throwSpeed = 15f;
    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private float dropIntensity = 1f;
    [SerializeField] private int damage = 5;
    [SerializeField] private float returnKnockback = 8f;
    [SerializeField] private float startLag = 0.3f;
    [SerializeField] private float endLag = 0.5f;
    [SerializeField] private float maxChargeTime = 1.5f;
    [SerializeField] private float maxDamageMultiplier = 3f;
    [SerializeField] private float maxSpeedMultiplier = 2f;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Melee Parry")]
    [SerializeField] private float parryKnockbackForce = 10f;
    [SerializeField] private float parryKnockbackUp = 4f;

    [Header("Held Sprite")]
    [SerializeField] private float elfHitRadius = 0.4f;

    public Action OnParry;

    private enum SpearState { Held, WindingUp, Thrown, Stuck, Returning, Recovering }
    private SpearState state = SpearState.Held;

    public bool IsWindingUp => state == SpearState.WindingUp;

    private Transform spearTransform;
    private Rigidbody2D spearRb;
    private Collider2D spearCol;
    private Collider2D playerCol;
    private SpriteRenderer spearSr;

    private Transform spearOriginalParent;
    private Vector3 spearLocalPos;
    private Quaternion spearLocalRot;
    private Vector3 spearLocalScale;

    private Vector2 knockbackDir;
    private float lagTimer;
    private float chargeTimer;
    private int currentDamage;
    private float currentThrowSpeed;

    // Set when the throw button is released during startup lag so the throw fires once lag expires
    private bool pendingThrow;

    private readonly HashSet<int> returnHitIds = new();
    private readonly List<ElfMover> _elfSnapshot = new();

    private Transform stuckToTransform;
    private Vector3 stuckLocalPos;
    private Quaternion stuckLocalRot;

    void Start()
    {
        spearTransform = spear.transform;
        spearRb = spear.GetComponent<Rigidbody2D>();
        spearCol = spear.GetComponent<Collider2D>();
        playerCol = GetComponent<Collider2D>();

        spearSr = spear.GetComponent<SpriteRenderer>();
        if (spearSr == null) spearSr = spear.GetComponentInChildren<SpriteRenderer>();

        spearOriginalParent = spearTransform.parent;
        spearLocalPos = spearTransform.localPosition;
        spearLocalRot = spearTransform.localRotation;
        spearLocalScale = spearTransform.localScale;

        currentDamage = damage;
        currentThrowSpeed = throwSpeed;

        spearRb.simulated = false;

        SpearCollisionRelay relay = spear.GetComponent<SpearCollisionRelay>();
        if (relay != null) relay.Init(this);

        Physics2D.IgnoreCollision(spearCol, playerCol);
    }

    void Update()
    {
        if (spearSr != null)
        {
            if (state == SpearState.WindingUp)
                spearSr.flipX = transform.localScale.x < 0f;
            else if (state == SpearState.Held)
                spearSr.flipX = false;
        }

        switch (state)
        {
            case SpearState.WindingUp:
                if (lagTimer > 0f)
                {
                    lagTimer -= Time.deltaTime;
                    // Startup just finished 
                    if (lagTimer <= 0f && pendingThrow)
                    {
                        pendingThrow = false;
                        ThrowSpear();
                    }
                }
                else
                {
                    chargeTimer += Time.deltaTime;
                    if (chargeTimer >= maxChargeTime)
                    {
                        chargeTimer = maxChargeTime;
                        ThrowSpear();
                    }
                }
                break;

            case SpearState.Stuck:
                if (stuckToTransform != null)
                    spearTransform.SetPositionAndRotation(
                        stuckToTransform.TransformPoint(stuckLocalPos),
                        stuckToTransform.rotation * stuckLocalRot);
                break;

            case SpearState.Thrown:
                RotateSpearToVelocity();
                CheckElfCollision();
                break;

            case SpearState.Returning:
                MoveSpearToPlayer();
                break;

            case SpearState.Recovering:
                lagTimer -= Time.deltaTime;
                if (lagTimer <= 0f)
                {
                    playerMovement.SetMovementLocked(false);
                    state = SpearState.Held;
                }
                break;
        }
    }

    public bool TryMeleeParry()
    {
        // Parry window is the startup lag only
        if (state != SpearState.WindingUp || lagTimer <= 0f) return false;

        // Capture facing before resetting spear rotation (still aimed at cursor)
        float facingSign = spearTransform.right.x >= 0f ? 1f : -1f;

        state = SpearState.Held;
        pendingThrow = false;
        playerMovement.SetMovementLocked(false);
        lagTimer = 0f;
        chargeTimer = 0f;

        // Reset spear to its held pose so the sprite doesn't stay at the wind-up angle
        spearTransform.SetParent(spearOriginalParent);
        spearTransform.SetLocalPositionAndRotation(spearLocalPos, spearLocalRot);
        spearTransform.localScale = spearLocalScale;
        if (spearSr != null) spearSr.flipX = false;

        if (OrbManager.Instance != null) OrbManager.Instance.AddOrb();

        playerMovement.Knockback(new Vector2(-facingSign * parryKnockbackForce, parryKnockbackUp));

        return true;
    }

    public void Throw(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (state == SpearState.Held)
                StartWindUp();
            else if (state == SpearState.Stuck || state == SpearState.Thrown)
                StartReturn();
        }
        else if (context.canceled)
        {
            if (state == SpearState.WindingUp)
            {
                if (lagTimer <= 0f)
                    ThrowSpear();
                else
                    pendingThrow = true; 
            }
        }
    }

    private void StartWindUp()
    {
        pendingThrow = false;
        playerMovement.SetMovementLocked(true);
        lagTimer = startLag;
        chargeTimer = 0f;
        state = SpearState.WindingUp;
    }

    private void ThrowSpear()
    {
        float t = (maxChargeTime > 0f) ? Mathf.Clamp01(chargeTimer / maxChargeTime) : 0f;
        currentDamage = Mathf.RoundToInt(Mathf.Lerp(damage, damage * maxDamageMultiplier, t));
        currentThrowSpeed = Mathf.Lerp(throwSpeed, throwSpeed * maxSpeedMultiplier, t);
        chargeTimer = 0f;

        playerMovement.SetMovementLocked(false);

        Vector3 worldPos = spearTransform.position;
        Quaternion worldRot = spearTransform.rotation;

        spearTransform.SetParent(null);
        spearTransform.position = worldPos;
        spearTransform.rotation = worldRot;
        spearTransform.localScale = spearLocalScale;

        spearRb.simulated = true;
        spearRb.gravityScale = dropIntensity;
        spearRb.velocity = (Vector2)spearTransform.right * currentThrowSpeed;
        spearRb.angularVelocity = 0f;

        if (spearSr != null) spearSr.flipX = false;

        AudioManager.Instance?.TocaSFX(AudioManager.Instance.EfeitoDaLanca);
        state = SpearState.Thrown;
    }

    private void CheckElfCollision()
    {
        _elfSnapshot.Clear();
        _elfSnapshot.AddRange(ElfMover.Active);
        foreach (ElfMover elf in _elfSnapshot)
        {
            if (elf == null || !elf.isActiveAndEnabled) continue;
            if (Vector2.Distance(spearTransform.position, elf.transform.position) > elfHitRadius) continue;

            if (!elf.TryGetComponent(out HealthController hc))
                hc = elf.GetComponentInParent<HealthController>();
            if (hc == null) continue;

            hc.TakeDamage(currentDamage);
            if (hc.currentHealth <= 0)
            {
                spearRb.velocity = new Vector2(0f, spearRb.velocity.y);
                spearRb.angularVelocity = 0f;
            }
            else
            {
                stuckToTransform = elf.transform;
                stuckLocalPos = stuckToTransform.InverseTransformPoint(spearTransform.position);
                stuckLocalRot = Quaternion.Inverse(stuckToTransform.rotation) * spearTransform.rotation;
                spearRb.velocity = Vector2.zero;
                spearRb.angularVelocity = 0f;
                spearRb.simulated = false;
                state = SpearState.Stuck;
            }
            return;
        }
    }

    private void RotateSpearToVelocity()
    {
        if (spearRb.velocity.sqrMagnitude < 0.01f) return;

        float angle = Mathf.Atan2(spearRb.velocity.y, spearRb.velocity.x) * Mathf.Rad2Deg;
        spearRb.SetRotation(angle);
        spearRb.angularVelocity = 0f;
    }

    public void OnSpearHit(Collider2D other)
    {
        if (state != SpearState.Thrown) return;
        Parryble parryble = other.GetComponent<Parryble>();
        if (parryble != null)
        {
            parryble.OnParried();
            ConverterEmOrb(other);
            OnParry?.Invoke();
            ParryReceive();
            return;
        }

        Galinha galinha = other.GetComponent<Galinha>() ?? other.GetComponentInParent<Galinha>();
        if (galinha != null)
        {
            ConverterEmOrb(other);
            OnParry?.Invoke();
            ParryReceive();
            return;
        }

        HealthController enemyHealth = other.GetComponent<HealthController>()
            ?? other.GetComponentInParent<HealthController>();
        if (enemyHealth != null && !(enemyHealth is PlayerHealthController))
        {
            enemyHealth.TakeDamage(currentDamage);
            if (enemyHealth.currentHealth <= 0)
            {
                // Morreu: cai reto em vez de cravar
                spearRb.velocity = new Vector2(0f, spearRb.velocity.y);
                spearRb.angularVelocity = 0f;
                return;
            }

            // Não morreu: crava no inimigo
            stuckToTransform = other.transform;
            stuckLocalPos = stuckToTransform.InverseTransformPoint(spearTransform.position);
            stuckLocalRot = Quaternion.Inverse(stuckToTransform.rotation) * spearTransform.rotation;
            spearRb.velocity = Vector2.zero;
            spearRb.angularVelocity = 0f;
            spearRb.simulated = false;
            state = SpearState.Stuck;
            return;
        }
        if (other.CompareTag("Projectile"))
        {
            ConverterEmOrb(other);
            OnParry?.Invoke();
            ParryReceive();
            return;
        }

        stuckToTransform = other.transform;
        stuckLocalPos = stuckToTransform.InverseTransformPoint(spearTransform.position);
        stuckLocalRot = Quaternion.Inverse(stuckToTransform.rotation) * spearTransform.rotation;
        spearRb.velocity = Vector2.zero;
        spearRb.angularVelocity = 0f;
        spearRb.simulated = false;
        state = SpearState.Stuck;
    }

    // Converte o próprio projétil em orb — NÃO cria objeto novo
    private void ConverterEmOrb(Collider2D projetil)
    {
        Rigidbody2D projRb = projetil.GetComponent<Rigidbody2D>();
        float projSpeed = projRb != null && projRb.velocity.sqrMagnitude > 0.01f
            ? projRb.velocity.magnitude
            : projetil.GetComponent<BasicProjectile>()?.Speed ?? 10f;

        ParriedOrb parriedOrb = projetil.gameObject.GetComponent<ParriedOrb>()
                             ?? projetil.gameObject.AddComponent<ParriedOrb>();
        parriedOrb.Init(projSpeed);
    }

    private void ParryReceive()
    {
        spearRb.simulated = false;
        spearTransform.SetParent(spearOriginalParent);
        spearTransform.localPosition = spearLocalPos;
        spearTransform.localRotation = spearLocalRot;
        spearTransform.localScale = spearLocalScale;
        state = SpearState.Held;
    }

    private void StartReturn()
    {
        returnHitIds.Clear();
        stuckToTransform = null;
        knockbackDir = ((Vector2)transform.position - (Vector2)spearTransform.position).normalized;
        spearRb.simulated = false;
        state = SpearState.Returning;
    }

    private void MoveSpearToPlayer()
    {
        Vector2 toPlayer = (Vector2)transform.position - (Vector2)spearTransform.position;
        float step = returnSpeed * Time.deltaTime;

        if (toPlayer.magnitude <= step)
        {
            ReceiveSpear();
            return;
        }

        spearTransform.position = Vector2.MoveTowards(spearTransform.position, transform.position, step);

        Vector2 fromPlayer = (Vector2)spearTransform.position - (Vector2)transform.position;
        float angle = Mathf.Atan2(fromPlayer.y, fromPlayer.x) * Mathf.Rad2Deg;
        spearTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        Collider2D[] hits = Physics2D.OverlapCircleAll(spearTransform.position, 0.5f);
        foreach (Collider2D hit in hits)
        {
            if (hit == playerCol || hit == spearCol) continue;
            if (returnHitIds.Contains(hit.GetInstanceID())) continue;
            returnHitIds.Add(hit.GetInstanceID());

            Parryble parryble = hit.GetComponent<Parryble>();
            if (parryble != null)
            {
                parryble.OnParried();
                ConverterEmOrb(hit);
                OnParry?.Invoke();
                continue;
            }

            if (!hit.TryGetComponent(out Galinha galinha))
                galinha = hit.GetComponentInParent<Galinha>();
            if (galinha != null)
            {
                ConverterEmOrb(hit);
                OnParry?.Invoke();
                continue;
            }

            if (hit.CompareTag("Projectile"))
            {
                ConverterEmOrb(hit);
                OnParry?.Invoke();
                continue;
            }

            if (hit.TryGetComponent(out EnemyHealthController enemyHealth))
                enemyHealth.TakeDamage(currentDamage);
        }
    }

    private void ReceiveSpear()
    {
        if (!playerMovement.IsGrounded())
            playerMovement.Knockback(knockbackDir * returnKnockback);

        spearTransform.SetParent(spearOriginalParent);
        spearTransform.localPosition = spearLocalPos;
        spearTransform.localRotation = spearLocalRot;
        spearTransform.localScale = spearLocalScale;

        spearRb.simulated = false;

        if (endLag <= 0f)
            state = SpearState.Held;
        else
        {
            lagTimer = endLag;
            playerMovement.LockPreservingMomentum();
            state = SpearState.Recovering;
        }
    }
}
