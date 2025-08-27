using UnityEngine;
using TMPro;

public class EconomyUIController : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private TextMeshProUGUI moneyLabel;
    [SerializeField] private TextMeshProUGUI damageUpgradeLabel;
    [SerializeField] private TextMeshProUGUI speedUpgradeLabel;
    [SerializeField] private TextMeshProUGUI rangeUpgradeLabel;

    bool subscribed;

    void OnEnable()
    {
        TrySubscribeOrQueue();
    }

    System.Collections.IEnumerator WaitForECThenSubscribe()
    {
        while (EconomyController.Instance == null) yield return null;
        TrySubscribeOrQueue();
    }

    void TrySubscribeOrQueue()
    {
        var ec = EconomyController.Instance;
        if (ec != null)
        {
            EconomyController.Instance.OnMoneyChanged += UpdateMoneyUI;
            subscribed = true;
            UpdateMoneyUI(ec.CurrentMoney);  // immediate draw
            UpdateUpgradeCostsUI();          // also refresh tower info
        }
        else
        {
            StartCoroutine(WaitForECThenSubscribe());
        }
    }

    void OnDisable()
    {
        var ec = EconomyController.Instance;
        if (ec != null && subscribed)
        {
            ec.OnMoneyChanged -= UpdateMoneyUI;
            subscribed = false;
        }
    }

    void UpdateMoneyUI(int currentMoney)
    {
        if (moneyLabel)
            moneyLabel.text = $"${currentMoney}";

        // refresh upgrade pricing whenever money changes
        UpdateUpgradeCostsUI();
    }

    public void UpdateUpgradeCostsUI()
    {
        if (TowerUpgradeController.SelectedTower == null)
        {
            if (damageUpgradeLabel) damageUpgradeLabel.text = "";
            if (speedUpgradeLabel) speedUpgradeLabel.text = "";
            if (rangeUpgradeLabel) rangeUpgradeLabel.text = "";
            return;
        }

        var upgrade = TowerUpgradeController.SelectedTower.GetComponent<TowerUpgrade>();
        if (upgrade == null) return;

        if (damageUpgradeLabel) damageUpgradeLabel.text = $"${upgrade.GetDamageUpgradeCost()}";
        if (speedUpgradeLabel) speedUpgradeLabel.text = $"${upgrade.GetSpeedUpgradeCost()}";
        if (rangeUpgradeLabel) rangeUpgradeLabel.text = $"${upgrade.GetRangeUpgradeCost()}";
    }
}
