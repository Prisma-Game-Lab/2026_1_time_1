using System.Collections;
using UnityEngine;

public class CoalemosAI : MonoBehaviour
{
    [Header("Attack References")]
    [SerializeField] private CoalemosChicken   chickenAttack;
    [SerializeField] private CoalemosHandAttack handAttack;
    [SerializeField] private CoalemosElf        elfAttack;

    [Header("Chicken Rain Settings")]
    [SerializeField] private int   chickenCount    = 20;
    [SerializeField] private float chickenInterval = 0.5f;

    [Header("Hand Attack Settings")]
    [SerializeField] private float swipeHeight = -3f;

    [Header("Timing")]
    [SerializeField] private float startDelay          = 4f;
    [SerializeField] private float delayBetweenAttacks = 3f;
    [SerializeField] private float comboInternalDelay  = 0.4f;

    private enum AttackMove
    {
        ChickenRain,
        LeftSlam,
        RightSlam,
        LeftSwipe,
        RightSwipe,
        LeftSlamRightSwipe,
        RightSlamLeftSwipe,
        BothSlam,
        BothSwipe,
        ElfWaveLeft,
        ElfWaveRight,
    }

    // Shuffle-bag deck: every attack appears exactly once per cycle
    private AttackMove[] deck;
    private int deckIndex;

    private WaitForSeconds attackDelay;
    private WaitForSeconds comboDelay;

    void Start()
    {
        attackDelay = new WaitForSeconds(delayBetweenAttacks);
        comboDelay  = new WaitForSeconds(comboInternalDelay);
        BuildDeck();
        StartCoroutine(AttackCycle());
    }

    private void BuildDeck()
    {
        System.Array all = System.Enum.GetValues(typeof(AttackMove));
        deck = new AttackMove[all.Length];
        for (int i = 0; i < all.Length; i++)
            deck[i] = (AttackMove)all.GetValue(i);
        Shuffle(deck);
        deckIndex = 0;
    }

    private void Shuffle(AttackMove[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }

    private AttackMove NextAttack()
    {
        if (deckIndex >= deck.Length)
        {
            AttackMove lastPlayed = deck[^1];
            Shuffle(deck);
            // Prevent the same move from appearing back-to-back across deck boundaries
            if (deck[0] == lastPlayed && deck.Length > 1)
            {
                int swapWith = Random.Range(1, deck.Length);
                (deck[0], deck[swapWith]) = (deck[swapWith], deck[0]);
            }
            deckIndex = 0;
        }
        return deck[deckIndex++];
    }

    private IEnumerator AttackCycle()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            yield return StartCoroutine(ExecuteAttack(NextAttack()));
            yield return attackDelay;
        }
    }

    private IEnumerator ExecuteAttack(AttackMove move)
    {
        switch (move)
        {
            case AttackMove.ChickenRain:
                chickenAttack.ChickenRain(chickenCount, chickenInterval);
                yield return new WaitUntil(() => !chickenAttack.IsAttacking);
                break;

            case AttackMove.LeftSlam:
                handAttack.HandSlam(CoalemosHandAttack.Hand.Left);
                yield return new WaitUntil(() => !handAttack.IsAttacking);
                break;

            case AttackMove.RightSlam:
                handAttack.HandSlam(CoalemosHandAttack.Hand.Right);
                yield return new WaitUntil(() => !handAttack.IsAttacking);
                break;

            case AttackMove.LeftSwipe:
                handAttack.HandSwipe(CoalemosHandAttack.Hand.Left, swipeHeight);
                yield return new WaitUntil(() => !handAttack.IsAttacking);
                break;

            case AttackMove.RightSwipe:
                handAttack.HandSwipe(CoalemosHandAttack.Hand.Right, swipeHeight);
                yield return new WaitUntil(() => !handAttack.IsAttacking);
                break;

            case AttackMove.LeftSlamRightSwipe:
                handAttack.HandSlam(CoalemosHandAttack.Hand.Left);
                yield return comboDelay;
                handAttack.HandSwipe(CoalemosHandAttack.Hand.Right, swipeHeight);
                yield return new WaitUntil(() => !handAttack.IsAttacking);
                break;

            case AttackMove.RightSlamLeftSwipe:
                handAttack.HandSlam(CoalemosHandAttack.Hand.Right);
                yield return comboDelay;
                handAttack.HandSwipe(CoalemosHandAttack.Hand.Left, swipeHeight);
                yield return new WaitUntil(() => !handAttack.IsAttacking);
                break;

            case AttackMove.BothSlam:
                handAttack.HandSlam(CoalemosHandAttack.Hand.Left);
                yield return comboDelay;
                handAttack.HandSlam(CoalemosHandAttack.Hand.Right);
                yield return new WaitUntil(() => !handAttack.IsAttacking);
                break;

            case AttackMove.BothSwipe:
                handAttack.HandSwipe(CoalemosHandAttack.Hand.Left, swipeHeight);
                yield return comboDelay;
                handAttack.HandSwipe(CoalemosHandAttack.Hand.Right, swipeHeight);
                yield return new WaitUntil(() => !handAttack.IsAttacking);
                break;

            case AttackMove.ElfWaveLeft:
                elfAttack.ElfWave(fromLeft: true);
                yield return new WaitUntil(() => !elfAttack.IsAttacking);
                break;

            case AttackMove.ElfWaveRight:
                elfAttack.ElfWave(fromLeft: false);
                yield return new WaitUntil(() => !elfAttack.IsAttacking);
                break;
        }
    }
}
