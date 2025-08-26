using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    // Hook into managers/UI
    public System.Action<EnemyHealth> OnDeath;
    public System.Action<EnemyHealth, int> OnDamaged; // (enemy, newHP)
    [SerializeField] private GameObject healthBarPrefab;
    private EnemyHealthUI healthUI;
    void Awake()
    {
        currentHealth = maxHealth;
    }
    void Start()
    {
        if (healthBarPrefab)
        {
            var bar = Instantiate(healthBarPrefab, transform);
            healthUI = bar.GetComponent<EnemyHealthUI>();
            if (healthUI) healthUI.Init(this);
        }
    }
    public void Init(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
    }


    public void TakeDamage(int amount, EnemySpawner manager = null, EnemyPathAgent agent = null)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        OnDamaged?.Invoke(this, currentHealth);

        if (currentHealth <= 0)
        {
            if (manager != null && agent != null)
                manager.NotifyEnemyKilled(agent);
            else
                Destroy(gameObject);

            OnDeath?.Invoke(this);
        }
    }
}
