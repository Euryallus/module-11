using UnityEngine;

// ||=======================================================================||
// || FaceCamera: Forces a GameObject to face in the direciton of the       ||
// ||   player, for a 'billboard' effect.                                   ||
// ||=======================================================================||
// || Used on various prefabs.  						                    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class FaceCamera : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera playerCamera = Camera.main;

        if (playerCamera != null)
        {
            // While using the main player camera, face towards it
            transform.forward = playerCamera.transform.forward;
        }
    }
}