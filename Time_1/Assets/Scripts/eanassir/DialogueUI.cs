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

    [Header("Grupos de Bot�es")]
    [Tooltip("GameObject que cont�m Comprar + Cancelar lado a lado.")]
    [SerializeField] private GameObject grupoOferta;
    [Tooltip("GameObject que cont�m apenas o bot�o Fechar.")]
    [SerializeField] private GameObject grupoSimples;

    [Header("Bot�es")]
    [SerializeField] private Button botaoComprar;
    [SerializeField] private Button botaoCancelar;
    [SerializeField] private Button botaoFechar;

    private Action callbackComprar;
    private Action callbackCancelar;
    private bool cursorOverrideActive;

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
        PushCursor();
    }
    public void MostrarSimples(string mensagem)
    {
        if (texto != null) texto.text = mensagem;
        if (grupoOferta != null) grupoOferta.SetActive(false);
        if (grupoSimples != null) grupoSimples.SetActive(true);
        if (painel != null) painel.SetActive(true);
        PushCursor();
    }
    public void Fechar()
    {
        if (painel != null) painel.SetActive(false);
        callbackComprar = null;
        callbackCancelar = null;
        PopCursor();
    }
    private void PushCursor()
    {
        if (!cursorOverrideActive)
        {
            PlayerAim.ForceShowCursor(true);
            cursorOverrideActive = true;
        }
    }
    private void PopCursor()
    {
        if (cursorOverrideActive)
        {
            PlayerAim.ForceShowCursor(false);
            cursorOverrideActive = false;
        }
    }
    private void OnDestroy()
    {
        PopCursor();
    }
    public bool EstaAberto => painel != null && painel.activeSelf;
}