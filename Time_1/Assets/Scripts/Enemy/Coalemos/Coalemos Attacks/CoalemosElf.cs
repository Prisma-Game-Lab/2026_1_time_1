using System.Collections;
using UnityEngine;

public class CoalemosElf : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject elfPrefab;
    [SerializeField] private CoalemosMovement movement;

    [Header("Arena Bounds")]
    [SerializeField] private float arenaLeft = -10f;
    [SerializeField] private float arenaRight = 10f;
    [SerializeField] private float spawnHeightMin = -2f;
    [SerializeField] private float spawnHeightMax = 3f;

    [Header("Wave Settings")]
    [SerializeField] private int elfCount = 5;
    [SerializeField] private float spawnInterval = 0.35f;
    [SerializeField] private float elfSpeed = 6f;
    [Tooltip("Tempo máximo de vida. O duende some antes se já tiver cruzado a arena.")]
    [SerializeField] private float elfLifetime = 6f;
    [Tooltip("Tempo extra depois de cruzar a arena antes de destruir.")]
    [SerializeField] private float exitMargin = 0.5f;

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
        float dirX = fromLeft ? 1f : -1f;
        WaitForSeconds wait = new(spawnInterval);

        // Tempo para cruzar a arena inteira + margem
        float tempoTravessia = elfSpeed > 0f
            ? Mathf.Abs(arenaRight - arenaLeft) / elfSpeed + exitMargin
            : elfLifetime;
        float lifetime = Mathf.Min(elfLifetime, tempoTravessia);

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

            if (!elf.TryGetComponent(out Rigidbody2D rb))
                rb = elf.GetComponentInChildren<Rigidbody2D>();
            if (rb != null) rb.simulated = false;

            Duende duende = elf.GetComponentInChildren<Duende>();
            if (duende != null) duende.DisableContactDamage();

            foreach (Collider2D col in elf.GetComponentsInChildren<Collider2D>())
                col.isTrigger = true;

            ElfMover mover = elf.AddComponent<ElfMover>();
            mover.Init(dirX, elfSpeed);

            Destroy(elf, lifetime);

            yield return wait;
        }

        if (movement != null) { movement.Unfreeze(); movement.SetHandsRaised(false); }
        waveCoroutine = null;
    }
}