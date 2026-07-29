using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuPrincipal : MonoBehaviour
{
    [Header("Paineis")]
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelPrincipal;

    [Header("Cutscene")]
    [SerializeField] private CutsceneController cutsceneController;

    private void Start()
    {
        painelOpcoes.SetActive(false);
        MusicManager.PlayMusic("menu");
    }
    public void AoBotaoIniciar()
    {
        if (cutsceneController != null)
            cutsceneController.IniciarCutscene();
        else
            SceneManager.LoadScene("AreaInicial");
    }
    public void AoBotaoOpcoes()
    {
        painelOpcoes.SetActive(true);
        painelPrincipal.SetActive(false);
    }
    public void AoFecharOpcoes()
    {
        painelOpcoes.SetActive(false);
        painelPrincipal.SetActive(true);
    }
    public void AoBotaoSair()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}