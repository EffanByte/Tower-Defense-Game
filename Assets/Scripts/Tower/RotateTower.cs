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

    }
}
