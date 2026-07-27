using System.Collections;
using UnityEngine;

public class NezhaChute : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NezhaMovement movement;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    [Tooltip("Hitbox do chute: collider FILHO (Is Trigger) com o script ChifreCollider. " +
             "Fica ligada so durante o avanco. O dano e o knockback ficam no ChifreCollider.")]
    [SerializeField] private Collider2D chuteHitbox;

    [Header("Tempo")]
    [Tooltip("Espera antes de partir (telegrafo). E o 'tempo x' ajustavel.")]
    [SerializeField] private float tempoPreparacao = 0.6f;
    [Tooltip("Teto de seguranca para o avanco (caso nao alcance o alvo).")]
    [SerializeField] private float duracaoChute = 0.6f;
    [Tooltip("Recuperacao depois do chute.")]
    [SerializeField] private float recuperacao = 0.4f;

    [Header("Chute")]
    [Tooltip("Velocidade do avanco (absurda).")]
    [SerializeField] private float velocidadeChute = 40f;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        if (movement == null) movement = GetComponent<NezhaMovement>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (chuteHitbox != null) chuteHitbox.enabled = false; // so liga durante o golpe
    }

    public void Iniciar()
    {
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        IsAttacking = true;

        // Nivel do chao = onde o boss esta parado agora. O avanco fica preso nesse Y,
        // entao e IMPOSSIVEL o chute furar o chao ou levar o boss pra fora do mapa.
        float chaoY = transform.position.y;

        // Telegrafo
        movement.Stop();
        movement.FacePlayer();
        yield return new WaitForSeconds(tempoPreparacao);

        // Compromete o alvo no inicio do avanco (esquivavel): X do player, no chao.
        movement.FacePlayer();
        Vector2 alvo = new Vector2(playerTransform.position.x, chaoY);

        // Desliga a gravidade so por seguranca; o movimento e por MoveTowards (nao por velocidade fisica).
        movement.FreezeInAir();
        if (chuteHitbox != null) chuteHitbox.enabled = true;

        float t = 0f;
        while (t < duracaoChute)
        {
            // MoveTowards nunca ultrapassa o alvo -> sem overshoot, sem tunneling.
            transform.position = Vector2.MoveTowards(transform.position, alvo, velocidadeChute * Time.deltaTime);
            if (Vector2.Distance(transform.position, alvo) <= 0.05f) break; // chegou no player

            t += Time.deltaTime;
            yield return null;
        }

        // Desliga a hitbox e garante velocidade zero antes de devolver a gravidade.
        if (chuteHitbox != null) chuteHitbox.enabled = false;
        rb.velocity = Vector2.zero;

        movement.ReleaseFromAir();
        movement.Stop();
        yield return new WaitForSeconds(recuperacao);

        IsAttacking = false;
    }
}