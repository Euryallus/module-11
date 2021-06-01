using UnityEngine;

// BuildPointType: Determines which type of modular pieces should snap to a BuildPoint
public enum BuildPointType
{
    Floor,
    Wall,
    Stairs,
    RoofSide
}

// ||=======================================================================||
// || BuildPoint: A point that certain types of modular pieces will snap    ||
// ||   to (based on BuildPointType) to make the building process easier.   ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Construction/BuildPoint               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class BuildPoint : MonoBehaviour
{
    public BuildPointType BuildPointType { get { return buildPointType; } }

    [SerializeField] private BuildPointType buildPointType;     // The type of this build point, determines which pieces will snap to it
    [SerializeField] private Collider       buildPointCollider; // The collider to allow raycasts to hit the BuildPoint
    
    public void SetColliderEnabled(bool colliderEnabled)
    {
        // Enables/disables the collider
        buildPointCollider.enabled = colliderEnabled;
    }

    private void Start()
    {
        // Add to the list of all placed build points in the world on start
        WorldSave.Instance.AddPlacedBuildPoint(this);
    }

    private void OnDestroy()
    {
        // Remove from the list of all placed build points when destroyed
        WorldSave.Instance.RemovePlacedBuildPoint(this);
    }
}
