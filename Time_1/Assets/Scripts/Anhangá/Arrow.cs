using System.Collections;
using UnityEngine;

public class Arrow : BasicProjectile
{
    [SerializeField] private float launchDelay = 0.8f;
    [SerializeField] private float lifetime    = 6f;

    private bool launched = false;


    public void Init(float launchDelay, float lifetime, float speed)
    {
        this.launchDelay = launchDelay;
        this.lifetime    = lifetime;
        this.speed       = speed;
    }

    void Start()
    {
        StartCoroutine(LaunchRoutine());
    }

    private IEnumerator LaunchRoutine()
    {
        yield return new WaitForSeconds(launchDelay);
        launched = true;
        Destroy(gameObject, lifetime);
    }

    protected override void Update()
    {
        if (!launched) return;
        transform.Translate(Vector2.up * (speed * Time.deltaTime));
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!launched) return;

        if (!other.TryGetComponent(out PlayerHealthController playerHealth))
            playerHealth = other.GetComponentInParent<PlayerHealthController>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
