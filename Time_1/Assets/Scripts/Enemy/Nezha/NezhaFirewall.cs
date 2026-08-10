using System.Collections;
using UnityEngine;

public class NezhaFirewall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NezhaMovement movement;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject firewallPrefab;

    [Header("Walk to Corner")]
    [SerializeField] private float edgeMargin = 1.5f;
    [SerializeField] private float cornerThreshold = 0.4f;
    [SerializeField] private float walkTimeout = 4f;

    [Header("Charge")]
    [SerializeField] private Sprite chargeSprite;
    [SerializeField] private float chargeTime = 1.2f;

    [Header("Wall Segments")]
    [Tooltip("Number of firewall segments spawned.")]
    [SerializeField] private int segmentCount = 6;
    [Tooltip("Horizontal spacing between each segment.")]
    [SerializeField] private float segmentSpacing = 1.5f;
    [Tooltip("Delay in seconds between spawning each segment.")]
    [SerializeField] private float spawnDelay = 0.15f;
    [Tooltip("Vertical offset applied to every segment. Negative = lower.")]
    [SerializeField] private float spawnYOffset = -0.5f;

    [Header("Recovery")]
    [SerializeField] private float recoveryTime = 0.6f;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        if (movement == null) movement = GetComponent<NezhaMovement>();
        if (cam == null) cam = Camera.main;

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    public void Iniciar() => StartCoroutine(Routine());

    private IEnumerator Routine()
    {
        IsAttacking = true;

        float targetX = PickCornerX();

        // Walk to corner
        float elapsed = 0f;
        while (Mathf.Abs(transform.position.x - targetX) > cornerThreshold && elapsed < walkTimeout)
        {
            movement.WalkToX(targetX);
            elapsed += Time.deltaTime;
            yield return null;
        }
        movement.Stop();
        movement.FacePlayer();

        // Charge pose
        Sprite originalSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        if (spriteRenderer != null && chargeSprite != null)
            spriteRenderer.sprite = chargeSprite;

        yield return new WaitForSeconds(chargeTime);

        // Restore sprite before spawning
        if (spriteRenderer != null && originalSprite != null)
            spriteRenderer.sprite = originalSprite;

        // Spawn wall segments horizontally from the corner toward center
        if (firewallPrefab != null)
        {
            // Spread inward from the corner Nezha is standing at
            Camera c = cam != null ? cam : Camera.main;
            float centerX = c != null ? c.transform.position.x : 0f;
            float spreadDir = transform.position.x < centerX ? 1f : -1f;
            Vector2 basePos = new Vector2(transform.position.x, transform.position.y + spawnYOffset);
            for (int i = 0; i < segmentCount; i++)
            {
                Vector2 pos = basePos + Vector2.right * (spreadDir * i * segmentSpacing);
                Instantiate(firewallPrefab, pos, Quaternion.identity);
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        yield return new WaitForSeconds(recoveryTime);
        IsAttacking = false;
    }

    private float PickCornerX()
    {
        Camera c = cam != null ? cam : Camera.main;
        if (c == null) return transform.position.x;

        float halfWidth = c.orthographicSize * c.aspect;
        float centerX = c.transform.position.x;

        // Go to the corner farthest from the player.
        bool playerIsRight = playerTransform != null && playerTransform.position.x > centerX;
        return playerIsRight
            ? centerX - halfWidth + edgeMargin
            : centerX + halfWidth - edgeMargin;
    }
}
