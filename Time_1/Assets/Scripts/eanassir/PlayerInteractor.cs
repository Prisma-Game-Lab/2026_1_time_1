using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Verificação de Proximidade")]
    [Tooltip("Intervalo em segundos entre as verificações de proximidade.")]
    [SerializeField] private float verificacaoIntervalo = 0.1f;

    [Tooltip("Quando true, ignora interagíveis se o jogo estiver pausado (Time.timeScale == 0).")]
    [SerializeField] private bool respeitarPausa = true;
    private Interactable interagivelAtual;
    private float timer;
    void Update()
    {
        if (respeitarPausa && Time.timeScale == 0f) return;

        timer -= Time.unscaledDeltaTime;
        if (timer <= 0f)
        {
            timer = verificacaoIntervalo;
            AtualizarInteragivelMaisProximo();
        }

        if (interagivelAtual != null
            && Keyboard.current != null
            && Keyboard.current.fKey.wasPressedThisFrame)
        {
            interagivelAtual.Interagir();
        }
    }
    private void AtualizarInteragivelMaisProximo()
    {
        Interactable maisProximo = null;
        float menorDist = float.MaxValue;
        Vector2 pos = transform.position;

        for (int i = 0; i < Interactable.All.Count; i++)
        {
            Interactable it = Interactable.All[i];
            if (it == null || !it.PodeInteragir()) continue;

            float d = Vector2.Distance(pos, it.transform.position);
            if (d <= it.RaioInteracao && d < menorDist)
            {
                menorDist = d;
                maisProximo = it;
            }
        }
        if (maisProximo != interagivelAtual)
        {
            if (interagivelAtual != null) interagivelAtual.MostrarIcone(false);
            interagivelAtual = maisProximo;
            if (interagivelAtual != null) interagivelAtual.MostrarIcone(true);
        }
    }
    void OnDisable()
    {
        if (interagivelAtual != null) interagivelAtual.MostrarIcone(false);
        interagivelAtual = null;
    }
}