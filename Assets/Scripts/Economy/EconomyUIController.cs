using UnityEngine;
using TMPro;

public class MonetizationUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyLabel;

    void OnEnable()
    {
        if (EconomyController.Instance)
        {
            EconomyController.Instance.OnMoneyChanged += UpdateUI;
            UpdateUI(EconomyController.Instance.CurrentMoney);
        }
    }

    void OnDisable()
    {
        if (EconomyController.Instance)
            EconomyController.Instance.OnMoneyChanged -= UpdateUI;
    }

    void UpdateUI(int currentMoney)
    {
        if (moneyLabel)
            moneyLabel.text = $"${currentMoney}";
    }
}
