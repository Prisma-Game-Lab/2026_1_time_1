using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Barra de Vida")]
    [SerializeField] private Slider healthBar;

    [Header("Orbs")]
    [SerializeField] private GameObject orbIconPrefab;
    [SerializeField] private Transform orbContainer;
    [SerializeField] private Color orbAtiva = Color.white;
    [SerializeField] private Color orbVazia = new Color(1, 1, 1, 0.25f);

    [Header("Referências")]
    [SerializeField] private HealthController playerHealth;
    [SerializeField] private OrbManager orbManager;

    private Image[] orbIcons;

    void Start()
    {
        playerHealth.OnHealthChanged += AtualizarVida;
        orbManager.OnOrbsChanged += AtualizarOrbs;
        CriarIcones(orbManager.MaxOrbs);
        AtualizarVida(playerHealth.currentHealth, playerHealth.MaxHealth);
        AtualizarOrbs(orbManager.CurrentOrbs, orbManager.MaxOrbs);
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= AtualizarVida;
        if (orbManager != null) orbManager.OnOrbsChanged -= AtualizarOrbs;
    }

    private void AtualizarVida(int atual, int maximo)
    {
        if (healthBar != null)
            healthBar.value = (float)atual / maximo;
    }

    private void AtualizarOrbs(int atual, int maximo)
    {
        for (int i = 0; i < orbIcons.Length; i++)
            orbIcons[i].color = i < atual ? orbAtiva : orbVazia;
    }

    private void CriarIcones(int quantidade)
    {
        orbIcons = new Image[quantidade];
        for (int i = 0; i < quantidade; i++)
        {
            GameObject go = Instantiate(orbIconPrefab, orbContainer);
            orbIcons[i] = go.GetComponent<Image>();
            orbIcons[i].color = orbVazia;
        }
    }
}