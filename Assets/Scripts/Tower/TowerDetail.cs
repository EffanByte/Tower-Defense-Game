using System;
using UnityEngine;

public class TowerDetail : MonoBehaviour
{
    [Header("Tower Info")]
    public string towerName = "Basic Turret";

    [Header("Runtime Stats")]
    [SerializeField] private int kills = 0;
    [SerializeField] private int level = 1;

    // Events so UI/other systems can react
    public event Action<int> OnKillsChanged;  // new kill count
    public event Action<int> OnLevelChanged;  // new level

    public int Kills => kills;
    public int Level => level;

    /// <summary>
    /// Increment kills by 1 and fire event.
    /// </summary>
    public void AddKill()
    {
        kills++;
        OnKillsChanged?.Invoke(kills);
        Debug.Log($"[{towerName}] Kill count = {kills}");
    }

    /// <summary>
    /// Add multiple kills at once (e.g. AoE).
    /// </summary>
    public void AddKills(int count)
    {
        kills += Mathf.Max(0, count);
        OnKillsChanged?.Invoke(kills);
        Debug.Log($"[{towerName}] Kill count = {kills}");
    }

    /// <summary>
    /// Set the tower’s level directly.
    /// </summary>
    public void SetLevel(int newLevel)
    {
        if (newLevel < 1) newLevel = 1;
        if (newLevel != level)
        {
            level = newLevel;
            OnLevelChanged?.Invoke(level);
            Debug.Log($"[{towerName}] Tower leveled up → {level}");
        }
    }

    /// <summary>
    /// Increase tower level by 1.
    /// </summary>
    public void LevelUp()
    {
        SetLevel(level + 1);
    }
}
