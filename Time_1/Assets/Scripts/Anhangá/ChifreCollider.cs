using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChifreCollider : MonoBehaviour
{
    [Tooltip("Dano causado ao player a cada chifrada")]
    [SerializeField] private int dano = 1;

    [Tooltip("Força do empurrão aplicado ao player no contato")]
    [SerializeField] private float knockbackForce = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerHealthController pHC)) return;

        pHC.TakeDamage(dano, "Melee");

        if (!other.TryGetComponent(out PlayerMovement pMovement)) return;
        Vector2 knockDir = (Vector2)(other.transform.position - transform.position);
        if (knockDir.sqrMagnitude < 0.01f) knockDir = Vector2.up;
        pMovement.Knockback(knockDir.normalized * knockbackForce);
    }
}