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
    [SerializeField] private EnemyKind enemyType;
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
            // ---- Notify spawner (cleanup / path logic) ----
            if (manager != null && agent != null)
                manager.NotifyEnemyKilled(agent);

            // ---- Stop animation ----
            GetComponent<Animator>().enabled = false;
            GetComponent<EnemyPathAgent>().enabled = false;
            // ---- Rotate to lay flat (90°) ----
            transform.rotation = Quaternion.Euler(90f, 90f, 0f);

            // ---- Change layer to Dead ----
            gameObject.layer = LayerMask.NameToLayer("Dead");

            // ---- Monetization reward ----
            if (EconomyController.Instance != null)
                EconomyController.Instance.RewardEnemy(enemyType);

            OnDeath?.Invoke(this);
        }
    }

}
