using UnityEngine;

public class Fireball : BasicProjectile
{
    [SerializeField] private float gravityScale = 2f;
    [SerializeField] private float lifetime = 4f;

    private Vector2 velocity;
    private bool launched;
    private float lifeTimer;

    public void Launch(Vector2 direction)
    {
        velocity = direction.normalized * speed;
        launched = true;
    }

    public void LaunchWithVelocity(Vector2 vel)
    {
        velocity = vel;
        launched = true;
    }

    protected override void Update()
    {
        if (!launched) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime) { Destroy(gameObject); return; }

        velocity.y += Physics2D.gravity.y * gravityScale * Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    public override Vector2 GetMovementDirection() => velocity.normalized;

    public float GravityScale => gravityScale;

    // Returns the launch direction (unit vector) needed to hit 'target' from 'origin'
    // using this fireball's speed and gravityScale, accounting for the parabolic arc.
    // Falls back to a straight aim if no ballistic solution exists.
    public static Vector2 ComputeBallisticDirection(Vector2 origin, Vector2 target, float speed, float gravityScale)
    {
        Vector2 disp = target - origin;
        float g = Physics2D.gravity.y * gravityScale; // negative

        // Solve: 0.25*g²*u² - (v²+dy*g)*u + |disp|² = 0   (u = t²)
        float a    = 0.25f * g * g;
        float b    = -(speed * speed + disp.y * g);
        float c    = disp.sqrMagnitude;
        float disc = b * b - 4f * a * c;

        if (disc >= 0f && a > float.Epsilon)
        {
            float sqrtDisc = Mathf.Sqrt(disc);
            float u1 = (-b - sqrtDisc) / (2f * a);
            float u2 = (-b + sqrtDisc) / (2f * a);
            float u  = u1 > 0f ? u1 : u2;
            if (u > 0f)
            {
                float t = Mathf.Sqrt(u);
                return new Vector2(disp.x / t, (disp.y - 0.5f * g * u) / t).normalized;
            }
        }

        return disp.normalized; // fallback: direct aim
    }
}
