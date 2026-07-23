using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ChuvaDeOrbs : MonoBehaviour
{
    [Header("Prefab do Nabo")]
    [Tooltip("Seu prefab de nabo/orb. Ao ser coletado, deve chamar OrbManager.Instance.AddOrb().")]
    [SerializeField] private GameObject naboPrefab;

    [Header("Área de spawn (topo da tela)")]
    [Tooltip("Centro da faixa de spawn. Se vazio, usa este GameObject.")]
    [SerializeField] private Transform centroSpawn;
    [Tooltip("Largura horizontal da chuva.")]
    [SerializeField] private float largura = 10f;

    [Header("Cadência")]
    [Tooltip("Segundos entre cada nabo.")]
    [SerializeField] private float intervalo = 0.6f;
    [Tooltip("0 = chove até chamar Parar(). >0 = para sozinho após spawnar essa quantidade.")]
    [SerializeField] private int quantidadeMax = 0;

    [Header("Queda")]
    [Tooltip("Velocidade de queda aplicada se o nabo tiver Rigidbody2D dinâmico. 0 = deixa a física/prefab decidir.")]
    [SerializeField] private float velocidadeQueda = 3f;

    private Coroutine rotina;
    private readonly List<GameObject> spawnados = new();

    private Vector3 Centro => centroSpawn != null ? centroSpawn.position : transform.position;

    public void Iniciar()
    {
        if (naboPrefab == null)
        {
            Debug.LogError("[ChuvaDeOrbs] 'naboPrefab' não atribuído no Inspector.", this);
            return;
        }
        if (rotina != null) StopCoroutine(rotina);
        rotina = StartCoroutine(Chover());
    }
    public void Parar(bool limparRestantes = false)
    {
        if (rotina != null) { StopCoroutine(rotina); rotina = null; }

        if (limparRestantes)
        {
            foreach (GameObject g in spawnados)
                if (g != null) Destroy(g);
        }
        spawnados.Clear();
    }
    private IEnumerator Chover()
    {
        int contador = 0;
        while (quantidadeMax <= 0 || contador < quantidadeMax)
        {
            SpawnarUm();
            contador++;
            yield return new WaitForSeconds(intervalo);
        }
        rotina = null;
    }
    private void SpawnarUm()
    {
        float x = Centro.x + Random.Range(-largura / 2f, largura / 2f);
        Vector3 pos = new Vector3(x, Centro.y, 0f);

        GameObject nabo = Instantiate(naboPrefab, pos, Quaternion.identity);
        spawnados.Add(nabo);

        if (velocidadeQueda > 0f)
        {
            Rigidbody2D rb = nabo.GetComponent<Rigidbody2D>();
            if (rb != null && !rb.isKinematic)
                rb.velocity = new Vector2(0f, -velocidadeQueda);
        }
    }
}