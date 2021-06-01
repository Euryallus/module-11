using UnityEngine;

// ||=======================================================================||
// || ModularPieceType: When attached to a modular piece prefab, defines    ||
// ||   the type of the piece and allows it to be destroyed and             ||
// ||   saved/loaded with the world.                                        ||
// ||=======================================================================||
// || Used on all prefabs in: Joe/Environment/Construction/ModularPieces    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[System.Serializable]
public enum ModularPieceType
{
    // Contains all types of modular pieces that can be placed,
    //   used for saving/loading so the system knows which piece to place

    WoodFloor,
    WoodWall,
    WoodRoofSide,
    WoodStairs,
    WoodHalfWall
}

public class ModularPiece : DestructableObject, IPersistentPlacedObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Modular Piece")]
    [SerializeField] private ModularPieceType   pieceType;          // The modular piece type this script is attatched to

    #endregion

    protected override void Start()
    {
        base.Start();

        // Tell the WorldSave that this is a player-placed object that should be saved with the world
        WorldSave.Instance.AddPlacedObjectToSave(this);
    }

    private void OnDestroy()
    {
        // This object no longer exists in the world, remove it from the save list
        WorldSave.Instance.RemovePlacedObjectFromSaveList(this);
    }

    public void AddDataToWorldSave(SaveData saveData)
    {
        // Save the position, rotation and type of this modular piece in the world
        saveData.AddData("modularPiece*",   new ModularPieceSaveData()
                                            {
                                                Position = new float[3] { transform.position.x, transform.position.y, transform.position.z },
                                                Rotation = new float[3] { transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z },
                                                PieceType = pieceType
                                            });
    }

    public override void Destroyed()
    {
        base.Destroyed();
        Destroy(gameObject);
    }
}

// ModularPieceSaveData contains data used for saving/loading modular pieces

[System.Serializable]
public class ModularPieceSaveData : TransformSaveData
{
    public ModularPieceType PieceType;
}