using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DialogueUI : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject painel;
    [SerializeField] private TextMeshProUGUI texto;

    [Header("Grupos de Botões")]
    [Tooltip("GameObject que contém Comprar + Cancelar lado a lado.")]
    [SerializeField] private GameObject grupoOferta;
    [Tooltip("GameObject que contém apenas o botão Fechar.")]
    [SerializeField] private GameObject grupoSimples;

    [Header("Botões")]
    [SerializeField] private Button botaoComprar;
    [SerializeField] private Button botaoCancelar;
    [SerializeField] private Button botaoFechar;

    private Action callbackComprar;
    private Action callbackCancelar;

    void Awake()
    {
        if (botaoComprar != null) botaoComprar.onClick.AddListener(() => callbackComprar?.Invoke());
        if (botaoCancelar != null) botaoCancelar.onClick.AddListener(() => callbackCancelar?.Invoke());
        if (botaoFechar != null) botaoFechar.onClick.AddListener(Fechar);

        if (painel != null) painel.SetActive(false);
    }
    public void MostrarOferta(string mensagem, Action onComprar, Action onCancelar)
    {
        callbackComprar = onComprar;
        callbackCancelar = onCancelar;

        if (texto != null) texto.text = mensagem;
        if (grupoOferta != null) grupoOferta.SetActive(true);
        if (grupoSimples != null) grupoSimples.SetActive(false);
        if (painel != null) painel.SetActive(true);
    }
    public void MostrarSimples(string mensagem)
    {
        if (texto != null) texto.text = mensagem;
        if (grupoOferta != null) grupoOferta.SetActive(false);
        if (grupoSimples != null) grupoSimples.SetActive(true);
        if (painel != null) painel.SetActive(true);
    }
    public void Fechar()
    {
        if (painel != null) painel.SetActive(false);
        callbackComprar = null;
        callbackCancelar = null;
    }
    public bool EstaAberto => painel != null && painel.activeSelf;
}