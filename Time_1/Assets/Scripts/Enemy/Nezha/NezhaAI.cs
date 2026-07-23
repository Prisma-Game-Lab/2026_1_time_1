using System.Collections;
using UnityEngine;
public class NezhaAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NezhaMovement movement;
    [SerializeField] private NezhaFireball fireballAttack;
    [SerializeField] private NezhaChute chuteAttack;                 
    [SerializeField] private NezhaTeleporteSlam teleporteSlamAttack; 
    [SerializeField] private Camera cam;

    [Header("Timing")]
    [SerializeField] private float minIdleTime = 1.2f;
    [SerializeField] private float maxIdleTime = 2.5f;

    [Header("Neutral Movement")]
    [SerializeField][Range(0f, 1f)] private float edgeMoveProbability = 0.3f;
    [SerializeField] private float edgeMargin = 1.5f;

    [Header("Attack Weights")]
    [SerializeField] private float weightFireball = 1f;
    [SerializeField] private float weightChute = 1f;          
    [SerializeField] private float weightTeleporteSlam = 1f;  

    private int lastAttack = -1;

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        StartCoroutine(MainLoop());
    }
    private IEnumerator MainLoop()
    {
        while (true)
        {
            yield return IdlePhase();

            int attack = PickAttack();
            lastAttack = attack;

            switch (attack)
            {
                case 0:
                    fireballAttack.Iniciar();
                    yield return new WaitUntil(() => !fireballAttack.IsAttacking);
                    break;

                case 1: // NOVO — Chute
                    chuteAttack.Iniciar();
                    yield return new WaitUntil(() => !chuteAttack.IsAttacking);
                    break;

                case 2: // NOVO — Teleporte + esmagamento
                    teleporteSlamAttack.Iniciar();
                    yield return new WaitUntil(() => !teleporteSlamAttack.IsAttacking);
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
                movement.WalkToX(edgeTarget);
            else
                movement.WalkTowardsPlayer();

            elapsed += Time.deltaTime;
            yield return null;
        }

        movement.Stop();
    }
    private int PickAttack()
    {
        float[] weights = { weightFireball, weightChute, weightTeleporteSlam };

        if (lastAttack >= 0 && lastAttack < weights.Length)
            weights[lastAttack] = 0f;

        float total = 0f;
        for (int i = 0; i < weights.Length; i++) total += weights[i];

        if (total <= 0f) return 0;

        float roll = Random.value * total;
        for (int i = 0; i < weights.Length; i++)
        {
            if (roll < weights[i]) return i;
            roll -= weights[i];
        }

        return 0;
    }

    private float GetEdgeX()
    {
        if (cam == null) return transform.position.x;

        float halfWidth = cam.orthographicSize * cam.aspect;
        float centerX = cam.transform.position.x;

        bool goLeft = Random.value < 0.5f;
        return goLeft
            ? centerX - halfWidth + edgeMargin
            : centerX + halfWidth - edgeMargin;
    }
}