using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerShooting))]
public class PlayerHealthController : HealthController
{
    [SerializeField] GameObject PlayerReference;
    [SerializeField] GameObject PlayerCanvas;

    [Header("I-Frames")]
    [SerializeField] private float iframeDuration = 1f;

    private PlayerShooting playerShooting;
    private float iframeTimer = 0f;

    void Awake()
    {
        playerShooting = GetComponent<PlayerShooting>();
        currentHealth = MaxHealth;
    }

    void Update()
    {
        if (iframeTimer > 0f)
            iframeTimer -= Time.deltaTime;
    }

    public void TakeDamage(int dmg, string attackTag)
    {
        if (attackTag == "Melee" && playerShooting != null && playerShooting.TryMeleeParry())
        {
            Debug.Log("GET PARRIED ");
            iframeTimer = iframeDuration;
            return;
        }
        TakeDamage(dmg);
    }

    public new void TakeDamage(int dmg)
    {
        if (iframeTimer > 0f) return;
        base.TakeDamage(dmg);
        iframeTimer = iframeDuration;
    }

    public override void Die()
    {
        PlayerReference.SetActive(false);
        PlayerCanvas.SetActive(false);
    }
}
