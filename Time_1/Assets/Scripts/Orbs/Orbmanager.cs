using System;
using UnityEngine;
public class OrbManager : MonoBehaviour
{
    public static OrbManager Instance { get; private set; }

    [Header("Configurações")]
    [SerializeField] private int maxOrbs = 5;
    [Tooltip("Quantidade de orbs consumida por uma cura")]
    [SerializeField] private int custoCura = 5;
    [Header("Debug")]
    [SerializeField] private bool logs = true;

    public int CurrentOrbs { get; private set; }
    public int MaxOrbs => maxOrbs;
    public int CustoCura => custoCura;
    public bool TemOrbsSuficientes => CurrentOrbs >= custoCura;

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
        if (logs) Debug.Log($"[ORB] Orb coletada. Total: {CurrentOrbs}/{maxOrbs}");
    }
    // Consome exatamente o custo da cura. Retorna true se conseguiu consumir.
    public bool ConsumirOrbsCura()
    {
        if (CurrentOrbs < custoCura)
        {
            if (logs) Debug.Log($"[CURA] Orbs insuficientes: {CurrentOrbs}/{custoCura}. Consumo cancelado.");
            return false;
        }
        CurrentOrbs -= custoCura;
        OnOrbsChanged?.Invoke(CurrentOrbs, maxOrbs);
        if (logs) Debug.Log($"[CURA] Consumidas {custoCura} orbs. Restante: {CurrentOrbs}/{maxOrbs}");
        return true;
    }
    // Perde orbs ao tomar dano 
    public void PerderOrbsPorDano(int quantidade)
    {
        if (quantidade <= 0 || CurrentOrbs == 0) return;
        int antes = CurrentOrbs;
        CurrentOrbs = Mathf.Max(CurrentOrbs - quantidade, 0);
        OnOrbsChanged?.Invoke(CurrentOrbs, maxOrbs);
        if (logs) Debug.Log($"[ORB] Dano: perdeu {antes - CurrentOrbs} orb(s). Total: {CurrentOrbs}/{maxOrbs}");
    }
}