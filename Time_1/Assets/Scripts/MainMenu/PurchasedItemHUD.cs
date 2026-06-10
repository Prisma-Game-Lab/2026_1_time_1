using UnityEngine;
using UnityEngine.UI;

public class PurchasedItemHUD : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject iconePrefab;
    [SerializeField] private Transform container;

    [Header("Referência")]
    [Tooltip("Opcional. Se nulo, usa PurchasedItemManager.Instance.")]
    [SerializeField] private PurchasedItemManager manager;

    void Start()
    {
        if (manager == null) manager = PurchasedItemManager.Instance;

        Debug.Log($"[PurchasedItemHUD] Start — manager: {(manager != null ? "OK" : "NULO")}, " +
                  $"iconePrefab: {(iconePrefab != null ? iconePrefab.name : "NULO")}, " +
                  $"container: {(container != null ? container.name : "NULO")}", this);

        if (manager == null)
        {
            Debug.LogError("[PurchasedItemHUD] PurchasedItemManager nao encontrado na cena.", this);
            return;
        }

        manager.OnItemAdicionado += AdicionarVisual;
        foreach (var it in manager.Itens) AdicionarVisual(it);
    }

    void OnDestroy()
    {
        if (manager != null) manager.OnItemAdicionado -= AdicionarVisual;
    }

    private void AdicionarVisual(PurchasedItemManager.Item item)
    {
        Debug.Log($"[PurchasedItemHUD] Recebido item: nome='{item.nome}', " +
                  $"icone={(item.icone != null ? item.icone.name : "NULO")}", this);

        if (iconePrefab == null)
        {
            Debug.LogError("[PurchasedItemHUD] iconePrefab nao atribuido.", this);
            return;
        }
        if (container == null)
        {
            Debug.LogError("[PurchasedItemHUD] container nao atribuido.", this);
            return;
        }

        GameObject go = Instantiate(iconePrefab, container);
        Image img = go.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("[PurchasedItemHUD] iconePrefab nao tem componente Image.", this);
            return;
        }

        if (item.icone != null) img.sprite = item.icone;
        else Debug.LogWarning("[PurchasedItemHUD] Item sem sprite — verifique 'Icone Item' no NPCVendor.", this);
    }
}