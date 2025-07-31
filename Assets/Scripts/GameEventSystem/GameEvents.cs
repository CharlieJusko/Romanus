using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;


    private void Awake()
    {
        current = this; 
    }

    // Common
    public event Action onStart;
    public event Action onUpdate;
    public event Action onFixedUpdate;
    public event Action onLateUpdate;

    // Player
    public event Action<string> triggerPlayerAnimation;
    public event Action onDisablePlayerMovement;
    public event Action onEnablePlayerMovement;

    // Activation
    public event Action<string> onActivate;
    public event Action onDeactivate;

    // Attacking
    public event Action onFirearmShoot;

    // Targeting
    public event Action<Targetable> onTarget;

    // Enemy
    public event Action onPlayerDetected;
    public event Action<AudioClip> onEnemyHit;
    public event Action<int> onEnemyDeath;

    // Sanity
    public event Action<int> onFavorGain;
    public event Action<int> onFavorSummation;

    #region Common
    public void OnStart()
    {
        if(onStart != null) 
            onStart();
    }

    public void OnUpdate()
    {
        if(onUpdate != null)
            onUpdate();
    }

    public void OnFixedUpdate()
    {
        if(onFixedUpdate != null)
            onFixedUpdate();
    }

    public void OnLateUpdate()
    {
        if(onLateUpdate != null)
            onLateUpdate();
    }
    #endregion

    #region Player
    public void TriggerPlayerAnimation(string triggerName)
    {
        if(triggerPlayerAnimation != null)
            triggerPlayerAnimation(triggerName);
    }

    public void OnDisablePlayerMovement()
    {
        if(onDisablePlayerMovement != null)
            onDisablePlayerMovement();
    }

    public void OnEnablePlayerMovement()
    {
        if(onEnablePlayerMovement != null)
            onEnablePlayerMovement();
    }
    #endregion

    #region Activation
    public void OnActivate(string animationTrigger = default)
    {
        if(onActivate != null)
            onActivate(animationTrigger);
    }

    public void OnDeactivate()
    {
        if(onDeactivate != null)
            onDeactivate();
    }
    #endregion

    #region Attacking
    public void FirearmShoot()
    {
        if(onFirearmShoot != null)
            onFirearmShoot();
    }
    #endregion

    #region Targeting
    public void OnTarget(Targetable t)
    {
        if(onTarget != null)
            onTarget(t);
    }
    #endregion

    #region Enemy
    public void OnPlayerDetected()
    {
        if(onPlayerDetected != null)
            onPlayerDetected();
    }
    public void EnemyHit(AudioClip hitSFX)
    {
        if(onEnemyHit != null)
            onEnemyHit(hitSFX);
    }

    public void EnemyDeath(int amount)
    {
        if(onEnemyDeath != null)
            onEnemyDeath(amount);
    }
    #endregion

    #region Sanity
    public void FavorGain(int amount)
    {
        if(onFavorGain != null)
            onFavorGain(amount);
    }

    public void FavorSummation(int totalFavor)
    {
        if(onFavorSummation != null)
            onFavorSummation(totalFavor);
    }
    #endregion
}
