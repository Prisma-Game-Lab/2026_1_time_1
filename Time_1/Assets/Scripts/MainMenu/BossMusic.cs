using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMusic : MonoBehaviour
{
    [Tooltip("Musica desta cena de boss.")]
    [SerializeField] private string nomeMusica = "boss";

    [Tooltip("Tocar em loop.")]
    [SerializeField] private bool loop = true;

    [SerializeField] private bool tocarNoStart = true;

    private void Start()
    {
        if (tocarNoStart) Tocar();
    }

    public void Tocar()
    {
        if (string.IsNullOrEmpty(nomeMusica)) return;
        MusicManager.PlayMusic(nomeMusica);
    }
}