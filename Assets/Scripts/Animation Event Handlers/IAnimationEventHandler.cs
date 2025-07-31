using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IAnimationEventHandler
{
    public void OnDodgeExit() { }

    public void OnLungeAttackEnter() { }

    public void OnLungeAttackExit() { }

    public void OnEnableAttackDamage(string parameters) { }

    public void OnDisableAttackDamage(string tagName) { }

    public void EnableCounter(string parameters) { }

    public void DisableCounter() { }
}
