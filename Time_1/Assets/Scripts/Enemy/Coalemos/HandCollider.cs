using UnityEngine;

public class HandCollider : MonoBehaviour
{
    [SerializeField] private int handDamage;
    [SerializeField] private float knockbackForce = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerHealthController pHC)) return;
        if (pHC.IsInvincible) return;

        pHC.TakeDamage(handDamage, "Melee");

        if (!other.TryGetComponent(out PlayerMovement pMovement)) return;
        Vector2 knockDir = (Vector2)(other.transform.position - transform.position);
        if (knockDir.sqrMagnitude < 0.01f) knockDir = Vector2.up;
        pMovement.Knockback(knockDir.normalized * knockbackForce);
    }
}
