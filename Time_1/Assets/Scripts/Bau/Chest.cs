using UnityEngine;

public class Chest : Interactable
{
    [Header("Recompensa")]
    [SerializeField] private int moedasRecompensa = 5;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteFechado;
    [SerializeField] private Sprite spriteAberto;

    [Header("Diálogo (opcional)")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField, TextArea] private string mensagemAberto = "Este baú já foi aberto.";

    private bool aberto;

    void Start()
    {
        Debug.Log($"[Chest] Start — spriteRenderer: {(spriteRenderer != null ? "OK" : "NULO")}, " +
                  $"spriteFechado: {(spriteFechado != null ? spriteFechado.name : "NULO")}, " +
                  $"spriteAberto: {(spriteAberto != null ? spriteAberto.name : "NULO")}, " +
                  $"aberto: {aberto}", this);

        if (spriteRenderer != null && spriteFechado != null && !aberto)
            spriteRenderer.sprite = spriteFechado;
    }

    public override bool PodeInteragir() => true;

    public override void Interagir()
    {
        Debug.Log($"[Chest] Interagir() chamado — aberto antes: {aberto}", this);

        if (!aberto)
        {
            aberto = true;

            Debug.Log($"[Chest] Bloco PRIMEIRA INTERAÇÃO — " +
                      $"spriteRenderer: {(spriteRenderer != null ? "OK" : "NULO")}, " +
                      $"spriteAberto: {(spriteAberto != null ? spriteAberto.name : "NULO")}, " +
                      $"CoinManager.Instance: {(CoinManager.Instance != null ? "OK" : "NULO")}, " +
                      $"moedasRecompensa: {moedasRecompensa}", this);

            if (spriteRenderer != null && spriteAberto != null)
            {
                spriteRenderer.sprite = spriteAberto;
                Debug.Log($"[Chest] Sprite trocado para: {spriteRenderer.sprite.name}", this);
            }
            else
            {
                Debug.LogWarning("[Chest] Sprite NÃO trocado — verifique spriteRenderer e spriteAberto.", this);
            }

            if (CoinManager.Instance != null)
            {
                int antes = CoinManager.Instance.CurrentCoins;
                CoinManager.Instance.AdicionarMoedas(moedasRecompensa);
                Debug.Log($"[Chest] Moedas: {antes} -> {CoinManager.Instance.CurrentCoins}", this);
            }
            else
            {
                Debug.LogError("[Chest] CoinManager.Instance e NULO — nao ha CoinManager na cena.", this);
            }
        }
        else
        {
            Debug.Log("[Chest] Entrou no ELSE — bau ja estava aberto.", this);
            dialogueUI?.MostrarSimples(mensagemAberto);
        }
    }
}