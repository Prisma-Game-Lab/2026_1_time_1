using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private RectTransform cursor;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform spear;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteRenderer playerSprite;

    private Camera cam;
    private Camera canvasCamera;

    void Start()
    {
        cam = Camera.main;

        canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;
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
        cursor.localPosition = localPoint;

        if (cam != null)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z))
            );

            // Rotate the spear toward the cursor only while it is held by the player
            if (spear != null && spear.parent == playerTransform)
            {
                Vector2 direction = (Vector2)mouseWorld - (Vector2)spear.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                spear.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Flip the player sprite to face the cursor
            if (playerTransform != null && playerSprite != null)
            {
                playerSprite.flipX = mouseWorld.x > playerTransform.position.x;
            }
        }
    }
}
