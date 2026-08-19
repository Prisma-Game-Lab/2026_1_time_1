using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    public static int Lives = 5;

    [Header("Cena")]
    [SerializeField] private string sceneToLoad;

    [Header("UI")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button giveUpButton;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        PlayerHealthController.OnPlayerDied += Mostrar;
    }

    private void OnDisable()
    {
        PlayerHealthController.OnPlayerDied -= Mostrar;
    }

    private void Mostrar()
    {
        if (canvas != null) canvas.SetActive(true);
        Time.timeScale = 0f;
        PlayerAim.ForceShowCursor(true);
        RefreshUI();
        if (animator != null)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.SetTrigger("FadeIn");
        }
    }

    private void RefreshUI()
    {
        if (livesText != null) livesText.text = $"X {Lives}";
        if (restartButton != null) restartButton.interactable = Lives > 0;
    }

    public void Reiniciar()
    {
        if (Lives <= 0) return;
        Lives--;
        PlayerAim.ForceShowCursor(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }

    public void IrParaMenu()
    {
        Lives = 5;
        PlayerAim.ForceShowCursor(false);
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.IrParaMenu();
        else SceneManager.LoadScene("MainMenu");
    }
}
