using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class NPCTutorialEaNassir : Interactable
{
    [Header("Conexões da Interface")]
    public GameObject canvasDialogo;
    public TMP_Text textoDialogo;

    [Header("Botões")]
    public GameObject btnSair;
    public GameObject btnContinuar;
    public GameObject btnIrParaFase;

    [Header("Textos do Diálogo")]
    [TextArea]
    public string[] falas =
    {
        "Olá! Vejo que você é novo por aqui. Quer saber como as coisas funcionam?",
        "Excelente. Passe por aquele portal e eu te mostrarei o básico."
    };

    [Header("Configuração de Transição")]
    public string TUTORIAL = "TUTORIAL";

    private int indiceFala = 0;

    public override bool PodeInteragir() => true;

    public override void Interagir()
    {
        indiceFala = 0;

        canvasDialogo.SetActive(true);
        btnSair.SetActive(true);

        MostrarFalaAtual();

        Time.timeScale = 0f;
        PlayerAim.ForceShowCursor(true);
    }
    public void BotaoContinuar()
    {
        indiceFala++;

        if (indiceFala >= falas.Length)
            indiceFala = falas.Length - 1;

        MostrarFalaAtual();
    }

    public void BotaoSair()
    {
        canvasDialogo.SetActive(false);

        Time.timeScale = 1f;
        PlayerAim.ForceShowCursor(false);
    }

    public void BotaoIrParaFase()
    {
        Time.timeScale = 1f;
        PlayerAim.ForceShowCursor(false);

        SceneManager.LoadScene(TUTORIAL);
    }
    private void MostrarFalaAtual()
    {
        if (falas == null || falas.Length == 0)
            return;

        textoDialogo.text = falas[indiceFala];
        bool ultimaFala = indiceFala >= falas.Length - 1;

        btnContinuar.SetActive(!ultimaFala);
        btnIrParaFase.SetActive(ultimaFala);
    }
}