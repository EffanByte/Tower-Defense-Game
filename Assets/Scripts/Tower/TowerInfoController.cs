using UnityEngine;
public class TowerInfoController : MonoBehaviour
{
    [SerializeField] private InputRouter inputRouter;

    void OnEnable()
    {
        inputRouter.OnTowerClicked += ShowTowerInfo;
    }

    void OnDisable()
    {
        inputRouter.OnTowerClicked -= ShowTowerInfo;
    }

    void ShowTowerInfo(GameObject tower)
    {
        var detail = tower.GetComponent<TowerDetail>();
        if (detail)
        {
            Debug.Log($"Tower {detail.towerName} Lvl {detail.Level}, Kills {detail.Kills}");
            // TODO: open UI panel
        }
    }
}
