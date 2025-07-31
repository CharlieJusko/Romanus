using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
	public float smoothing = 5f;
	public LayerMask cullLayers;
	Vector3 offset;

	// Use this for initialization
	void Start() {
		offset = transform.position - player.position;
	}

    private void Update()
    {
		IsometricCulling();
    }

    // Update is called once per frame
    void FixedUpdate() {
		Vector3 targetCamPos = player.position + offset;
		transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
	}

	void IsometricCulling()
	{
        Ray ray = new Ray(transform.position, (player.transform.position - transform.position).normalized);
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cullLayers))
		{
			if(hit.collider.CompareTag("Player"))
			{
				print("Player!");
			} 
			else
			{
				print("Cull! " + hit.collider.transform.name);
			}
		}
    }
}
