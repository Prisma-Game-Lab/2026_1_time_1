using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalinhaHealthController : EnemyHealthController
{
    private Galinha galinha;

    private void Awake()
    {
        galinha = GetComponent<Galinha>();
    }
    public override void Die()
    {
        galinha?.Explodir();
    }
}