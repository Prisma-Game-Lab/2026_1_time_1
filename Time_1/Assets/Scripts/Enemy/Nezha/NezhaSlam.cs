using System.Collections;
using UnityEngine;

public class NezhaSlam : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NezhaMovement movement;
    [SerializeField] private Rigidbody2D   rb;
    [SerializeField] private Transform     centerTransform;
    [SerializeField] private Transform     spawnPoint;
    [SerializeField] private GameObject    shockwavePrefab;
    [SerializeField] private GameObject    fireballPrefab;

    [Header("Walk to Center")]
    [SerializeField] private float centerThreshold = 0.3f;
    [SerializeField] private float walkTimeout     = 3f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 18f;
    [SerializeField] private float apexTimeout = 3f;

    [Header("Slam")]
    [SerializeField] private float hangTime    = 0.3f;
    [SerializeField] private float slamSpeed   = 30f;
    [SerializeField] private float slamTimeout = 3f;

    [Header("Shockwave")]
    [SerializeField] private int shockwaveCount = 2;

    [Header("Fireballs")]
    [SerializeField] private int   fireballCount       = 5;
    [SerializeField] private float fireballSpread      = 70f;
    [SerializeField] private float fireballLaunchSpeed = 20f;

    [Header("Recovery")]
    [SerializeField] private float recoveryTime = 0.5f;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        if (movement == null) movement = GetComponent<NezhaMovement>();
        if (rb       == null) rb       = GetComponent<Rigidbody2D>();
    }

    public void Iniciar() => StartCoroutine(Routine());

    private IEnumerator Routine()
    {
        IsAttacking = true;

        // Walk to arena center
        float elapsed = 0f;
        while (centerTransform != null &&
               Mathf.Abs(transform.position.x - centerTransform.position.x) > centerThreshold &&
               elapsed < walkTimeout)
        {
            movement.WalkToX(centerTransform.position.x);
            elapsed += Time.deltaTime;
            yield return null;
        }
        movement.Stop();
        movement.FacePlayer();

        // Jump straight up
        movement.Jump(jumpForce);

        elapsed = 0f;
        while (!movement.IsFalling && elapsed < apexTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        movement.FreezeInAir();
        yield return new WaitForSeconds(hangTime);

        // Slam down
        Collider2D[] myCols = GetComponents<Collider2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Collider2D[] playerCols = playerObj != null
            ? playerObj.GetComponentsInChildren<Collider2D>()
            : System.Array.Empty<Collider2D>();
        foreach (var mc in myCols)
            foreach (var pc in playerCols)
                Physics2D.IgnoreCollision(mc, pc, true);

        movement.ReleaseFromAir();
        if (rb != null) rb.velocity = Vector2.down * slamSpeed;

        elapsed = 0f;
        while (!movement.IsGrounded && elapsed < slamTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        movement.Stop();

        foreach (var mc in myCols)
            foreach (var pc in playerCols)
                Physics2D.IgnoreCollision(mc, pc, false);

        // Spawn effects 
        Vector2 spawnPos = spawnPoint != null
            ? (Vector2)spawnPoint.position
            : (Vector2)transform.position;

        // Shockwaves 
        if (shockwavePrefab != null)
        {
            for (int i = 0; i < shockwaveCount; i++)
            {
                Vector2 dir = i % 2 == 0 ? Vector2.right : Vector2.left;
                GameObject sw = Instantiate(shockwavePrefab, spawnPos, Quaternion.identity);
                if (sw.TryGetComponent(out Shockwave s)) s.Launch(dir);
            }
        }

        // Fireballs
        if (fireballPrefab != null)
        {
            float halfSpread = fireballSpread * 0.5f;
            for (int i = 0; i < fireballCount; i++)
            {
                float   angle  = 90f + Random.Range(-halfSpread, halfSpread);
                float   rad    = angle * Mathf.Deg2Rad;
                Vector2 dir    = new(Mathf.Cos(rad), Mathf.Sin(rad));

                GameObject fb = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
                if (fb.TryGetComponent(out Fireball fireball))
                    fireball.LaunchWithVelocity(dir * fireballLaunchSpeed);
            }
        }

        yield return new WaitForSeconds(recoveryTime);
        IsAttacking = false;
    }
}
