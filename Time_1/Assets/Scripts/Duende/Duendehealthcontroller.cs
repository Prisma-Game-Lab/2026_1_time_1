using UnityEngine;

public class DundeHealthController : HealthController
{
    [SerializeField] private Duende duende;
    private bool _morreu = false;
    private void Awake()
    {
        if (duende == null)
            duende = GetComponentInParent<Duende>();

        if (duende == null)
            Debug.LogError("[DundeHealthController] Duende não encontrado. Atribua no Inspector.", this);
    }
    public override void Die()
    {
        if (_morreu) return;
        _morreu = true;

        if (duende != null)
            duende.OnDeath();
        else
            gameObject.SetActive(false);
    }
}