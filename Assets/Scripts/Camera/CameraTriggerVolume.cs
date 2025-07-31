using Unity.Cinemachine;
using UnityEngine;


[RequireComponent (typeof(BoxCollider))]
public class CameraTriggerVolume : MonoBehaviour
{
    public CinemachineCamera virtualCam;
    public int defaultPriority = 10;
    private CinemachineBrain brain;

    private void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
        defaultPriority = virtualCam.Priority;
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(brain.ActiveVirtualCamera != (ICinemachineCamera)virtualCam)
            {
                virtualCam.gameObject.SetActive(true);
                virtualCam.Prioritize();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(brain.ActiveVirtualCamera == (ICinemachineCamera)virtualCam)
                virtualCam.gameObject.SetActive(false);
        }
    }
}
