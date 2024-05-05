using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RaycastListener : MonoBehaviour
{
    public event EventHandler OnRaycastEnabled;
    private void OnEnable()
    {
        OnRaycastEnabled?.Invoke(this, EventArgs.Empty);
    }
}
