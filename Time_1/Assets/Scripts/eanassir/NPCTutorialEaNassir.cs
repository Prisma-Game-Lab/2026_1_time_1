using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 

public class NPCTutorialEaNassir : Interactable 
{
    [Header("Conexões da Interface (UI)")]
    public GameObject canvasDialogo;
    public TMP_Text textoDialogo;
    
    [Header("Botões")]
    public GameObject btnSair;
    public GameObject btnContinuar;
    public GameObject btnIrParaFase;

    [Header("Textos do Diálogo")]
    [TextArea] public string primeiraFala = "Olá! Vejo que você é novo por aqui. Quer saber como as coisas funcionam?";
    [TextArea] public string segundaFala = "Excelente. Passe por aquele portal e eu te mostrarei o básico.";
    
    [Header("Configuração de Transição")]
    public string TUTORIAL = "TUTORIAL";

    public override bool PodeInteragir() => true;

    public override void Interagir()
    {
        canvasDialogo.SetActive(true);
        textoDialogo.text = primeiraFala;
        
        btnSair.SetActive(true);
        btnContinuar.SetActive(true);
        btnIrParaFase.SetActive(false);

        Time.timeScale = 0f; 
    }

    public void BotaoSair()
    {
        canvasDialogo.SetActive(false);
        
        Time.timeScale = 1f; 
    }

    public void BotaoContinuar()
    {
        textoDialogo.text = segundaFala;
        btnContinuar.SetActive(false);
        btnIrParaFase.SetActive(true);
    }

    public void BotaoIrParaFase()
    {
        Time.timeScale = 1f; 
        
        SceneManager.LoadScene(TUTORIAL);
    }
}