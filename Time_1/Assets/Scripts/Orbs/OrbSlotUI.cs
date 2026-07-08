using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrbSlotUI : MonoBehaviour
{
    [Tooltip("Imagem da chama (fundo). Aparece só quando o nabo está coletado.")]
    [SerializeField] private Image chama;

    [Tooltip("Imagem do nabo (frente, sempre visível).")]
    [SerializeField] private Image nabo;

    public void Configurar(bool aceso, Sprite spriteChama)
    {
        if (chama != null)
        {
            chama.enabled = aceso;
            if (aceso && spriteChama != null)
                chama.sprite = spriteChama;
        }
    }
}