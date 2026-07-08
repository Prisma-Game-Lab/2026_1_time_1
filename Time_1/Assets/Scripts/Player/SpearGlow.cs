using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpearGlow : MonoBehaviour
{
    [Header("Referência da lança")]
    [SerializeField] private SpriteRenderer spearRenderer;

    [Header("Cor do contorno")]
    [SerializeField] private Color glowColor = new Color(1f, 0.5f, 0.1f, 1f);

    [Header("Alpha por contagem de nabos")]
    [SerializeField] private float alphaMin = 0f;
    [SerializeField] private float alphaMax = 0.6f;
    [SerializeField] private float fadeSpeed = 8f;

    private SpriteRenderer sr;
    private bool subscrito;
    private float alphaAlvo;

    private void Awake() => sr = GetComponent<SpriteRenderer>();

    private void OnEnable() => TentarSubscrever();
    private void Start() => TentarSubscrever();

    private void TentarSubscrever()
    {
        if (subscrito || OrbManager.Instance == null) return;

        OrbManager.Instance.OnOrbsChanged += AoMudarNabos;
        subscrito = true;

        // Sincroniza o estado inicial
        AoMudarNabos(OrbManager.Instance.CurrentOrbs, OrbManager.Instance.MaxOrbs);
    }
    private void OnDisable()
    {
        if (OrbManager.Instance != null)
            OrbManager.Instance.OnOrbsChanged -= AoMudarNabos;
        subscrito = false;
    }
    private void AoMudarNabos(int atual, int maximo)
    {
        float t = maximo > 0 ? (float)atual / maximo : 0f;
        alphaAlvo = Mathf.Lerp(alphaMin, alphaMax, t);
    }
    private void LateUpdate()
    {
        if (sr == null) return;

        if (spearRenderer != null)
        {
            sr.sprite = spearRenderer.sprite;
            sr.flipX = spearRenderer.flipX;
            sr.flipY = spearRenderer.flipY;
        }

        float a = fadeSpeed <= 0f
            ? alphaAlvo
            : Mathf.MoveTowards(sr.color.a, alphaAlvo, fadeSpeed * Time.deltaTime);

        Color c = glowColor;
        c.a = a;
        sr.color = c;
    }
}