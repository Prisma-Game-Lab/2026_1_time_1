using System.Collections;
using UnityEngine;

public class CoalemosAI : MonoBehaviour
{
    [Header("Attack References")]
    [SerializeField] private CoalemosChicken  chickenAttack;
    [SerializeField] private CoalemosHandAttack handAttack;

    [Header("Chicken Rain Settings")]
    [SerializeField] private int   chickenCount    = 20;
    [SerializeField] private float chickenInterval = 0.5f;

    [Header("Hand Swipe Settings")]
    [SerializeField] private float swipeHeight = -3f;

    [Header("Timing")]
    [SerializeField] private float delayBetweenAttacks = 1.5f;

    private WaitForSeconds attackDelay;

    void Start()
    {
        attackDelay = new WaitForSeconds(delayBetweenAttacks);
        StartCoroutine(AttackCycle());
    }

    private IEnumerator AttackCycle()
    {
        while (true)
        {
            // Chicken Rain
            chickenAttack.ChickenRain(chickenCount, chickenInterval);
            yield return new WaitUntil(() => !chickenAttack.IsAttacking);
            yield return attackDelay;

            // Hand Slam (left hand)
            handAttack.HandSlam(CoalemosHandAttack.Hand.Left);
            yield return new WaitUntil(() => !handAttack.IsAttacking);
            yield return attackDelay;

            // Hand Swipe (right hand)
            handAttack.HandSwipe(CoalemosHandAttack.Hand.Right, swipeHeight);
            yield return new WaitUntil(() => !handAttack.IsAttacking);
            yield return attackDelay;
        }
    }
}
