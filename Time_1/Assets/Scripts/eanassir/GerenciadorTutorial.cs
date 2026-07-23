using UnityEngine;
using TMPro;

public class GerenciadorTutorial : MonoBehaviour
{
    [Header("Interface do Tutorial")]
    public GameObject canvasDialogo;
    public TMP_Text textoDialogo;
    public GameObject btnContinuar;

    [Header("Controle do Player (Pausa Real)")]
    public GameObject jogadorGeral;
    [Tooltip("Scripts do player a desligar durante as caixas de texto (ex: PlayerMovement, PlayerShooting, HealController)")]
    public MonoBehaviour[] scriptsDoPlayerParaDesligar;

    [Header("Ganchos dos Sistemas Existentes")]
    [Tooltip("HealthController do Player (arraste o PlayerHealthController).")]
    public HealthController vidaPlayer;
    [Tooltip("PlayerShooting do Player (dispara OnParry).")]
    public PlayerShooting playerShooting;

    [Header("Ataques")]
    [Tooltip("Mover do 1º ataque que atravessa a tela.")]
    public TutorialHazardMover inimigoTravessia;
    [Tooltip("Mover do 2º ataque (reutiliza a travessia, com velocidade MENOR).")]
    public TutorialHazardMover inimigoTravessiaLento;
    [Tooltip("Spawner do projétil parável (fase pós-cura).")]
    public ProjetilTutorialSpawner projetilParavel;

    [Header("Chuva de nabos + Portal")]
    public ChuvaDeOrbs chuvaDeOrbs;
    [Tooltip("Portal (PortaCena) que aparece no fim. Deixe DESATIVADO na cena.")]
    public GameObject portal;

    [Header("Configuração da Coleta")]
    [Tooltip("Quantos nabos coletar (bater com o custoCura do OrbManager = 3).")]
    public int orbsParaAvancar = 3;

    [Header("Debug")]
    [Tooltip("Liga logs no Console para bugtest do passo de cura.")]
    public bool logsDiagnostico = true;

    [Header("Textos - Introdução + Pogo/Moeda")]
    [TextArea] public string fala1 = "Bem-vindo ao treinamento. Como pagamento, já debitei uma vida sua.";
    [TextArea] public string fala2 = "Você já sabe andar, pular e atacar. Agora precisa dominar as mecânicas.";
    [TextArea] public string fala3 = "Use o 'pogo' para pegar aquela moeda: atire a lança no chão, pule e chame a lança de volta.";
    [TextArea] public string fala4 = "Muito bem. Agora vamos para a próxima lição...";

    [Header("Textos - 1º Ataque (atravessa a tela)")]
    [TextArea] public string falaAntesDoAtaque = "Um ataque vai cruzar a arena! Desvie... ou tente dar um PARRY nele.";
    [TextArea] public string falaLevouDano = "Você levou dano! Para se recuperar, colete nabos espalhados pela arena.";
    [TextArea] public string falaDeuParry = "Parry perfeito! É assim que se devolve um golpe. Agora colete alguns nabos.";

    [Header("Textos - Coleta + Cura")]
    [TextArea] public string falaPosColeta = "Boa! Você juntou nabos suficientes. Agora use a CURA para restaurar sua vida.";
    [TextArea] public string falaFinal = "Curado! Agora vamos treinar o parry de verdade.";

    [Header("Textos - Fase do Projétil Parável")]
    [TextArea] public string falaIntroProjetil = "Vou atirar um projétil. Acerte-o com sua lança para transformá-lo em orb (PARRY).";
    [TextArea] public string falaProjetilOk = "Isso! Você parou o projétil e virou orb. Agora um último desafio...";
    [TextArea] public string falaProjetilFalhou = "Ainda não. Fique atento e tente o parry de novo!";

    [Header("Textos - 2º Ataque (travessia lenta) + Portal")]
    [TextArea] public string falaTravessiaLentaFinal = "Parry impecável! Você está pronto. Continue para abrir o portal.";
    [TextArea] public string falaTravessiaLentaFalhou = "Quase! Foque no tempo do parry e tente outra vez.";

    private enum Passo
    {
        Intro1, Intro2, Pogo, EsperandoMoeda, PosMoeda,
        AntesAtaque, EsperandoResultado, CaixaResultado,
        EsperandoOrbs, PosColeta, EsperandoCura, FimCura,
        CaixaIntroProjetil, EsperandoProjetil, CaixaProjetilOk, CaixaProjetilFalhou,
        EsperandoTravessiaLenta, CaixaTravessiaFinal, CaixaTravessiaFalhou,
        Fim
    }

    private Passo passo;
    private int ultimaVidaConhecida;
    private bool inscritoOrbs;

    // ---------------------------------------------------------------

    void Start()
    {
        if (vidaPlayer != null)
        {
            vidaPlayer.OnDamageTaken += AoTomarDano;
            vidaPlayer.OnHealthChanged += AoMudarVida;
            ultimaVidaConhecida = vidaPlayer.currentHealth;
        }
        else Debug.LogError("[Tutorial] 'vidaPlayer' não atribuído!", this);

        if (playerShooting != null) playerShooting.OnParry += AoDarParry;
        else Debug.LogWarning("[Tutorial] 'playerShooting' não atribuído — parry não dispara.", this);

        if (inimigoTravessiaLento != null) inimigoTravessiaLento.OnTravessiaCompleta += AoErrarTravessiaLenta;
        if (projetilParavel != null) projetilParavel.OnWhiff += AoErrarProjetil;

        if (portal != null) portal.SetActive(false);

        TentarInscreverOrbs();

        passo = Passo.Intro1;
        MostrarTexto(fala1);
        TravarJogador(true);
    }
    void OnDestroy()
    {
        if (vidaPlayer != null)
        {
            vidaPlayer.OnDamageTaken -= AoTomarDano;
            vidaPlayer.OnHealthChanged -= AoMudarVida;
        }
        if (playerShooting != null) playerShooting.OnParry -= AoDarParry;
        if (inimigoTravessiaLento != null) inimigoTravessiaLento.OnTravessiaCompleta -= AoErrarTravessiaLenta;
        if (projetilParavel != null) projetilParavel.OnWhiff -= AoErrarProjetil;
        if (inscritoOrbs && OrbManager.Instance != null)
            OrbManager.Instance.OnOrbsChanged -= AoMudarOrbs;
    }
    private void TentarInscreverOrbs()
    {
        if (inscritoOrbs || OrbManager.Instance == null) return;
        OrbManager.Instance.OnOrbsChanged += AoMudarOrbs;
        inscritoOrbs = true;
    }
    // BOTÃO "CONTINUAR"
    public void AvancarDialogo()
    {
        switch (passo)
        {
            case Passo.Intro1: passo = Passo.Intro2; MostrarTexto(fala2); break;
            case Passo.Intro2: passo = Passo.Pogo; MostrarTexto(fala3); break;

            case Passo.Pogo:
                passo = Passo.EsperandoMoeda;
                EsconderCaixa(); TravarJogador(false);
                break;

            case Passo.PosMoeda:
                passo = Passo.AntesAtaque; MostrarTexto(falaAntesDoAtaque);
                break;

            case Passo.AntesAtaque:
                // 1º ataque atravessa a tela
                passo = Passo.EsperandoResultado;
                EsconderCaixa(); TravarJogador(false);
                inimigoTravessia?.Iniciar();
                break;

            case Passo.CaixaResultado:
                // chove nabos
                passo = Passo.EsperandoOrbs;
                EsconderCaixa(); TravarJogador(false);
                TentarInscreverOrbs();
                chuvaDeOrbs?.Iniciar();
                break;

            case Passo.PosColeta:
                // solta o player para curar
                passo = Passo.EsperandoCura;
                EsconderCaixa(); TravarJogador(false);
                if (logsDiagnostico)
                {
                    int orbs = OrbManager.Instance != null ? OrbManager.Instance.CurrentOrbs : -1;
                    int hp = vidaPlayer != null ? vidaPlayer.currentHealth : -1;
                    int hpMax = vidaPlayer != null ? vidaPlayer.MaxHealth : -1;
                    Debug.Log($"[Tutorial] Entrando em ESPERANDO CURA. HP={hp}/{hpMax} orbs={orbs}. " +
                              $"(Para curar: precisa HP < max E orbs >= custoCura.)", this);
                }
                break;

            case Passo.FimCura:
                passo = Passo.CaixaIntroProjetil; MostrarTexto(falaIntroProjetil);
                break;

            case Passo.CaixaIntroProjetil:
                // dispara o projétil parável
                passo = Passo.EsperandoProjetil;
                EsconderCaixa(); TravarJogador(false);
                projetilParavel?.Iniciar();
                break;

            case Passo.CaixaProjetilOk:
                // 2º ataque: travessia LENTA
                passo = Passo.EsperandoTravessiaLenta;
                EsconderCaixa(); TravarJogador(false);
                inimigoTravessiaLento?.Iniciar();
                break;

            case Passo.CaixaProjetilFalhou:
                // repete o projétil
                passo = Passo.EsperandoProjetil;
                EsconderCaixa(); TravarJogador(false);
                projetilParavel?.Iniciar();
                break;

            case Passo.CaixaTravessiaFinal:
                // aparece o portal e encerra o tutorial
                if (portal != null) portal.SetActive(true);
                passo = Passo.Fim;
                EncerrarDialogo();
                break;

            case Passo.CaixaTravessiaFalhou:
                // repete a travessia lenta
                passo = Passo.EsperandoTravessiaLenta;
                EsconderCaixa(); TravarJogador(false);
                inimigoTravessiaLento?.Iniciar();
                break;
        }
    }
    // GANCHOS DE GAMEPLAY
    public void EventoPegouMoeda()
    {
        if (passo != Passo.EsperandoMoeda) return;
        passo = Passo.PosMoeda;
        MostrarTexto(fala4); TravarJogador(true);
    }
    private void AoTomarDano(int dano)
    {
        switch (passo)
        {
            case Passo.EsperandoResultado:
                FinalizarPrimeiroAtaque(falaLevouDano);
                break;
            case Passo.EsperandoProjetil:
                FalharProjetil();
                break;
            case Passo.EsperandoTravessiaLenta:
                FalharTravessiaLenta();
                break;
        }
    }
    private void AoDarParry()
    {
        switch (passo)
        {
            case Passo.EsperandoResultado:
                FinalizarPrimeiroAtaque(falaDeuParry);
                break;
            case Passo.EsperandoProjetil:
                AcertarProjetil();
                break;
            case Passo.EsperandoTravessiaLenta:
                AcertarTravessiaLenta();
                break;
        }
    }
    private void AoErrarProjetil()
    {
        if (passo != Passo.EsperandoProjetil) return;
        FalharProjetil();
    }
    private void AoErrarTravessiaLenta()
    {
        if (passo != Passo.EsperandoTravessiaLenta) return;
        FalharTravessiaLenta();
    }
    // --- 1º ataque ---
    private void FinalizarPrimeiroAtaque(string texto)
    {
        passo = Passo.CaixaResultado;
        inimigoTravessia?.Parar();
        MostrarTexto(texto); TravarJogador(true);
    }
    // --- Projétil parável ---
    private void AcertarProjetil()
    {
        passo = Passo.CaixaProjetilOk;
        projetilParavel?.Parar();
        MostrarTexto(falaProjetilOk); TravarJogador(true);
    }
    private void FalharProjetil()
    {
        passo = Passo.CaixaProjetilFalhou;
        projetilParavel?.Parar();
        MostrarTexto(falaProjetilFalhou); TravarJogador(true);
    }
    // --- Travessia lenta ---
    private void AcertarTravessiaLenta()
    {
        passo = Passo.CaixaTravessiaFinal;
        inimigoTravessiaLento?.Parar();
        MostrarTexto(falaTravessiaLentaFinal); TravarJogador(true);
    }
    private void FalharTravessiaLenta()
    {
        passo = Passo.CaixaTravessiaFalhou;
        inimigoTravessiaLento?.Parar();
        MostrarTexto(falaTravessiaLentaFalhou); TravarJogador(true);
    }
    // --- Coleta / Cura ---
    private void AoMudarOrbs(int atual, int max)
    {
        if (passo != Passo.EsperandoOrbs || atual < orbsParaAvancar) return;
        passo = Passo.PosColeta;
        chuvaDeOrbs?.Parar(true);
        MostrarTexto(falaPosColeta); TravarJogador(true);
    }
    private void AoMudarVida(int atual, int max)
    {
        if (logsDiagnostico)
            Debug.Log($"[Tutorial] Vida mudou: atual={atual} anterior={ultimaVidaConhecida} passo={passo}", this);
        if (passo == Passo.EsperandoCura && atual >= ultimaVidaConhecida)
        {
            if (logsDiagnostico) Debug.Log("[Tutorial] Cura detectada -> caixa fim-cura.", this);
            passo = Passo.FimCura;
            MostrarTexto(falaFinal); TravarJogador(true);
        }
        ultimaVidaConhecida = atual;
    }
    // UI + TRAVA
    private void MostrarTexto(string t)
    {
        if (canvasDialogo != null) canvasDialogo.SetActive(true);
        if (textoDialogo != null) textoDialogo.text = t;
        if (btnContinuar != null) btnContinuar.SetActive(true);
    }
    private void EsconderCaixa()
    {
        if (canvasDialogo != null) canvasDialogo.SetActive(false);
    }
    public void EncerrarDialogo()
    {
        EsconderCaixa(); TravarJogador(false);
    }
    private void TravarJogador(bool travar)
    {
        foreach (MonoBehaviour script in scriptsDoPlayerParaDesligar)
            if (script != null) script.enabled = !travar;

        if (travar && jogadorGeral != null)
        {
            Rigidbody2D rb = jogadorGeral.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }
    }
}