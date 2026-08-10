using System.Collections;
using UnityEngine;
public class NezhaAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NezhaMovement movement;
    [SerializeField] private NezhaFireball fireballAttack;
    [SerializeField] private NezhaChute chuteAttack;
    [SerializeField] private NezhaTeleporteSlam teleporteSlamAttack;
    [SerializeField] private NezhaSlam slamAttack;
    [SerializeField] private NezhaFirewall firewallAttack;
    [Tooltip("Ataque de chao disparado ao terminar a flutuacao (com chance).")]
    [SerializeField] private NezhaChaoDeFogo chaoDeFogoAttack;
    [SerializeField] private Camera cam;
    [Tooltip("Se vazio, tenta achar pela tag 'Player' no Start.")]
    [SerializeField] private Transform playerTransform;

    [Header("Timing")]
    [SerializeField] private float minIdleTime = 0.4f;
    [SerializeField] private float maxIdleTime = 1.0f;

    [Header("Neutral Movement")]
    [Tooltip("Distancia horizontal que o boss tenta manter do player no neutro.")]
    [SerializeField] private float preferredSpacing = 4.5f;
    [SerializeField] private float spacingTolerance = 1.2f;
    [SerializeField][Range(0f, 1f)] private float edgeMoveProbability = 0.2f;
    [SerializeField] private float edgeMargin = 1.5f;

    [Header("Attack Weights (base)")]
    [SerializeField] private float weightFireball = 1f;
    [SerializeField] private float weightChute = 1f;
    [SerializeField] private float weightTeleporteSlam = 1f;
    [SerializeField] private float weightSlam = 1f;
    [SerializeField] private float weightFirewall = 1f;

    [Header("Context Tuning")]
    [Tooltip("Distancia horizontal considerada 'perto'.")]
    [SerializeField] private float closeRange = 3f;
    [Tooltip("Distancia horizontal considerada 'longe'.")]
    [SerializeField] private float farRange = 8f;
    [Tooltip("Diferenca de altura para considerar o player 'no ar/acima'.")]
    [SerializeField] private float airHeightThreshold = 2f;
    [Tooltip("Velocidade vertical do player acima da qual ele e considerado 'pulando'.")]
    [SerializeField] private float airVelThreshold = 3f;
    [Tooltip("O peso do ultimo ataque e multiplicado por isto (evita repetir sem travar de vez).")]
    [SerializeField][Range(0f, 1f)] private float repeatPenalty = 0.25f;
    [Tooltip("0 = totalmente reativo, 1 = quase aleatorio. Mantem imprevisibilidade.")]
    [SerializeField][Range(0f, 1f)] private float randomness = 0.25f;

    [Header("Flutuacao (voo ocasional)")]
    [Tooltip("Chance de flutuar entre ataques (respeitando o cooldown).")]
    [SerializeField][Range(0f, 1f)] private float floatChance = 0.35f;
    [SerializeField] private float floatMinDuration = 3f;
    [SerializeField] private float floatMaxDuration = 5f;
    [Tooltip("Tempo minimo entre o fim de uma flutuacao e a proxima.")]
    [SerializeField] private float floatCooldown = 9f;
    [Tooltip("Altura acima da posicao atual para pairar.")]
    [SerializeField] private float floatHeight = 3.5f;
    [SerializeField] private float floatHorizSpeed = 3.5f;
    [SerializeField] private float floatVertSpeed = 4f;
    [Tooltip("Espacamento horizontal que o boss tenta manter do player enquanto paira.")]
    [SerializeField] private float floatChaseSpacing = 2.5f;
    [SerializeField] private float floatLandTimeout = 4f;
    [Tooltip("Chance de soltar o ataque de chao ao terminar uma flutuacao.")]
    [SerializeField][Range(0f, 1f)] private float chanceChaoPosFloat = 0.5f;

    private int lastAttack = -1;
    private float lastFloatTime = -999f;

    // Rastreamento vertical do player (estima se esta pulando).
    private float prevPlayerY;
    private float playerVerticalVel;

    private void Start()
    {
        if (cam == null) cam = Camera.main;

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (playerTransform != null) prevPlayerY = playerTransform.position.y;

        StartCoroutine(MainLoop());
    }
    private void Update()
    {
        if (playerTransform == null) return;
        float y = playerTransform.position.y;
        playerVerticalVel = (y - prevPlayerY) / Mathf.Max(Time.deltaTime, 1e-5f);
        prevPlayerY = y;
    }
    private IEnumerator MainLoop()
    {
        while (true)
        {
            yield return IdlePhase();

            // Flutuacao ocasional entre ataques: so reposiciona/persegue, nao ataca.
            if (Time.time - lastFloatTime >= floatCooldown && Random.value < floatChance)
            {
                yield return FloatPhase();
                lastFloatTime = Time.time;
                continue;
            }
            int attack = PickAttack();
            lastAttack = attack;

            switch (attack)
            {
                case 0:
                    fireballAttack.Iniciar();
                    yield return new WaitUntil(() => !fireballAttack.IsAttacking);
                    break;

                case 1:
                    chuteAttack.Iniciar();
                    yield return new WaitUntil(() => !chuteAttack.IsAttacking);
                    break;

                case 2:
                    teleporteSlamAttack.Iniciar();
                    yield return new WaitUntil(() => !teleporteSlamAttack.IsAttacking);
                    break;

                case 3:
                    slamAttack.Iniciar();
                    yield return new WaitUntil(() => !slamAttack.IsAttacking);
                    break;

                case 4:
                    firewallAttack.Iniciar();
                    yield return new WaitUntil(() => !firewallAttack.IsAttacking);
                    break;
            }
        }
    }
    private IEnumerator IdlePhase()
    {
        float idleTime = Random.Range(minIdleTime, maxIdleTime);
        float elapsed = 0f;

        bool goToEdge = Random.value < edgeMoveProbability;
        float edgeTarget = goToEdge ? GetEdgeX() : 0f;

        while (elapsed < idleTime)
        {
            if (goToEdge)
            {
                movement.WalkToX(edgeTarget);
            }
            else
            {
                // Mantem o espacamento preferido: aproxima se longe, recua se colado.
                float dist = PlayerXDistance();
                if (dist > preferredSpacing + spacingTolerance)
                    movement.WalkTowardsPlayer();
                else if (dist < preferredSpacing - spacingTolerance)
                    WalkAwayFromPlayer();
                else
                {
                    movement.Stop();
                    movement.FacePlayer();
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        movement.Stop();
    }
    private IEnumerator FloatPhase()
    {
        float duration = Random.Range(floatMinDuration, floatMaxDuration);
        float hoverY = transform.position.y + floatHeight;
        float elapsed = 0f;

        movement.FreezeInAir();

        while (elapsed < duration)
        {
            // Persegue mantendo um espacamento horizontal, pairando na altura de voo.
            float targetX = ComputeChaseX(floatChaseSpacing);
            movement.HoverTowards(targetX, hoverY, floatHorizSpeed, floatVertSpeed);

            elapsed += Time.deltaTime;
            yield return null;
        }
        movement.ReleaseFromAir();

        // Espera aterrissar antes de voltar ao loop normal.
        float land = 0f;
        while (!movement.IsGrounded && land < floatLandTimeout)
        {
            land += Time.deltaTime;
            yield return null;
        }
        movement.Stop();

        // Ao bater no chao apos flutuar, com chance, cobre o chao da arena de dano.
        if (chaoDeFogoAttack != null && Random.value < chanceChaoPosFloat)
        {
            chaoDeFogoAttack.Iniciar();
            yield return new WaitUntil(() => !chaoDeFogoAttack.IsAttacking);
        }
    }

    private int PickAttack()
    {
        float dist = PlayerXDistance();
        float heightDiff = playerTransform != null
            ? playerTransform.position.y - transform.position.y
            : 0f;

        bool playerAbove = heightDiff > airHeightThreshold;
        bool playerAirborne = playerAbove || playerVerticalVel > airVelThreshold;

        // t = 0 quando perto, 1 quando longe.
        float rangeT = Mathf.InverseLerp(closeRange, farRange, dist);

        // 5 ataques: 0 = Fireball, 1 = Chute, 2 = TeleporteSlam, 3 = Slam, 4 = Firewall.
        float[] w = { weightFireball, weightChute, weightTeleporteSlam, weightSlam, weightFirewall };

        // Fireball: chuva aerea, melhor de longe / com espaco.
        w[0] *= Mathf.Lerp(0.4f, 1.6f, rangeT);

        // Chute: dash rasteiro, otimo em media/longa distancia no mesmo nivel.
        float chuteRange = Mathf.InverseLerp(closeRange * 0.5f, farRange, dist);
        float sameLevel = playerAbove ? 0.4f : 1.2f;
        w[1] *= chuteRange * sameLevel + 0.2f;

        // TeleporteSlam: teleporta ate o player; brilha quando ele esta no ar/acima ou longe.
        w[2] *= playerAirborne ? 1.8f : 1.0f;
        w[2] *= Mathf.Lerp(0.8f, 1.4f, rangeT);

        // Slam: pulo + esmagao no centro da arena; melhor de perto e com o player no chao.
        w[3] *= playerAirborne ? 0.6f : 1.4f;
        w[3] *= Mathf.Lerp(1.3f, 0.7f, rangeT);

        // Firewall: parede de fogo no canto; melhor quando player esta no chao e longe.
        w[4] *= playerAirborne ? 0.3f : 1.4f;
        w[4] *= Mathf.Lerp(0.5f, 1.5f, rangeT);

        // Penaliza (nao zera) o ultimo ataque -> variedade sem travar combos.
        if (lastAttack >= 0 && lastAttack < w.Length)
            w[lastAttack] *= repeatPenalty;

        // Injeta aleatoriedade controlada para nao ficar explorável.
        float avg = 0f;
        for (int i = 0; i < w.Length; i++) avg += w[i];
        avg /= w.Length;

        for (int i = 0; i < w.Length; i++)
        {
            w[i] = Mathf.Max(0f, w[i]);
            w[i] = w[i] * (1f - randomness) + randomness * Random.Range(0f, avg * 2f);
        }

        return WeightedPick(w);
    }

    private int WeightedPick(float[] weights)
    {
        float total = 0f;
        for (int i = 0; i < weights.Length; i++) total += weights[i];

        if (total <= 0f) return Random.Range(0, weights.Length);

        float roll = Random.value * total;
        for (int i = 0; i < weights.Length; i++)
        {
            roll -= weights[i];
            if (roll < 0f) return i;
        }

        for (int i = weights.Length - 1; i >= 0; i--)
            if (weights[i] > 0f) return i;

        return 0;
    }

    private float PlayerXDistance()
    {
        if (playerTransform == null) return 0f;
        return Mathf.Abs(playerTransform.position.x - transform.position.x);
    }

    // Alvo horizontal que mantem 'spacing' de distancia do player, no lado onde o boss ja esta.
    private float ComputeChaseX(float spacing)
    {
        if (playerTransform == null) return transform.position.x;
        float side = Mathf.Sign(transform.position.x - playerTransform.position.x);
        if (side == 0f) side = 1f;
        return ClampToArena(playerTransform.position.x + side * spacing);
    }

    private void WalkAwayFromPlayer()
    {
        if (playerTransform == null) { movement.Stop(); return; }
        float dir = Mathf.Sign(transform.position.x - playerTransform.position.x);
        if (dir == 0f) dir = 1f;
        movement.WalkToX(ClampToArena(transform.position.x + dir * 3f));
    }

    private float ClampToArena(float x)
    {
        Camera c = cam != null ? cam : Camera.main;
        if (c == null) return x;
        float halfWidth = c.orthographicSize * c.aspect;
        float centerX = c.transform.position.x;
        return Mathf.Clamp(x, centerX - halfWidth + edgeMargin, centerX + halfWidth - edgeMargin);
    }

    private float GetEdgeX()
    {
        if (cam == null) return transform.position.x;

        float halfWidth = cam.orthographicSize * cam.aspect;
        float centerX = cam.transform.position.x;

        // Go to the edge opposite the player so Nezha doesn't corner them.
        bool playerIsRight = playerTransform != null && playerTransform.position.x > centerX;
        bool goLeft = playerIsRight;
        return goLeft
            ? centerX - halfWidth + edgeMargin
            : centerX + halfWidth - edgeMargin;
    }
}