using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactDestructible : MonoBehaviour, IDestructible
{
    [Header("Base")]
    [SerializeField] bool destruct;
    [SerializeField] bool destructed = false;
    [SerializeField] GameObject vfxPrefab;
    public Vector3 vfxSpawnPointOffset;
    public LayerMask impactLayers;
    [Space(5)]
    [Header("GameObject Destroy")]
    public bool destroyOnDestruct = false;
    public float destoryTimer = 0f;
    [Space(5)]
    [Header("Decimation Parts")]
    public bool decimateOnDestruct = false;
    public Transform decimateParent;
    public Vector2 decimateMinMax = new Vector2();
    public MeshRenderer[] meshesToHide;
    public LayerMask partsIgnoreLayer;
    Vector3 hitPoint;
    Vector3 otherColliderPosition;

    public bool CanDestruct { get { return destruct; } set { destruct = value; } }
    public bool Destructed { get { return destructed; } set { destructed = value; } }
    public GameObject DesctructionVFXPrefab { get { return vfxPrefab; } set { vfxPrefab = value; } }

    public void Destruct()
    {
        if(destructed)
            return;

        destructed = true;

        if(decimateOnDestruct)
        {
            Decimate();
        }

        if(vfxPrefab != null)
        {
            Vector3 defaultSpawnPoint = transform.position + vfxSpawnPointOffset;
            GameObject fx = Instantiate(vfxPrefab, defaultSpawnPoint, Quaternion.identity);
            Destroy(fx, 3f);
        }

        if(destroyOnDestruct)
        {
            Destroy(gameObject, destoryTimer);
        }
    }

    void Decimate()
    {
        GetComponent<Collider>().enabled = false;
        //Vector3 explodePos = hitPoint;
        Vector3 forceDirection = hitPoint - otherColliderPosition;
        forceDirection.y = 0;

        foreach(MeshRenderer mr in meshesToHide)
            mr.enabled = false;

        //explodePos.y += transform.localScale.y / 2;
        Transform cellParent = decimateParent != null ? decimateParent : transform;
        foreach(Transform child in cellParent)
        {
            if(!child.gameObject.activeSelf)
                child.gameObject.SetActive(true);

            Rigidbody childRB = child.GetComponent<Rigidbody>();
            childRB.isKinematic = false;
            //childRB.AddExplosionForce(Random.Range(decimateMinMax.x, decimateMinMax.y), explodePos, 50f);
            childRB.AddForceAtPosition(forceDirection.normalized * Random.Range(decimateMinMax.x, decimateMinMax.y), hitPoint, ForceMode.Impulse);
            childRB.excludeLayers = partsIgnoreLayer;
            //childRB.AddForceAtPosition()
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if((impactLayers & (1 << collision.gameObject.layer)) != 0 && CanDestruct)
        {
            hitPoint = collision.contacts[0].point;
            otherColliderPosition = collision.collider.transform.position;
            Destruct();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if((impactLayers & (1 << other.gameObject.layer)) != 0)
        {
            hitPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            otherColliderPosition = other.transform.position;
            Destruct();
        }
    }
}
