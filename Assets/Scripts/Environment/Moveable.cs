using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour, IMoveable
{
    [SerializeField] bool canMove;
    [SerializeField] float speed;
    Vector3 direction;
    [Range(0f, 1f)]
    public float hitDirToBackwardLerp = 0.5f;
    public float hitDirY = 0.25f;


    public float Speed { get => speed; set => speed = value; }
    public Vector3 Direction { get => direction; set => direction = value; }
    public bool CanMove { get => canMove; set => canMove = value; }

    public void Move()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.AddForce(direction * speed, ForceMode.Impulse);
    }

    public void Stop()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.collider;
        if(other.CompareTag("Weapon") && CanMove)
        {
            Vector3 contactPoint = collision.contacts[0].point;
            WeaponContact(other, contactPoint);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Weapon") && CanMove)
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            WeaponContact(other, contactPoint);
        }
    }

    void WeaponContact(Collider other, Vector3 contactPoint)
    {
        Vector3 hitDir = (contactPoint - other.transform.root.position);
        Vector3 backward = other.transform.root.forward;
        Vector3 moveDirection = Vector3.Lerp(hitDir.normalized, backward, hitDirToBackwardLerp);
        moveDirection.y = hitDirY;
        direction = moveDirection;
        Move();
    }
}
