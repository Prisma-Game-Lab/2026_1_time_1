using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("Configurações")]
    [SerializeField] private int moedasIniciais = 0;
    [SerializeField] private int moedasMaximas = 999;
    public int CurrentCoins { get; private set; }
    public int MaxCoins => moedasMaximas;

    // (atual, maximo)
    public event Action<int, int> OnCoinsChanged;
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CurrentCoins = moedasIniciais;
    }
    void Start()
    {
        // Garante que a HUD pegue o valor inicial mesmo que ela inicialize depois
        OnCoinsChanged?.Invoke(CurrentCoins, moedasMaximas);
    }
    public bool TemMoedasSuficientes(int valor) => CurrentCoins >= valor;
    public void AdicionarMoedas(int valor)
    {
        if (valor <= 0) return;
        int antes = CurrentCoins;
        CurrentCoins = Mathf.Min(CurrentCoins + valor, moedasMaximas);
        if (CurrentCoins != antes)
            OnCoinsChanged?.Invoke(CurrentCoins, moedasMaximas);
    }
    public bool GastarMoedas(int valor)
    {
        if (valor <= 0) return true;
        if (!TemMoedasSuficientes(valor)) return false;
        CurrentCoins -= valor;
        OnCoinsChanged?.Invoke(CurrentCoins, moedasMaximas);
        return true;
    }
}