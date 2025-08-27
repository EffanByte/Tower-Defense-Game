using System;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Config")]
    [Min(1)] public int maxHealth = 20;

    [SerializeField] private int currentHealth;

    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDeath;

    void Awake()
    {
        // Robust singleton
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        currentHealth = Mathf.Max(1, maxHealth);  // start full
        OnHealthChanged?.Invoke(currentHealth, maxHealth);  // SAFE invoke
        Debug.Log($"[HealthManager] Awake → {currentHealth}/{maxHealth}");
    }

    public int Current => currentHealth;

    public void Damage(int amount)
    {
        if (amount <= 0) return;

        int old = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - amount);

        // SAFE invoke (no listeners = no crash)
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();  
            Time.timeScale = 0f; // ⏸ Pause everything
            Debug.Log("[HealthManager] Dead");
        }
    }
    public static HealthManager GetHealthManager() => Instance;
}
