using UnityEngine;
using System;

public class EconomyController : MonoBehaviour
{
    public static EconomyController Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private int startingMoney = 300;

    [Header("Tower Costs")]
    public int gunCost = 100;
    public int sniperCost = 175;
    public int flameCost = 200;
    public int chaosCost = 1000;

    [Header("Enemy Rewards")]
    public int gruntReward = 5;
    public int scoutReward = 10;
    public int heavyReward = 20;
    public int bossReward = 500;

    private int currentMoney;

    public event Action<int> OnMoneyChanged; // UI can subscribe

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentMoney = startingMoney;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    // --- Public API ---

    public int CurrentMoney => currentMoney;

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        Debug.Log($"[Monetization] +${amount}, total=${currentMoney}");
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            Debug.Log($"[Monetization] -${amount}, total=${currentMoney}");
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        else
        {
            Debug.Log("[Monetization] Not enough money!");
            return false;
        }
    }

    // --- Enemy kill rewards ---

    public void RewardEnemy(EnemyKind kind)
    {
        int reward = 0;
        switch (kind)
        {
            case EnemyKind.Grunt: reward = gruntReward; break;
            case EnemyKind.Stealth: reward = scoutReward; break;
            case EnemyKind.Heavy: reward = heavyReward; break;
            case EnemyKind.Boss: reward = bossReward; break;
        }
        if (reward > 0) AddMoney(reward);
    }

}
