using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayInBoundaries : MonoBehaviour
{
    [SerializeField] private float minX, maxX, minY, maxY, minZ, maxZ;

    void Update()
    {
        if (!CheckIfInBoundaries())
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minX, maxX),
                Mathf.Clamp(transform.position.y, minY, maxY),
                Mathf.Clamp(transform.position.z, minZ, maxZ)
            );
        }
    }

    private bool CheckIfInBoundaries()
    {
        return transform.position.x >= minX && transform.position.x <= maxX &&
               transform.position.y >= minY && transform.position.y <= maxY &&
               transform.position.z >= minZ && transform.position.z <= maxZ;
    }
}
