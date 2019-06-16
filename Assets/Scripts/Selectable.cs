using System;
using UnityEngine;

abstract public class Selectable : MonoBehaviour
{
    public bool Selected { get; set; }

    public abstract void OnSelected();

    public Selectable()
    {
        Selected = false;
    }

    private void OnMouseUp()
    {
        OnSelected();
    }
}
