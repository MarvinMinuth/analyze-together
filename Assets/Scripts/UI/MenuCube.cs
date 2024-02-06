using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCube : MonoBehaviour
{
    public GameObject target;
    public float distanceToPlace = 1.0f;
    private bool isDragged = false;

    // Start is called before the first frame update
    void Start()
    {
        AlignToTarget();
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDragged)
        {
            AlignToTarget();
        }
    }

    public void OnSelectExit()
    {
        isDragged = false;
        AlignToTarget();
    }

    public void OnSelectEnter()
    {
        isDragged = true;
    }

    public void AlignToTarget()
    {
        // Stelle sicher, dass das Ziel-GameObject existiert
        if (target != null)
        {
            // Richte das Hauptobjekt in Richtung des Ziel-GameObjects aus
            transform.LookAt(target.transform);

            // Setze die X- und Z-Rotation auf 0 und behalte die Y-Rotation bei
            Vector3 currentRotation = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, currentRotation.y, 0);
            transform.position.Set(transform.position.x, 0, transform.position.z);
        }
    }

    public void PlaceInFrontOfTarget()
    {
        transform.position.Set(target.transform.position.x, target.transform.position.y, target.transform.position.z+1);
        AlignToTarget();
    }
}
