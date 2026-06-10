using TMPro;
using UnityEngine;

public class CoinHUD : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private string prefixo = "x";

    private bool subscrito;

    void Start() => TentarSubscrever();
    void OnEnable() => TentarSubscrever();

    private void TentarSubscrever()
    {
        if (subscrito) return;
        if (CoinManager.Instance == null)
        {
            Debug.LogWarning("[CoinHUD] CoinManager.Instance ainda nulo — vai tentar de novo em Start.", this);
            return;
        }

        CoinManager.Instance.OnCoinsChanged += AtualizarTexto;
        subscrito = true;
        AtualizarTexto(CoinManager.Instance.CurrentCoins, CoinManager.Instance.MaxCoins);
        Debug.Log("[CoinHUD] Subscrito ao OnCoinsChanged.", this);
    }

    void OnDisable()
    {
        if (subscrito && CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinsChanged -= AtualizarTexto;
            subscrito = false;
        }
    }

    private void AtualizarTexto(int atual, int max)
    {
        if (texto == null)
        {
            Debug.LogError("[CoinHUD] Campo 'texto' nao atribuido no Inspector.", this);
            return;
        }
        texto.text = $"{prefixo}{atual}";
        Debug.Log($"[CoinHUD] HUD atualizado: {texto.text}", this);
    }
}