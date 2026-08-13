using System.Collections;
using UnityEngine;
public class WinScreenManager : MonoBehaviour
{
    public static WinScreenManager Instance { get; private set; }

    [Header("Configuração")]
    [SerializeField] private float delay = 1.5f;
    [SerializeField] private bool pauseGame = true;
    [SerializeField] private Transform parentCanvas;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private bool alreadyShown;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    public void ShowWinScreen(GameObject winScreenPrefab)
    {
        if (alreadyShown) return;
        if (winScreenPrefab == null)
        {
            Debug.LogWarning("[WinScreenManager] winScreenPrefab nulo — nada a mostrar.");
            return;
        }
        alreadyShown = true;
        StartCoroutine(ShowRoutine(winScreenPrefab));
    }

    private IEnumerator ShowRoutine(GameObject prefab)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        GameObject tela = parentCanvas != null
            ? Instantiate(prefab, parentCanvas)
            : Instantiate(prefab);
        tela.SetActive(true);

        if (pauseGame)
            Time.timeScale = 0f;

        if (displayDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(displayDuration);

            yield return FadeOut(tela);

            if (pauseGame)
                Time.timeScale = 1f;

            if (tela != null)
                Destroy(tela);

            alreadyShown = false;
        }
    }

    private IEnumerator FadeOut(GameObject tela)
    {
        if (tela == null || fadeOutDuration <= 0f)
            yield break;
        CanvasGroup cg = tela.GetComponent<CanvasGroup>();
        if (cg == null) cg = tela.AddComponent<CanvasGroup>();

        Animator anim = tela.GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        float start = cg.alpha;
        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, 0f, t / fadeOutDuration);
            yield return null;
        }
        cg.alpha = 0f;
    }
}