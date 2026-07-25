using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnhangaRaizes : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AnhangaMovement movement;
    [SerializeField] private GameObject raizPrefab;
    [SerializeField] private GameObject brilhoOlhos;

    [Header("Layout das Brechas")]
    [SerializeField] private int numeroDeSlots = 9;
    [SerializeField] private int numeroDeBrechas = 3;
    [Tooltip("Altura do chao onde as raizes nascem")]
    [SerializeField] private float groundY = -3.5f;
    [Tooltip("Margem nas bordas pra nao nascer colado nas paredes")]
    [SerializeField] private float margemBorda = 0.5f;

    [Header("Escala")]
    [SerializeField] private float escalaAviso = 0.3f;
    [SerializeField] private float escalaFinal = 1f;

    [Header("Tempos")]
    [Tooltip("Fase 1 e pequena, SEM dano")]
    [SerializeField] private float tempoAviso = 0.8f;
    [Tooltip("Fase 2 e cresce; o DANO liga aqui")]
    [SerializeField] private float tempoCrescimento = 0.25f;
    [Tooltip("Fase 3 e erguida")]
    [SerializeField] private float tempoEmPe = 1.5f;
    [Tooltip("Fase 4 e desce e some")]
    [SerializeField] private float tempoRecolhe = 0.4f;

    [Header("audio")]
    [SerializeField] private AudioClip sfxRaizes;

    private Coroutine routine;
    public bool IsAttacking => routine != null;

    // Escala original do prefab, lida uma vez.
    private Vector3 prefabBaseScale;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<AnhangaMovement>();
        if (raizPrefab != null)
            prefabBaseScale = raizPrefab.transform.localScale;
    }
    public void Iniciar()
    {
        if (routine != null) return;
        if (raizPrefab == null || movement == null)
        {
            return;
        }
        routine = StartCoroutine(RaizesRoutine());
    }
    private IEnumerator RaizesRoutine()
    {
        List<float> posicoes = CalcularPosicoes();
        HashSet<int> brechas = SortearBrechas(posicoes.Count);

        List<Transform> raizes = new();
        List<Collider2D> colliders = new();

        for (int i = 0; i < posicoes.Count; i++)
        {
            if (brechas.Contains(i)) continue;

            Vector3 pos = new Vector3(posicoes[i], groundY, 0f);
            GameObject go = Instantiate(raizPrefab, pos, Quaternion.identity);
            go.transform.localScale = prefabBaseScale * escalaAviso;

            Collider2D col = go.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            Destroy(go, tempoAviso + tempoCrescimento + tempoEmPe + tempoRecolhe + 1f);

            raizes.Add(go.transform);
            colliders.Add(col);
        }

        if (brilhoOlhos != null) brilhoOlhos.SetActive(true);
        if (sfxRaizes != null) SFXManager.PlaySFX("anhanga_raizes");

        yield return new WaitForSeconds(tempoAviso);

        if (brilhoOlhos != null) brilhoOlhos.SetActive(false);

        for (int i = 0; i < colliders.Count; i++)
            if (colliders[i] != null) colliders[i].enabled = true;

        float t = 0f;
        while (t < tempoCrescimento)
        {
            t += Time.deltaTime;
            float k = tempoCrescimento > 0f ? Mathf.Clamp01(t / tempoCrescimento) : 1f;
            float mult = Mathf.Lerp(escalaAviso, escalaFinal, k);
            AplicarEscala(raizes, mult);
            yield return null;
        }
        AplicarEscala(raizes, escalaFinal);
        yield return new WaitForSeconds(tempoEmPe);
        t = 0f;
        while (t < tempoRecolhe)
        {
            t += Time.deltaTime;
            float k = tempoRecolhe > 0f ? Mathf.Clamp01(t / tempoRecolhe) : 1f;
            float mult = Mathf.Lerp(escalaFinal, 0f, k);
            AplicarEscala(raizes, mult);
            yield return null;
        }
        foreach (Transform r in raizes)
            if (r != null) Destroy(r.gameObject);

        routine = null;
    }
    private List<float> CalcularPosicoes()
    {
        float minX = movement.MinX + margemBorda;
        float maxX = movement.MaxX - margemBorda;
        List<float> pos = new();

        if (numeroDeSlots <= 1)
        {
            pos.Add((minX + maxX) * 0.5f);
            return pos;
        }
        float passo = (maxX - minX) / (numeroDeSlots - 1);
        for (int i = 0; i < numeroDeSlots; i++)
            pos.Add(minX + passo * i);
        return pos;
    }
    private HashSet<int> SortearBrechas(int total)
    {
        HashSet<int> brechas = new();
        int alvo = Mathf.Clamp(numeroDeBrechas, 0, total - 1);
        int tentativas = 0;
        while (brechas.Count < alvo && tentativas < 1000)
        {
            brechas.Add(Random.Range(0, total));
            tentativas++;
        }
        return brechas;
    }
    private void AplicarEscala(List<Transform> raizes, float multiplicador)
    {
        Vector3 escala = prefabBaseScale * multiplicador;
        foreach (Transform r in raizes)
            if (r != null) r.localScale = escala;
    }
}