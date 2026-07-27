using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class DanoContatoTutorial : MonoBehaviour, Parryble
{
    [Tooltip("Dano aplicado ao player ao encostar.")]
    [SerializeField] private int dano = 1;

    [Tooltip("Se marcado, aplica dano só uma vez (passagem única).")]
    [SerializeField] private bool danoUnico = true;

    [Header("Debug")]
    [Tooltip("Liga logs no Console para bugtest.")]
    [SerializeField] private bool logsDiagnostico = true;

    private bool jaResolveu;
    private Transform playerRef;
    private float minDist;

    private void OnEnable()
    {
        jaResolveu = false;
        minDist = float.MaxValue;
        if (logsDiagnostico)
        {
            var col = GetComponent<Collider2D>();
            var rb = GetComponent<Rigidbody2D>();
            Debug.Log($"[DanoTutorial] ATIVADO. Collider={(col != null)} isTrigger={(col != null && col.isTrigger)} " +
                      $"Rigidbody2D={(rb != null)} " +
                      $"RBtype={(rb != null ? rb.bodyType.ToString() : "-")} " +
                      $"Layer={LayerMask.LayerToName(gameObject.layer)}", this);

            var p = GameObject.FindGameObjectWithTag("Player");
            playerRef = p != null ? p.transform : null;
            if (p == null)
                Debug.LogWarning("[DanoTutorial] Nenhum objeto com tag 'Player' encontrado na cena!", this);
            else
            {
                var prb = p.GetComponent<Rigidbody2D>() ?? p.GetComponentInChildren<Rigidbody2D>();
                Debug.Log($"[DanoTutorial] Player='{p.name}' Layer={LayerMask.LayerToName(p.layer)} " +
                          $"RBtype={(prb != null ? prb.bodyType.ToString() : "SEM Rigidbody2D")}", this);
            }
        }
    }
    private void Update()
    {
        if (logsDiagnostico && playerRef != null)
        {
            float d = Vector2.Distance(transform.position, playerRef.position);
            if (d < minDist) minDist = d;
        }
    }
    private void OnDisable()
    {
        if (logsDiagnostico && minDist < float.MaxValue)
            Debug.Log($"[DanoTutorial] Fim da travessia. Distância MÍNIMA ao player (centro-a-centro) = {minDist:F2}", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (logsDiagnostico)
            Debug.Log($"[DanoTutorial] TRIGGER com '{other.name}' (tag={other.tag}, layer={LayerMask.LayerToName(other.gameObject.layer)})", this);

        if (danoUnico && jaResolveu)
        {
            if (logsDiagnostico) Debug.Log("[DanoTutorial] Ignorado: já resolveu (danoUnico).", this);
            return;
        }
        PlayerHealthController pHC = other.GetComponent<PlayerHealthController>();
        if (pHC == null) pHC = other.GetComponentInParent<PlayerHealthController>();
        if (pHC == null)
        {
            if (logsDiagnostico) Debug.Log($"[DanoTutorial] '{other.name}' não tem PlayerHealthController. Sem dano.", this);
            return;
        }
        if (pHC.IsInvincible)
        {
            if (logsDiagnostico) Debug.Log("[DanoTutorial] Player INVENCÍVEL (i-frames). Dano bloqueado.", this);
            return;
        }
        if (logsDiagnostico) Debug.Log($"[DanoTutorial] Aplicando {dano} de dano (HP antes={pHC.currentHealth}).", this);
        pHC.TakeDamage(dano, "Melee");
        jaResolveu = true;
        if (logsDiagnostico) Debug.Log($"[DanoTutorial] Dano aplicado (HP depois={pHC.currentHealth}).", this);
    }
    public void OnParried()
    {
        jaResolveu = true;
        if (logsDiagnostico) Debug.Log("[DanoTutorial] Parry de projétil (OnParried).", this);
    }
}