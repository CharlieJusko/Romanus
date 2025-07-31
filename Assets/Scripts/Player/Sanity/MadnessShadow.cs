using UnityEngine;

public class MadnessShadow : MadnessEffect
{
    //[Tooltip("Include children shadows?")]
    //[SerializeField] private bool useChildren = false;


    public override void Activate()
    {
        active = true;
        gameObject.SetActive(true);
    }

    public override void Deactivate()
    {
        active = false;
        gameObject.SetActive(false);
    }
}
