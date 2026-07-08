using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMusic : MonoBehaviour
{
    [Tooltip("Música desta cena de boss.")]
    [SerializeField] private AudioClip musica;

    [Tooltip("Tocar em loop.")]
    [SerializeField] private bool loop = true;

    [SerializeField] private bool tocarNoStart = true;

    private void Start()
    {
        if (tocarNoStart) Tocar();
    }

    public void Tocar()
    {
        if (musica == null) return;
        AudioManager.Instance?.TocaMusica(musica, loop);
    }
}