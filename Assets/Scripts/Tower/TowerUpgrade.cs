using UnityEngine;

public class TowerUpgrade : MonoBehaviour
{
    [HideInInspector] public int baseDamage;
    [HideInInspector] public float baseFireCooldown;
    [HideInInspector] public float baseRange;

    public int damageLevel = 1;
    public int speedLevel = 1;
    public int rangeLevel = 1;

    public int damageStep = 10;
    public float speedStep = 0.2f;
    public float rangeStep = 2f;

    public int smallDamageStep = 2;
    public float smallSpeedStep = 0.05f;
    public int MaxRangeLevel = 3;

    // Buffs applied from outside (e.g. OP tower aura)
    private float auraDamageMult = 1f;
    private float auraSpeedMult = 1f;

    public int CurrentDamage { get; private set; }
    public float CurrentCooldown { get; private set; }
    public float CurrentRange { get; private set; }

    public void Init(int dmg, float cooldown, float range)
    {
        baseDamage = dmg;
        baseFireCooldown = cooldown;
        baseRange = range;
        RecalculateStats();
    }

    public void InitOP(int dmg, float cooldown, float range,
                       int bigDamageStep, int smallDamageStep,
                       float bigSpeedStep, float smallSpeedStep,
                       float rangeStep, int maxRangeLvl)
    {
        baseDamage = dmg;
        baseFireCooldown = cooldown;
        baseRange = range;

        damageStep = bigDamageStep;
        this.smallDamageStep = smallDamageStep;
        speedStep = bigSpeedStep;
        this.smallSpeedStep = smallSpeedStep;
        this.rangeStep = rangeStep;
        MaxRangeLevel = maxRangeLvl;

        RecalculateStats();
    }
    public void SetAuraBuff(float damageMult, float speedMult)
    {
        auraDamageMult = damageMult;
        auraSpeedMult = speedMult;
        RecalculateStats();
    }

    public void ClearAuraBuff()
    {
        auraDamageMult = 1f;
        auraSpeedMult = 1f;
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        // --- base scaling ---
        int dmg;
        if (damageLevel <= 3)
            dmg = baseDamage + (damageLevel - 1) * damageStep;
        else
            dmg = baseDamage + (2 * damageStep) + ((damageLevel - 3) * smallDamageStep);

        float cooldown;
        if (speedLevel <= 3)
            cooldown = Mathf.Max(0.1f, baseFireCooldown - (speedLevel - 1) * speedStep);
        else
            cooldown = Mathf.Max(0.05f, baseFireCooldown - (2 * speedStep) - ((speedLevel - 3) * smallSpeedStep));

        float rng;
        if (rangeLevel <= MaxRangeLevel)
            rng = baseRange + (rangeLevel - 1) * rangeStep;
        else
            rng = baseRange + (MaxRangeLevel - 1) * rangeStep;

        // --- apply aura buffs ---
        CurrentDamage = Mathf.RoundToInt(dmg * auraDamageMult);
        CurrentCooldown = cooldown / auraSpeedMult;
        CurrentRange = rng;
    }

    public void UpgradeDamage()
    {
        damageLevel++;
        RecalculateStats();
        Debug.Log($"[TowerUpgrade] {gameObject.name} Damage upgraded → {CurrentDamage} (Lvl {damageLevel})");
    }

    public void UpgradeSpeed()
    {
        speedLevel++;
        RecalculateStats();
        Debug.Log($"[TowerUpgrade] {gameObject.name} Speed upgraded → {CurrentCooldown:F2}s cooldown (Lvl {speedLevel})");
    }

    public void UpgradeRange()
    {
        if (rangeLevel < MaxRangeLevel)
        {
            rangeLevel++;
            RecalculateStats();
            Debug.Log($"[TowerUpgrade] {gameObject.name} Range upgraded → {CurrentRange} (Lvl {rangeLevel})");
        }
        else
        {
            Debug.Log($"[TowerUpgrade] {gameObject.name} Range is already maxed at Lvl {MaxRangeLevel}.");
        }
    }
    
}
