using UnityEngine;
using TMPro;

public class GerenciadorTutorial : MonoBehaviour
{
    [Header("Interface do Tutorial")]
    public GameObject canvasDialogo;
    public TMP_Text textoDialogo;
    public GameObject btnContinuar;

    [Header("Controle do Player (Pausa Real)")]
    [Tooltip("Arraste o objeto do Player aqui")]
    public GameObject jogadorGeral;
    [Tooltip("Arraste aqui os scripts do player que você quer desligar (ex: script de andar, script de atirar)")]
    public MonoBehaviour[] scriptsDoPlayerParaDesligar;

    [Header("Textos da Primeira Parte")]
    [TextArea] public string fala1 = "Bem-vindo ao treinamento. Como pagamento, já debitei uma vida sua.";
    [TextArea] public string fala2 = "Você já sabe andar, pular e atacar. Agora precisa dominar as mecânicas.";
    [TextArea] public string fala3 = "Use o 'pogo' para pegar aquela moeda: atire a lança no chão, pule e chame a lança de volta.";

    [Header("Textos da Segunda Parte (Pós-Moeda)")]
    [TextArea] public string fala4 = "Muito bem. Agora vamos para a próxima lição...";

    private int indiceFaseAtual = 0;

    void Start()
    {
        IniciarDialogo();
    }

    public void IniciarDialogo()
    {
        canvasDialogo.SetActive(true);
        
        // CONGELA O JOGADOR (Em vez de congelar o tempo)
        TravarJogador(true);

        indiceFaseAtual = 1;
        textoDialogo.text = fala1;
        
        TirarVidaDoPlayer();
    }

    public void AvancarDialogo()
    {
        indiceFaseAtual++;

        if (indiceFaseAtual == 2)
        {
            textoDialogo.text = fala2;
        }
        else if (indiceFaseAtual == 3)
        {
            textoDialogo.text = fala3;
        }
        else if (indiceFaseAtual == 4)
        {
            EncerrarDialogo();
        }
        else if (indiceFaseAtual == 5)
        {
            EncerrarDialogo();
        }
    }

    public void EncerrarDialogo()
    {
        canvasDialogo.SetActive(false);
        
        // LIBERA O JOGADOR PARA SE MOVER
        TravarJogador(false);
    }

    public void EventoPegouMoeda()
    {
        canvasDialogo.SetActive(true);
        TravarJogador(true); 
        
        indiceFaseAtual = 4;
        textoDialogo.text = fala4;
    }

    private void TirarVidaDoPlayer()
    {
        Debug.Log("O Player perdeu uma vida como pagamento!");
        // Aqui entrará o código futuro de tirar vida
    }

    // --- NOVA FUNÇÃO PARA TRAVAR/DESTRAVAR O PLAYER ---
    private void TravarJogador(bool travar)
    {
        // Se 'travar' for true, ele desativa os scripts. Se for false, ele ativa.
        foreach (MonoBehaviour script in scriptsDoPlayerParaDesligar)
        {
            if (script != null)
            {
                script.enabled = !travar; 
            }
        }
        
        // Opcional: Você pode parar a velocidade física do player aqui se ele estiver escorregando
        if (travar && jogadorGeral != null)
        {
            Rigidbody2D rb = jogadorGeral.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }
    }
}