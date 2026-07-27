using System.Collections;
using UnityEngine;

public class TutorialHazardMover : MonoBehaviour
{
    [Header("Objeto que atravessa")]
    [Tooltip("O objeto de dano. Pode começar DESATIVADO na cena. Se vazio, usa este próprio GameObject.")]
    [SerializeField] private GameObject objeto;

    [Header("Trajeto")]
    [Tooltip("Ponto de partida (fora da tela, de um lado).")]
    [SerializeField] private Transform pontoInicio;
    [Tooltip("Ponto de chegada (fora da tela, do outro lado).")]
    [SerializeField] private Transform pontoFim;
    [Tooltip("Velocidade da travessia (unidades/seg).")]
    [SerializeField] private float velocidade = 12f;

    [Header("Fim do trajeto")]
    [Tooltip("Desativa o objeto ao terminar a travessia (ou ao chamar Parar).")]
    [SerializeField] private bool desativarNoFim = true;

    public System.Action OnTravessiaCompleta;

    private Coroutine rotina;

    private GameObject Alvo => objeto != null ? objeto : gameObject;

    public void Iniciar()
    {
        if (rotina != null) StopCoroutine(rotina);
        rotina = StartCoroutine(Atravessar());
    }
    public void Parar()
    {
        if (rotina != null) { StopCoroutine(rotina); rotina = null; }
        if (desativarNoFim && Alvo != null) Alvo.SetActive(false);
    }
    private IEnumerator Atravessar()
    {
        GameObject alvo = Alvo;

        Vector3 ini = pontoInicio != null ? pontoInicio.position : alvo.transform.position;
        Vector3 fim = pontoFim != null ? pontoFim.position : alvo.transform.position;

        alvo.transform.position = ini;
        alvo.SetActive(true);

        float dist = Vector3.Distance(ini, fim);
        float dur = velocidade > 0.01f ? dist / velocidade : 0.01f;
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            alvo.transform.position = Vector3.Lerp(ini, fim, t / dur);
            yield return null;
        }

        alvo.transform.position = fim;
        if (desativarNoFim) alvo.SetActive(false);
        rotina = null;
        OnTravessiaCompleta?.Invoke();
    }
}