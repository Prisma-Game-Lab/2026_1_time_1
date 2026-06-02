using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthController : HealthController
{

    [SerializeField] GameObject PlayerReference;

    [SerializeField] GameObject PlayerCanvas;


    public override void Die()
    {
        PlayerReference.SetActive(false);
        PlayerCanvas.SetActive(false);
    }
}
