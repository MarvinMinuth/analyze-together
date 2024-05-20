using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterBodyPartVisuals : MonoBehaviour, IVisuals
{
    [SerializeField] private List<Renderer> renderers;
    private bool isVisible = true;

    public event EventHandler OnVisibilityChanged;

    public void Hide()
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        isVisible = false;
        OnVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Show()
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }

        isVisible = true;
        OnVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsVisible()
    {
        return isVisible;
    }

    public void SetLocalPosition(Vector3 position)
    {
        transform.localPosition = position;
    }

    public void SetLocalRotation(Quaternion rotation)
    {
        transform.localRotation = rotation;
    }
}
