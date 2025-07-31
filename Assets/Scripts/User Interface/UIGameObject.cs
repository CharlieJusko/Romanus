using UnityEngine;

public class UIGameObject : MonoBehaviour
{
    public Canvas canvas;
    public Transform target;


    private void Awake()
    {
        canvas = FindFirstObjectByType<Canvas>();
    }

    private void Start()
    {
        if(target != null)
            transform.SetParent(canvas.transform);
    }

    private void FixedUpdate()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        if(target != null)
            transform.position = Camera.main.WorldToScreenPoint(target.position);
    }
}
