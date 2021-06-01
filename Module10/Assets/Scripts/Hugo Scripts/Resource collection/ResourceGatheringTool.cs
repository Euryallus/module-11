using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Animation objects used when player destroys a Collectable Resource
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour
public class ResourceGatheringTool : MonoBehaviour
{
    public CollectableResource attachedResource;    // Ref. to parent CollectableResource
    public string swingSoundName = "whoosh";        // Name of sound that plays when tool is "swung" through the air
    public string hitSoundName;                     // Name of sound that plays when tool makes contact with resource
    public ParticleGroup particle;                  // Allows particles (e.g. splinters or sparks) to be played when tool makes contact

    public void StopSwinging()
    {
        // Called at end of anim
        // Allows player to move again
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().StartMoving();
        // Turns held tool's mesh renderer back on
        attachedResource.toolRenderer.enabled = true;
        // Flags resource as being hittable again
        attachedResource.canBeHit = true;
    }

    public void SwingEvents()
    {
        // Added by Joe Allen

        // Plays audio part way through animation (before makes contact)
        AudioManager.Instance.PlaySoundEffect3D(swingSoundName, transform.position);
    }

    public void ChopEvents()
    {
        // Added by Joe Allen, edited by Hugo Bailey

        // Run when tool makes contact with resource
        // Screenshakes to give impact
        GameObject.FindGameObjectWithTag("Player").GetComponent<CameraShake>().ShakeCameraForTime(0.3f, CameraShakeType.ReduceOverTime, 0.05f);
        // Plays hit sound
        AudioManager.Instance.PlaySoundEffect3D(hitSoundName, transform.position);
        // Plays particle effects
        particle.PlayEffect();
        // Checks to see if resource can be broken yet
        attachedResource.TryToDestroy();
    }
}
