using UnityEngine;

public class OrbFisica : MonoBehaviour
{
    [Header("Tempo de Vida")]
    [SerializeField] private float lifetime = 2f;

    [Header("Valor")]
    [SerializeField] private int orbValue = 1;

    private Rigidbody2D rb;
    private bool coletada;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[OrbFisica] Rigidbody2D não encontrado!", this);
            return;
        }
        Debug.Log($"[OrbFisica] Spawnou em {transform.position} | gravityScale: {rb.gravityScale}");
    }
    private void Start()
    {
        if (OrbManager.Instance == null)
            Debug.LogError("[OrbFisica] OrbManager.Instance é null!");

        Destroy(gameObject, lifetime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[OrbFisica] Trigger com: {other.gameObject.name} | Tag: {other.tag}");

        if (coletada) return;

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[OrbFisica] Ignorando — tag: {other.tag}");
            return;
        }
        coletada = true;
        Debug.Log("[OrbFisica] Player coletou!");
        OrbManager.Instance?.AddOrb(orbValue);
        Destroy(gameObject);
    }
}