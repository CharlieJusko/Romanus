using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBehaviors : MonoBehaviour
{
    public bool smoke = false;
    public bool buff = false;

    private void Update()
    {
        GetComponent<Animator>().SetBool("Smoke", smoke);
        GetComponent<Animator>().SetBool("Buff", buff);
    }
}
