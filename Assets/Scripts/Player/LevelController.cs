using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public LevelRecords[] levels;
    [SerializeField]
    private int currentLevelIndex = 0;

    [Header("Enemy Counters")]
    private int totalEnemies;
    public int currentEnemiesKilled = 0;

    [Header("Timer")]
    private DateTime gameStartTime;
    public TimeSpan currentDuration;

    public bool triggerComplete = false;
    public bool saveTrigger = false;
    public bool loadTrigger = false;


    private void Awake()
    {
        TestLoad();
    }

    private void Start()
    {
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        currentEnemiesKilled = 0;
        gameStartTime = DateTime.Now;
    }

    private void Update()
    {
        currentDuration = DateTime.Now - gameStartTime;

        if(saveTrigger)
        {
            saveTrigger = false;
            TestSave();
        }

        if(loadTrigger)
        {
            loadTrigger = false;
            TestLoad();
        }

        if(triggerComplete)
        {
            triggerComplete = false;
            Complete();
        }
    }

    public int AddKill()
    {
        return ++currentEnemiesKilled;
    }

    public void Complete()
    {
        levels[currentLevelIndex].complete = true;
        levels[currentLevelIndex].enemiesKOd = currentEnemiesKilled > levels[currentLevelIndex].enemiesKOd ? currentEnemiesKilled : levels[currentLevelIndex].enemiesKOd;
        levels[currentLevelIndex].completionTime = currentDuration < levels[currentLevelIndex].completionTime ? currentDuration : levels[currentLevelIndex].completionTime;
        SaveSystem.SaveLevel(levels[currentLevelIndex]);
    }

    public void TestSave()
    {
        foreach(LevelRecords level in levels)
        {
            SaveSystem.SaveLevel(level);
        }
    }

    public void TestLoad()
    {
        for(int i = 0; i < levels.Length; i++)
        {
            levels[i] = SaveSystem.LoadLevel(levels[i].name);
        }
    }
}
