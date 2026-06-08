using System.Collections;
using UnityEngine;

public class CoalemosChicken : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject chickenPrefab;
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CoalemosMovement movement;

    private Coroutine chickenRainCoroutine;

    public bool IsAttacking => chickenRainCoroutine != null;

    public void ChickenRain(int numberOfChickens, float spawnInterval)
    {
        if (chickenRainCoroutine != null)
            StopCoroutine(chickenRainCoroutine);

        chickenRainCoroutine = StartCoroutine(ChickenRainRoutine(numberOfChickens, spawnInterval));
    }

    private IEnumerator ChickenRainRoutine(int numberOfChickens, float spawnInterval)
    {
        if (movement != null) movement.StartPacing();

        for (int i = 0; i < numberOfChickens; i++)
        {
            GameObject chicken = Instantiate(chickenPrefab, spawnPoint.position, Quaternion.identity);
            if (i % 2 != 0)
            {
                Vector3 scale = chicken.transform.localScale;
                scale.x *= -1f;
                chicken.transform.localScale = scale;
            }
            yield return new WaitForSeconds(spawnInterval);
        }

        if (movement != null) movement.StopPacing();
        chickenRainCoroutine = null;
    }
}
