using UnityEngine;

public class DummyEnemy : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float fireRate = 3f;

    private float fireTimer;

    void Start()
    {
        fireTimer = fireRate;
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            fireTimer = fireRate;
        }
    }
}
