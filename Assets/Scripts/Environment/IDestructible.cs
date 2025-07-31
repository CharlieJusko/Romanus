using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDestructible
{
    public bool CanDestruct { get; set; }
    public bool Destructed { get; set; }
    public GameObject DesctructionVFXPrefab { get; set; }
    public void Destruct();
}
