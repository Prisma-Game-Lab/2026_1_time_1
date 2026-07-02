using System.Collections;
using UnityEngine;

public class CoalemosHandAttack : MonoBehaviour
{
    public enum Hand { Left, Right }

    [Header("References")]
    [SerializeField] private CoalemosMovement coalemosMovement;

    [Header("Hand Sprites")]
    [SerializeField] private Sprite slamSprite;
    [SerializeField] private Sprite swipeLeftSprite;
    [SerializeField] private Sprite swipeRightSprite;

    private Transform player;

    [Header("Hand Slam")]
    [SerializeField] private float slamTrackSpeed  = 6f;
    [SerializeField] private float slamHeightAbove = 3f;
    [SerializeField] private float slamWindupDelay  = 0.6f;
    [SerializeField] private float slamPreDropDelay = 0.25f;
    [SerializeField] private float slamDropSpeed    = 30f;
    [SerializeField] private float slamReturnSpeed  = 8f;
    [SerializeField] private float slamJitterMax    = 0.25f;

    [Header("Hand Swipe")]
    [SerializeField] private float swipeSpeed         = 10f;
    [SerializeField] private float swipeApproachSpeed = 15f;
    [SerializeField] private float swipeArenaMinX     = -10f;
    [SerializeField] private float swipeArenaMaxX     =  10f;
    [SerializeField] private float swipeStartupDelay  = 0.3f;

    private Coroutine leftHandCoroutine;
    private Coroutine rightHandCoroutine;

    private WaitForSeconds waitImpact;
    private WaitForSeconds waitPreDrop;
    private WaitForSeconds waitSwipeStartup;

    public bool IsAttacking => leftHandCoroutine != null || rightHandCoroutine != null;

    void Start()
    {
        player           = coalemosMovement.Player;
        waitImpact       = new WaitForSeconds(0.2f);
        waitPreDrop      = new WaitForSeconds(slamPreDropDelay);
        waitSwipeStartup = new WaitForSeconds(swipeStartupDelay);
    }

    public void HandSlam(Hand hand)
    {
        bool isLeft = hand == Hand.Left;
        ref Coroutine slot = ref isLeft ? ref leftHandCoroutine : ref rightHandCoroutine;
        if (slot != null) StopCoroutine(slot);
        slot = StartCoroutine(HandSlamRoutine(isLeft));
    }

    public void HandSwipe(Hand hand, float height)
    {
        bool isLeft = hand == Hand.Left;
        ref Coroutine slot = ref isLeft ? ref leftHandCoroutine : ref rightHandCoroutine;
        if (slot != null) StopCoroutine(slot);
        slot = StartCoroutine(HandSwipeRoutine(isLeft, height));
    }

    private IEnumerator HandSlamRoutine(bool isLeft)
    {
        GameObject      handObj  = isLeft ? coalemosMovement.LeftHand : coalemosMovement.RightHand;
        Collider2D      handCol  = handObj.GetComponentInChildren<Collider2D>();
        Rigidbody2D     handRb   = handObj.GetComponentInChildren<Rigidbody2D>();
        Animator        handAnim = handObj.GetComponentInChildren<Animator>();
        SpriteRenderer  handSr   = handObj.GetComponentInChildren<SpriteRenderer>();

        coalemosMovement.SetHandLocked(isLeft, true);
        if (handAnim != null) handAnim.enabled = false;
        Sprite originalSprite = handSr != null ? handSr.sprite : null;
        if (handSr != null && slamSprite != null) handSr.sprite = slamSprite;
        bool wasKinematic = handRb != null && handRb.isKinematic;
        if (handRb != null) handRb.isKinematic = true;

        Vector3 HoverTarget() => new(player.position.x, player.position.y + slamHeightAbove, handObj.transform.position.z);

        // Tracking phase — small constant jitter while the hand chases the player
        while (Vector3.Distance(handObj.transform.position, HoverTarget()) > 0.15f)
        {
            handObj.transform.position = Vector3.MoveTowards(handObj.transform.position, HoverTarget(), slamTrackSpeed * Time.deltaTime);
            float jitter = slamJitterMax * 0.25f;
            handObj.transform.position += new Vector3(Random.Range(-jitter, jitter), 0f, 0f);
            yield return null;
        }

        // Windup phase — jitter ramps from 0 to max as the hand winds up to slam
        float windupTimer = slamWindupDelay;
        while (windupTimer > 0f)
        {
            Vector3 current = handObj.transform.position;
            handObj.transform.position = new(player.position.x, current.y, current.z);
            float t = 1f - windupTimer / slamWindupDelay;
            float jitter = slamJitterMax * t;
            handObj.transform.position += new Vector3(Random.Range(-jitter, jitter), 0f, 0f);
            windupTimer -= Time.deltaTime;
            yield return null;
        }

        float slamX = handObj.transform.position.x;
        float slamY = player.position.y;

        yield return waitPreDrop;

        if (handCol != null) handCol.enabled = true;

        // Drop phase — no jitter, clean straight fall
        Vector3 slamTarget = new(slamX, slamY, handObj.transform.position.z);
        while (Vector3.Distance(handObj.transform.position, slamTarget) > 0.05f)
        {
            handObj.transform.position = Vector3.MoveTowards(handObj.transform.position, slamTarget, slamDropSpeed * Time.deltaTime);
            yield return null;
        }

        handObj.transform.position = slamTarget;

        yield return waitImpact;

        if (handCol != null) handCol.enabled = false;
        if (handRb  != null) handRb.isKinematic = wasKinematic;
        if (handSr  != null) handSr.sprite = originalSprite;
        if (handAnim != null) handAnim.enabled = true;

        Vector3 restTarget = coalemosMovement.GetHandWorldRestPos(isLeft);
        while (Vector3.Distance(handObj.transform.position, restTarget) > 0.1f)
        {
            restTarget = coalemosMovement.GetHandWorldRestPos(isLeft);
            handObj.transform.position = Vector3.MoveTowards(handObj.transform.position, restTarget, slamReturnSpeed * Time.deltaTime);
            yield return null;
        }

        coalemosMovement.SetHandLocked(isLeft, false);

        if (isLeft) leftHandCoroutine  = null;
        else        rightHandCoroutine = null;
    }

    private IEnumerator HandSwipeRoutine(bool isLeft, float height)
    {
        GameObject     handObj  = isLeft ? coalemosMovement.LeftHand : coalemosMovement.RightHand;
        Collider2D     handCol  = handObj.GetComponentInChildren<Collider2D>();
        Rigidbody2D    handRb   = handObj.GetComponentInChildren<Rigidbody2D>();
        Animator       handAnim = handObj.GetComponentInChildren<Animator>();
        SpriteRenderer handSr   = handObj.GetComponentInChildren<SpriteRenderer>();

        coalemosMovement.SetHandLocked(isLeft, true);

        if (handAnim != null) handAnim.enabled = false;
        Sprite originalSprite = handSr != null ? handSr.sprite : null;
        Sprite swipeSprite    = isLeft ? swipeLeftSprite : swipeRightSprite;
        if (handSr != null && swipeSprite != null) handSr.sprite = swipeSprite;
        bool wasKinematic = handRb != null && handRb.isKinematic;
        if (handRb != null) handRb.isKinematic = true;

        Transform originalParent = handObj.transform.parent;
        float swipeZ = handObj.transform.position.z;
        handObj.transform.SetParent(null);

        float startX = isLeft ? swipeArenaMinX : swipeArenaMaxX;
        float endX   = isLeft ? swipeArenaMaxX : swipeArenaMinX;

        // Travel to swipe start position
        Vector3 swipeStart = new(startX, height, swipeZ);
        while (Vector3.Distance(handObj.transform.position, swipeStart) > 0.05f)
        {
            handObj.transform.position = Vector3.MoveTowards(handObj.transform.position, swipeStart, swipeApproachSpeed * Time.deltaTime);
            yield return null;
        }

        // Brief startup hold before the swipe executes
        yield return waitSwipeStartup;

        if (handCol != null) handCol.enabled = true;

        Vector3 endPos = new(endX, height, swipeZ);
        while (Vector3.Distance(handObj.transform.position, endPos) > 0.05f)
        {
            handObj.transform.position = Vector3.MoveTowards(handObj.transform.position, endPos, swipeSpeed * Time.deltaTime);
            yield return null;
        }

        if (handCol != null) handCol.enabled = false;

        handObj.transform.SetParent(originalParent);
        if (handRb  != null) handRb.isKinematic = wasKinematic;
        if (handSr  != null) handSr.sprite = originalSprite;
        if (handAnim != null) handAnim.enabled = true;

        Vector3 restTarget = coalemosMovement.GetHandWorldRestPos(isLeft);
        while (Vector3.Distance(handObj.transform.position, restTarget) > 0.1f)
        {
            restTarget = coalemosMovement.GetHandWorldRestPos(isLeft);
            handObj.transform.position = Vector3.MoveTowards(handObj.transform.position, restTarget, slamReturnSpeed * Time.deltaTime);
            yield return null;
        }

        coalemosMovement.SetHandLocked(isLeft, false);

        if (isLeft) leftHandCoroutine  = null;
        else        rightHandCoroutine = null;
    }
}
