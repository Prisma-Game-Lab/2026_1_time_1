using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private RectTransform cursor;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform spear;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private PlayerMovement playerMovement;

    private Camera cam;
    private Camera canvasCamera;
    private float originalScaleX;
    private bool aimReversed;

    public void SetAimReversed(bool reversed) => aimReversed = reversed;

    void Start()
    {
        cam = Camera.main;
        Cursor.visible = false;

        canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (playerTransform != null)
            originalScaleX = playerTransform.localScale.x;
    }

    private void Update()
    {
        if (cursor == null || canvas == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();

        // Move the cursor UI element
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            screenPos,
            canvasCamera,
            out Vector2 localPoint
        );
        cursor.localPosition = aimReversed ? -localPoint : localPoint;

        if (cam != null)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z))
            );

            bool isWindingUp = playerShooting != null && playerShooting.IsWindingUp;

            // Rotate the spear toward the cursor only during wind-up
            if (spear != null && spear.parent == playerTransform && isWindingUp)
            {
                Vector2 direction = (Vector2)mouseWorld - (Vector2)spear.position;
                if (aimReversed) direction = -direction;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                spear.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Flip the entire player GameObject so the spear flips with it.
            // Face cursor during wind-up, else face movement direction.
            if (playerTransform != null)
            {
                bool faceRight;
                bool shouldFlip;

                if (isWindingUp)
                {
                    faceRight = aimReversed
                        ? mouseWorld.x < playerTransform.position.x
                        : mouseWorld.x > playerTransform.position.x;
                    shouldFlip = true;
                }
                else if (playerMovement != null && playerMovement.HorizontalInput != 0f)
                {
                    faceRight = playerMovement.HorizontalInput > 0f;
                    shouldFlip = true;
                }
                else
                {
                    shouldFlip = false;
                    faceRight = false; 
                }

                if (shouldFlip)
                {
                    Vector3 s = playerTransform.localScale;
                    s.x = faceRight ? -originalScaleX : originalScaleX;
                    playerTransform.localScale = s;
                }
            }
        }
    }
}
