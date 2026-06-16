using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HealController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private HealthController playerHealth;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Cura")]
    [Tooltip("Quantidade de vida restaurada por cura")]
    [SerializeField] private int quantidadeCura = 1;

    [Tooltip("Tempo (s) travado canalizando antes da vida ser aplicada")]
    [SerializeField] private float tempoCanalizacao = 0.6f;

    [Header("Penalidade ao tomar dano")]
    [Tooltip("Quantas orbs o jogador perde a cada dano recebido")]
    [SerializeField] private int orbsPerdidasAoTomarDano = 1;

    [Header("Visual (placeholder)")]
    [Tooltip("Objeto da aura, ativado durante a canalização. Opcional.")]
    [SerializeField] private GameObject auraCura;

    [Header("Debug")]
    [SerializeField] private bool logs = true;

    private bool curando;
    private float timer;

    private void Start()
    {
        if (auraCura != null) auraCura.SetActive(false);

        if (playerHealth != null)
            playerHealth.OnDamageTaken += AoTomarDano;
        else
            Debug.LogError("[CURA] playerHealth NÃO atribuído no HealController!", this);
    }
    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDamageTaken -= AoTomarDano;
    }
    // Ligar à ação "Curar" no Input System.
    public void Curar(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (curando) return;

        if (logs) Debug.Log("[CURA] Tentativa de cura.");

        if (OrbManager.Instance == null)
        {
            Debug.LogError("[CURA] OrbManager.Instance é null! Não há OrbManager na cena?");
            return;
        }
        if (!OrbManager.Instance.TemOrbsSuficientes)
        {
            if (logs) Debug.Log($"[CURA] Cancelada: orbs insuficientes ({OrbManager.Instance.CurrentOrbs}/{OrbManager.Instance.CustoCura}).");
            return;
        }

        IniciarCura();
    }
    private void IniciarCura()
    {
        // Consome as orbs imediatamente — perdidas mesmo que a cura seja cancelada.
        if (!OrbManager.Instance.ConsumirOrbsCura()) return;

        curando = true;
        timer = tempoCanalizacao;

        playerMovement?.SetMovementLocked(true);
        if (auraCura != null) auraCura.SetActive(true);
        AudioManager.Instance?.TocaSFX(AudioManager.Instance.EfeitoDeCura);

        if (logs) Debug.Log($"[CURA] Canalizando por {tempoCanalizacao}s...");
    }
    private void Update()
    {
        if (!curando) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            FinalizarCura();
    }
    private void FinalizarCura()
    {
        if (logs) Debug.Log($"[CURA] Vida antes: {playerHealth.currentHealth}/{playerHealth.MaxHealth}");

        playerHealth.Heal(quantidadeCura);

        if (logs) Debug.Log($"[CURA] Cura aplicada (+{quantidadeCura}). Vida depois: {playerHealth.currentHealth}/{playerHealth.MaxHealth}");

        Encerrar();
    }
    private void AoTomarDano(int dano)
    {
        // Penalidade de orbs por dano (independente da cura).
        OrbManager.Instance?.PerderOrbsPorDano(orbsPerdidasAoTomarDano);

        // Se estava curando, o dano cancela. As orbs já consumidas continuam perdidas.
        if (curando)
        {
            if (logs) Debug.Log("[CURA] Cura CANCELADA por dano. Orbs perdidas.");
            Encerrar();
        }
    }
    private void Encerrar()
    {
        curando = false;
        playerMovement?.SetMovementLocked(false);
        if (auraCura != null) auraCura.SetActive(false);
    }
}