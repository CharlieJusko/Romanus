using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CharacterProgression
{
    public CounterProgress[] counterStylesProgresses;

    public CharacterProgression(CharacterProgression progression)
    {
        counterStylesProgresses = progression.counterStylesProgresses;
    }
}

public struct CounterProgress
{
    public string fightingStyle;
    public bool canCounter;
    public float progress;
}
