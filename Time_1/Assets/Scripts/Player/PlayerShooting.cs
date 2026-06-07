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

    // Tracks which enemy colliders were already damaged during a single return trip
    private readonly HashSet<int> returnHitIds = new();

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
                lagTimer -= Time.deltaTime;
                if (lagTimer <= 0f)
                    ThrowSpear();
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
        if (!context.performed) return;

        if (state == SpearState.Held)
            StartWindUp();
        else if (state == SpearState.Stuck)
            StartReturn();
    }

    // Wind-up then throw

    private void StartWindUp()
    {
        if (startLag <= 0f)
        {
            ThrowSpear();
            return;
        }
        playerMovement.SetMovementLocked(true);
        lagTimer = startLag;
        state = SpearState.WindingUp;
    }

    private void ThrowSpear()
    {
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
        spearRb.velocity     = (Vector2)spearTransform.right * throwSpeed;
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
            enemyHealth.TakeDamage(damage);
            if (enemyHealth.currentHealth <= 0)
            {
                // Enemy died: drop straight down instead of sticking
                spearRb.velocity        = new Vector2(0f, spearRb.velocity.y);
                spearRb.angularVelocity = 0f;
                // Keep simulated = true so gravity pulls the spear to the ground
                return;
            }
        }

        // Stick the spear in world space
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

        // Direction FROM spear TO player
        knockbackDir = ((Vector2)transform.position - (Vector2)spearTransform.position).normalized;

        // Spear is already frozen in world space
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
            enemyHealth.TakeDamage(damage);
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
