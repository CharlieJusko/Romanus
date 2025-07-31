using UnityEngine;

public interface IActivatable
{
    public bool CanActivate {  get; set; }

    public void Activate();
}
