using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollider : MonoBehaviour
{
    [SerializeField] private int handDamage;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealthController pHC = other.GetComponent<PlayerHealthController>();
        if(pHC == null)
        {
            return;
        }
        pHC.TakeDamage(handDamage);

    }
}
