using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabableWeapon : MonoBehaviour
{
    public Transform targetHand;
    public Vector3 targetPosition;
    public Vector3 targetEuler;
    public Vector3 targetScale;

    private Transform startingParent;
    private Vector3 startingPosition;
    private Vector3 startingEuler;
    private Vector3 startingScale;

    [Space(5)]
    [SerializeField] TrailRenderer bulletTrail;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] float fadeTime = 1f;

    private void Start()
    {
        startingParent = GetComponent<Transform>().parent;
        startingPosition = transform.localPosition;
        startingEuler = transform.localEulerAngles;
        startingScale = transform.localScale;
    }

    public void Pickup()
    {
        transform.parent = targetHand;
        transform.localPosition = targetPosition;
        transform.localEulerAngles = targetEuler;
        transform.localScale = targetScale;
    }

    public void Drop(bool reparent=true)
    {
        if(reparent)
        {
            transform.parent = startingParent;
            transform.localPosition = startingPosition;
            transform.localEulerAngles = startingEuler;
            transform.localScale = startingScale;
        }
    }

    public void CreateBulletTrailLaser(Vector3 endPos, bool hitSomething)
    {
        TrailRenderer trail = Instantiate(bulletTrail, bulletSpawn.position, Quaternion.identity);
        StartCoroutine(FadeTrail(trail, endPos, hitSomething));
    }

    IEnumerator FadeTrail(TrailRenderer trail, Vector3 endPos, bool hitSomething)
    {
        float timer = 0;
        Vector3 startPos = trail.transform.position;
        while(timer < fadeTime)
        {
            trail.transform.position = Vector3.Lerp(startPos, endPos, timer);
            timer += Time.deltaTime / trail.time;
            yield return null;
        }

        //if(hitSomething)
        //{
        //    var impact = Instantiate(impactEffect, endPos, Quaternion.identity);
        //    StartCoroutine(DestroyImpact(impact, 0.75f));
        //}

        Destroy(trail.gameObject, trail.time);
    }
}
