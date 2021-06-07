using UnityEngine;

// Module 11

public class FaceCamera : MonoBehaviour
{
    private Transform playerMainCameraTransform;

    private void Start()
    {
        playerMainCameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {    
        if(playerMainCameraTransform != null)
        {
            transform.LookAt(playerMainCameraTransform.position);
        }
    }
}