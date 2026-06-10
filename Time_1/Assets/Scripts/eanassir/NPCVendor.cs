using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCVendor : Interactable
{
    [Header("Item à Venda")]
    [SerializeField] private string nomeItem = "Item Misterioso";
    [SerializeField] private Sprite iconeItem;
    [SerializeField] private int preco = 5;

    [Header("Diálogos")]
    [Tooltip("Use {preco} no texto para inserir o valor automaticamente.")]
    [SerializeField, TextArea] private string dialogoOferta = "Gostaria de comprar este item por {preco} moedas?";
    [SerializeField, TextArea] private string dialogoPosCompra = "Obrigado pela compra.";

    [Header("Evento de Falha (sem moedas)")]
    [Tooltip("Componente NPCFailureSpawner — opcional. Se nulo, falha apenas fecha o diálogo.")]
    [SerializeField] private NPCFailureSpawner spawnerFalha;

    [Header("Referências de UI/Inventário")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private PurchasedItemManager purchasedItemManager;

    private bool jaVendeu;
    public override bool PodeInteragir() => true;
    public override void Interagir()
    {
        if (dialogueUI == null)
        {
            Debug.LogError("[NPCVendor] dialogueUI não atribuído no Inspector.", this);
            return;
        }
        if (jaVendeu)
        {
            dialogueUI.MostrarSimples(dialogoPosCompra);
            return;
        }
        string textoFinal = dialogoOferta.Replace("{preco}", preco.ToString());
        dialogueUI.MostrarOferta(textoFinal, OnComprarClicado, OnCancelarClicado);
    }
    private void OnComprarClicado()
    {
        if (CoinManager.Instance == null)
        {
            Debug.LogError("[NPCVendor] CoinManager.Instance é nulo — adicione um GameObject com CoinManager na cena.");
            return;
        }
        if (!CoinManager.Instance.TemMoedasSuficientes(preco))
        {
            // Sem moedas → fecha diálogo e dispara evento de falha
            dialogueUI.Fechar();
            spawnerFalha?.DispararFalha();
            return;
        }
        CoinManager.Instance.GastarMoedas(preco);
        jaVendeu = true;
        // Adiciona ao inventário visual
        purchasedItemManager?.AdicionarItem(nomeItem, iconeItem);

        dialogueUI.Fechar();
    }

    private void OnCancelarClicado()
    {
        dialogueUI.Fechar();
    }
}