using System;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Config")]
    [Min(1)] public int maxHealth = 20;

    private int currentHealth;

    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDeath;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        currentHealth = maxHealth; // start full
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public int Current => currentHealth;

    public void Damage(int amount)
    {
        if (amount <= 0) return;
        int old = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth != old) OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth == 0) OnDeath?.Invoke();
    }
    
}

