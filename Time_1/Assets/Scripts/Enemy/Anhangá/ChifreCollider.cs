using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChifreCollider : MonoBehaviour
{
    [Tooltip("Dano causado ao player")]
    [SerializeField] private int dano = 1;

    [Tooltip("Força do empurrão (modo normal)")]
    [SerializeField] private float knockbackForce = 10f;

    [Header("Knockback pra Cima (investida)")]
    [Tooltip("Se marcado, joga o player pra cima em vez de empurrar na direção do impacto")]
    [SerializeField] private bool forcarParaCima;
    [Tooltip("Força vertical (pra cima)")]
    [SerializeField] private float forcaVertical = 12f;
    [Tooltip("Força horizontal lateral (direção depende de qual lado o player é atingido)")]
    [SerializeField] private float forcaLateral = 3f;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerHealthController pHC)) return;
        pHC.TakeDamage(dano, "Melee");
        if (!other.TryGetComponent(out PlayerMovement pMovement)) return;
        if (forcarParaCima)
        {
            float dirX = other.transform.position.x >= transform.position.x ? 1f : -1f;
            pMovement.Knockback(new Vector2(dirX * forcaLateral, forcaVertical));
        }
        else
        {
            Vector2 knockDir = (Vector2)(other.transform.position - transform.position);
            if (knockDir.sqrMagnitude < 0.01f) knockDir = Vector2.up;
            pMovement.Knockback(knockDir.normalized * knockbackForce);
        }
    }
}