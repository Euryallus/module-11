using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Module 11

public class ItemStackPickup : MonoBehaviour, IPersistentPlacedObject
{
    private void Start()
    {
        // Tell the WorldSave that this is an object that should be saved with the world
        WorldSave.Instance.AddPlacedObjectToSave(this);
    }

    private void OnDestroy()
    {
        // This object no longer exists in the world, remove it from the save list
        WorldSave.Instance.RemovePlacedObjectFromSaveList(this);
    }

    public void AddDataToWorldSave(SaveData saveData)
    {
        saveData.AddData("itemStackPickup*",    new ItemStackPickupSaveData()
                                                {
                                                    Position = new float[3] { transform.position.x, transform.position.y, transform.position.z },
                                                    Rotation = new float[3] { transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z }
                                                });
    }
}

[System.Serializable]
public class ItemStackPickupSaveData : TransformSaveData
{
}