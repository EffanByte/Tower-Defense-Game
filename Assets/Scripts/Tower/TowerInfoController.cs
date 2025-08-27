using UnityEditor.Rendering.Universal;
using UnityEngine;
public class TowerInfoController : MonoBehaviour
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
            infoUI.SetActive(true);
        else
            infoUI.SetActive(false);
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
