using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private float flashDuration     = 0.1f;
    [SerializeField] private Color flashColor        = Color.red;
    [SerializeField] private float iframeBlinkInterval = 0.08f;
    [SerializeField] private SpriteRenderer[] renderers;

    private Coroutine activeFlash;
    private Coroutine iframeFlash;

    void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void Flash()
    {
        if (!gameObject.activeInHierarchy) return;
        if (activeFlash != null) StopCoroutine(activeFlash);
        activeFlash = StartCoroutine(FlashRoutine());
    }

    public void StartIFrameFlash()
    {
        if (!gameObject.activeInHierarchy) return;
        if (iframeFlash != null) StopCoroutine(iframeFlash);
        iframeFlash = StartCoroutine(IFrameBlinkRoutine());
    }

    public void StopIFrameFlash()
    {
        if (iframeFlash != null) { StopCoroutine(iframeFlash); iframeFlash = null; }
        foreach (var sr in renderers)
            if (sr != null) sr.color = Color.white;
    }

    private IEnumerator FlashRoutine()
    {
        foreach (var sr in renderers)
            if (sr != null) sr.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        foreach (var sr in renderers)
            if (sr != null) sr.color = Color.white;

        activeFlash = null;
    }

    private IEnumerator IFrameBlinkRoutine()
    {
        var wait = new WaitForSeconds(iframeBlinkInterval);
        while (true)
        {
            foreach (var sr in renderers)
                if (sr != null) sr.color = Color.clear;
            yield return wait;
            foreach (var sr in renderers)
                if (sr != null) sr.color = Color.white;
            yield return wait;
        }
    }
}
