using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 2f, 0);

    private Transform target;
    private EnemyHealth health;

    public void Init(EnemyHealth h)
    {
        health = h;
        target = h.transform;

        health.OnDamaged += OnDamaged;
        health.OnDeath += OnDeath;

        UpdateFill();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Stick above enemy
        transform.position = target.position + worldOffset;

        // Face the camera
        if (Camera.main)
            transform.rotation = Camera.main.transform.rotation;
    }

    void OnDamaged(EnemyHealth h, int curHP)
    {
        UpdateFill();
    }

    void OnDeath(EnemyHealth h)
    {
        Destroy(gameObject);
    }

    void UpdateFill()
    {
        if (progressBar && health != null)
        {
            float percent = (float)health.CurrentHealth / health.MaxHealth;
            progressBar.value = percent;
        }
    }
}
