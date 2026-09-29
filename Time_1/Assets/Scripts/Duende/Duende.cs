using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Duende : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Sprite Aleatório")]
    [SerializeField] private Sprite[] elfSprites;

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxVelocity = 8f;
    [SerializeField] private float directionChangeIntervalMin = 0.5f;
    [SerializeField] private float directionChangeIntervalMax = 2f;

    [Header("Pulos")]
    [SerializeField] private float jumpFrequency = 1f;
    [SerializeField] private float jumpForceMin = 8f;
    [SerializeField] private float jumpForceMax = 14f;

    [Header("Detecção")]
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private float attackRadius = 1.5f;

    [Header("Combate")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float contactDamageCooldown = 0.8f;
    [SerializeField] private float knockbackForce = 6f;

    [Header("Ataque à Distância")]
    [SerializeField] private bool enableRangedAttack = false;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private int projectileDamage = 5;
    [SerializeField] private float fireInterval = 2.5f;

    [Header("Mira")]
    [Tooltip("Mira onde o player vai estar quando o tiro chegar.")]
    [SerializeField] private bool preverMovimento = true;
    [Tooltip("1 = previsão total, 0 = mira na posição atual.")]
    [SerializeField, Range(0f, 1f)] private float fatorPrevisao = 1f;
    [Tooltip("Distância máxima que a previsão pode deslocar o alvo.")]
    [SerializeField] private float previsaoMaxima = 4f;
    [Tooltip("Erro de mira em unidades de mundo (não cresce com a distância).")]
    [SerializeField] private float desvioMira = 0.3f;
    [Tooltip("Atraso aleatório antes do primeiro tiro, para o duende entrar na arena.")]
    [SerializeField] private float primeiroTiroMin = 0.5f;
    [SerializeField] private float primeiroTiroMax = 1.2f;
    [Tooltip("Margem da tela (0 a 0.5). Fora dela o duende não atira.")]
    [SerializeField, Range(0f, 0.5f)] private float margemTela = 0.05f;
    [Tooltip("Desenha as linhas de mira na aba Scene (verde = centro do alvo, vermelha = mira final).")]
    [SerializeField] private bool debugMira = false;

    [Header("Áudio")]
    [SerializeField] private AudioClip sfxJump;
    [SerializeField] private AudioClip sfxAttack;
    [SerializeField] private AudioClip sfxDeath;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private bool _isAlive = true;

    // Alvo
    private Collider2D _playerCol;
    private Rigidbody2D _playerRb;

    // Movimento
    private float _currentDirectionX = 1f;
    private float _directionTimer = 0f;
    private float _nextDirectionChange;

    // Pulo
    private float _jumpTimer = 0f;
    private float _jumpInterval;

    // Combate
    private float _contactTimer = 0f;
    private float _fireTimer = 0f;
    private bool _contactDamageEnabled = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        _col.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
        _jumpInterval = JumpFrequencyToInterval(jumpFrequency);

        if (spriteRenderer != null && elfSprites != null && elfSprites.Length > 0)
            spriteRenderer.sprite = elfSprites[Random.Range(0, elfSprites.Length)];

        // Direção inicial: esquerda ou direita com igual probabilidade
        _currentDirectionX = Random.value < 0.5f ? -1f : 1f;
        _nextDirectionChange = Random.Range(directionChangeIntervalMin, directionChangeIntervalMax);

        CacheAlvo();

        // Não atira no primeiro frame (evita tiro da borda da arena)
        _fireTimer = Random.Range(primeiroTiroMin, primeiroTiroMax);
    }

    private void CacheAlvo()
    {
        // Referência para o prefab (asset) em vez do player da cena: descarta e busca de novo
        if (playerTransform != null && !playerTransform.gameObject.scene.IsValid())
        {
            Debug.LogWarning("[Duende] Player Transform aponta para um prefab, não para o player da cena. Buscando pela tag.", this);
            playerTransform = null;
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogWarning("[Duende] Jogador não encontrado. Atribua playerTransform ou use a tag 'Player'.", this);
        }

        if (playerTransform == null) return;

        // Collider de corpo: precisa estar ATIVO e não ser trigger
        _playerCol = null;
        foreach (Collider2D c in playerTransform.GetComponentsInChildren<Collider2D>())
        {
            if (c.enabled && !c.isTrigger) { _playerCol = c; break; }
        }

        _playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (_playerRb == null)
            _playerRb = playerTransform.GetComponentInChildren<Rigidbody2D>();

        if (debugMira)
            Debug.Log($"[Duende] Alvo: {playerTransform.name} | Collider: {(_playerCol != null ? _playerCol.name : "nenhum")}", this);
    }

    private void Update()
    {
        if (!_isAlive) return;
        TickTimers();
        // HandleMovement();
        // HandleJump();
        // FlipSprite();

        // Combate só ocorre se jogador estiver dentro do raio de detecção
        if (playerTransform != null && JogadorDetectado())
        {
            HandleContactDamage();
            HandleRangedAttack();
        }
    }

    private void TickTimers()
    {
        _directionTimer += Time.deltaTime;
        _jumpTimer += Time.deltaTime;
        _contactTimer = Mathf.Max(0f, _contactTimer - Time.deltaTime);
        _fireTimer = Mathf.Max(0f, _fireTimer - Time.deltaTime);
    }

    // ── Movimento ────────────────────────────────────────────────────
    private void HandleMovement()
    {
        if (_directionTimer >= _nextDirectionChange)
            TrocarDirecao();

        float targetVelocityX = _currentDirectionX * moveSpeed;
        float newVelocityX = Mathf.MoveTowards(_rb.velocity.x, targetVelocityX, 25f * Time.deltaTime);
        newVelocityX = Mathf.Clamp(newVelocityX, -maxVelocity, maxVelocity);

        _rb.velocity = new Vector2(newVelocityX, _rb.velocity.y);
    }

    private void TrocarDirecao()
    {
        // Sempre inverte — garante alternância real
        _currentDirectionX = -_currentDirectionX;
        _directionTimer = 0f;
        _nextDirectionChange = Random.Range(directionChangeIntervalMin, directionChangeIntervalMax);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isAlive) return;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                TrocarDirecao();
                return;
            }
        }
    }

    private void HandleJump()
    {
        if (_jumpTimer < _jumpInterval) return;
        if (!IsGrounded()) return;

        _jumpTimer = 0f;

        // Sorteia o comportamento do pulo — 4 opções com pesos iguais
        float sorteio = Random.value;
        float forcaX;
        float forcaY = Random.Range(jumpForceMin, jumpForceMax);
        if (sorteio < 0.25f)
        {
            forcaX = _currentDirectionX * moveSpeed;
        }
        else if (sorteio < 0.5f)
        {
            TrocarDirecao();
            forcaX = _currentDirectionX * moveSpeed;
        }
        else if (sorteio < 0.75f)
        {
            forcaX = _currentDirectionX * moveSpeed * 1.5f;
        }
        else
        {
            forcaX = _currentDirectionX * moveSpeed * 0.3f;
        }
        _rb.velocity = new Vector2(forcaX, forcaY);
        SFXManager.PlaySFX("duende_pulo");
    }

    private float JumpFrequencyToInterval(float frequency)
    {
        if (frequency <= 0f) return float.MaxValue;
        return 1f / frequency;
    }

    private bool JogadorDetectado()
    {
        return Vector2.Distance(transform.position, playerTransform.position) <= detectionRadius;
    }

    public void DisableContactDamage() => _contactDamageEnabled = false;

    // ── Dano por contato ─────────────────────────────────────────────
    private void HandleContactDamage()
    {
        if (!_contactDamageEnabled) return;
        if (_contactTimer > 0f) return;
        if (Vector2.Distance(transform.position, playerTransform.position) > attackRadius) return;

        PlayerHealthController playerHealth = playerTransform.GetComponentInChildren<PlayerHealthController>();
        if (playerHealth == null) return;

        playerHealth.TakeDamage(contactDamage);
        _contactTimer = contactDamageCooldown;

        if (_playerRb != null)
        {
            Vector2 knockDir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            _playerRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    // ── Ataque à distância ───────────────────────────────────────────
    private void HandleRangedAttack()
    {
        if (!enableRangedAttack) return;
        if (projectilePrefab == null) return;
        if (_fireTimer > 0f) return;
        if (!DentroDaTela()) return;

        FireProjectile();
        _fireTimer = fireInterval;
    }

    private void FireProjectile()
    {
        if (playerTransform == null) return;

        Transform spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
        Vector2 origem = spawnPos.position;
        Vector2 alvo = CalcularPontoDeMira(origem);

        // Desvio perpendicular à linha de tiro, em unidades de mundo
        Vector2 dir = (alvo - origem).normalized;
        Vector2 perpendicular = new Vector2(-dir.y, dir.x);
        alvo += perpendicular * Random.Range(-desvioMira, desvioMira);
        dir = (alvo - origem).normalized;

        if (debugMira)
            Debug.DrawLine(origem, alvo, Color.red, 1f);

        GameObject proj = Instantiate(projectilePrefab, origem, Quaternion.identity);

        DundeProjectile dp = proj.GetComponent<DundeProjectile>();
        if (dp != null)
            dp.Initialize(dir, projectileSpeed, projectileDamage);
        else if (proj.TryGetComponent(out Rigidbody2D projRb))
            projRb.velocity = dir * projectileSpeed;

        SFXManager.PlaySFX("duende_ataque");
    }

    private Vector2 CalcularPontoDeMira(Vector2 origem)
    {
        // Centro real do corpo do player (não o pivô da raiz)
        Vector2 centro = (_playerCol != null && _playerCol.enabled)
            ? (Vector2)_playerCol.bounds.center
            : (Vector2)playerTransform.position;

        if (debugMira)
            Debug.DrawLine(origem, centro, Color.green, 1f);

        if (!preverMovimento || _playerRb == null || projectileSpeed <= 0f)
            return centro;

        Vector2 v = _playerRb.velocity * fatorPrevisao;

        // Gravidade só entra se o player estiver no ar
        bool noAr = Mathf.Abs(_playerRb.velocity.y) > 0.05f;
        Vector2 g = noAr ? Physics2D.gravity * _playerRb.gravityScale * fatorPrevisao : Vector2.zero;

        Vector2 alvo = centro;
        for (int i = 0; i < 3; i++)
        {
            float t = Vector2.Distance(origem, alvo) / projectileSpeed;
            alvo = centro + v * t + 0.5f * g * t * t;
        }

        // Limita o quanto a previsão pode se afastar do player (evita picos de velocidade)
        return centro + Vector2.ClampMagnitude(alvo - centro, previsaoMaxima);
    }

    private bool DentroDaTela()
    {
        Camera cam = Camera.main;
        if (cam == null) return true;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        return vp.x > margemTela && vp.x < 1f - margemTela
            && vp.y > margemTela && vp.y < 1f - margemTela;
    }

    // ── Utilidades ───────────────────────────────────────────────────
    private void FlipSprite()
    {
        if (spriteRenderer == null) return;
        if (Mathf.Abs(_rb.velocity.x) > 0.05f)
            spriteRenderer.flipX = _rb.velocity.x < 0f;
    }

    private bool IsGrounded()
    {
        Bounds bounds = _col.bounds;
        return Physics2D.OverlapBox(
            new Vector2(bounds.center.x, bounds.min.y),
            new Vector2(bounds.size.x * 0.9f, 0.1f),
            0f,
            groundLayer
        );
    }

    public void OnDeath()
    {
        if (!_isAlive) return;
        _isAlive = false;

        _rb.velocity = Vector2.zero;
        _rb.simulated = false;
        _col.enabled = false;

        SFXManager.PlaySFX("duende_morte");
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}