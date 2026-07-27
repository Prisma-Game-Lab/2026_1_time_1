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

    [Header("Jump (disparo)")]
    [SerializeField] private float jumpForce = 15f;

    [Header("Contornar o player (pulo por cima)")]
    [Tooltip("Se o player estiver a menos disto no caminho ate a ponta, o boss pula por cima em vez de empurrar.")]
    [SerializeField] private float jumpTriggerDist = 2.2f;
    [Tooltip("Diferenca de altura maxima pra considerar o player 'no caminho' (mesmo nivel).")]
    [SerializeField] private float mesmoNivelThreshold = 1.5f;
    [Tooltip("Velocidade horizontal do pulo por cima.")]
    [SerializeField] private float jumpOverHorizSpeed = 6f;
    [Tooltip("Forca vertical do pulo por cima.")]
    [SerializeField] private float jumpOverForce = 14f;
    [Tooltip("Tempo maximo esperando o pulo aterrissar.")]
    [SerializeField] private float jumpOverTimeout = 1.5f;

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

        // Vai para a PONTA MAIS LONGE do player (em vez de um canto aleatorio).
        float cornerX = GetFarCornerX();
        float elapsed = 0f;

        while (Mathf.Abs(transform.position.x - cornerX) > 0.3f && elapsed < walkTimeout)
        {
            float dir = Mathf.Sign(cornerX - transform.position.x);

            if (PlayerNoCaminho(dir) && movement.IsGrounded)
            {
                // Player no caminho: pula em ARCO por cima dele em vez de empurrar.
                movement.PuloArco(dir, jumpOverHorizSpeed, jumpOverForce);
                yield return new WaitForSeconds(0.1f); // deixa descolar do chao

                float jt = 0.1f;
                while (jt < jumpOverTimeout && !movement.IsGrounded)
                {
                    jt += Time.deltaTime;
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                movement.WalkToX(cornerX);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        movement.Stop();

        // Pula pra cima e congela no ar durante o tiro (igual antes).
        movement.FacePlayer();
        movement.Jump(jumpForce);
        movement.Stop();

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

        movement.ReleaseFromAir();

        float landElapsed = 0f;
        while (!movement.IsGrounded && landElapsed < landTimeout)
        {
            landElapsed += Time.deltaTime;
            yield return null;
        }

        IsAttacking = false;
    }

    // Ponta (borda da tela) MAIS DISTANTE do player.
    private float GetFarCornerX()
    {
        Camera c = cam != null ? cam : Camera.main;
        if (c == null) return transform.position.x;

        float halfWidth = c.orthographicSize * c.aspect;
        float centerX   = c.transform.position.x;
        float leftX     = centerX - halfWidth + edgeMargin;
        float rightX    = centerX + halfWidth - edgeMargin;

        if (playerTransform == null)
            return Random.value < 0.5f ? leftX : rightX;

        float px = playerTransform.position.x;
        // A ponta mais longe e a que tem maior distancia horizontal ate o player.
        return Mathf.Abs(px - leftX) >= Mathf.Abs(px - rightX) ? leftX : rightX;
    }

    // True se o player esta A FRENTE (na direcao 'dir'), perto, e mais ou menos no mesmo nivel.
    private bool PlayerNoCaminho(float dir)
    {
        if (playerTransform == null) return false;
        float dx = playerTransform.position.x - transform.position.x;
        if (Mathf.Sign(dx) != dir) return false;
        if (Mathf.Abs(dx) > jumpTriggerDist) return false;
        return Mathf.Abs(playerTransform.position.y - transform.position.y) < mesmoNivelThreshold;
    }
}
