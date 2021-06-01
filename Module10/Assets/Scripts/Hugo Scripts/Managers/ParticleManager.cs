using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Responsible for spawning and managing particle groups
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class ParticleManager : MonoBehaviour
{
                        public static ParticleManager Instance;                                     // Stores reference to self (allows access via ParticleManager.Instance.___() )

    [SerializeField]    private List<ParticleIndex> particleObjects = new List<ParticleIndex>();    // Stores array of index's (allows groups to be spawned & managed via name)
                        private List<GameObject> initialisedEffects = new List<GameObject>();       // List of refs to initialised groups to manage

    private void Awake()
    {
        // Ensure that an instance of the class does not already exist
        if (Instance == null)
        {
            //Set this class as the instance and ensure that it stays when changing scenes
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // If there is an existing instance that is not this, destroy the GameObject this script is connected to
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }


    
    void Update()
    {
        // Checks if any active effects have ended - if so, remove from list and delete
        for (int i = 0; i < initialisedEffects.Count; i++)
        {
            GameObject obj = initialisedEffects[i];
            if(obj.GetComponent<ParticleGroup>().HasStopped())
            {
                initialisedEffects.Remove(obj);
                Destroy(obj);

                i--;
            }
        }
    }

    public void SpawnParticle(Vector3 position, string name)
    {
        // Checks if name is present in list of effects
        foreach(ParticleIndex sys in particleObjects)
        {
            if(sys.sysName == name)
            {
                // If effect is present, create new instance of it at position given
                initialisedEffects.Add(Instantiate(sys.effect));
                initialisedEffects[initialisedEffects.Count - 1].transform.position = position;
            }
        }
    }
}
