using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Used to allow resources to be gathered from sources that have multiple components (e.g. rocks which crack open when "mined")
// Development window:  Prototype phase
// Inherits from:       DestructableObject

public class CollectableResource : DestructableObject
{
    [SerializeField]    private GameObject[] enableOnDestruction;   // Array of objects to enable when resource is destroyed
    [SerializeField]    private GameObject[] disableOnDestruction;  // Array of objects to disable when resource is destroyed (e.g. static mesh elements replaced w/ rigidbodies)

    [SerializeField]    private GameObject toolHolder;              // GameObject containing tool animation (allows rotation of anim)
    [SerializeField]    private Animator toolAnimator;              // Tool animator ref.

    [SerializeField]    private Collider colliderDisableOnDestroy;  // Collider to disable when resource is broken (e.g. mesh collider for static element)

    [HideInInspector]   public bool canBeHit = true;                // Checks if resource can be mined (is false if animation is taking place)
                        public MeshRenderer toolRenderer;           // Ref. to mesh renderer of item in players hand (allows it to be hidden)
                        public bool toBeDestroyed = false;          // Bool allowing tool to run "destroy" function (can be called by AnimationEvent instead of TakeHit() )

    // Called when a held tool interacts with DestructableObject
    public override void TakeHit()
    {
        if(canBeHit)
        {
            canBeHit = false;
            // Rotates animation to "face" same direction as player (tool anim is visible from any angle)
            toolHolder.transform.forward = GameObject.FindGameObjectWithTag("Player").transform.forward;
            // Re-assigns tool renderer to ensure it can be enabled / disabled
            toolRenderer = GameObject.FindGameObjectWithTag("MainCamera").transform.GetChild(1).GetComponent<HeldTool>().toolRenderer;
            Debug.Log(toolRenderer.name);

            toolRenderer.enabled = false;
            // Stops player from moving while hitting resource
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().StopMoving();
            // Runs "swing" animation on tool
            toolAnimator.SetTrigger("Swing");

            base.TakeHit();
        }
    }

    // Instead of having this function destroy the resource, it flags it as being "ready" to be destroyed by animation event later
    public override void Destroyed()
    {
        toBeDestroyed = true;
    }

    // Run via animation event when tool strikes resource - replaces Destoryed()
    public virtual void TryToDestroy()
    {
        if(toBeDestroyed)
        {
            base.Destroyed();

            // Enables / Disables appropriate gameobjects
            foreach (GameObject obj in disableOnDestruction)
            {
                obj.SetActive(false);
            }
            foreach(GameObject obj in enableOnDestruction)
            {
                obj.SetActive(true);

                // If any objects being enabled are "fractured" (e.g. rock breaks into pieces) run Explode() on said object
                if(obj.GetComponent<FracturedObject>())
                {
                    obj.GetComponent<FracturedObject>().Explode();
                }
            }

            // If a collider has been assigned to colliderDisableOnDestroy, disable said collider
            if(colliderDisableOnDestroy != null)
            {
                colliderDisableOnDestroy.enabled = false;
            }
        }
    }
}

