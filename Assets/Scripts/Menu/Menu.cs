using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject firstButton;
    public bool pauseTime = false;
    [Range(0f, 1f)]
    public float timeScale = 0f;

    public virtual void OnEnterMenu() { }
    public virtual void OnExitMenu() { }
}
