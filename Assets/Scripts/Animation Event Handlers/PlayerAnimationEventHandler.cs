using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerAnimationEventHandler : MonoBehaviour, IAnimationEventHandler
{
    [SerializeField] Player player;
    [SerializeField] LayerMask enemyLayer;


    public void TriggerEnemyDeath()
    {
        player.attack.TriggerEnemyDeath();
    }

    public void PlayVFX(VisualEffect vfx)
    {
        vfx.Play();
    }

    public void ThrowWeapon(Throwable weapon)
    {
        weapon.Throw(transform.forward);
        player.attack.AttackType = 10;
    }

    public void ReturnWeapon(Throwable weapon)
    {
        weapon.GetComponent<Collider>().isTrigger = true;
        weapon.GetComponent<Rigidbody>().isKinematic = true;
        weapon.GetComponent<Rigidbody>().useGravity = false;

        //weapon.InstantReturn();
        weapon.SetLastValues();
        weapon.ReturnTrigger = true;

        player.attack.AttackType = 0;
    }

    public void GrabWeapon(GrabableWeapon weapon)
    {
        weapon.Pickup();
    }

    public void HolsterWeapon(GrabableWeapon weapon)
    {
        weapon.Drop(true);
    }

    public void TriggerParticleFXPlay(GameObject fxPrefab)
    {
        Vector3 defaultSpawnPoint = transform.position + transform.forward * 22.5f;
        GameObject fx = Instantiate(fxPrefab, defaultSpawnPoint, Quaternion.identity);
        Destroy(fx, 3f);
    }

    public void TriggerCloseRangeFXPlay(GameObject fxPrefab)
    {
        Vector3 defaultSpawnPoint = transform.position;// + transform.forward * 22.5f;
        GameObject fx = Instantiate(fxPrefab, defaultSpawnPoint, Quaternion.identity);
        Destroy(fx, 3f);
    }

    public void TriggerMidRangeFXPlay(GameObject fxPrefab)
    {
        Vector3 defaultSpawnPoint = transform.position + transform.forward * 18;
        //if(player.attack.Enemy != null)
        //{
        //    defaultSpawnPoint = player.attack.Enemy.transform.position;
        //}
        GameObject fx = Instantiate(fxPrefab, defaultSpawnPoint, Quaternion.identity);
        fx.transform.forward = transform.forward;
        var eulerAngles = fx.transform.eulerAngles;
        eulerAngles.y += fxPrefab.transform.eulerAngles.y;
        fx.transform.eulerAngles = eulerAngles;
        Destroy(fx, 3f);
    }

    public void SpawnProjectile(ProjectileSpawner spawner)
    {
        spawner.Spawn();
    }

    public void ReleaseProjectile(ProjectileSpawner spawner)
    {
        spawner.Release();
    }


    public void TriggerCutsceneAnimation()
    {
        GetComponent<Animator>().SetTrigger("Hit");
    }

    public void LookAtTarget(bool look) => player.attack.LookAtTargetDuringAttack = look;

    public void EnableCollisions(GameObject weapon)
    {
        weapon.GetComponent<Collider>().enabled = true;
    }

    public void DisableCollisions(GameObject weapon)
    {
        weapon.GetComponent<Collider>().enabled = false;
    }

    public void EnableLunge()
    {
        player.attack.Lunge = true;
    }

    public void DisableLunge()
    {
        player.attack.Lunge = false;
    }

    public void EnableSpeedVFX()
    {
        var trails = GetComponentsInChildren<TrailRenderer>();
        foreach(TrailRenderer trail in trails)
        {
            trail.enabled = true;
            trail.emitting = true;
        }
    }

    public void DisableSpeedVFX()
    {
        var trails = GetComponentsInChildren<TrailRenderer>();
        foreach(TrailRenderer trail in trails)
        {
            trail.emitting = false;
        }
    }

    public void EnableGroundDust()
    {

    }

    public void DisableGroundDust()
    {

    }
}
