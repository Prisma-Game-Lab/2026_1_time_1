using System.Collections;
using UnityEngine;

public class NezhaTeleporteSlam : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NezhaMovement movement;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    [Tooltip("Hitbox do esmagamento: collider FILHO (Is Trigger) com o script ChifreCollider. " +
             "Fica ligada so durante a descida. O dano e o knockback ficam no ChifreCollider.")]
    [SerializeField] private Collider2D slamHitbox;

    [Header("Parte 1 - teleporte atras + arremesso")]
    [Tooltip("Deslocamento relativo ao player para 'atras dele'.")]
    [SerializeField] private Vector2 deslocamentoAtras = new Vector2(0f, -1f);
    [Tooltip("Forca que joga o player para cima.")]
    [SerializeField] private float forcaParaCima = 18f;
    [SerializeField] private float forcaLateralParte1 = 0f;
    [Tooltip("Se marcado, a Parte 1 tambem da dano (padrao: nao da).")]
    [SerializeField] private bool parte1DaDano = false;
    [SerializeField] private int danoParte1 = 1;
    [Tooltip("Se marcado, o dano da Parte 1 usa tag 'Melee' (pode ser parryado).")]
    [SerializeField] private bool parryavel = true;

    [Header("Espera entre as partes")]
    [Tooltip("Tempo com o player subindo antes do esmagamento.")]
    [SerializeField] private float tempoAntesDoSlam = 0.35f;

    [Header("Parte 2 - esmagamento")]
    [Tooltip("Altura acima do player para onde Nezha teleporta antes de descer.")]
    [SerializeField] private float alturaAcima = 4f;
    [Tooltip("Velocidade da descida (absurda).")]
    [SerializeField] private float velocidadeSlam = 45f;
    [Tooltip("Teto de seguranca para a descida (caso nao alcance o chao).")]
    [SerializeField] private float duracaoSlam = 1f;

    [Header("Recuperacao")]
    [SerializeField] private float recuperacao = 0.4f;

    [Header("Limites da arena (evita sair do mapa)")]
    [Tooltip("Se marcado, prende os teleportes dentro da arena.")]
    [SerializeField] private bool prenderNaArena = true;
    [Tooltip("Camera usada para calcular os limites. Vazio = Camera.main.")]
    [SerializeField] private Camera cam;
    [Tooltip("Margem para dentro das bordas.")]
    [SerializeField] private float margem = 1f;
    [Tooltip("Opcional: retangulo exato da arena. Se 'Limite Max' > 'Limite Min', usa estes valores.")]
    [SerializeField] private Vector2 limiteMin;
    [SerializeField] private Vector2 limiteMax;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        if (movement == null) movement = GetComponent<NezhaMovement>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (cam == null) cam = Camera.main;
        if (slamHitbox != null) slamHitbox.enabled = false; // so liga na descida
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

        // Nivel do chao = onde o boss esta parado ANTES de teleportar. A descida vai parar
        // exatamente nesse Y, entao e IMPOSSIVEL o slam furar o chao.
        float chaoY = transform.position.y;

        // ---------- PARTE 1: teleporta atras e joga o player pra cima ----------
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

        // ---------- PARTE 2: aparece acima e esmaga pra baixo ----------
        Vector2 acima = PrenderNaArena((Vector2)playerTransform.position + Vector2.up * alturaAcima);
        transform.position = acima;
        movement.FacePlayer();

        // Alvo do esmagamento: X do player, travado no Y do chao (nunca abaixo).
        Vector2 alvo = PrenderNaArena(new Vector2(playerTransform.position.x, chaoY));
        alvo.y = chaoY; // garante o chao, sem o clamp de arena mexer no Y

        movement.FreezeInAir();
        if (slamHitbox != null) slamHitbox.enabled = true;

        float t = 0f;
        while (t < duracaoSlam)
        {
            // MoveTowards nunca ultrapassa o alvo -> para exatamente no chao, sem tunneling.
            transform.position = Vector2.MoveTowards(transform.position, alvo, velocidadeSlam * Time.deltaTime);
            if (Vector2.Distance(transform.position, alvo) <= 0.05f) break;

            t += Time.deltaTime;
            yield return null;
        }

        if (slamHitbox != null) slamHitbox.enabled = false;
        rb.velocity = Vector2.zero;

        movement.ReleaseFromAir();
        movement.Stop();
        yield return new WaitForSeconds(recuperacao);

        IsAttacking = false;
    }
}