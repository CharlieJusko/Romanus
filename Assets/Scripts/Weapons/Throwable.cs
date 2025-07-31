using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    [Header("Starting Values (Set in MonoBehaviour Start())")]
    public Transform parent;
    public Vector3 startingPos;
    public Vector3 startingEuler;
    public Vector3 startingScale;

    [Space(5)]
    [Header("Throwable Values")]
    public float speed = 500f;
    public float returnTime = 0.75f;
    public LayerMask returnExclusionLayers;
    public LayerMask thrownExclusionLayers;
    public bool sticky = false;
    public LayerMask stickyLayers;
    public bool bouncy = false;
    public LayerMask bouncyLayers;
    //private List<Transform> bouncedOff = new List<Transform>();
    private Transform lastBounce;

    [Space(5)]
    [Header("Rotation")]
    public Vector3 thrownForward = Vector3.up;
    public Vector3 thrownUp = Vector3.forward;
    public bool rotate = false;
    public Vector3 rotateAround = Vector3.zero;
    public float rotationSpeed = 15f;
    

    [Space(5)]
    [Header("VFX")]
    public GameObject thrownFX; // TODO
    public TrailRenderer trail;
    public GameObject killFX;

    private float timeElapsed = 0f;
    private Vector3 lastPosition;
    private Vector3 lastEuler;
    private Vector3 travelDirection;

    public bool ReturnTrigger {  get; set; }

    void Start()
    {
        parent = transform.parent;
        startingPos = transform.localPosition;
        startingEuler = transform.localEulerAngles;
        startingScale = transform.localScale;
        if(trail != null)
        {
            trail.emitting = false;
        }
        GetComponent<Collider>().excludeLayers = returnExclusionLayers;
    }

    private void Update()
    {
        if(ReturnTrigger)
        {
            ReturnLerp();
        }

        if(!GetComponent<Collider>().isTrigger)
        {
            if(rotate)
            {
                transform.Rotate(rotateAround * (rotationSpeed * GetComponent<Rigidbody>().linearVelocity.normalized.magnitude * Time.deltaTime));
            }
        }
    }

    public void SetLastValues()
    {
        lastPosition = transform.position;
        lastEuler = transform.localEulerAngles;
    }

    public void Throw(Vector3 direction)
    {
        GetComponent<Collider>().excludeLayers = thrownExclusionLayers;
        ApplyForce(direction);
    }

    public void InstantReturn()
    {
        transform.parent = parent;
        transform.localPosition = startingPos;
        transform.localEulerAngles = startingEuler;
        transform.localScale = startingScale;

        timeElapsed = 0f;
        if(trail != null)
        {
            trail.emitting = false;
        }
        ReturnTrigger = false;
        GetComponent<Collider>().excludeLayers = returnExclusionLayers;
    }

    void ReturnLerp()
    {
        lastBounce = null;
        if(timeElapsed < returnTime)
        {
            Vector3 targetPos = parent.position;
            targetPos.x += startingPos.x;
            targetPos.y += startingPos.y;
            targetPos.z += startingPos.z;

            transform.position = Vector3.Lerp(lastPosition, targetPos, timeElapsed / returnTime);
            transform.eulerAngles = Vector3.Lerp(lastEuler, startingEuler, timeElapsed / returnTime);
            timeElapsed += Time.deltaTime;
        } 
        else
        {
            InstantReturn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if((bouncyLayers & (1 << collision.gameObject.layer)) != 0)
        {
            if(bouncy)
            {
                if(lastBounce != collision.transform)
                {
                    lastBounce = collision.transform;
                    // TODO: Fake the bounce angle to target near enemies
                    Vector3 bounceDirection = Vector3.Reflect(travelDirection, collision.contacts[0].normal);
                    if(bounceDirection.magnitude == 0)
                    {
                        bounceDirection = new Vector3(-GetComponent<Rigidbody>().linearVelocity.x, 0f, -GetComponent<Rigidbody>().linearVelocity.z).normalized;
                    }
                    bounceDirection.y = 0f;
                    ApplyForce(bounceDirection);
                }
            }
        }

        if((stickyLayers & (1 << collision.gameObject.layer)) != 0)
        {
            if(sticky)
            {
                Transform parent = collision.transform;
                Vector3 hitPos = transform.position;

                if(collision.gameObject.TryGetComponent(out SmartEnemy enemy))
                {
                    hitPos.y = enemy.transform.position.y + 9f;
                    parent = enemy.midSpine != null ? enemy.midSpine : enemy.GetClosestBone(collision.contacts[0].point);
                }

                Stick(parent, hitPos);
            }
        }
    }

    void ApplyForce(Vector3 direction)
    {
        travelDirection = direction.normalized;

        transform.parent = null;
        transform.forward = thrownForward;
        transform.up = thrownUp;
        if(trail != null)
        {
            trail.emitting = true;
        }

        GetComponent<Collider>().isTrigger = false;
        var rb = GetComponent<Rigidbody>();
        // Reset Velocity
        rb.isKinematic = true;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;

        rb.useGravity = true;
        rb.AddForce(direction * speed, ForceMode.Impulse);
    }

    void Stick(Transform parent, Vector3 position)
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        transform.position = position;
        transform.parent = parent;
    }
}
