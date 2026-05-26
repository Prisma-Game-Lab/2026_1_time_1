using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject spear;

    [SerializeField] private float throwSpeed = 15f;

    [SerializeField] private float returnSpeed = 10f;

    // Gravity scale applied to the spear when thrown
    [SerializeField] private float dropIntensity = 1f;

    // Velocity magnitude applied to the player on spear catch
    [SerializeField] private float returnKnockback = 8f;

    [SerializeField] private PlayerMovement playerMovement;

    private enum SpearState { Held, Thrown, Stuck, Returning }
    private SpearState state = SpearState.Held;

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

        // Tell the relay who to notify on trigger hit
        SpearCollisionRelay relay = spear.GetComponent<SpearCollisionRelay>();
        if (relay != null) relay.Init(this);

        // Player and spear should never interact with each other's collider
        Physics2D.IgnoreCollision(spearCol, playerCol);
    }

    void Update()
    {
        switch (state)
        {
            case SpearState.Thrown:
                RotateSpearToVelocity();
                break;

            case SpearState.Returning:
                MoveSpearToPlayer();
                break;
        }
    }

    //Input callback

    public void Throw(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (state == SpearState.Held)
            ThrowSpear();
        else if (state == SpearState.Stuck)
            StartReturn();
    }

    // Throw 

    private void ThrowSpear()
    {
        // Preserve world transform before detaching from player
        Vector3    worldPos = spearTransform.position;
        Quaternion worldRot = spearTransform.rotation;

        spearTransform.SetParent(null);
        spearTransform.position   = worldPos;
        spearTransform.rotation   = worldRot;
        spearTransform.localScale = spearLocalScale;

        // PlayerAim already rotated the spear toward the cursor
        spearRb.simulated    = true;
        spearRb.gravityScale = dropIntensity;
        spearRb.velocity     = (Vector2)spearTransform.right * throwSpeed;
        spearRb.angularVelocity = 0f;

        state = SpearState.Thrown;
    }

    // Rotate spear to visually follow its arc while in flight
    private void RotateSpearToVelocity()
    {
        if (spearRb.velocity.sqrMagnitude < 0.01f) return;

        float angle = Mathf.Atan2(spearRb.velocity.y, spearRb.velocity.x) * Mathf.Rad2Deg;
        // Use the physics body's rotation so it doesn't fight the simulation
        spearRb.SetRotation(angle);
        spearRb.angularVelocity = 0f; // prevent contacts from adding spin
    }

    // ── Stick (called by SpearCollisionRelay) ────────────────────────────────────

    public void OnSpearHit(Collider2D other)
    {
        if (state != SpearState.Thrown) return;

        // Freeze the spear in world space — no parenting avoids any scale/rotation distortion
        spearRb.velocity        = Vector2.zero;
        spearRb.angularVelocity = 0f;
        spearRb.simulated       = false;

        state = SpearState.Stuck;
    }

    // ── Return ───────────────────────────────────────────────────────────────────

    private void StartReturn()
    {
        // Direction FROM spear TO player — this is the knockback direction on catch
        knockbackDir = ((Vector2)transform.position - (Vector2)spearTransform.position).normalized;

        // Spear is already frozen in world space (no parent to detach from)
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

        state = SpearState.Held;
    }
}
