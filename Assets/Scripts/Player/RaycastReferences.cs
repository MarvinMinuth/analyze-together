using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastReferences : MonoBehaviour
{
    public static RaycastReferences Instance;

    public LineRenderer leftHandRaycast;
    public LineRenderer rightHandRaycast;

    public LineRenderer teleportRaycast;

    private void Awake()
    {
        Instance = this;
    }
}
