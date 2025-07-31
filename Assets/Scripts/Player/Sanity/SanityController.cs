using System.Collections.Generic;
using UnityEngine;

public class SanityController : MonoBehaviour
{
    [SerializeField] private List<MadnessEffect> madnessEffects = new List<MadnessEffect>();


    private void Start()
    {
        GameEvents.current.onFavorSummation += SanityUpdate;
    }

    private void SanityUpdate(int totalFavor)
    {
        foreach(MadnessEffect effect in madnessEffects)
        {
            if(totalFavor < effect.FavorRequirement)
                effect.Deactivate();
            else
                effect.Activate();
        }
    }
}
