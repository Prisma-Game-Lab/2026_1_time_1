using System;
using UnityEngine;

public class OrbManager : MonoBehaviour
{
    public static OrbManager Instance { get; private set; }

    [Header("Configurações")]
    [SerializeField] private int maxOrbs = 5;
    [SerializeField] private int orbsParaCurar = 5;
    [SerializeField] private int curaQuantidade = 1;

    [Header("Referências")]
    [SerializeField] private HealthController playerHealth;
    [SerializeField] private OrbSpawner orbSpawner;

    public int CurrentOrbs { get; private set; }
    public int MaxOrbs => maxOrbs;
    public Action<int, int> OnOrbsChanged;

    private PlayerShooting playerShooting;
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Chamado pelo PlayerShooting quando ele inicializa
    public void RegistrarPlayerShooting(PlayerShooting ps)
    {
        if (playerShooting != null)
            playerShooting.OnParry -= OnParryHappened;

        playerShooting = ps;
        playerShooting.OnParry += OnParryHappened;
        Debug.Log("[OrbManager] PlayerShooting registrado com sucesso.");
    }
    void OnDisable()
    {
        if (playerShooting != null)
            playerShooting.OnParry -= OnParryHappened;
    }
    public void RegistrarLanca(Transform lanca)
    {
        if (orbSpawner != null)
            orbSpawner.RegistrarLanca(lanca);
        else
            Debug.LogError("[OrbManager] orbSpawner é null ao registrar lança!");
    }
    private void OnParryHappened()
    {
        Debug.Log("[OrbManager] Parry detectado!");
        if (orbSpawner == null)
        {
            Debug.LogError("[OrbManager] orbSpawner é null!");
            return;
        }
        orbSpawner.NotificarParry();
    }
    public void AddOrb(int amount = 1)
    {
        int antes = CurrentOrbs;
        CurrentOrbs = Mathf.Min(CurrentOrbs + amount, maxOrbs);
        Debug.Log($"[OrbManager] AddOrb: {antes} → {CurrentOrbs} (max: {maxOrbs})");
        OnOrbsChanged?.Invoke(CurrentOrbs, maxOrbs);

        if (CurrentOrbs >= orbsParaCurar)
        {
            Debug.Log($"[OrbManager] {orbsParaCurar} orbs atingidas! Curando {curaQuantidade} HP");
            CurrentOrbs -= orbsParaCurar;
            playerHealth?.Heal(curaQuantidade);
            OnOrbsChanged?.Invoke(CurrentOrbs, maxOrbs);
        }
    }
}