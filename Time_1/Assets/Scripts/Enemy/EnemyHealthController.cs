using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthController : HealthController
{
    [SerializeField] GameObject EnemyReference;

    public override void Die()
    {
        EnemyReference.SetActive(false);
    }
}
