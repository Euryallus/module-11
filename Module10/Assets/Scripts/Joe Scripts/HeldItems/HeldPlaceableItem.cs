using UnityEngine;

// ||=======================================================================||
// || HeldPlaceableItem: A base class for all held items that act as a      ||
// ||   visual indicator for an object that can be placed in the world.     ||
// ||=======================================================================||
// || Used on prefabs: HeldItems/Placeables/BuildPreview_CraftingTable      ||
// ||                  HeldItems/Placeables/BuildPreview_CustomisingTable   ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class HeldPlaceableItem : HeldItem
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Placeable Item")]

    [SerializeField]
    private GameObject itemPrefab;                                      // The GameObject prefab to be spawned when the item is placed

    [SerializeField]
    private Color standardColour = new Color(0.0f, 0.670f, 0.670f);     // The base colour of the material to use when object placement is valid

    [SerializeField] [ColorUsage(false, true)]
    private Color standardEmissive = new Color(0.0f, 0.125f, 0.125f);   // The emissive colour of the material to use when object placement is valid

    [SerializeField]
    private Color warningColour = new Color(0.707f, 0.0f, 0.0f);        // The base colour of the material to use when object placement is invalid

    [SerializeField] [ColorUsage(false, true)]
    private Color warningEmissive = new Color(0.125f, 0.0f, 0.0f);      // The emissive colour of the material to use when object placement is invalid

    [SerializeField] protected float    maxPlaceDistance = 10.0f;       // The maximum valid distance from the player that an item can be placed at

    [SerializeField] private string     placementSound;                 // Sound to be played when the object is placed

    [SerializeField] private Collider   mainCollider;                   // Collider to use for the detection of valid placement by default

    [SerializeField] private Collider   snapCollider;                   // Collider to use for the detection of valid placement when the item is snapping to a point
                                                                        //   (snap colliders are usually more generous to allow certain behaviour such as walls
                                                                        //   that snap together while overlapping slightly at the corners)

    #endregion

    protected bool colliding;   // Whether the held GameObject is currently colliding with anything (and hence cannot be placed)
    protected bool inRange;     // Whether the held GameObject is closer to the player than maxPlaceDistance
    protected bool snapping;    // Whether the held GameObject is snapping to a point

    protected CameraShake playerCameraShake;                      // The script for shaking the player's camera on placement

    protected Vector3     placePos;                               // The position that the item will be placed at
    protected float       rotation;                               // The current exact rotation of the item
    private float         angleInterval = DefaultAngleInterval;   // How much the rotation angle is incremented for each key press
    protected float       visualRotation;                         // The rotation of item GameObject, which not be exactly equal to rotation
                                                                  //   as items visually move smoothly towards the target rotation


    const float DefaultAngleInterval    = 30.0f;                  // Default angle incrementation when a piece is not snapping
    const float SnapAngleInterval       = 90.0f;                  // Angle incrementation when a piece is snapping to a point


    protected override void Awake()
    {
        base.Awake();

        // Get a reference to the camera shake script on the player
        playerCameraShake = playerTransform.GetComponent<CameraShake>();
    }

    private void Start()
    {
        // Enable the main collider and disable the snap collider since placeable items aren't snapping by default
        if(snapCollider != null)
        {
            snapCollider.enabled = false;
        }
        mainCollider.enabled = true;
    }

    protected virtual void Update()
    {
        // Rotate the GameObject about the y-axis based on visualRotation
        gameObject.transform.rotation = Quaternion.Euler(0.0f, visualRotation, 0.0f);

        // Layer mask to use for raycast (see below)
        LayerMask layerMask = ~(LayerMask.GetMask("BuildPreview") | LayerMask.GetMask("Ignore Raycast"));

        // Send out a raycast from the player's camera that ignores object on the layers: BuildPreview and Ignore Raycast

        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit hitInfo, maxPlaceDistance, layerMask))
        {
            // The raycast hit something
            CameraRaycastHit(hitInfo);
        }
        else
        {
            // The raycast hit nothing
            CameraRaycastNoHit();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            // Rotate anti-clockwise by the set angle interval when Z is pressed
            rotation -= angleInterval;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Rotate clockwise by the set angle interval when X is pressed
            rotation += angleInterval;
        }

        // Linearly interpolate the visual rotation towards the target rotation, using time as the 't' value for smooth 'ease out' movement
        visualRotation = Mathf.Lerp(visualRotation, rotation, Time.deltaTime * 40.0f);
    }

    protected virtual void CameraRaycastHit(RaycastHit hitInfo)
    {
        // The raycast (which used maxPlaceDistance as a parameter) hit something, so the point of placement is in range
        SetInRange(true);

        // Set the placement point to the hit point of the raycast
        placePos = hitInfo.point;

        // Move the GameObject to the placement point
        gameObject.transform.position = placePos;
    }

    protected virtual void CameraRaycastNoHit()
    {
        // The raycast hit nothing, so the placement point is out of range
        SetInRange(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Another object entered the active trigger, the preview GameObject is colliding
        SetColliding(true);
    }

    private void OnTriggerStay(Collider other)
    {
        // Another object is inside the active trigger, the preview GameObject is colliding
        SetColliding(true);
    }

    private void OnTriggerExit(Collider other)
    {
        // An object left the active trigger, the preview GameObject is no longer colliding
        SetColliding(false);
    }

    protected void SetColliding(bool colliding)
    {
        // Set colliding and update placement state to reflect the new value
        this.colliding = colliding;

        UpdatePlacementState();
    }

    protected void SetInRange(bool inRange)
    {
        // Set whether this object is in range and update placement state to reflect the new value
        this.inRange = inRange;

        UpdatePlacementState();
    }

    protected void SetSnapping(bool snapping)
    {
        if(snapping != this.snapping)
        {
            // Only update snapping if the value changed to prevent colliders constantly being enabled/disabled

            this.snapping = snapping;

            // Set the angle interval based on whether the object is currently snapping
            angleInterval = snapping ? SnapAngleInterval : DefaultAngleInterval;

            // Enable/disable main and snapping colliders based on the new value

            mainCollider.enabled = !snapping;

            if(snapCollider != null)
            {
                snapCollider.enabled = snapping;
            }

            // Set colliding to false - this is done because by default, if the snap collider is enabled and is not colliding
            //   while the main one was colliding previously, colliding would incorrectly remain as true because Unity does
            //   not recalculate collisions on enable/disable by default
            colliding = false;

            // Update placement state to reflect changes
            UpdatePlacementState();
        }
    }

    protected void UpdatePlacementState()
    {
        Color baseColour;
        Color emissiveColour;

        if (!colliding && inRange)
        {
            // The object is not colliding and is in range - placement is valid, use default colours
            baseColour      = standardColour;
            emissiveColour  = standardEmissive;
        }
        else
        {
            // The object is colliding or out of range - placement is invalid, use warning colours
            baseColour      = warningColour;
            emissiveColour  = warningEmissive;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            //Set all build preview material colours to the colours set above

            if (transform.GetChild(i).CompareTag("BuildPreviewMaterial"))
            {
                transform.GetChild(i).GetComponent<MeshRenderer>().materials[0].SetColor("_BaseColor",     baseColour);
                transform.GetChild(i).GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", emissiveColour);
            }
        }
    }

    public override void PerformMainAbility()
    {
        base.PerformMainAbility();

        if(Cursor.lockState == CursorLockMode.Locked)
        {
            // Items can only be placed if the cursor is locked, i.e. the player is not in a menu

            if (!colliding && inRange)
            {
                // Placement is valid, build the item
                PlaceItem();
            }
            else
            {
                // Placement is invalid, notify the player
                NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.ItemCannotBePlaced);
            }
        }
    }

    protected virtual GameObject PlaceItem()
    {
        if(!string.IsNullOrEmpty(placementSound))
        {
            // Play the placement sound (if one was set) at the position of placement
            AudioManager.Instance.PlaySoundEffect3D(placementSound, transform.position);
        }

        // Tiny offset to help prevent z-fighting
        float randomOffset = Random.Range(0.0f, 0.001f);
        Vector3 offsetPlacePos = new Vector3(placePos.x + randomOffset, placePos.y + randomOffset, placePos.z + randomOffset);

        // Shake the player camera slightly
        playerCameraShake.ShakeCameraForTime(0.2f, CameraShakeType.ReduceOverTime, 0.02f);

        // Instantiate the placed object
        GameObject placedGameObj = Instantiate(itemPrefab, offsetPlacePos, Quaternion.Euler(0.0f, rotation, 0.0f));

        // Setup the PlaceableDestructible script if the object has one
        PlaceableDestructible placeableDestructible = placedGameObj.GetComponent<PlaceableDestructible>();

        if(placeableDestructible != null)
        {
            placeableDestructible.SetupAsPlacedObject();
        }

        // Remove the item from the player's hotbar so one item cannot be used multiple times
        RemoveItemFromHotbar();

        return placedGameObj;
    }

    protected virtual void RemoveItemFromHotbar()
    {
        // Remove the item from the player's hotbar
        HotbarPanel hotbar = GameObject.FindGameObjectWithTag("Hotbar").GetComponent<HotbarPanel>();
        hotbar.RemoveItemFromHotbar(item);
    }
}
