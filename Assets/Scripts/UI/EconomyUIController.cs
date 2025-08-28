using UnityEngine;
using TMPro;

public class EconomyUIController : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private TextMeshProUGUI moneyLabel;
    [SerializeField] private TextMeshProUGUI damageUpgradeLabel;
    [SerializeField] private TextMeshProUGUI speedUpgradeLabel;
    [SerializeField] private TextMeshProUGUI rangeUpgradeLabel;

    [Header("Tower Prices UI")]
    [SerializeField] private TextMeshProUGUI gunPriceLabel;
    [SerializeField] private TextMeshProUGUI sniperPriceLabel;
    [SerializeField] private TextMeshProUGUI flamePriceLabel;
    [SerializeField] private TextMeshProUGUI chaosPriceLabel;

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
            UpdateUpgradeCostsUI();
            UpdateTowerPricesUI();
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

    public void UpdateTowerPricesUI()
    {
        var ec = EconomyController.Instance;
        if (!ec) return;

        if (gunPriceLabel) gunPriceLabel.text = $"${ec.gunCost}";
        if (sniperPriceLabel) sniperPriceLabel.text = $"${ec.sniperCost}";
        if (flamePriceLabel) flamePriceLabel.text = $"${ec.flameCost}";
        if (chaosPriceLabel) chaosPriceLabel.text = $"${ec.chaosCost}";
    }
}
