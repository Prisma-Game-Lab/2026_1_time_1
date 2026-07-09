using System.Collections;
using UnityEngine;

public class AnhangaArrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform  player;
    [SerializeField] private Transform  mapCenter;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius       = 8f;
    [SerializeField] private float spawnHeightBuffer = 1f;   
    [SerializeField] private float arrowLaunchDelay  = 0.8f;
    [SerializeField] private float arrowLifetime     = 6f;
    [SerializeField] private float arrowSpeed        = 16f;

    private Coroutine attackCoroutine;
    public bool IsAttacking => attackCoroutine != null;

    public void HomingArrows(int nArrows, float arrowInterval)
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(HomingArrowsRoutine(nArrows, arrowInterval));
    }

    private IEnumerator HomingArrowsRoutine(int nArrows, float arrowInterval)
    {
        var wait = new WaitForSeconds(arrowInterval);

        for (int i = 0; i < nArrows; i++)
        {
            SpawnArrow();
            yield return wait;
        }

        attackCoroutine = null;
    }

    private void SpawnArrow()
    {
        if (player == null || arrowPrefab == null) return;

        Vector3 center = mapCenter != null ? mapCenter.position : Vector3.zero;
        center.z = 0f;

        // Constrain angle to the arc where spawn Y >= player.y + buffer.
        
        float minSin   = Mathf.Clamp((player.position.y + spawnHeightBuffer - center.y) / spawnRadius, -1f, 1f);
        float minAngle = Mathf.Asin(minSin);           
        float maxAngle = Mathf.PI - minAngle;           

        float   angle    = Random.Range(minAngle, maxAngle);
        Vector3 spawnPos = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnRadius;

       
        Vector2 toPlayer   = (Vector2)(player.position - spawnPos);
        float   arrowAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f;

        GameObject obj = Instantiate(arrowPrefab, spawnPos, Quaternion.Euler(0f, 0f, arrowAngle));

        if (obj.TryGetComponent(out Arrow arrow))
            arrow.Init(arrowLaunchDelay, arrowLifetime, arrowSpeed);
    }
}
