using UnityEngine;

public class GunTurret : MonoBehaviour
{
    [Header("Gun setup")]
    private Transform gun; // assign in inspector, or defaults to child 1

    [Header("Targeting")]
    public float range = 4f;
    public string enemyLayerName = "Enemy";

    private int enemyLayer;

    void Awake()
    {
        gun = transform.GetChild(1); // hard-coded to 2nd child

        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer < 0)
            Debug.LogError($"Layer '{enemyLayerName}' not found! Add it in Unity's Layer settings.");
    }

    void Update()
    {
        Transform target = GetClosestEnemy();

        if (target != null)
        {
            // Rotate gun toward target
            Vector3 dir = (target.position - gun.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
            gun.rotation = Quaternion.Slerp(gun.rotation, lookRot, Time.deltaTime * 10f);
        }
    }

    Transform GetClosestEnemy()
    {
        // Look only in Enemy layer
        Collider[] hits = Physics.OverlapSphere(transform.position, range, 1 << enemyLayer);

        float minDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }
        Debug.Log("Nearest enemy: " + nearest);
        return nearest;
    }

    // Draw turret radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
