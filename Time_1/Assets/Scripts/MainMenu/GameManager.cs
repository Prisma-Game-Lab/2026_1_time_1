using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Configurações do Jogo")]
    public int totalBosses = 3;
    public int CurrentBossIndex { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void IniciarJogo()
    {
        CurrentBossIndex = 0;
        IsGameOver = false;
        CarregarCenaBoss(CurrentBossIndex);
    }
    public void BossDerrotado()
    {
        CurrentBossIndex++;

        if (CurrentBossIndex >= totalBosses)
            VenceuJogo();
        else
        {
            AudioManager.Instance?.TocaMusica(AudioManager.Instance.MusicaDoBoss);
            CarregarCenaBoss(CurrentBossIndex);
        }
    }
    public void GameOver()
    {
        IsGameOver = true;
        AudioManager.Instance?.ParaMusica();
        Debug.Log("Noob down!");
        SceneManager.LoadScene("GameOver");
    }
    private void VenceuJogo()
    {
        AudioManager.Instance?.ParaMusica();
        Debug.Log("Você venceu todos os bosses!");
        SceneManager.LoadScene("WinScreen");
    }
    private void CarregarCenaBoss(int bossIndex)
    {
        string sceneName = "Boss" + bossIndex;
        SceneManager.LoadScene(sceneName);
    }
    public void IrParaMenu()
    {
        AudioManager.Instance?.TocaMusica(AudioManager.Instance.MusicaDoMenu);
        SceneManager.LoadScene("MainMenu");
    }
}