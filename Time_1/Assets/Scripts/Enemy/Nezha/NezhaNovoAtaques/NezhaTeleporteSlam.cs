using System.Collections;
using UnityEngine;

public class NezhaTeleporteSlam : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private NezhaMovement movement;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    [Tooltip("Ponto de origem do teste de acerto do esmagamento. Se vazio, usa este transform.")]
    [SerializeField] private Transform pontoAcerto;

    [Header("Parte 1 — teleporte atrás + arremesso")]
    [Tooltip("Deslocamento relativo ao player para 'atrás dele'. Ajuste o sinal/altura como quiser.")]
    [SerializeField] private Vector2 deslocamentoAtras = new Vector2(0f, -1f);
    [Tooltip("Força que joga o player para cima.")]
    [SerializeField] private float forcaParaCima = 18f;
    [SerializeField] private float forcaLateralParte1 = 0f;
    [Tooltip("Se marcado, a Parte 1 também dá dano (padrão: não dá).")]
    [SerializeField] private bool parte1DaDano = false;
    [SerializeField] private int danoParte1 = 1;

    [Header("Espera entre as partes")]
    [Tooltip("Tempo com o player subindo antes do esmagamento.")]
    [SerializeField] private float tempoAntesDoSlam = 0.35f;

    [Header("Parte 2 — esmagamento (sempre dá dano)")]
    [Tooltip("Altura acima do player para onde Nezha teleporta antes de descer.")]
    [SerializeField] private float alturaAcima = 4f;
    [Tooltip("Velocidade da descida (absurda).")]
    [SerializeField] private float velocidadeSlam = 45f;
    [SerializeField] private float duracaoSlam = 0.5f;
    [SerializeField] private float raioAcerto = 0.8f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int danoSlam = 1;
    [SerializeField] private float knockbackLateral = 6f;
    [SerializeField] private float knockbackVertical = 8f;
    [Tooltip("Se marcado, o dano usa tag 'Melee' (pode ser parryado).")]
    [SerializeField] private bool parryavel = true;

    [Header("Recuperação")]
    [SerializeField] private float recuperacao = 0.4f;

    [Header("Limites da arena (evita sair do mapa)")]
    [Tooltip("Se marcado, prende os teleportes dentro da arena.")]
    [SerializeField] private bool prenderNaArena = true;
    [Tooltip("Câmera usada para calcular os limites. Vazio = Camera.main.")]
    [SerializeField] private Camera cam;
    [Tooltip("Margem para dentro das bordas (o boss não cola na borda).")]
    [SerializeField] private float margem = 1f;
    [Tooltip("Opcional: retângulo exato da arena. Se 'Limite Max' > 'Limite Min', usa estes valores no lugar da câmera.")]
    [SerializeField] private Vector2 limiteMin;
    [SerializeField] private Vector2 limiteMax;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        if (movement == null) movement = GetComponent<NezhaMovement>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (cam == null) cam = Camera.main;
    }
    private Vector2 PrenderNaArena(Vector2 pos)
    {
        if (!prenderNaArena) return pos;

        float minX, maxX, minY, maxY;
        if (limiteMax.x > limiteMin.x && limiteMax.y > limiteMin.y)
        {
            minX = limiteMin.x; maxX = limiteMax.x;
            minY = limiteMin.y; maxY = limiteMax.y;
        }
        else if (cam != null)
        {
            float hw = cam.orthographicSize * cam.aspect;
            float hh = cam.orthographicSize;
            Vector2 c = cam.transform.position;
            minX = c.x - hw; maxX = c.x + hw;
            minY = c.y - hh; maxY = c.y + hh;
        }
        else return pos;

        pos.x = Mathf.Clamp(pos.x, minX + margem, maxX - margem);
        pos.y = Mathf.Clamp(pos.y, minY + margem, maxY - margem);
        return pos;
    }
    public void Iniciar()
    {
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        IsAttacking = true;
        movement.Stop();

        // ---------- PARTE 1: teleporta atrás e joga o player pra cima ----------
        transform.position = PrenderNaArena((Vector2)playerTransform.position + deslocamentoAtras);
        movement.FacePlayer();

        var pm = playerTransform.GetComponent<PlayerMovement>() ?? playerTransform.GetComponentInParent<PlayerMovement>();
        if (pm != null) pm.Knockback(new Vector2(forcaLateralParte1, forcaParaCima));

        if (parte1DaDano)
        {
            var hp1 = playerTransform.GetComponent<PlayerHealthController>() ?? playerTransform.GetComponentInParent<PlayerHealthController>();
            if (hp1 != null && !hp1.IsInvincible)
            {
                if (parryavel) hp1.TakeDamage(danoParte1, "Melee");
                else hp1.TakeDamage(danoParte1);
            }
        }

        yield return new WaitForSeconds(tempoAntesDoSlam);

        // ---------- PARTE 2: aparece acima e esmaga pra baixo  ----------
        Vector2 acima = PrenderNaArena((Vector2)playerTransform.position + Vector2.up * alturaAcima);
        transform.position = acima;
        movement.FacePlayer();

        Vector2 dir = ((Vector2)playerTransform.position - acima);
        dir = dir.sqrMagnitude > 0.01f ? dir.normalized : Vector2.down;

        movement.FreezeInAir();
        rb.velocity = dir * velocidadeSlam;

        bool acertou = false;
        float t = 0f;
        Vector2 pontoAnterior = PontoAcerto();
        while (t < duracaoSlam)
        {
            if (!acertou)
            {
                Vector2 pontoAtual = PontoAcerto();
                Collider2D hit = VarrerAcerto(pontoAnterior, pontoAtual);
                if (hit != null)
                {
                    float sinalX = Mathf.Sign(hit.transform.position.x - transform.position.x);
                    Vector2 kb = new Vector2(sinalX * knockbackLateral, -knockbackVertical);
                    AplicarDano(hit, danoSlam, kb);
                    acertou = true;
                    rb.velocity *= 0.15f;
                }
                pontoAnterior = pontoAtual;
            }
            t += Time.deltaTime;
            yield return null;
        }

        movement.ReleaseFromAir();
        movement.Stop();
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
    // Varre o caminho entre o frame anterior e o atual (evita tunneling na descida rápida).
    private Collider2D VarrerAcerto(Vector2 de, Vector2 ate)
    {
        Collider2D o = Physics2D.OverlapCircle(ate, raioAcerto, playerLayer);
        if (o != null) return o;

        Vector2 delta = ate - de;
        float dist = delta.magnitude;
        if (dist > 0.001f)
        {
            RaycastHit2D h = Physics2D.CircleCast(de, raioAcerto, delta / dist, dist, playerLayer);
            if (h.collider != null) return h.collider;
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 c = pontoAcerto != null ? pontoAcerto.position : transform.position;
        Gizmos.color = new Color(0.4f, 0.5f, 1f, 0.4f);
        Gizmos.DrawWireSphere(c, raioAcerto);
    }
}