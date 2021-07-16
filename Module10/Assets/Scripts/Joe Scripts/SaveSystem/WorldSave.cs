using System;
using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || WorldSave: Handles save data for all objects placed in the world by   ||
// ||   the player, as well as the last save point used by the player.      ||
// ||=======================================================================||
// || Used on prefab: Joe/SaveSystem/WorldSave                              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class WorldSave : MonoBehaviour, IPersistentSceneObject
{
    public static WorldSave Instance;

    #region InspectorVariables
    // Variables in this region are set in the inspector. See tooltips for more info.

    // Prefabs to be instantiated when certain placeable objects are loaded into the world:
    [SerializeField] private GameObject signpostPrefab;
    [SerializeField] private GameObject modularWoodFloorPrefab;
    [SerializeField] private GameObject modularWoodWallPrefab;
    [SerializeField] private GameObject modularWoodHalfWallPrefab;
    [SerializeField] private GameObject modularWoodRoofPrefab;
    [SerializeField] private GameObject modularWoodStairsPrefab;
    [SerializeField] private GameObject craftingTablePrefab;
    [SerializeField] private GameObject customisingTablePrefab;
    [SerializeField] private GameObject itemStackPickupPrefab;

    #endregion

    #region Properties

    public string           UsedSceneSavePointId    { set { usedSceneSavePointId = value; } }
    public List<BuildPoint> PlacedBuildPoints       { get { return placedBuildPoints; } }

    #endregion

    private string usedSceneSavePointId;     // Id of the last save point that was used in the current scene, which will be used when respawning the player next time the scene is loaded

    private List<IPersistentPlacedObject>   placedObjectsToSave;    // All player-placed objects that should be saved with the world
    private List<BuildPoint>                placedBuildPoints;      // All build points added to the world when modular pieces were placed

    private void Awake()
    {
        //Ensure that an instance of the class does not already exist
        if (Instance == null)
        {
            //Set this class as the instance and ensure that it stays when changing scenes
            Instance = this;
        }
        //If there is an existing instance that is not this, destroy the GameObject this script is connected to
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        placedObjectsToSave = new List<IPersistentPlacedObject>();
        placedBuildPoints   = new List<BuildPoint>();
    }

    protected void Start()
    {
        // Subscribe to save/load events so all world data is saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events if the WorldSave GameObject is destroyed to prevent null reference errors
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    public void OnSceneSave(SaveData saveData)
    {
        Debug.Log("Saving world save");

        if (!string.IsNullOrEmpty(usedSceneSavePointId))
        {
            // Save the id of the last used save point
            saveData.AddData("usedSavePointId", usedSceneSavePointId);
        }
        else
        {
            // No save point id set, this should not happen unless forcing a save with debug tools
            Debug.LogWarning("Saving without setting a UsedSavePointId!");
        }

        // Also add save data for all objects placed in the world by the player
        for (int i = 0; i < placedObjectsToSave.Count; i++)
        {
            placedObjectsToSave[i].AddDataToWorldSave(saveData);
        }
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        Debug.Log("Loading world save");

        // Get the id of the save point to respawn the player at
        usedSceneSavePointId = saveData.GetData<string>("usedSavePointId");
    }

    public void OnSceneLoadConfigure(SaveData saveData)
    {
        // Load all objects placed by the player
        LoadPlayerPlacedObjects(saveData.GetSaveDataEntries());
    }

    private void LoadPlayerPlacedObjects(Dictionary<string, object> saveDataEntries)
    {

        // Base ids of all player placed objects that can be loaded:
        string[]    idsToLoad       = new string[] { "sign", "modularPiece", "craftingTable", "itemStackPickup" };

        bool        loadingObjects  = true;         // Whether objects are currently being loaded
        int         idToLoadIndex   = 0;            // Index for the id of the object type currently being load in the above idsToLoad array
        string      currentIdToLoad = idsToLoad[0]; // Id of the object type currently being loaded

        while (loadingObjects)
        {
            // When player-placed objects are saved, the '*' symbol is added to their id so each one is unique.
            //   As such, in order to load every instance of a certain object, the symbol will continue to be added to
            //   the id until an object with [currentIdToLoad] can no longer be found in the saveDataEntries dictionary.
            currentIdToLoad += "*";

            if (saveDataEntries.ContainsKey(currentIdToLoad))
            {
                // Element with currentIdToLoad found
                var currentElement = saveDataEntries[currentIdToLoad];

                // Load an object into the world based on the id
                switch (idsToLoad[idToLoadIndex])
                {
                    case "sign":
                        LoadPlacedSignpost(currentElement as SignpostSaveData);
                        break;

                    case "modularPiece":
                        LoadPlacedModularPiece(currentElement as ModularPieceSaveData);
                        break;

                    case "craftingTable":
                        LoadPlacedCraftingTable(currentElement as TransformSaveData);
                        break;

                    case "customisingTable":
                        LoadPlacedCustomisingTable(currentElement as TransformSaveData);
                        break;

                    case "itemStackPickup":
                        LoadItemStackPickup(currentElement as ItemStackPickupSaveData);
                        break;
                }
            }
            else
            {
                // All objects with the current id type were loaded

                if (idToLoadIndex < (idsToLoad.Length - 1))
                {
                    // End of array not reached, load the next object type
                    idToLoadIndex++;
                    currentIdToLoad = idsToLoad[idToLoadIndex];
                }
                else
                {
                    // All objects have been loaded
                    loadingObjects = false;
                }
            }
        }
    }

    private void LoadPlacedSignpost(SignpostSaveData data)
    {
        // Instantiate the signpost GameObject with the saved position/rotation
        GameObject signGameObj = Instantiate(signpostPrefab, new Vector3(data.Position[0], data.Position[1], data.Position[2]),
                            Quaternion.Euler(data.Rotation[0], data.Rotation[1], data.Rotation[2]));

        // Setup the signpost script with the loaded RelatedItemId (id of the item used when placing the signpost)

        Signpost signpostScript = signGameObj.GetComponent<Signpost>();

        signpostScript.SetupAsPlacedObject();
        signpostScript.SetRelatedItem(data.RelatedItemId);
    }

    private void LoadPlacedModularPiece(ModularPieceSaveData data)
    {
        GameObject prefabToSpawn = null;

        // Select which prefab to spawn based on the loaded ModularPieceType
        switch (data.PieceType)
        {
            case ModularPieceType.WoodFloor:
                prefabToSpawn = modularWoodFloorPrefab; break;

            case ModularPieceType.WoodWall:
                prefabToSpawn = modularWoodWallPrefab; break;

            case ModularPieceType.WoodHalfWall:
                prefabToSpawn = modularWoodHalfWallPrefab; break;

            case ModularPieceType.WoodRoofSide:
                prefabToSpawn = modularWoodRoofPrefab; break;

            case ModularPieceType.WoodStairs:
                prefabToSpawn = modularWoodStairsPrefab; break;
        }

        if (prefabToSpawn != null)
        {
            // Instantiate the modular piece GameObject with the saved position/rotation
            Instantiate(prefabToSpawn, new Vector3(data.Position[0], data.Position[1], data.Position[2]),
                            Quaternion.Euler(data.Rotation[0], data.Rotation[1], data.Rotation[2]));
        }
        else
        {
            // Error: No prefab for the PieceType
            Debug.LogWarning("Trying to load modular piece with unknown prefab: " + data.PieceType);
        }
    }

    private void LoadPlacedCraftingTable(TransformSaveData data)
    {
        // Instantiate the crafting table GameObject with the saved position/rotation
        CraftingTable craftingTable = Instantiate(craftingTablePrefab, new Vector3(data.Position[0], data.Position[1], data.Position[2]),
                                            Quaternion.Euler(data.Rotation[0], data.Rotation[1], data.Rotation[2])).GetComponent<CraftingTable>();

        // Setup the crafting table script
        craftingTable.SetupAsPlacedObject();
    }
    
    private void LoadPlacedCustomisingTable(TransformSaveData data)
    {
        // Instantiate the customising table GameObject with the saved position/rotation
        CustomisingTable customisingTable = Instantiate(customisingTablePrefab, new Vector3(data.Position[0], data.Position[1], data.Position[2]),
                                                Quaternion.Euler(data.Rotation[0], data.Rotation[1], data.Rotation[2])).GetComponent<CustomisingTable>();

        // Setup the customising table script
        customisingTable.SetupAsPlacedObject();
    }

    private void LoadItemStackPickup(ItemStackPickupSaveData data)
    {
        // Instantiate the item stack GameObject with the saved position/rotation
        ItemStackPickup itemStackPickup = Instantiate(itemStackPickupPrefab, new Vector3(data.Position[0], data.Position[1], data.Position[2]),
                                                Quaternion.Euler(data.Rotation[0], data.Rotation[1], data.Rotation[2])).GetComponent<ItemStackPickup>();

        // Setup the customising table script
        itemStackPickup.Setup(data.ItemId, data.ItemQuantity, GameSceneUI.Instance.PlayerInventory);
    }

    public bool MovePlayerToSpawnPoint()
    {
        if (!string.IsNullOrEmpty(usedSceneSavePointId))
        {
            // A usedSavePointId was loaded

            // Find all save points in the world
            GameObject[] savePoints = GameObject.FindGameObjectsWithTag("SavePoint");

            for (int i = 0; i < savePoints.Length; i++)
            {
                // Get the ISavePoint component from each point so its id/respawn position can be retrieved
                ISavePoint currentSavePoint = savePoints[i].GetComponent<ISavePoint>();

                if (currentSavePoint.GetSavePointId() == usedSceneSavePointId)
                {
                    // Matching ids - found the save point that was used!

                    Debug.Log("Moving player to save point: " + usedSceneSavePointId + ", position: " + currentSavePoint.GetRespawnPosition());

                    currentSavePoint.SetAsUsed();

                    //Move player to the position of the spawn transform at the point they last saved
                    GameObject.FindGameObjectWithTag("Player").transform.position = currentSavePoint.GetRespawnPosition() + Vector3.up;

                    // Player was successfully moved to spawn point
                    return true;
                }
            }

            // No matching save point found
            return false;
        }

        // No used save point was loaded
        return false;
    }

    // Adding/removing placed objects and build points from their respective arrays:

    public void AddPlacedObjectToSave(IPersistentPlacedObject objToSave)
    {
        placedObjectsToSave.Add(objToSave);
    }

    public void RemovePlacedObjectFromSaveList(IPersistentPlacedObject objToRemove)
    {
        placedObjectsToSave.Remove(objToRemove);
    }

    public void AddPlacedBuildPoint(BuildPoint point)
    {
        placedBuildPoints.Add(point);
    }

    public void RemovePlacedBuildPoint(BuildPoint point)
    {
        placedBuildPoints.Remove(point);
    }
}

// TransformSaveData: The base class for SaveData of all player placed objects, saves a positon/rotation
//======================================================================================================

[Serializable]
public class TransformSaveData
{
    public float[] Position;
    public float[] Rotation;
}