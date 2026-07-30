using System.Collections;
using UnityEngine;

public class NezhaChute : MonoBehaviour
{
    [Header("Refer�ncias")]
    [SerializeField] private NezhaMovement movement;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    [Tooltip("Ponto de origem do teste de acerto. Se vazio, usa este transform.")]
    [SerializeField] private Transform pontoAcerto;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite kickingSprite;

    [Header("Tempo")]
    [Tooltip("Espera antes de partir (telegrafo). � o 'tempo x' ajust�vel.")]
    [SerializeField] private float tempoPreparacao = 0.6f;
    [Tooltip("Dura��o do avan�o do chute.")]
    [SerializeField] private float duracaoChute = 0.4f;
    [Tooltip("Recupera��o depois do chute.")]
    [SerializeField] private float recuperacao = 0.4f;

    [Header("Chute")]
    [Tooltip("Velocidade do avan�o (absurda).")]
    [SerializeField] private float velocidadeChute = 40f;
    [SerializeField] private float raioAcerto = 0.8f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Dano / Knockback")]
    [SerializeField] private int dano = 1;
    [SerializeField] private float knockbackForca = 14f;
    [SerializeField] private float knockbackCima = 4f;
    [Tooltip("Se marcado, o dano usa tag 'Melee' (pode ser parryado). Desmarque para chute impar�vel.")]
    [SerializeField] private bool parryavel = true;
    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        if (movement == null) movement = GetComponent<NezhaMovement>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }
    public void Iniciar()
    {
        StartCoroutine(Routine());
    }
    private IEnumerator Routine()
    {
        IsAttacking = true;

        Sprite originalSprite = spriteRenderer != null ? spriteRenderer.sprite : null;

        // Telegrafo
        movement.Stop();
        movement.FacePlayer();
        yield return new WaitForSeconds(tempoPreparacao);

        // Dire��o capturada no in�cio do avan�o
        Vector2 origem = transform.position;
        Vector2 dir = ((Vector2)playerTransform.position - origem);
        dir = dir.sqrMagnitude > 0.01f ? dir.normalized : Vector2.right;
        movement.FacePlayer();

        // Avan�o reto (sem gravidade) na velocidade absurda
        if (spriteRenderer != null && kickingSprite != null)
            spriteRenderer.sprite = kickingSprite;

        movement.FreezeInAir();
        rb.velocity = dir * velocidadeChute;

        bool acertou = false;
        float t = 0f;
        Vector2 pontoAnterior = PontoAcerto();
        while (t < duracaoChute)
        {
            if (!acertou)
            {
                Vector2 pontoAtual = PontoAcerto();
                Collider2D hit = VarrerAcerto(pontoAnterior, pontoAtual);
                if (hit != null)
                {
                    Vector2 kb = new Vector2(Mathf.Sign(dir.x == 0 ? 1f : dir.x) * knockbackForca, knockbackCima);
                    AplicarDano(hit, dano, kb);
                    acertou = true;
                    rb.velocity *= 0.15f; // "trava" o chute ao conectar
                }
                pontoAnterior = pontoAtual;
            }
            t += Time.deltaTime;
            yield return null;
        }
        movement.ReleaseFromAir();
        movement.Stop();
        if (spriteRenderer != null && originalSprite != null)
            spriteRenderer.sprite = originalSprite;
        yield return new WaitForSeconds(recuperacao);

        IsAttacking = false;
    }
    private void AplicarDano(Collider2D playerCol, int qtd, Vector2 knockback)
    {
        var hp = playerCol.GetComponent<PlayerHealthController>() ?? playerCol.GetComponentInParent<PlayerHealthController>();
        if (hp == null) return;
        if (hp.IsInvincible) return;

        if (parryavel) hp.TakeDamage(qtd, "Melee");
        else hp.TakeDamage(qtd);

        var pm = playerCol.GetComponent<PlayerMovement>() ?? playerCol.GetComponentInParent<PlayerMovement>();
        if (pm != null) pm.Knockback(knockback);
    }
    private Vector2 PontoAcerto() => pontoAcerto != null ? (Vector2)pontoAcerto.position : (Vector2)transform.position;

    // Varre o caminho entre o frame anterior e o atual (evita tunneling em velocidade alta).
    private Collider2D VarrerAcerto(Vector2 de, Vector2 ate)
    {
        Collider2D o = FindPlayerInOverlap(Physics2D.OverlapCircleAll(ate, raioAcerto));
        if (o != null) return o;

        Vector2 delta = ate - de;
        float dist = delta.magnitude;
        if (dist > 0.001f)
        {
            foreach (RaycastHit2D h in Physics2D.CircleCastAll(de, raioAcerto, delta / dist, dist))
                if (h.collider.CompareTag("Player")) return h.collider;
        }
        return null;
    }

    private static Collider2D FindPlayerInOverlap(Collider2D[] cols)
    {
        foreach (var c in cols)
            if (c.CompareTag("Player")) return c;
        return null;
    }
    private void OnDrawGizmosSelected()
    {
        Vector3 c = pontoAcerto != null ? pontoAcerto.position : transform.position;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(c, raioAcerto);
    }
}