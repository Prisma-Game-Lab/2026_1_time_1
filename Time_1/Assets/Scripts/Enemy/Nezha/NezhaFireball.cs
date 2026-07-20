using System.Collections;
using UnityEngine;

public class NezhaFireball : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NezhaMovement movement;
    [SerializeField] private Transform     playerTransform;
    [SerializeField] private GameObject    fireballPrefab;
    [SerializeField] private Transform     spawnPoint;
    [SerializeField] private Camera        cam;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 15f;

    [Header("Fireballs")]
    [SerializeField] private int   count         = 5;
    [SerializeField] private float spreadAngle   = 50f;
    [SerializeField] private float aimHeightOffset = 1f;

    [Header("Waves")]
    [SerializeField] private int   waves        = 2;
    [SerializeField] private float waveInterval = 0.8f;

    [Header("Positioning")]
    [SerializeField] private float edgeMargin  = 1.5f;
    [SerializeField] private float walkTimeout = 3f;
    [SerializeField] private float landTimeout = 5f;

    public bool IsAttacking { get; private set; }

    private void Start()
    {
        if (cam == null) cam = Camera.main;
    }

    public void Iniciar()
    {
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        IsAttacking = true;

        // Move to a corner of the player's view
        float cornerX = GetCornerX();
        float elapsed  = 0f;
        while (Mathf.Abs(transform.position.x - cornerX) > 0.3f && elapsed < walkTimeout)
        {
            movement.WalkToX(cornerX);
            elapsed += Time.deltaTime;
            yield return null;
        }
        movement.Stop();

        // Jump straight up from the corner
        movement.FacePlayer();
        movement.Jump(jumpForce);
        movement.Stop();

        // Wait for apex, then freeze in the air for the entire shooting phase
        yield return new WaitUntil(() => movement.IsFalling);
        movement.FreezeInAir();

        float halfSpread = spreadAngle * 0.5f;
        float step       = count > 1 ? spreadAngle / (count - 1) : 0f;

        for (int wave = 0; wave < waves; wave++)
        {
            movement.FacePlayer();

            Vector2 spawnPos    = spawnPoint != null ? (Vector2)spawnPoint.position : (Vector2)transform.position;
            Vector2 aimTarget   = (Vector2)playerTransform.position + Vector2.up * aimHeightOffset;
            Vector2 toPlayer    = (aimTarget - spawnPos).normalized;
            float   centerAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

            // Fire all fireballs in this wave simultaneously
            for (int i = 0; i < count; i++)
            {
                float   angle = centerAngle + halfSpread - i * step;
                float   rad   = angle * Mathf.Deg2Rad;
                Vector2 dir   = new(Mathf.Cos(rad), Mathf.Sin(rad));

                GameObject fb = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
                if (fb.TryGetComponent(out Fireball fireball))
                    fireball.Launch(dir);
            }

            if (wave < waves - 1)
                yield return new WaitForSeconds(waveInterval);
        }

        // Release hover and wait for Nezha to land
        movement.ReleaseFromAir();

        float landElapsed = 0f;
        while (!movement.IsGrounded && landElapsed < landTimeout)
        {
            landElapsed += Time.deltaTime;
            yield return null;
        }

        IsAttacking = false;
    }

    private float GetCornerX()
    {
        Camera c = cam != null ? cam : Camera.main;
        if (c == null) return transform.position.x;
        float halfWidth = c.orthographicSize * c.aspect;
        float centerX   = c.transform.position.x;
        bool  goLeft    = Random.value < 0.5f;
        return goLeft
            ? centerX - halfWidth + edgeMargin
            : centerX + halfWidth - edgeMargin;
    }
}
