using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public static readonly List<Interactable> All = new();

    [Header("Interação")]
    [SerializeField] protected float raioInteracao = 2f;
    [SerializeField] protected Transform pontoIcone;
    [SerializeField] protected GameObject iconePrefab;

    private GameObject iconeAtual;
    public float RaioInteracao => raioInteracao;
    public Transform PontoIcone => pontoIcone != null ? pontoIcone : transform;
    public virtual bool PodeInteragir() => true;

    public abstract void Interagir();

    public void MostrarIcone(bool mostrar)
    {
        if (mostrar && PodeInteragir())
        {
            if (iconeAtual == null && iconePrefab != null)
            {
                iconeAtual = Instantiate(iconePrefab, PontoIcone.position, Quaternion.identity, PontoIcone);
                iconeAtual.transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            EsconderIcone();
        }
    }
    private void EsconderIcone()
    {
        if (iconeAtual != null)
        {
            Destroy(iconeAtual);
            iconeAtual = null;
        }
    }
    protected virtual void OnEnable() => All.Add(this);
    protected virtual void OnDisable()
    {
        All.Remove(this);
        EsconderIcone();
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, raioInteracao);
    }
}