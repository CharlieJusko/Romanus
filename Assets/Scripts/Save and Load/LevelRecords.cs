using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelRecords
{
    public string name;
    public bool complete = false;
    public int enemiesKOd = 0;
    public TimeSpan completionTime = TimeSpan.MaxValue;

    public LevelRecords(LevelRecords level)
    {
        name = level.name;
        complete = level.complete;
        enemiesKOd = level.enemiesKOd;
        completionTime = level.completionTime;
    }
}
