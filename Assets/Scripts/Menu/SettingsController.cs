using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Settings
{
    public bool slowTimeOnHit = true;
    [Range(0.01f, 0.1f)]
    public float slowTimeFactor = 0.05f;
}

public class SettingsController : MonoBehaviour
{
    public static SettingsController instance;
    public Settings settings;

    void Awake()
    {
        Settings testSettings = SaveSystem.LoadSettings();
        if(testSettings != null)
        {
            settings = testSettings;
        }
        instance = this;
    }

}