using UnityEngine;
using UnityEngine.UI;
public class HUDController : MonoBehaviour
{
    [Header("Vida (um ícone por ponto)")]
    [SerializeField] private GameObject vidaIconPrefab;
    [SerializeField] private Transform vidaContainer;
    [SerializeField] private Sprite vidaCheia;
    [SerializeField] private Sprite vidaVazia;

    [Header("Nabos")]
    [SerializeField] private GameObject orbIconPrefab;
    [SerializeField] private Transform orbContainer;
    [SerializeField] private Sprite orbSpriteVazia;
    [SerializeField] private Sprite orbSpriteCheia;

    [Header("Referências")]
    [SerializeField] private HealthController playerHealth;
    [SerializeField] private OrbManager orbManager;

    private Image[] vidaIcons;
    private Image[] orbIcons;

    void Start()
    {
        playerHealth.OnHealthChanged += AtualizarVida;
        orbManager.OnOrbsChanged += AtualizarOrbs;

        CriarIconesVida(playerHealth.MaxHealth);
        CriarIconesOrb(orbManager.MaxOrbs);

        AtualizarVida(playerHealth.currentHealth, playerHealth.MaxHealth);
        AtualizarOrbs(orbManager.CurrentOrbs, orbManager.MaxOrbs);
    }
    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= AtualizarVida;
        if (orbManager != null) orbManager.OnOrbsChanged -= AtualizarOrbs;
    }
    // ── Vida ─────────────────────────────────────────────
    private void CriarIconesVida(int maximo)
    {
        vidaIcons = new Image[maximo];
        for (int i = 0; i < maximo; i++)
        {
            GameObject go = Instantiate(vidaIconPrefab, vidaContainer);
            vidaIcons[i] = go.GetComponent<Image>();
        }
    }
    private void AtualizarVida(int atual, int maximo)
    {
        if (vidaIcons == null) return;
        for (int i = 0; i < vidaIcons.Length; i++)
        {
            bool cheio = i < atual;
            if (vidaCheia != null && vidaVazia != null)
                vidaIcons[i].sprite = cheio ? vidaCheia : vidaVazia;
            else
                vidaIcons[i].color = cheio ? Color.white : new Color(1, 1, 1, 0.25f);
        }
    }
    // ── Nabos ────────────────────────────────────────────
    private void CriarIconesOrb(int quantidade)
    {
        orbIcons = new Image[quantidade];
        for (int i = 0; i < quantidade; i++)
        {
            GameObject go = Instantiate(orbIconPrefab, orbContainer);
            orbIcons[i] = go.GetComponent<Image>();
            orbIcons[i].sprite = orbSpriteVazia;
        }
    }
    private void AtualizarOrbs(int atual, int maximo)
    {
        if (orbIcons == null) return;
        for (int i = 0; i < orbIcons.Length; i++)
            orbIcons[i].sprite = i < atual ? orbSpriteCheia : orbSpriteVazia;
    }
}