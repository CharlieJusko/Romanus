using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
{
    public bool killOnCollision = true;
    [Header("Rotation")]
    public bool rotate = false;
    public float rotateAmount = 45;
    [Space(5)]
    [Header("Time")]
    public bool timed = false;
    public float maxAliveTime = 5f;
    [Space(5)]
    public float range = 100;
    [Space(5)]
    [Header("Speed")]
    public float speed = 25f;
    [Space(5)]
    [Range(0f, 1f)]
    public float accuracy;
    [Space(5)]
    [Header("Additional FX")]
    public GameObject muzzleVFXPrefab;
    public GameObject impactVFXPrefab;
    public List<GameObject> trails;
    public Vector3 hitFXSpawnOffset = Vector3.zero;

    private Vector3 startPosition;
    private Vector3 offset;
    private bool collided;
    private Rigidbody rb;
    private float timeAlive = 0f;

    public Firearm FiredFrom { get; set; }


    // At Spawn
    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();

        //used to create a radius for the accuracy and have a very unique randomness
        if(accuracy < 1f)
        {
            for(int i = 0; i < 2; i++)
            {
                float calculatedAccuracy = 1 - accuracy;
                var val = Random.Range(-calculatedAccuracy, calculatedAccuracy);
                var index = Random.Range(0, 2);
                if(i == 0)
                {
                    if(index == 0)
                        offset = new Vector3(0, -val, 0);
                    else
                        offset = new Vector3(0, val, 0);
                } 
                else
                {
                    if(index == 0)
                        offset = new Vector3(0, offset.y, -val);
                    else
                        offset = new Vector3(0, offset.y, val);
                }
            }
        }

        if(muzzleVFXPrefab != null)
        {
            var muzzleVFX = Instantiate(muzzleVFXPrefab, transform.position, Quaternion.identity);
            muzzleVFX.transform.forward = transform.forward + offset;
            var ps = muzzleVFX.GetComponent<ParticleSystem>();
            if(ps != null)
                Destroy(muzzleVFX, ps.main.duration);
            else
            {
                var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzleVFX, psChild.main.duration);
            }
        }
    }

    void FixedUpdate()
    {
        if(rotate)
            transform.Rotate(0, 0, rotateAmount, Space.Self);
        if(speed != 0 && rb != null)
            rb.position += (transform.forward + offset) * (speed * Time.deltaTime);
        if(timed)
        {
            AliveTimer();
            if(timeAlive >= maxAliveTime)
            {
                Vector3 spawnPoint = transform.position;
                spawnPoint += hitFXSpawnOffset;
                HitAndDestroy(spawnPoint, Quaternion.identity);
            }
        }

        if(Vector3.Distance(transform.position, startPosition) >= range)
            StartCoroutine(DestroyParticle(0f));
    }

    void HitAndDestroy(Vector3 spawnPoint, Quaternion spawnRotation)
    {
        SpawnHitFX(spawnPoint, spawnRotation);
        StartCoroutine(DestroyParticle(0f));
    }

    void SpawnHitFX(Vector3 spawnPoint, Quaternion spawnRotation)
    {
        if(impactVFXPrefab != null)
        {
            var hitVFX = Instantiate(impactVFXPrefab, spawnPoint, transform.rotation);
            //var ps = hitVFX.GetComponent<ParticleSystem>();

            if(hitVFX.TryGetComponent(out ParticleSystem ps))
            {
                Destroy(hitVFX, ps.main.duration);
            } 
            else if(hitVFX.TryGetComponent(out VisualEffect vfx))
            {
                Destroy(hitVFX, vfx.GetFloat("MaxLifetime"));
            }
            else
            {
                if(hitVFX.transform.GetChild(0).TryGetComponent(out ParticleSystem psChild))
                    Destroy(hitVFX, psChild.main.duration);
                else
                    Destroy(hitVFX, 1f);
            }
        }
    }

    public IEnumerator DestroyParticle(float waitTime)
    {

        if(transform.childCount > 0 && waitTime != 0)
        {
            List<Transform> tList = new List<Transform>();

            foreach(Transform t in transform.GetChild(0).transform)
            {
                tList.Add(t);
            }

            while(transform.GetChild(0).localScale.x > 0)
            {
                yield return new WaitForSeconds(0.01f);
                transform.GetChild(0).localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                for(int i = 0; i < tList.Count; i++)
                {
                    tList[i].localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
        }

        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }

    void AliveTimer()
    {
        if(timeAlive <= maxAliveTime)
        {
            timeAlive += Time.deltaTime;
        } else
        {
            timeAlive = maxAliveTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 pos = contact.point;

        if(collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(FiredFrom.Damage, parry:true);
        }

        if(killOnCollision)
        {
            if(!collision.gameObject.CompareTag("Projectile") && !collided)
            {
                collided = true;

                if(trails.Count > 0)
                {
                    for(int i = 0; i < trails.Count; i++)
                    {
                        trails[i].transform.parent = null;
                        var ps = trails[i].GetComponent<ParticleSystem>();
                        if(ps != null)
                        {
                            ps.Stop();
                            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
                        }
                    }
                }

                speed = 0;
                GetComponent<Rigidbody>().isKinematic = true;

                Quaternion rot = Quaternion.FromToRotation(-transform.forward, contact.normal);
                HitAndDestroy(pos, rot);
            }
        } 
        else
        {
            Quaternion rot = Quaternion.FromToRotation(-transform.forward, contact.normal);
            SpawnHitFX(pos, rot);
        }
    }
}
