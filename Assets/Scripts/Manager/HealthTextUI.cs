using TMPro;
using UnityEngine;

public class HealthTextUI : MonoBehaviour
{
    public TextMeshProUGUI label;
    bool subscribed;

    void Awake()
    {
        if (!label) label = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        TrySubscribeOrQueue();
    }

    void OnDisable()
    {
        var hm = HealthManager.Instance;
        if (hm != null && subscribed)
        {
            hm.OnHealthChanged -= UpdateLabel;
            subscribed = false;
        }
    }

    void TrySubscribeOrQueue()
    {
        var hm = HealthManager.Instance;
        if (hm != null)
        {
            hm.OnHealthChanged += UpdateLabel;
            subscribed = true;
            UpdateLabel(hm.Current, hm.maxHealth);  // immediate draw
        }
        else
        {
            // HM not ready yet — try again next frame
            Debug.Log("[HealthTextUI] Waiting for HealthManager…");
            StartCoroutine(WaitForHMThenSubscribe());
        }
    }

    System.Collections.IEnumerator WaitForHMThenSubscribe()
    {
        while (HealthManager.Instance == null) yield return null;
        TrySubscribeOrQueue();
    }

    void UpdateLabel(int cur, int max)
    {
        if (!label)
        {
            Debug.LogWarning("[HealthTextUI] Label not assigned.");
            return;
        }
        label.text = $"{cur} / {max} HP";
    }
}
