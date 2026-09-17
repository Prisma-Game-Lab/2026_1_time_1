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

    [Header("Porta secreta")]
    [Tooltip("Liga a porta secreta. Começa DESLIGADA: sem botão e sem porta.")]
    [SerializeField] private bool habilitarPortaSecreta = false;
    [Tooltip("Botão extra que aparece na última fala e cria a porta. Pode ficar vazio.")]
    [SerializeField] private GameObject btnPortaSecreta;
    [Tooltip("Prefab com PortaCena + Collider2D (Is Trigger). Pode ficar vazio se não usar a porta.")]
    [SerializeField] private GameObject portaCenaPrefab;
    [Tooltip("Onde a porta aparece. Vazio = posição deste NPC.")]
    [SerializeField] private Transform pontoDaPorta;

    private int indiceFala = 0;
    private GameObject portaInstanciada;

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
    public void BotaoPortaSecreta()
    {
        SpawnarPorta();
        if (btnPortaSecreta != null) btnPortaSecreta.SetActive(false);
    }
    private void SpawnarPorta()
    {
        if (!habilitarPortaSecreta) return;
        if (portaCenaPrefab == null || portaInstanciada != null) return;

        Vector3 pos = pontoDaPorta != null ? pontoDaPorta.position : transform.position;
        portaInstanciada = Instantiate(portaCenaPrefab, pos, Quaternion.identity);
    }

    private void MostrarFalaAtual()
    {
        if (falas == null || falas.Length == 0)
            return;

        textoDialogo.text = falas[indiceFala];
        bool ultimaFala = indiceFala >= falas.Length - 1;

        btnContinuar.SetActive(!ultimaFala);
        btnIrParaFase.SetActive(ultimaFala);
        if (btnPortaSecreta != null)
            btnPortaSecreta.SetActive(ultimaFala && habilitarPortaSecreta && portaInstanciada == null);
    }
}