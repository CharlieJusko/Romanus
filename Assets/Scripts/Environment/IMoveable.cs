using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable
{
    public bool CanMove { get; set; }
    public float Speed { get; set; }
    public Vector3 Direction { get; set; }

    public void Move();

    public void Stop();

}
