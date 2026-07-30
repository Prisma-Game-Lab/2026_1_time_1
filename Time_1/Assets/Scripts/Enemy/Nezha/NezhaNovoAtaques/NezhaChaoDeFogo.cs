using System.Collections;
using UnityEngine;

public class NezhaChaoDeFogo : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera cam;
    [Tooltip("Prefab do hazard: precisa de Collider2D (Is Trigger) + ChifreCollider. O dano fica nele.")]
    [SerializeField] private GameObject hazardPrefab;

    [Header("Area / Chao")]
    [Tooltip("Se marcado, usa o Y atual do boss como altura do chao (bom logo apos pousar da flutuacao).")]
    [SerializeField] private bool usarYDoBoss = true;
    [Tooltip("Altura do chao (usada se 'Usar Y Do Boss' estiver desmarcado).")]
    [SerializeField] private float alturaChao = -3.5f;
    [Tooltip("Se marcado, estica o hazard em X pra cobrir toda a largura da arena.")]
    [SerializeField] private bool esticarParaArena = true;
    [Tooltip("Margem nas bordas da arena.")]
    [SerializeField] private float margemBorda = 0.3f;
    [Tooltip("Retangulo exato da arena (opcional). Se Limite Max.x > Limite Min.x, usa no lugar da camera.")]
    [SerializeField] private Vector2 limiteMin;
    [SerializeField] private Vector2 limiteMax;

    [Header("Tempos")]
    [Tooltip("Aviso antes de ligar o dano (telegrafo, sem dano).")]
    [SerializeField] private float tempoAviso = 0.7f;
    [Tooltip("Quanto tempo o fogo fica ATIVO dando dano (o 'x segundos').")]
    [SerializeField] private float duracaoAtivo = 3f;
    [Tooltip("Tempo antes de destruir depois de desligar o dano.")]
    [SerializeField] private float tempoRecolhe = 0.4f;

    [Header("Audio")]
    [Tooltip("Id do SFX no SFXManager. Vazio = nao toca.")]
    [SerializeField] private string sfxId = "";

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }
    public void Iniciar()
    {
        if (!IsAttacking && hazardPrefab != null) StartCoroutine(Routine());
    }
    private IEnumerator Routine()
    {
        IsAttacking = true;

        float minX, maxX;
        ArenaX(out minX, out maxX);
        float centroX = (minX + maxX) * 0.5f;
        float chaoY = usarYDoBoss ? transform.position.y : alturaChao;

        GameObject go = Instantiate(hazardPrefab, new Vector3(centroX, chaoY, 0f), Quaternion.identity);
        if (esticarParaArena) EsticarParaLargura(go, maxX - minX);

        // Coleta os colliders do hazard e os DESLIGA na fase de aviso (telegrafo sem dano).
        Collider2D[] cols = go.GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D c in cols) c.enabled = false;

        if (!string.IsNullOrEmpty(sfxId)) SFXManager.PlaySFX(sfxId);

        yield return new WaitForSeconds(tempoAviso);

        // Fase ATIVA: liga o dano por 'duracaoAtivo' segundos.
        foreach (Collider2D c in cols) if (c != null) c.enabled = true;
        yield return new WaitForSeconds(duracaoAtivo);

        // Desliga o dano e some.
        foreach (Collider2D c in cols) if (c != null) c.enabled = false;
        yield return new WaitForSeconds(tempoRecolhe);

        if (go != null) Destroy(go);

        IsAttacking = false;
    }

    private void ArenaX(out float minX, out float maxX)
    {
        if (limiteMax.x > limiteMin.x)
        {
            minX = limiteMin.x + margemBorda;
            maxX = limiteMax.x - margemBorda;
            return;
        }
        Camera c = cam != null ? cam : Camera.main;
        if (c != null)
        {
            float hw = c.orthographicSize * c.aspect;
            float cx = c.transform.position.x;
            minX = cx - hw + margemBorda;
            maxX = cx + hw - margemBorda;
            return;
        }
        minX = transform.position.x - 8f;
        maxX = transform.position.x + 8f;
    }

    // Estica o hazard em X ate cobrir 'larguraAlvo' (largura da arena), usando a largura atual do collider/render.
    private void EsticarParaLargura(GameObject go, float larguraAlvo)
    {
        float larguraAtual = 0f;
        Collider2D col = go.GetComponentInChildren<Collider2D>();
        if (col != null) larguraAtual = col.bounds.size.x;
        else
        {
            Renderer r = go.GetComponentInChildren<Renderer>();
            if (r != null) larguraAtual = r.bounds.size.x;
        }
        if (larguraAtual <= 0.0001f) return;

        float fator = larguraAlvo / larguraAtual;
        Vector3 s = go.transform.localScale;
        s.x *= fator;
        go.transform.localScale = s;
    }
}