using System;
using UnityEngine;
public class TowerUpgradeController : MonoBehaviour
{
    [SerializeField] private InputRouter inputRouter;
    [SerializeField] private GameObject infoUI;

    public static GameObject SelectedTower;
    void OnEnable()
    {
        inputRouter.OnTowerClicked += ShowUpgrade;
        inputRouter.OnNonTowerClicked += HideUpgrade;
    }

    void OnDisable()
    {
        inputRouter.OnTowerClicked -= ShowUpgrade;
        inputRouter.OnNonTowerClicked -= HideUpgrade;
    }

    void HideUpgrade()
    {
        SelectedTower = null;
        ToggleUI();
    }

    void ShowUpgrade(GameObject tower)
    {
        SelectedTower = tower;
        ToggleUI();
    }

    void ToggleUI()
    {
        if (SelectedTower != null)
        {
         //   infoUI.SetActive(true);
            toggleRangeVisualizer();
        }
        else
        {
         //  infoUI.SetActive(false);
            toggleRangeVisualizer();    
        }
    }

    void toggleRangeVisualizer()
    {
        if (SelectedTower != null)
        {
            var rv = SelectedTower.GetComponent<RangeVisualizer>();
            if (rv != null)
                rv.enabled = !rv.enabled;
        }
    }
    public void HandleUpgradeDamage()
    {
        SelectedTower.GetComponent<TowerUpgrade>().UpgradeDamage();
    }

    public void HandleUpgradeSpeed()
    {
        SelectedTower.GetComponent<TowerUpgrade>().UpgradeSpeed();
    }

    public void HandleUpgradeRange()
    {
        SelectedTower.GetComponent<TowerUpgrade>().UpgradeRange();
    }


}
