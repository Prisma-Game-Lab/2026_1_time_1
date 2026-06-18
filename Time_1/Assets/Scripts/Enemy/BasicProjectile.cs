using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask environmentLayers;

    public float Speed => speed;

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerHealthController playerHealth))
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & environmentLayers) != 0)
            Destroy(gameObject);
    }
}
