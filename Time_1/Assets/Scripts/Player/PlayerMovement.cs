using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Jump Feel")]
    [SerializeField] private float fallGravityMultiplier = 3f;      // Extra gravity while falling
    [SerializeField] private float jumpCutGravityMultiplier = 2f;   // Extra gravity when jump key released early
    [SerializeField] private float jumpCutVelocity = 3f;            // Upward velocity is clamped to this when jump key is released early
    [SerializeField] private float ascendGravityMultiplier = 1.5f;  // Extra gravity while ascending with jump held

    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask groundLayer;

    [Header("Knockback")]
    // Rate at which horizontal knockback decays.
    // ~20 matches the default vertical deceleration from gravity, giving both axes equal duration.
    [SerializeField] private float knockbackDecay = 20f;

    public float HorizontalInput => horizontalMovement;

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");

    private float horizontalMovement;
    private float verticalInput;
    private bool movementLocked;
    private bool hasJumped;      // true only after a player-initiated jump; gates the jumpCut branch
    private float knockbackVelocityX;
    private bool isWalking;

    void Start()
    {
        // Zero friction prevents the player from sticking to walls.
        col.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
    }
    // Called by PlayerShooting when the spear is caught.
    public void Knockback(Vector2 velocity)
    {
        rb.velocity        = velocity;
        knockbackVelocityX = velocity.x;
        hasJumped          = false;
    }
    void Update()
    {
        // Decay horizontal knockback toward zero, independent of player input direction.
        knockbackVelocityX = Mathf.MoveTowards(knockbackVelocityX, 0f, knockbackDecay * Time.deltaTime);
        // Final X = decaying knockback + player-controlled movement (zero if locked during end lag).
        float inputX = movementLocked ? 0f : horizontalMovement;
        rb.velocity = new Vector2(knockbackVelocityX + inputX * moveSpeed, rb.velocity.y);

        bool walking = inputX != 0f;
        if (walking != isWalking)
        {
            isWalking = walking;
            if (animator != null)
                animator.SetBool(IsWalkingHash, isWalking);
        }
        if (rb.velocity.y < 0)
        {
            hasJumped = false; 
            rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallGravityMultiplier - 1) * Time.deltaTime);
        }
        else if (rb.velocity.y > 0)
        {
            if (verticalInput <= 0 || !hasJumped)
            {
                if (hasJumped)
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, jumpCutVelocity));

                rb.velocity += Vector2.up * (Physics2D.gravity.y * (jumpCutGravityMultiplier - 1) * Time.deltaTime);
            }
            else
            {
                rb.velocity += Vector2.up * (Physics2D.gravity.y * (ascendGravityMultiplier - 1) * Time.deltaTime);
            }
        }
    }
    public void SetMovementLocked(bool locked) => movementLocked = locked;

    public void LockPreservingMomentum()
    {
        knockbackVelocityX = rb.velocity.x;
        movementLocked = true;
    }
    public bool IsGrounded()
    {
        Bounds bounds = col.bounds;
        return Physics2D.OverlapBox(
            new Vector2(bounds.center.x, bounds.min.y),
            new Vector2(bounds.size.x * 0.9f, 0.1f),
            0f,
            groundLayer
        );
    }
    public void Move(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        horizontalMovement = input.x;
        verticalInput = input.y;
        if (context.performed && input.y > 0 && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            hasJumped = true;
            if (AudioManager.Instance != null)
                AudioManager.Instance.TocaSFX(AudioManager.Instance.EfeitoDePulo);
        }
    }
}
