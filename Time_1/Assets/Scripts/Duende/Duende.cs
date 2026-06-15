using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Duende : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform projectileSpawnPoint;

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
    [SerializeField] private float detectionRadius = 6f;
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

    [Header("Áudio")]
    [SerializeField] private AudioClip sfxJump;
    [SerializeField] private AudioClip sfxAttack;
    [SerializeField] private AudioClip sfxDeath;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private bool _isAlive = true;

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

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        _col.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
        _jumpInterval = JumpFrequencyToInterval(jumpFrequency);
        // Direção inicial: esquerda ou direita com igual probabilidade
        _currentDirectionX = Random.value < 0.5f ? -1f : 1f;
        _nextDirectionChange = Random.Range(directionChangeIntervalMin, directionChangeIntervalMax);
        // Localiza jogador automaticamente pela tag
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogWarning("[Duende] Jogador não encontrado. Atribua playerTransform ou use a tag 'Player'.", this);
        }
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

    private void HandleMovement()
    {
        // Troca de direção por timer
        if (_directionTimer >= _nextDirectionChange)
            TrocarDirecao();

        float targetVelocityX = _currentDirectionX * moveSpeed;
        float newVelocityX = Mathf.MoveTowards(_rb.velocity.x, targetVelocityX, 25f * Time.deltaTime);
        newVelocityX = Mathf.Clamp(newVelocityX, -maxVelocity, maxVelocity);

        _rb.velocity = new Vector2(newVelocityX, _rb.velocity.y);
    }
    private void TrocarDirecao()
    {
        // Sempre inverte — garante alternância real, nunca fica na mesma direção por dois timers seguidos
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
            // Continua na direção atual com impulso normal
            forcaX = _currentDirectionX * moveSpeed;
        }
        else if (sorteio < 0.5f)
        {
            // Inverte a direção no ar
            TrocarDirecao();
            forcaX = _currentDirectionX * moveSpeed;
        }
        else if (sorteio < 0.75f)
        {
            // Impulso diagonal forte — salta mais para o lado
            forcaX = _currentDirectionX * moveSpeed * 1.5f;
        }
        else
        {
            // Impulso curto — quase no lugar
            forcaX = _currentDirectionX * moveSpeed * 0.3f;
        }
        _rb.velocity = new Vector2(forcaX, forcaY);
        AudioManager.Instance?.TocaSFX(sfxJump);
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

    // ── Dano por contato ─────────────────────────────────────────────
    private void HandleContactDamage()
    {
        if (_contactTimer > 0f) return;
        if (Vector2.Distance(transform.position, playerTransform.position) > attackRadius) return;

        PlayerHealthController playerHealth = playerTransform.GetComponent<PlayerHealthController>();
        if (playerHealth == null) return;

        playerHealth.TakeDamage(contactDamage);
        _contactTimer = contactDamageCooldown;

        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockDir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            playerRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }
    }
    private void HandleRangedAttack()
    {
        if (!enableRangedAttack) return;
        if (projectilePrefab == null) return;
        if (_fireTimer > 0f) return;

        FireProjectile();
        _fireTimer = fireInterval;
    }
    private void FireProjectile()
    {
        if (playerTransform == null) return;

        Transform spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
        Vector2 dir = ((Vector2)playerTransform.position - (Vector2)spawnPos.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, spawnPos.position, Quaternion.identity);

        DundeProjectile dp = proj.GetComponent<DundeProjectile>();
        if (dp != null)
            dp.Initialize(dir, projectileSpeed, projectileDamage);
        else
        {
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
                projRb.velocity = dir * projectileSpeed;
        }

        AudioManager.Instance?.TocaSFX(sfxAttack);
    }
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

        AudioManager.Instance?.TocaSFX(sfxDeath);
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