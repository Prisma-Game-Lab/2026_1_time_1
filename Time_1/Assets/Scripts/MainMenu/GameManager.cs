using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Configuracoes do Jogo")]
    public int totalBosses = 3;
    public int CurrentBossIndex { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;

    [Header("Vidas")]
    [SerializeField] private int startingLives = 5;
    public int CurrentLives { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentLives = startingLives;
    }
    public void IniciarJogo()
    {
        CurrentBossIndex = 0;
        IsGameOver = false;
        CurrentLives = startingLives;
        CarregarCenaBoss(CurrentBossIndex);
    }
    public void UseLife()
    {
        CurrentLives = Mathf.Max(0, CurrentLives - 1);
    }
    public void BossDerrotado()
    {
        CurrentBossIndex++;

        if (CurrentBossIndex >= totalBosses)
            VenceuJogo();
        else
        {
            MusicManager.PlayMusic("boss");
            CarregarCenaBoss(CurrentBossIndex);
        }
    }
    public void GameOver()
    {
        IsGameOver = true;
        MusicManager.StartFadeOut();
        Debug.Log("Noob down!");
        SceneManager.LoadScene("GameOver");
    }
    private void VenceuJogo()
    {
        MusicManager.StartFadeOut();
        Debug.Log("Voce venceu todos os bosses!");
        SceneManager.LoadScene("WinScreen");
    }
    private void CarregarCenaBoss(int bossIndex)
    {
        string sceneName = "Boss" + bossIndex;
        SceneManager.LoadScene(sceneName);
    }
    public void IrParaMenu()
    {
        MusicManager.PlayMusic("menu");
        SceneManager.LoadScene("MainMenu");
    }
}