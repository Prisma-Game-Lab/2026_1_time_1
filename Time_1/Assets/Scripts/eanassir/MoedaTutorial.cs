using UnityEngine;

public class MoedaTutorial : MonoBehaviour
{
    [Tooltip("Arraste o objeto que tem o GerenciadorTutorial aqui")]
    public GerenciadorTutorial gerenciador;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se quem encostou na moeda foi o Player
        // IMPORTANTE: O seu Player precisa ter a Tag "Player" configurada nele
        if (collision.CompareTag("Player"))
        {
            // Avisa o Boss que a moeda foi pega
            gerenciador.EventoPegouMoeda();
            
            // Destrói a moeda da cena
            Destroy(gameObject);
        }
    }
}