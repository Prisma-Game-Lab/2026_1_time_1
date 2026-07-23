using UnityEngine;

public class ProjetilTutorialSpawner : MonoBehaviour
{
    [Header("Projétil parável (existente)")]
    [Tooltip("Prefab do projétil que já existe no jogo. Precisa da tag 'Projectile' para virar orb no parry.")]
    [SerializeField] private GameObject projetilPrefab;
    [Tooltip("De onde o projétil sai.")]
    [SerializeField] private Transform pontoSpawn;

    [Header("Mira")]
    [Tooltip("Se marcado, gira o projétil para ir na direção do player.")]
    [SerializeField] private bool mirarNoPlayer = true;
    [Tooltip("Alvo. Se vazio, procura o objeto com tag 'Player'.")]
    [SerializeField] private Transform alvo;
    [Tooltip("Ajuste fino de ângulo (graus). Se o projétil se move em outro eixo (ex.: 'para cima' como a flecha), use 90 ou -90.")]
    [SerializeField] private float offsetAngulo = 0f;

    [Header("Detecção de erro (passou/whiff)")]
    [Tooltip("Se o projétil se afastar mais que isto do ponto de spawn sem parry, conta como erro.")]
    [SerializeField] private float distanciaMaxima = 25f;
    [Tooltip("Tempo máximo (s) sem resolução antes de considerar erro. 0 = desliga.")]
    [SerializeField] private float tempoLimite = 6f;

    public System.Action OnWhiff;

    private GameObject atual;
    private Vector3 origem;
    private bool monitorando;
    private float timer;

    public void Iniciar()
    {
        if (projetilPrefab == null || pontoSpawn == null)
        {
            Debug.LogError("[ProjetilTutorial] 'projetilPrefab' ou 'pontoSpawn' não atribuído.", this);
            return;
        }

        origem = pontoSpawn.position;
        atual = Instantiate(projetilPrefab, origem, pontoSpawn.rotation);

        if (mirarNoPlayer)
        {
            Transform t = alvo != null ? alvo : BuscarPlayer();
            if (t != null)
            {
                Vector2 dir = ((Vector2)t.position - (Vector2)origem).normalized;
                float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + offsetAngulo;
                atual.transform.rotation = Quaternion.Euler(0f, 0f, ang);
            }
            else
            {
                Debug.LogWarning("[ProjetilTutorial] Alvo não encontrado (tag 'Player'). Usando rotação do pontoSpawn.", this);
            }
        }
        monitorando = true;
        timer = tempoLimite;
    }

    public void Parar()
    {
        monitorando = false;
        if (atual != null) Destroy(atual);
        atual = null;
    }
    private void Update()
    {
        if (!monitorando || atual == null) return;

        if (Vector2.Distance(atual.transform.position, origem) >= distanciaMaxima) { Whiff(); return; }

        if (tempoLimite > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) Whiff();
        }
    }
    private void Whiff()
    {
        monitorando = false;
        if (atual != null) { Destroy(atual); atual = null; }
        OnWhiff?.Invoke();
    }
    private Transform BuscarPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        return p != null ? p.transform : null;
    }
}