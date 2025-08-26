using UnityEngine;

public class RotateTower : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GameObject gun = transform.GetChild(1).gameObject; // Hard coded for 2nd (gun) child
        gun.transform.Rotate(0, 2, 0); // Fixed for now, will have to add if for tracking
    }
}
