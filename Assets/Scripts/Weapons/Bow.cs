using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [Header("Damage and Range")]
    public GameObject arrowPrefab;
    [SerializeField]
    protected float damage = 5f;
    [SerializeField]
    protected float range = 10f;
    [SerializeField]
    protected float spread = 2f;
    [SerializeField]
    protected Transform bulletSpawnPos;

    [Space(5)]
    [Header("VFX")]
    public GameObject grabbedArrow;


    private GameObject currentArrow;
    private bool shot = false;


    private void Start()
    {
        grabbedArrow.SetActive(false);
    }

    public void Shoot()
    {
        shot = true;
        Vector3 shootDirection = GetShootDirection();
        //currentArrow.GetComponent<Arrow>().Shoot(shootDirection);
    }

    protected Vector3 GetShootDirection()
    {
        Vector3 targetPos = bulletSpawnPos.position + bulletSpawnPos.forward * range;

        targetPos = new Vector3(
            targetPos.x + Random.Range(-spread, spread),
            bulletSpawnPos.position.y + Random.Range(-spread, spread),
            targetPos.z + Random.Range(-spread, spread)
        );

        Vector3 direction = targetPos - bulletSpawnPos.position;
        return direction.normalized;
    }

    #region Animation Event Handlers
    public void GrabArrow()
    {
        grabbedArrow.SetActive(true);
    }

    public void DrawArrow()
    {
        grabbedArrow.SetActive(false);
        shot = false;
        currentArrow = Instantiate(arrowPrefab, bulletSpawnPos.position, bulletSpawnPos.rotation, bulletSpawnPos);
        currentArrow.GetComponent<Rigidbody>().isKinematic = true;
        //currentArrow.GetComponent<Arrow>().damage = damage;
    }
    #endregion
}
