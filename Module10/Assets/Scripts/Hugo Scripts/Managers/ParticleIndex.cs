using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Used to index particle groups & the gameobjects associated
// Development window:  Prototype phase
// Inherits from:       N/A

[System.Serializable]
public class ParticleIndex
{
    public string sysName;      // Name (or ID) associated w/ the effect
    public GameObject effect;   // Prefab containing effects (with ParticleGroup component)
    public bool loop;           // Whether effect should loop (currently unused)
}
