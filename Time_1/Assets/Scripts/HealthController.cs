using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HealthController : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    public int MaxHealth => maxHealth;

    [HideInInspector] public int currentHealth;

    public Action<int, int> OnHealthChanged;
    void Awake()
    {
        currentHealth = maxHealth;
    }
    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
            currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0)
            Die();
    }
    public virtual void Die()
    {
        // Death logic
    }
}