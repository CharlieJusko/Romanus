using UnityEngine;

public class Damageable : MonoBehaviour, IDamageable
{
    [SerializeField] private float health = 10f;


    public float Health {  get { return health; } set {  health = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {

    }
}
