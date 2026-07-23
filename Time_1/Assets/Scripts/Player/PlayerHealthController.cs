using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerShooting))]
public class PlayerHealthController : HealthController
{
    [SerializeField] GameObject PlayerReference;
    [SerializeField] GameObject PlayerCanvas;
    [SerializeField] GameObject GameOverPanel;

    [Header("I-Frames")]
    [SerializeField] private float iframeDuration = 1f;

    [Header("Parry Effect")]
    [SerializeField] private GameObject parryEffect;
    [SerializeField] private float parryEffectDuration = 0.3f;

    private PlayerShooting playerShooting;
    private DamageFlash damageFlash;
    private float iframeTimer = 0f;
    private float parryEffectTimer = 0f;
    private bool wasInvincible = false;

    public bool IsInvincible => iframeTimer > 0f;

    void Awake()
    {
        playerShooting = GetComponent<PlayerShooting>();
        currentHealth = MaxHealth;
        damageFlash = GetComponent<DamageFlash>();
    }

    void Update()
    {
        if (iframeTimer > 0f)
            iframeTimer -= Time.deltaTime;

        bool invincible = iframeTimer > 0f;
        if (!invincible && wasInvincible && damageFlash != null)
            damageFlash.StopIFrameFlash();
        wasInvincible = invincible;

        if (parryEffectTimer > 0f)
        {
            parryEffectTimer -= Time.deltaTime;
            if (parryEffectTimer <= 0f && parryEffect != null)
                parryEffect.SetActive(false);
        }
    }

    public void TakeDamage(int dmg, string attackTag)
    {
        if (attackTag == "Melee" && playerShooting != null && playerShooting.TryMeleeParry())
        {
            iframeTimer = iframeDuration;
            parryEffectTimer = parryEffectDuration;
            if (parryEffect != null) parryEffect.SetActive(true);
            return;
        }
        TakeDamage(dmg);
    }

    public new void TakeDamage(int dmg)
    {
        if (iframeTimer > 0f) return;
        base.TakeDamage(dmg);
        iframeTimer = iframeDuration;
        if (damageFlash != null) damageFlash.StartIFrameFlash();
    }

    public override void Die()
    {
        if (PlayerReference != null) PlayerReference.SetActive(false);
        if (PlayerCanvas != null) PlayerCanvas.SetActive(false);
        if (GameOverPanel != null) GameOverPanel.SetActive(true);
    }
}
