using UnityEngine;

public class NezhaMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D  col;
    [SerializeField] private LayerMask   groundLayer;
    [SerializeField] private Transform   playerTransform;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;

    public bool IsGrounded { get; private set; }
    public bool IsFalling  => rb != null && rb.velocity.y < -0.1f;

    private float savedGravityScale;

    public void FreezeInAir()
    {
        if (rb == null) return;
        savedGravityScale  = rb.gravityScale;
        rb.gravityScale    = 0f;
        rb.velocity        = Vector2.zero;
    }

    public void ReleaseFromAir()
    {
        if (rb == null) return;
        rb.gravityScale = savedGravityScale;
    }

    private void Update()
    {
        if (col == null || rb == null) return;

        Bounds b = col.bounds;
        IsGrounded = Physics2D.OverlapBox(
            new Vector2(b.center.x, b.min.y),
            new Vector2(b.size.x * 0.9f, 0.1f),
            0f,
            groundLayer
        );
    }

    public void WalkTowardsPlayer()
    {
        if (rb == null || playerTransform == null) return;
        float dir = Mathf.Sign(playerTransform.position.x - transform.position.x);
        rb.velocity = new Vector2(dir * walkSpeed, rb.velocity.y);
        FaceDirection(dir);
    }

    public void WalkToX(float targetX)
    {
        if (rb == null) return;
        float delta = targetX - transform.position.x;
        if (Mathf.Abs(delta) < 0.1f) { Stop(); return; }
        float dir = Mathf.Sign(delta);
        rb.velocity = new Vector2(dir * walkSpeed, rb.velocity.y);
        FaceDirection(dir);
    }

    public void Jump(float force)
    {
        if (rb == null) return;
        rb.velocity = new Vector2(rb.velocity.x, force);
    }

    public void Stop()
    {
        if (rb == null) return;
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    public void FacePlayer()
    {
        if (playerTransform == null) return;
        FaceDirection(Mathf.Sign(playerTransform.position.x - transform.position.x));
    }

    private void FaceDirection(float dir)
    {
        
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (dir >= 0f ? -1f : 1f);
        transform.localScale = s;
    }
}
