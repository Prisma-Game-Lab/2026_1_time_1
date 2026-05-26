using UnityEngine;

// Sits on the Spear GameObject and forwards trigger hits to PlayerShooting.
// Required because OnTriggerEnter2D must live on the same object as the collider.
public class SpearCollisionRelay : MonoBehaviour
{
    private PlayerShooting playerShooting;

    public void Init(PlayerShooting shooter)
    {
        playerShooting = shooter;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        playerShooting?.OnSpearHit(other);
    }
}
