using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ||=======================================================================||
// || ItemManager: Holds references to all items and crafting recipes.      ||
// ||   Also handles saving/loading custom items and allows custom property ||
// ||   data to be applied to items.                                        ||
// ||=======================================================================||
// || Used on prefab: Joe/Items/ItemManager                                 ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class ItemManager : MonoBehaviour, IPersistentObject
{
    public static ItemManager Instance; // Static instance of the class for simple access

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Item[]             items;              // Array containing all items
    [SerializeField] private CraftingRecipe[]   craftingRecipes;    // Array containing all crafting recipes

    #endregion

    #region Properties

    public Item[]           Items           { get { return items; } set { items = value; } }
    public CraftingRecipe[] CraftingRecipes { get { return craftingRecipes; } set { craftingRecipes = value; } }

    #endregion

    private Dictionary<string, Item>            itemsDict;              // Dictinary containing all standard items indexed by their ids
    private Dictionary<string, Item>            customItemsDict;        // Dictinary containing all player-created custom items indexed by their ids
    private Dictionary<string, CraftingRecipe>  craftingRecipesDict;    // Dictinary containing all crafting recipes indexed by their ids

    private int customItemUniqueId;

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

        // Add all items/crafting recipes to their respective dictionaries
        SetupDictionaries();
    }

    private void Start()
    {
        // Subscribe to save/load events so custom items will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events if the GameObject is destroyed to prevent null reference errors
        SaveLoadManager.Instance.UnsubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    public void OnSave(SaveData saveData)
    {
        Debug.Log("Saving custom inventory items");

        // Save the unique id to be used when creating a custom item
        saveData.AddData("customItemUniqueId", customItemUniqueId);

        // Save the number of custom items that player has created
        saveData.AddData("customItemCount", customItemsDict.Count);

        // Save all player-created custom items
        for (int i = 0; i < customItemsDict.Count; i++)
        {
            Item itemToSave = customItemsDict.ElementAt(i).Value;

            // Create save data with the item's id/name
            CustomItemSaveData itemData = new CustomItemSaveData()
            {
                Id = itemToSave.Id,
                BaseItemId = itemToSave.BaseItemId,
                UIName = itemToSave.UIName,
            };

            // Initialise the save data array for custom float properties
            itemData.CustomFloatProperties = new CustomFloatProperty[itemToSave.CustomFloatProperties.Length];

            // Add data for all custom float properties
            for (int j = 0; j < itemToSave.CustomFloatProperties.Length; j++)
            {
                itemData.CustomFloatProperties[j] = new CustomFloatProperty()
                {
                    Name            = itemToSave.CustomFloatProperties[j].Name,
                    UIName          = itemToSave.CustomFloatProperties[j].UIName,
                    Value           = itemToSave.CustomFloatProperties[j].Value,
                    UpgradeIncrease = itemToSave.CustomFloatProperties[j].UpgradeIncrease,
                    MinValue        = itemToSave.CustomFloatProperties[j].MinValue,
                    MaxValue        = itemToSave.CustomFloatProperties[j].MaxValue
                };
            }

            // Initialise the save data array for custom string properties
            itemData.CustomStringProperties = new CustomStringProperty[itemToSave.CustomStringProperties.Length];

            // Add data for all custom string properties
            for (int j = 0; j < itemToSave.CustomStringProperties.Length; j++)
            {
                itemData.CustomStringProperties[j] = new CustomStringProperty()
                {
                    Name = itemToSave.CustomStringProperties[j].Name,
                    UIName = itemToSave.CustomStringProperties[j].UIName,
                    Value = itemToSave.CustomStringProperties[j].Value
                };
            }

            // Add the created save data for the custom item, using the loop index to create a unique id
            saveData.AddData("customItem" + i, itemData );
        }
    }

    public void OnLoadSetup(SaveData saveData)
    {
        Debug.Log("Loading custom inventory items");

        // Load the unique id to be used when creating a custom item
        customItemUniqueId = saveData.GetData<int>("customItemUniqueId");
           
        // Load the number of custom items that player had created
        int customItemCount = saveData.GetData<int>("customItemCount");

        // Load all custom items
        for (int i = 0; i < customItemCount; i++)
        {
            // Get the save data for the current custom item
            CustomItemSaveData itemData = saveData.GetData<CustomItemSaveData>("customItem" + i);

            // Add the custom item based on the loaded ids
            Item loadedItem = AddCustomItem(itemData.Id, itemData.BaseItemId, itemData.BaseItemId);

            // Set the UI name of the custom item from the loaded value
            SetCustomItemUIName(itemData.Id, itemData.UIName);

            Item baseItem = GetItemWithId(itemData.BaseItemId);

            // Setup all custom float properties on the new item
            for (int j = 0; j < baseItem.CustomFloatProperties.Length; j++)
            {
                // Get the loaded data for the current custom float property
                var floatPropertyData = itemData.CustomFloatProperties[j];

                // If the loaded data is not null (may occur if the item was created before the custom property was added to the game)
                //   and its property name matches that of the base item (again, old items may have unused properties which should be skipped),
                //   then setup the property data from the loaded values
                if(floatPropertyData != null && floatPropertyData.Name == baseItem.CustomFloatProperties[j].Name)
                {
                    loadedItem.CustomFloatProperties[j] = new CustomFloatProperty()
                    {
                        Name            = floatPropertyData.Name,
                        UIName          = floatPropertyData.UIName,
                        Value           = floatPropertyData.Value,
                        UpgradeIncrease = floatPropertyData.UpgradeIncrease,
                        MinValue        = floatPropertyData.MinValue,
                        MaxValue        = floatPropertyData.MaxValue
                    };
                }
                // If the above condition fails, the item will retain the default property values of its base item
            }

            // Setup all custom string properties on the new item
            for (int j = 0; j < baseItem.CustomStringProperties.Length; j++)
            {
                // Get the loaded data for the current custom string property
                var stringPropertyData = itemData.CustomStringProperties[j];

                // Same process as above, but for string properties
                if (stringPropertyData != null && stringPropertyData.Name == baseItem.CustomStringProperties[j].Name)
                {
                    loadedItem.CustomStringProperties[j] = new CustomStringProperty()
                    {
                        Name = stringPropertyData.Name,
                        UIName = stringPropertyData.UIName,
                        Value = stringPropertyData.Value
                    };
                }
            }
        }
    }

    public void OnLoadConfigure(SaveData saveData) { } // Nothing to configure

    private void SetupDictionaries()
    {
        // Initialise all dictionaries
        itemsDict           = new Dictionary<string, Item>();
        customItemsDict     = new Dictionary<string, Item>();
        craftingRecipesDict = new Dictionary<string, CraftingRecipe>();

        // Add all items to a dictionary indexed by their ids
        for (int i = 0; i < items.Length; i++)
        {
            if(items[i] != null)
            {
                itemsDict.Add(items[i].Id, items[i]);
            }
        }

        // Add all crafting recipes to a dictionary indexed by the resulting item ids
        for (int i = 0; i < craftingRecipes.Length; i++)
        {
            if(craftingRecipes[i] != null)
            {
                craftingRecipesDict.Add(craftingRecipes[i].ResultItem.Item.Id, craftingRecipes[i]);
            }
        }
    }

    public Item GetItemWithId(string id)
    {
        // Returns the item with the given id, checking both standard and custom item dictionaries.

        if (itemsDict.ContainsKey(id))
        {
            return itemsDict[id];
        }
        else if (customItemsDict.ContainsKey(id))
        {
            return customItemsDict[id];
        }
        else
        {
            // No item found with the given id
            Debug.LogError("Trying to get item with invalid id: " + id);
            return null;
        }
    }

    public Item GetCustomItemWithId(string id)
    {
        // Returns the custom with the given id, or theows an error if the id is invalid

        if (customItemsDict.ContainsKey(id))
        {
            return customItemsDict[id];
        }
        else
        {
            Debug.LogError("Trying to get custom item with invalid id: " + id);
            return null;
        }
    }

    public Item AddCustomItem(string id, string baseItemId, string originalBaseItemId)
    {
        // Create a duplicate of the base item before editing certain values

        // Get the item the custom item is based on (the item that was placed into the customising table to create the final item)
        Item baseItem =         GetItemWithId(baseItemId);

        // Get the original item the custom item is based on. For items customised once, this will be the same as baseItem.
        //  For items customised multiple times, this is the item at the top of the heirarchy. e.g. If an axe has been customised twice,
        //  baseItem is the previous customised version, while originalBaseItem is the default axe item with no customisation applied).
        Item originalBaseItem = GetItemWithId(originalBaseItemId);

        if(originalBaseItem != null)
        {
            // Create a copy of the original base item since the customised item will share many of the same properties
            Item customItem = Instantiate(GetItemWithId(originalBaseItemId));

            // Give the new item a custom id
            customItem.Id = id;

            // Set the new item as a custom item
            customItem.CustomItem = true;

            // Set the item's baseItemId to be the id of the orignial item it's based on
            customItem.BaseItemId = originalBaseItemId;

            if(baseItem != null)
            {
                // Setup custom float properties from base item, so in the case of items that have already been customised once or more,
                //   the new item will have the previously edited property values by default
                for (int i = 0; i < baseItem.CustomFloatProperties.Length; i++)
                {
                    customItem.CustomFloatProperties[i].Value = baseItem.CustomFloatProperties[i].Value;
                }
            }
            else
            {
                // Error: invalid id for baseItem
                Debug.LogError("Trying to create custom item with invalid base item id: " + originalBaseItemId);
            }

            if (!customItemsDict.ContainsKey(customItem.Id))
            {
                // Add the new custom item to customItemsDict
                customItemsDict.Add(customItem.Id, customItem);

                Debug.Log("Added custom item with id: " + customItem.Id);

                return customItem;
            }
            else
            {
                // Warning: custom item with the same id already exists. This can occur sometimes when using a customising table to adjust properties
                //  once the custom item has already been added, in this case the warning can be safely ignored. However, it should not be ignored if it
                //  appears an an unexpected time, such as when saving/loading the game.
                Debug.LogWarning("Trying to create custom item with id that already exists: " + customItem.Id);
                return null;
            }
        }
        else
        {
            // Error: invalid id for originalBaseItem
            Debug.LogError("Trying to create custom item with invalid original base item id: " + originalBaseItemId);
            return null;
        }
    }

    public void SetCustomFloatItemData(string id, string customPropertyName, float value)
    {
        // Sets the value of a custom float property with name: customPropertyName for a custom item with the given id
        if (customItemsDict.ContainsKey(id))
        {
            customItemsDict[id].SetCustomFloatProperty(customPropertyName, value);
        }
        else
        {
            Debug.LogError("Trying to set data on custom item with invalid id: " + id);
        }
    }

    public void SetCustomStringItemData(string id, string customPropertyName, string value)
    {
        // Sets the value of a custom string property with name: customPropertyName for a custom item with the given id
        if (customItemsDict.ContainsKey(id))
        {
            customItemsDict[id].SetCustomStringProperty(customPropertyName, value);
        }
        else
        {
            Debug.LogError("Trying to set data on custom item with invalid id: " + id);
        }
    }

    public void SetCustomItemUIName(string id, string customUIName)
    {
        // Sets the UI name of a custom item with the given id
        if (customItemsDict.ContainsKey(id))
        {
            customItemsDict[id].UIName = customUIName;
        }
        else
        {
            Debug.LogError("Trying to set data on custom item with invalid id: " + id);
        }
    }

    public void RemoveCustomItem(string id)
    {
        // Removes an item with the given id from the dictionary containing custom items
        if (customItemsDict.ContainsKey(id))
        {
            customItemsDict.Remove(id);

            Debug.Log("Removed custom item with id: " + id);
        }
    }

    public string GetUniqueCustomItemId()
    {
        // Returns an id to be used for the creation of custom items,
        //   using customItemUniqueId (a value that is incremented each time a custom item is obtained by the player)
        //   to ensure the id is unique each time a new item is created

        string id = "customItem" + customItemUniqueId;

        return id;
    }

    public void IncrementUniqueCustomItemId()
    {
        // Increments the id to be used when creating custom items, see the comments
        //   on the function above for more info about custom item id's

        customItemUniqueId++;
    }

    public bool IsItemConsumable(string id)
    {
        Item item = GetItemWithId(id);

        // Check if the item with the given id is a consumable
        if(item != null && item is ConsumableItem)
        {
            // Item is a ConsumableItem
            return true;
        }

        // Item is not a ConsumableItem
        return false;
    }
}

// CustomItemSaveData: A serializable class used when saving/loading data about a custom item
//===========================================================================================

[System.Serializable]
public struct CustomItemSaveData
{
    // See Item.cs for descriptions of the variables below

    public string   Id;
    public string   BaseItemId;
    public string   UIName;
    public int      UpgradeLevel;

    public CustomFloatProperty[] CustomFloatProperties;

    public CustomStringProperty[] CustomStringProperties;
}