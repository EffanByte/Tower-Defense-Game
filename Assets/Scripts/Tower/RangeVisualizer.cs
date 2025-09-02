using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RangeVisualizer : MonoBehaviour
{
    private TowerUpgrade upgrade;
    private float range;
    [SerializeField] private int segments = 64;

    private LineRenderer lr;

    void Start()
    {
        upgrade = GetComponent<TowerUpgrade>();
        range = upgrade.CurrentRange;
        lr = GetComponent<LineRenderer>();
        lr.loop = true;
        lr.useWorldSpace = false;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red;
        lr.endColor = Color.red;
    }

    void Update()
    { 
        DrawCircle(upgrade.CurrentRange);
    }

    void DrawCircle(float r)
    {
        lr.positionCount = segments;
        Vector3 center = transform.position; // or turret base if offset

        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
            lr.SetPosition(i, transform.InverseTransformPoint(pos));
        }
    }

}
