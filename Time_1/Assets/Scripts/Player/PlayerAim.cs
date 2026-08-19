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

    
    private static int cursorForceCount = 0;
    public static void ForceShowCursor(bool show)
    {
        cursorForceCount += show ? 1 : -1;
        if (cursorForceCount < 0) cursorForceCount = 0;
    }

    public void SetAimReversed(bool reversed) => aimReversed = reversed;

    void Awake()
    {
        cursorForceCount = 0;
    }

    void Start()
    {
        cam = Camera.main;

        canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (playerTransform != null)
            originalScaleX = playerTransform.localScale.x;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        if (cursor == null || canvas == null) return;

        if (cursorForceCount > 0)
        {
            Cursor.visible = true;
            return;
        }

        bool inGameCursorVisible = cursor.gameObject.activeInHierarchy;
        Cursor.visible = !inGameCursorVisible;
        if (!inGameCursorVisible) return;

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
