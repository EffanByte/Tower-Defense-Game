using UnityEngine;
using TMPro;

public class MonetizationUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyLabel;

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
            EconomyController.Instance.OnMoneyChanged += UpdateUI;
            subscribed = true;
            Debug.Log("subscrbied to economy");
            UpdateUI(ec.CurrentMoney);  // immediate draw
        }
        else
        {
            // HM not ready yet — try again next frame
            StartCoroutine(WaitForECThenSubscribe());
        }
    }
    void OnDisable()
    {
        var ec = EconomyController.Instance;
        if (ec != null && subscribed)
        {
            ec.OnMoneyChanged -= UpdateUI;
            subscribed = false;
        }
    }

    void UpdateUI(int currentMoney)
    {
        Debug.Log(currentMoney);
        if (moneyLabel)
            moneyLabel.text = $"${currentMoney}";
    }
}
