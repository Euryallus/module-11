using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Acts as a controller for an array of particle effects - can allow groups of effects to be spawned and played at any point
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour


public class ParticleGroup: MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> systems = new List<ParticleSystem>(); // List of particle systems within the group

    // Returns status of effect (returns false if any system is still playing)
    public bool HasStopped()
    {
        // If any of the systems are still playing, return false (has not stopped)
        foreach(ParticleSystem sys in systems)
        {
            if(sys.isPlaying)
            {
                return false;
            }
        }

        // If no system is flagged as playing, return true (has stopped playing)
        return true;
    }

    // Prompts each system in the group to start playing
    public void PlayEffect()
    {
        foreach (ParticleSystem sys in systems)
        {
            sys.Play();
        }
    }
}
