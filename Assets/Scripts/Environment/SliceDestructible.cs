using UnityEngine;

public class SliceDestructible : MonoBehaviour, IDestructible
{
    enum SliceType
    {
        Vertical,
        Horizontal
    }

    [Header("Base")]
    [SerializeField] bool destruct;
    [SerializeField] bool destructed = false;
    [SerializeField] GameObject vfxPrefab;
    public Vector3 vfxSpawnPointOffset;
    public bool destroyOnDestruct = false;
    public float destoryTimer = 0f;

    [Space(5)]
    [Header("Sliced Parts")]
    [SerializeField] GameObject topHalf;
    [SerializeField] Transform topHalfSpawnPoint;
    [SerializeField] GameObject bottomHalf;
    [SerializeField] Transform bottomHalfSpawnPoint;

    [SerializeField] GameObject leftHalf;
    [SerializeField] Transform leftHalfSpawnPoint;
    [SerializeField] GameObject rightHalf;
    [SerializeField] Transform rightHalfSpawnPoint;

    public MeshRenderer[] meshesToHide;

    [Space(5)]
    [Header("Physics")]
    public bool addSplitForce = true;
    public float spltForce = 10f;

    Vector3 enterHitPos;
    Vector3 exitHitPoint;
    Vector3 hitDirection;
    SliceType sliceType;


    public bool CanDestruct { get { return destruct; } set { destruct = value; } }
    public bool Destructed { get { return destructed; } set { destructed = value; } }
    public GameObject DesctructionVFXPrefab { get { return vfxPrefab; } set { vfxPrefab = value; } }

    public void Destruct()
    {
        if(destructed)
            return;

        if(vfxPrefab != null)
        {
            Vector3 defaultSpawnPoint = transform.position + vfxSpawnPointOffset;
            GameObject fx = Instantiate(vfxPrefab, defaultSpawnPoint, Quaternion.identity);
            Destroy(fx, 3f);
        }

        GetComponent<Collider>().enabled = false;
        Vector3 forceDirection = exitHitPoint;
        forceDirection.y = 0.25f;

        foreach(MeshRenderer mr in meshesToHide)
            mr.enabled = false;

        GameObject half1;
        GameObject half2;
        if(sliceType == SliceType.Vertical)
        {
            half1 = Instantiate(leftHalf, leftHalfSpawnPoint.position, leftHalf.transform.rotation, transform);
            half2 = Instantiate(rightHalf, rightHalfSpawnPoint.position, rightHalf.transform.rotation, transform);
        }
        else
        {
            half1 = Instantiate(topHalf, topHalfSpawnPoint.position, topHalf.transform.rotation, transform);
            half2 = Instantiate(bottomHalf, bottomHalfSpawnPoint.position, bottomHalf.transform.rotation, transform);
        }

        
        if(addSplitForce)
        {
            half1.GetComponent<Rigidbody>().AddForceAtPosition(forceDirection.normalized * spltForce, enterHitPos, ForceMode.Impulse);
            half2.GetComponent<Rigidbody>().AddForceAtPosition(forceDirection.normalized * spltForce, enterHitPos, ForceMode.Impulse);
        }

        if(destroyOnDestruct)
        {
            Destroy(gameObject, destoryTimer);
        }
    }

    void CalcluateSliceDirectionMagnitude()
    {
        float hitX = Mathf.Abs(hitDirection.x);
        float hitY = Mathf.Abs(hitDirection.y);
        float hitZ = Mathf.Abs(hitDirection.z);
        sliceType = (hitY > hitX || hitY > hitZ) ? SliceType.Vertical : SliceType.Horizontal;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Weapon"))
        {
            enterHitPos = collision.contacts[0].point;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Weapon"))
        {
            exitHitPoint = collision.contacts[0].point;
            hitDirection = (exitHitPoint - enterHitPos).normalized;
            CalcluateSliceDirectionMagnitude();
            Destruct();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Weapon"))
        {
            enterHitPos = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Weapon"))
        {
            exitHitPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            hitDirection = (exitHitPoint - enterHitPos).normalized;
            CalcluateSliceDirectionMagnitude();
            Destruct();
        }
    }
}
