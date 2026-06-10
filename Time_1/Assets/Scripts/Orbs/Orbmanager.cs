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

    public int CurrentOrbs { get; private set; }
    public int MaxOrbs => maxOrbs;
    public Action<int, int> OnOrbsChanged;
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    public void AddOrb(int amount = 1)
    {
        CurrentOrbs = Mathf.Min(CurrentOrbs + amount, maxOrbs);
        OnOrbsChanged?.Invoke(CurrentOrbs, maxOrbs);
        if (CurrentOrbs >= orbsParaCurar)
        {
            CurrentOrbs -= orbsParaCurar;
            playerHealth?.Heal(curaQuantidade);
            OnOrbsChanged?.Invoke(CurrentOrbs, maxOrbs);
        }
    }
}