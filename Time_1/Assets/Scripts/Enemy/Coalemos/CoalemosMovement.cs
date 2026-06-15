using UnityEngine;

public class CoalemosMovement : MonoBehaviour
{
    [SerializeField] GameObject head;
    [SerializeField] GameObject leftHand;
    [SerializeField] GameObject rightHand;
    [SerializeField] Transform player;

    [Header("Wobble")]
    [SerializeField] float wobblePositionAmount = 0.1f;
    [SerializeField] float wobbleRotationAmount = 3f;
    [SerializeField] float wobbleSpeed = 1.5f;

    [Header("Head Tracking")]
    [SerializeField] float trackingMaxOffset = 0.8f;
    [SerializeField] float trackingSmoothTime = 0.4f;

    [Header("Root - Follow Player")]
    [SerializeField] float rootHeightOffset = 2f;
    [SerializeField] float rootTrackingMaxOffset = 3f;
    [SerializeField] float rootTrackingSmoothTime = 0.8f;

    [Header("Root - Pacing")]
    [SerializeField] float pacingSpeed = 3f;
    [SerializeField] float pacingHalfWidth = 4f;

    [Header("Hands Raised")]
    [SerializeField] private float handsRaisedYOffset = 2f;
    [SerializeField] private float handsRaiseSpeed    = 4f;

    Vector3 headBaseLocalPos;
    Vector3 leftHandOffsetFromHead;
    Vector3 rightHandOffsetFromHead;

    Vector3 headTrackingOffset;
    Vector3 headTrackingVelocity;

    Vector3 rootBaseWorldPos;
    Vector3 rootTrackingOffset;
    Vector3 rootTrackingVelocity;

    private enum RootMode { FollowPlayer, Pacing, Frozen }
    private RootMode rootMode = RootMode.FollowPlayer;

    private float pacingDirection = 1f;
    private float pacingCenterX;
    private float pacingY;

    private bool leftHandLocked;
    private bool rightHandLocked;
    private float currentHandsRaiseY;
    private float targetHandsRaiseY;

    public GameObject LeftHand  => leftHand;
    public GameObject RightHand => rightHand;
    public Transform  Player    => player;

    public void SetHandLocked(bool isLeft, bool locked)
    {
        if (isLeft) leftHandLocked  = locked;
        else        rightHandLocked = locked;
    }

    public void Freeze()
    {
        rootMode = RootMode.Frozen;
    }

    public void Unfreeze()
    {
        rootTrackingOffset   = transform.position - rootBaseWorldPos;
        rootTrackingVelocity = Vector3.zero;
        rootMode             = RootMode.FollowPlayer;
    }

    public void SetHandsRaised(bool raised)
    {
        targetHandsRaiseY = raised ? handsRaisedYOffset : 0f;
    }

    void Start()
    {
        headBaseLocalPos        = head.transform.localPosition;
        leftHandOffsetFromHead  = leftHand.transform.localPosition  - head.transform.localPosition;
        rightHandOffsetFromHead = rightHand.transform.localPosition - head.transform.localPosition;

        rootBaseWorldPos = transform.position + Vector3.up * rootHeightOffset;
    }

    void Update()
    {
        UpdateRootPosition();

        float t = Time.time * wobbleSpeed;

        UpdateHeadTracking();

        Vector3 headWobble = WobbleOffset(t, 0f);
        head.transform.SetLocalPositionAndRotation(
            headBaseLocalPos + headTrackingOffset + headWobble,
            Quaternion.Euler(0f, 0f, WobbleRotation(t, 0f)));

        currentHandsRaiseY = Mathf.MoveTowards(currentHandsRaiseY, targetHandsRaiseY, handsRaiseSpeed * Time.deltaTime);

        Vector3 headNow    = head.transform.localPosition;
        Vector3 raiseOffset = Vector3.up * currentHandsRaiseY;

        if (!leftHandLocked)
            leftHand.transform.SetLocalPositionAndRotation(
                headNow + leftHandOffsetFromHead + WobbleOffset(t, 1.1f) + raiseOffset,
                Quaternion.Euler(0f, 0f, WobbleRotation(t, 1.1f)));

        if (!rightHandLocked)
            rightHand.transform.SetLocalPositionAndRotation(
                headNow + rightHandOffsetFromHead + WobbleOffset(t, 2.3f) + raiseOffset,
                Quaternion.Euler(0f, 0f, WobbleRotation(t, 2.3f)));
    }

    public void StartPacing()
    {
        pacingCenterX   = rootBaseWorldPos.x;
        pacingY         = transform.position.y;
        pacingDirection = 1f;
        rootMode        = RootMode.Pacing;
    }

    public void StopPacing()
    {
        rootTrackingOffset   = transform.position - rootBaseWorldPos;
        rootTrackingVelocity = Vector3.zero;
        rootMode             = RootMode.FollowPlayer;
    }

    public Vector3 GetHandWorldRestPos(bool isLeft)
    {
        float t     = Time.time * wobbleSpeed;
        float phase = isLeft ? 1.1f : 2.3f;
        Vector3 headLocalPos = headBaseLocalPos + headTrackingOffset + WobbleOffset(t, 0f);
        Vector3 handOffset   = isLeft ? leftHandOffsetFromHead : rightHandOffsetFromHead;
        Vector3 handLocalPos = headLocalPos + handOffset + WobbleOffset(t, phase);
        return transform.TransformPoint(handLocalPos);
    }

    void UpdateRootPosition()
    {
        if (rootMode == RootMode.Frozen) return;

        if (rootMode == RootMode.FollowPlayer)
        {
            if (player == null) return;

            Vector3 toPlayer = player.position - rootBaseWorldPos;
            toPlayer.z = 0f;
            Vector3 target = Vector3.ClampMagnitude(toPlayer, rootTrackingMaxOffset);
            rootTrackingOffset = Vector3.SmoothDamp(rootTrackingOffset, target, ref rootTrackingVelocity, rootTrackingSmoothTime);
            transform.position = rootBaseWorldPos + rootTrackingOffset;
        }
        else
        {
            float newX = transform.position.x + pacingDirection * pacingSpeed * Time.deltaTime;
            float minX = pacingCenterX - pacingHalfWidth;
            float maxX = pacingCenterX + pacingHalfWidth;

            if (newX >= maxX) { newX = maxX; pacingDirection = -1f; }
            else if (newX <= minX) { newX = minX; pacingDirection =  1f; }

            transform.position = new(newX, pacingY, transform.position.z);
        }
    }

    void UpdateHeadTracking()
    {
        if (player == null) return;

        Vector3 playerLocal = transform.InverseTransformPoint(player.position);
        Vector3 toPlayer    = playerLocal - headBaseLocalPos;
        toPlayer.z = 0f;

        Vector3 target = Vector3.ClampMagnitude(toPlayer, trackingMaxOffset);
        headTrackingOffset = Vector3.SmoothDamp(headTrackingOffset, target, ref headTrackingVelocity, trackingSmoothTime);
    }

    Vector3 WobbleOffset(float t, float phase)
    {
        float px = Mathf.Sin(t * 0.9f + phase) * wobblePositionAmount;
        float py = Mathf.Sin(t * 1.1f + phase + 0.5f) * wobblePositionAmount;
        return new Vector3(px, py, 0f);
    }

    float WobbleRotation(float t, float phase)
    {
        return Mathf.Sin(t * 0.7f + phase + 1.0f) * wobbleRotationAmount;
    }
}
