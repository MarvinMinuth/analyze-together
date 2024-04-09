using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVisuals
{
    public event EventHandler OnVisibilityChanged;
    public bool IsVisible();
    public void Hide();
    public void Show();
}
