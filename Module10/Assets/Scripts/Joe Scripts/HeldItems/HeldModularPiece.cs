using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ||=======================================================================||
// || HeldModularPiece: A visual indicator for the placement of modular     ||
// ||   pieces when the player is holding one as an item.                   ||
// ||=======================================================================||
// || Used on all prefabs in: HeldItems/Placeables/ModularPieces            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class HeldModularPiece : HeldPlaceableItem
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Modular Piece")]
    [SerializeField] private BuildPointType[] snapToPointTypes;

    #endregion

    private Coroutine setupBuildPointsCoroutine;    // Coroutine used to enable/disable certain BuildPoints based on the item being held

    private void OnDestroy()
    {
        // Ensure that setupBuildPointsCoroutine is stopped if it's
        //   still active while this modular piece is being destroyed 
        if (setupBuildPointsCoroutine != null)
        {
            Debug.Log("Stopping build point coroutine");
            StopCoroutine(setupBuildPointsCoroutine);
        }
    }

    public override void Setup(Item item, ContainerSlotUI containerSlot)
    {
        base.Setup(item, containerSlot);

        // Start setting up BuildPoints for this modular piece
        setupBuildPointsCoroutine = StartCoroutine(SetupBuildPointsCoroutine());
    }

    private IEnumerator SetupBuildPointsCoroutine()
    {
        // BuildPoints are essentially colliders used to snap certain types of modular
        //   pieces to specific positions/rotations relative to pieces that have been placed.
        //   This coroutine enables the colliders of all BuildPoints that are needed for the
        //   placement of the current modular piece type, and disables all others.

        List<BuildPoint> buildPoints = WorldSave.Instance.PlacedBuildPoints;

        Debug.Log("Setting up " + buildPoints.Count + " build points");

        int numEnabled = 0; // Keeps track of how many build points are enabled

        // Loop through all placed build points
        for (int i = 0; i < buildPoints.Count; i++)
        {
            if (snapToPointTypes.Contains(buildPoints[i].BuildPointType))
            {
                // This modular piece snaps to the current type of build point, enable the build point's collider
                buildPoints[i].SetColliderEnabled(true);

                // Increment the enabled counter
                numEnabled++;
            }
            else
            {
                // This modular piece does not snap to the current type of build point, disable the build point's collider
                buildPoints[i].SetColliderEnabled(false);
            }

            // Wait a frame each time 500 points have been looped through to the setup process
            //   doesn't cause a noticeable lag spike. Worlds with only a few placed pieces may never
            //   hit this, but if the player builds something huge the number of build points can get quite large
            if(i % 500 == 0)
            {
                yield return null;
            }
        }

        // The coroutine is done
        setupBuildPointsCoroutine = null;

        Debug.Log("Build point setup done, " + numEnabled + " were enabled");
    }

    protected override void CameraRaycastHit(RaycastHit hitInfo)
    {
        if (hitInfo.collider.CompareTag("BuildPoint") &&
            snapToPointTypes.Contains(hitInfo.collider.gameObject.GetComponent<BuildPoint>().BuildPointType))
        {
            // The raycast from the player's camera hit a BuildPoint that this piece should snap to

            // Set the piece to be placed at the BuildPoint's position
            placePos = hitInfo.collider.transform.position;

            if(!snapping)
            {
                // Force the piece's rotation to that of the BuildPoint. This only happens when snapping
                //   is first enabled so the player can still rotate more freely after the initial 'snap' rotation
                rotation = hitInfo.collider.transform.rotation.eulerAngles.y;
            }

            // The build position is in range and the piece is now snapping to a point
            SetInRange(true);
            SetSnapping(true);
        }
        else
        {
            // Something that is not a valid BuildPoint was hit, perhaps another piece or a random object in the world

            // Since the position is in range of the raycast, the placement position is valid
            placePos = hitInfo.point;

            // The build position is in range but the piece is not snapping to a point
            SetInRange(true);
            SetSnapping(false);
        }

        gameObject.transform.position = placePos;
    }

    protected override void CameraRaycastNoHit()
    {
        base.CameraRaycastNoHit();

        // The player camera's raycast hit nothing, the piece is definitely not snapping to a point
        SetSnapping(false);
    }
}