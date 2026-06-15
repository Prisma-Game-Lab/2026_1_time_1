using System.Collections;
using UnityEngine;

public class CoalemosElf : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject elfPrefab;
    [SerializeField] private CoalemosMovement movement;

    [Header("Arena Bounds")]
    [SerializeField] private float arenaLeft      = -10f;
    [SerializeField] private float arenaRight     =  10f;
    [SerializeField] private float spawnHeightMin = -2f;
    [SerializeField] private float spawnHeightMax =  3f;

    [Header("Wave Settings")]
    [SerializeField] private int   elfCount      = 5;
    [SerializeField] private float spawnInterval = 0.35f;
    [SerializeField] private float elfSpeed      = 6f;
    [SerializeField] private float elfLifetime   = 6f;

    private Coroutine waveCoroutine;
    public bool IsAttacking => waveCoroutine != null;

    // fromLeft = true  -> elves spawn on the left wall and travel right
    // fromLeft = false -> elves spawn on the right wall and travel left
    public void ElfWave(bool fromLeft)
    {
        if (waveCoroutine != null) StopCoroutine(waveCoroutine);
        waveCoroutine = StartCoroutine(ElfWaveRoutine(fromLeft));
    }

    private IEnumerator ElfWaveRoutine(bool fromLeft)
    {
        if (movement != null) { movement.Freeze(); movement.SetHandsRaised(true); }

        float spawnX = fromLeft ? arenaLeft : arenaRight;
        float dirX   = fromLeft ? 1f : -1f;
        WaitForSeconds wait = new WaitForSeconds(spawnInterval);

        for (int i = 0; i < elfCount; i++)
        {
            float spawnY = Random.Range(spawnHeightMin, spawnHeightMax);
            GameObject elf = Instantiate(elfPrefab, new Vector3(spawnX, spawnY, 0f), Quaternion.identity);

            // Flip sprite so the elf faces its travel direction
            if (!fromLeft)
            {
                Vector3 s = elf.transform.localScale;
                s.x *= -1f;
                elf.transform.localScale = s;
            }

            // Disable the rigidbody so the elf ignores all physics walls
            if (!elf.TryGetComponent(out Rigidbody2D rb))
                rb = elf.GetComponentInChildren<Rigidbody2D>();
            if (rb != null) rb.simulated = false;

            foreach (Collider2D col in elf.GetComponentsInChildren<Collider2D>())
                col.isTrigger = true;

            ElfMover mover = elf.AddComponent<ElfMover>();
            mover.Init(dirX, elfSpeed);

            Destroy(elf, elfLifetime);

            yield return wait;
        }

        if (movement != null) { movement.Unfreeze(); movement.SetHandsRaised(false); }
        waveCoroutine = null;
    }
}
