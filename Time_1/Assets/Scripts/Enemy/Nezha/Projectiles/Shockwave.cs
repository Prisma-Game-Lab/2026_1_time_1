using UnityEngine;

public class Shockwave : BasicProjectile
{
    [SerializeField] private float lifetime = 3f;

    private Vector2 direction;
    private bool launched;
    private float lifeTimer;

    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;
        launched  = true;
        if (direction.x < 0f)
        {
            Vector3 s = transform.localScale;
            s.x = -Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    protected override void Update()
    {
        if (!launched) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime) { Destroy(gameObject); return; }

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    public override Vector2 GetMovementDirection() => direction;
}
