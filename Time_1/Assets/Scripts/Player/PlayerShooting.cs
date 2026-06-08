using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject spear;

    [SerializeField] private float throwSpeed = 15f;

    [SerializeField] private float returnSpeed = 10f;

    // Gravity scale applied to the spear when thrown
    [SerializeField] private float dropIntensity = 1f;

    // The damage the Spear causes
    [SerializeField] private int damage = 5;

    // Velocity magnitude applied to the player on spear catch
    [SerializeField] private float returnKnockback = 8f;

    // How long after pressing throw before the spear launches
    [SerializeField] private float startLag = 0.3f;

    // How long after catching the returned spear before the player can throw again
    [SerializeField] private float endLag = 0.5f;

    // How long the button can be held after startLag before auto-throwing at max charge
    [SerializeField] private float maxChargeTime = 1.5f;

    // Damage multiplier at full charge
    [SerializeField] private float maxDamageMultiplier = 3f;

    // Throw speed multiplier at full charge
    [SerializeField] private float maxSpeedMultiplier = 2f;

    [SerializeField] private PlayerMovement playerMovement;
    // NOVO
    [SerializeField] private OrbSpawner orbSpawner;
    //NOVO
    public Action OnParry;

    private enum SpearState { Held, WindingUp, Thrown, Stuck, Returning, Recovering }
    private SpearState state = SpearState.Held;

    public bool IsWindingUp => state == SpearState.WindingUp;

    private Transform spearTransform;
    private Rigidbody2D spearRb;
    private Collider2D spearCol;
    private Collider2D playerCol;

    // Cached held-state local transform so we can restore it on catch
    private Transform spearOriginalParent;
    private Vector3 spearLocalPos;
    private Quaternion spearLocalRot;
    private Vector3 spearLocalScale;

    // Direction from stuck spear toward player, computed when return starts
    private Vector2 knockbackDir;

    private float lagTimer;
    private float chargeTimer;

    // Damage and speed resolved at throw time based on charge
    private int currentDamage;
    private float currentThrowSpeed;

    // Tracks which enemy colliders were already damaged during a single return trip
    private readonly HashSet<int> returnHitIds = new();

    // Transform the spear is stuck to (null = stuck in static world space)
    private Transform stuckToTransform;
    private Vector3   stuckLocalPos;
    private Quaternion stuckLocalRot;

    void Start()
    {
        spearTransform = spear.transform;
        spearRb        = spear.GetComponent<Rigidbody2D>();
        spearCol       = spear.GetComponent<Collider2D>();
        playerCol      = GetComponent<Collider2D>();

        // Cache the spear's local transform while held
        spearOriginalParent = spearTransform.parent;
        spearLocalPos   = spearTransform.localPosition;
        spearLocalRot   = spearTransform.localRotation;
        spearLocalScale = spearTransform.localScale;

        currentDamage      = damage;
        currentThrowSpeed  = throwSpeed;

        // Spear physics is off while held
        spearRb.simulated = false;
        OrbManager.Instance?.RegistrarLanca(spearTransform);

        // Tell the relay who to notify on trigger hit
        SpearCollisionRelay relay = spear.GetComponent<SpearCollisionRelay>();
        if (relay != null) relay.Init(this);
        if (OrbManager.Instance != null)
            OrbManager.Instance.RegistrarPlayerShooting(this);
        else
            Debug.LogError("[PlayerShooting] OrbManager.Instance é null!");

        // Player and spear should never interact with each other's collider
        Physics2D.IgnoreCollision(spearCol, playerCol);
    }

    void Update()
    {
        switch (state)
        {
            case SpearState.WindingUp:
                if (lagTimer > 0f)
                {
                    lagTimer -= Time.deltaTime;
                }
                else
                {
                    // startLag finished — accumulate charge while button is held
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

    // Input callback

    public void Throw(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (state == SpearState.Held)
                StartWindUp();
            else if (state == SpearState.Stuck)
                StartReturn();
        }
        else if (context.canceled)
        {
            if (state == SpearState.WindingUp)
                ThrowSpear();
        }
    }

    // Wind-up then throw

    private void StartWindUp()
    {
        playerMovement.SetMovementLocked(true);
        lagTimer    = startLag;
        chargeTimer = 0f;
        state = SpearState.WindingUp;
    }

    private void ThrowSpear()
    {
        float t = (maxChargeTime > 0f) ? Mathf.Clamp01(chargeTimer / maxChargeTime) : 0f;
        currentDamage     = Mathf.RoundToInt(Mathf.Lerp(damage, damage * maxDamageMultiplier, t));
        currentThrowSpeed = Mathf.Lerp(throwSpeed, throwSpeed * maxSpeedMultiplier, t);
        chargeTimer = 0f;

        playerMovement.SetMovementLocked(false);

        // Preserve world transform before detaching from player
        Vector3    worldPos = spearTransform.position;
        Quaternion worldRot = spearTransform.rotation;

        spearTransform.SetParent(null);
        spearTransform.position   = worldPos;
        spearTransform.rotation   = worldRot;
        spearTransform.localScale = spearLocalScale;

        // PlayerAim rotated the spear toward the cursor during wind-up
        spearRb.simulated    = true;
        spearRb.gravityScale = dropIntensity;
        spearRb.velocity     = (Vector2)spearTransform.right * currentThrowSpeed;
        spearRb.angularVelocity = 0f;

        AudioManager.Instance?.TocaSFX(AudioManager.Instance.EfeitoDaLanca);
        state = SpearState.Thrown;
    }

    // Rotate spear to visually follow its arc while in flight
    private void RotateSpearToVelocity()
    {
        if (spearRb.velocity.sqrMagnitude < 0.01f) return;

        float angle = Mathf.Atan2(spearRb.velocity.y, spearRb.velocity.x) * Mathf.Rad2Deg + 180f;
        // Use the physics body's rotation so it doesn't fight the simulation
        spearRb.SetRotation(angle);
        spearRb.angularVelocity = 0f; // prevent contacts from adding spin
    }

    // Called by SpearCollisionRelay when the spear triggers something while Thrown

    public void OnSpearHit(Collider2D other)
    {
        if (state != SpearState.Thrown) return;
        Parryble parryble = other.GetComponent<Parryble>();
        if (parryble != null)
        {
            parryble.OnParried();
            OnParry?.Invoke();
            ParryReceive();
            return;
        }

        // Parry: destroy enemy projectile and immediately recall with no endlag
        if (other.CompareTag("Projectile"))
        {
            Debug.Log($"[PlayerShooting] Parry via tag Projectile: {other.gameObject.name}");
            Destroy(other.gameObject);
            Debug.Log($"[PlayerShooting] OnParry tem {OnParry?.GetInvocationList().Length ?? 0} listeners");
            OnParry?.Invoke();
            ParryReceive();
            return;
        }

        // Hit an enemy: deal damage first, then decide whether to stick or fall
        EnemyHealthController enemyHealth = other.GetComponent<EnemyHealthController>()
            ?? other.GetComponentInParent<EnemyHealthController>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(currentDamage);
            if (enemyHealth.currentHealth <= 0)
            {
                // Enemy died: drop straight down instead of sticking
                spearRb.velocity        = new Vector2(0f, spearRb.velocity.y);
                spearRb.angularVelocity = 0f;
                // Keep simulated = true so gravity pulls the spear to the ground
                return;
            }
        }

        // Stick the spear to whatever it hit so it tracks moving objects
        stuckToTransform = other.transform;
        stuckLocalPos    = stuckToTransform.InverseTransformPoint(spearTransform.position);
        stuckLocalRot    = Quaternion.Inverse(stuckToTransform.rotation) * spearTransform.rotation;
        spearRb.velocity        = Vector2.zero;
        spearRb.angularVelocity = 0f;
        spearRb.simulated       = false;
        state = SpearState.Stuck;
    }

    // Instant recall used by parry
    private void ParryReceive()
    {
        spearRb.simulated = false;
        spearTransform.SetParent(spearOriginalParent);
        spearTransform.localPosition = spearLocalPos;
        spearTransform.localRotation = spearLocalRot;
        spearTransform.localScale    = spearLocalScale;
        state = SpearState.Held;
    }

    //  Return

    private void StartReturn()
    {
        returnHitIds.Clear();
        stuckToTransform = null;

        // Direction FROM spear TO player
        knockbackDir = ((Vector2)transform.position - (Vector2)spearTransform.position).normalized;

        spearRb.simulated = false;
        state = SpearState.Returning;
    }

    private void MoveSpearToPlayer()
    {
        Vector2 toPlayer = (Vector2)transform.position - (Vector2)spearTransform.position;
        float   step     = returnSpeed * Time.deltaTime;

        if (toPlayer.magnitude <= step)
        {
            ReceiveSpear();
            return;
        }

        // Move toward player, handle first
        spearTransform.position = Vector2.MoveTowards(spearTransform.position, transform.position, step);

        // Point handle toward player: tip (+X) faces away from player, handle (-X) faces toward player
        Vector2 fromPlayer = (Vector2)spearTransform.position - (Vector2)transform.position;
        float angle = Mathf.Atan2(fromPlayer.y, fromPlayer.x) * Mathf.Rad2Deg;
        spearTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Deal damage to any enemies the returning spear passes through (once per enemy per trip)
        Collider2D[] hits = Physics2D.OverlapCircleAll(spearTransform.position, 0.5f);
        foreach (Collider2D hit in hits)
        {
            if (hit == playerCol || hit == spearCol) continue;
            if (returnHitIds.Contains(hit.GetInstanceID())) continue;

            if (!hit.TryGetComponent(out EnemyHealthController enemyHealth)) continue;

            returnHitIds.Add(hit.GetInstanceID());
            enemyHealth.TakeDamage(currentDamage);
        }
    }

    private void ReceiveSpear()
    {
        // Only knock the player back if they are airborne
        if (!playerMovement.IsGrounded())
            playerMovement.Knockback(knockbackDir * returnKnockback);

        // Re-attach spear to the player in its original local transform
        spearTransform.SetParent(spearOriginalParent);
        spearTransform.localPosition = spearLocalPos;
        spearTransform.localRotation = spearLocalRot;
        spearTransform.localScale    = spearLocalScale;

        spearRb.simulated = false;

        if (endLag <= 0f)
            state = SpearState.Held;
        else
        {
            lagTimer = endLag;
            playerMovement.SetMovementLocked(true);
            state = SpearState.Recovering;
        }
    }
}
