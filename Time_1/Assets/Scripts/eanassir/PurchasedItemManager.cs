using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
public class PurchasedItemManager : MonoBehaviour
{
    public static PurchasedItemManager Instance { get; private set; }

    public struct Item
    {
        public string nome;
        public Sprite icone;
    }
    private readonly List<Item> itens = new();
    public IReadOnlyList<Item> Itens => itens;

    public event Action<Item> OnItemAdicionado;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AdicionarItem(string nome, Sprite icone)
    {
        var item = new Item { nome = nome, icone = icone };
        itens.Add(item);
        OnItemAdicionado?.Invoke(item);
    }
}