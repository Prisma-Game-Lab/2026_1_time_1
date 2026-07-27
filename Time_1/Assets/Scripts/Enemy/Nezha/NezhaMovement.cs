using UnityEngine;

public class NezhaMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform playerTransform;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;

    public bool IsGrounded { get; private set; }
    public bool IsFalling => rb != null && rb.velocity.y < -0.1f;
    public bool IsHovering { get; private set; }

    private float savedGravityScale;

    private void Awake()
    {
        // Guarda a gravidade original logo de cara, para o hover/freeze restaurarem certo.
        if (rb != null) savedGravityScale = rb.gravityScale;
    }

    public void FreezeInAir()
    {
        if (rb == null) return;
        // So salva a gravidade se ainda nao estiver flutuando (evita salvar 0 por engano).
        if (!IsHovering) savedGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        IsHovering = true;
    }

    public void ReleaseFromAir()
    {
        if (rb == null) return;
        rb.gravityScale = savedGravityScale;
        IsHovering = false;
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

    // Drift aereo em direcao a um ponto enquanto flutua.
    // Assume que a gravidade ja foi zerada via FreezeInAir().
    public void HoverTowards(float targetX, float targetY, float horizSpeed, float vertSpeed)
    {
        if (rb == null) return;

        float dx = targetX - transform.position.x;
        float dy = targetY - transform.position.y;

        float vx = Mathf.Abs(dx) < 0.15f ? 0f : Mathf.Sign(dx) * horizSpeed;
        // Aproxima suavemente da altura de voo (satura para nao ficar teleportando).
        float vy = Mathf.Abs(dy) < 0.15f ? 0f : Mathf.Clamp(dy, -1f, 1f) * vertSpeed;

        rb.velocity = new Vector2(vx, vy);

        if (playerTransform != null)
            FaceDirection(Mathf.Sign(playerTransform.position.x - transform.position.x));
    }

    public void Jump(float force)
    {
        if (rb == null) return;
        rb.velocity = new Vector2(rb.velocity.x, force);
    }

    // Pulo em arco: sobe COM velocidade horizontal, pra contornar o player por cima em vez de empurrar.
    public void PuloArco(float dirX, float horizSpeed, float jumpForce)
    {
        if (rb == null) return;
        rb.velocity = new Vector2(dirX * horizSpeed, jumpForce);
        FaceDirection(dirX);
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