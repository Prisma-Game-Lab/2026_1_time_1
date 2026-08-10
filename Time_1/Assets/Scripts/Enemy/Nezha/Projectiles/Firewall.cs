using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Firewall : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 4f;

    private void Awake()
    {
        // Ensure the collider is always a trigger regardless of prefab settings.
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        var hp = other.GetComponent<PlayerHealthController>()
              ?? other.GetComponentInParent<PlayerHealthController>();
        if (hp == null || hp.IsInvincible) return;
        hp.TakeDamage(damage);
    }
}
