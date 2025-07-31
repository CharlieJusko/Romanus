using System;
using UnityEngine;

public abstract class MadnessEffect: MonoBehaviour
{
    public bool active;
    [SerializeField] private int favorRequirement;

    public int FavorRequirement { get { return favorRequirement; } }

    public abstract void Activate();

    public abstract void Deactivate();
}
