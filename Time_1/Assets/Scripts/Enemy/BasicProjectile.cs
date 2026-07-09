using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected int damage = 1;
    [SerializeField] private LayerMask environmentLayers;

    public float Speed => speed;

    protected virtual void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealthController playerHealth = other.GetComponent<PlayerHealthController>();
        if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerHealthController>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if ((environmentLayers & (1 << other.gameObject.layer)) != 0)
            Destroy(gameObject);
    }
}
