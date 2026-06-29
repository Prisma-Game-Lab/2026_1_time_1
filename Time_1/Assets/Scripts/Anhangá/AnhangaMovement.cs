using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnhangaMovement : MonoBehaviour
{
    [Header("Limites da Arena")]
    [SerializeField] private Transform limiteEsquerdo;
    [SerializeField] private Transform limiteDireito;

    [Header("Referências")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprite")]
    [SerializeField] private bool spriteOlhaParaDireita = true;

    [Header("Chão")]
    [SerializeField] private bool usarYDosLimites = true;

    public float MinX => Mathf.Min(limiteEsquerdo.position.x, limiteDireito.position.x);
    public float MaxX => Mathf.Max(limiteEsquerdo.position.x, limiteDireito.position.x);
    public Transform Player => player;

    private float GroundY
    {
        get
        {
            if (usarYDosLimites && limiteEsquerdo != null && limiteDireito != null)
                return (limiteEsquerdo.position.y + limiteDireito.position.y) * 0.5f;
            return transform.position.y;
        }
    }

    private void Awake()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else Debug.LogWarning("[AnhangaMovement] Player não encontrado (tag 'Player').", this);
        }
    }
    public void AndarParaPlayer(float velocidade)
    {
        if (player == null) return;
        int dir = DirecaoParaPlayer();
        float novoX = transform.position.x + dir * velocidade * Time.deltaTime;
        novoX = Mathf.Clamp(novoX, MinX, MaxX);
        transform.position = new Vector3(novoX, GroundY, transform.position.z);
        AtualizarFlip(dir);
    }
    public bool IrParaX(float alvoX, float velocidade)
    {
        int dir = alvoX >= transform.position.x ? 1 : -1;
        float novoX = Mathf.MoveTowards(transform.position.x, alvoX, velocidade * Time.deltaTime);
        transform.position = new Vector3(novoX, GroundY, transform.position.z);
        AtualizarFlip(dir);
        return Mathf.Abs(transform.position.x - alvoX) <= 0.05f;
    }

    public int DirecaoParaPlayer()
    {
        if (player == null) return 1;
        return player.position.x >= transform.position.x ? 1 : -1;
    }
    public void PassoHorizontal(int moveDir, int faceDir, float velocidade)
    {
        float novoX = Mathf.Clamp(transform.position.x + moveDir * velocidade * Time.deltaTime, MinX, MaxX);
        transform.position = new Vector3(novoX, GroundY, transform.position.z);
        AtualizarFlip(faceDir);
    }

    // Teleporta para um X (usado quando surge num dos lados), travando na altura do chão.
    public void PosicionarEm(float x)
    {
        float cx = Mathf.Clamp(x, MinX, MaxX);
        transform.position = new Vector3(cx, GroundY, transform.position.z);
    }

    // Vira o sprite para uma direção sem mover.
    public void Encarar(int direcao) => AtualizarFlip(direcao);
    private void AtualizarFlip(int direcao)
    {
        if (spriteRenderer == null) return;
        bool olhandoDireita = direcao > 0;
        spriteRenderer.flipX = (olhandoDireita != spriteOlhaParaDireita);
    }
    private void OnDrawGizmosSelected()
    {
        if (limiteEsquerdo != null && limiteDireito != null)
        {
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.9f);
            Gizmos.DrawLine(limiteEsquerdo.position, limiteDireito.position);
            Gizmos.DrawWireSphere(limiteEsquerdo.position, 0.25f);
            Gizmos.DrawWireSphere(limiteDireito.position, 0.25f);
        }
    }
}