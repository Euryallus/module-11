using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || LootChest: A chest that generates random loot from a loot table the   ||
// ||   first time it is opened.                                            ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Crafting & Chests/Loot Chest          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class LootChest : Chest, IPersistentObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Loot Chest Properties")]
    [SerializeField] private LootTable lootTable; // The loot table that determines which items will be generated and their chances of spawning

    #endregion

    private bool lootGenerated; //Whether loot has been generated for this chest

    protected override void Start()
    {
        base.Start();

        // Subscribe to save/load events so lootGenerated can be saved
        SaveLoadManager.Instance.SubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    protected override void OnDestroy()
    {
        // Unsubscribe from save/load events if the chest is destroyed to prevent null ref. errors
        SaveLoadManager.Instance.UnsubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    public void OnSave(SaveData saveData)
    {
        // Save whether loot has been generated
        saveData.AddData(itemContainer.ContainerId + "_lootGenerated", lootGenerated);
    }

    public void OnLoadSetup(SaveData saveData)
    {
        // Load whether loot has been generated
        lootGenerated = saveData.GetData<bool>(itemContainer.ContainerId + "_lootGenerated");
    }

    public void OnLoadConfigure(SaveData saveData) {} // Nothing to configure

    protected override void SetupChest()
    {
        if (!lootGenerated)
        {
            // First time opening this loot chest, generate the loot
            GenerateLoot();
        }
        else
        {
            // Update the chest UI to show its contents
            UpdateChestUI();
        }
    }

    private void GenerateLoot()
    {
        // Pick a random number of items to spawn bewteen the set min and max
        int numItemsToSpawn = Random.Range(lootTable.MinItems, lootTable.MaxItems + 1);

        Debug.Log("Generating " + numItemsToSpawn + " loot items for chest with container " + itemContainer.ContainerId);

        int itemsSpawned = 0; // Keeps track of how many items have been added

        // Step 1: Add the minimum quantity of each item
        for (int i = 0; i < lootTable.ItemPool.Count; i++)
        {
            WeightedItem weightedItem = lootTable.ItemPool[i];

            for (int j = 0; j < weightedItem.MinimumQuantity; j++)
            {
                itemContainer.TryAddItemToContainer(weightedItem.Item);
                itemsSpawned++;
            }
        }

        // Step 2: Fill remaining spaces with random weighted loot

        // Get a weighted list of items, the quantity of each item type added is equal to its weight,
        //   so items with a greater weight will have a greater chance of being randomly chosen
        List<Item> weightedItemPool = lootTable.GetWeightedItemPool();

        while (itemsSpawned < numItemsToSpawn)
        {
            // Randomly add items until numItemsToSpawn is reached
            
            Item itemToAdd = weightedItemPool[Random.Range(0, weightedItemPool.Count)];
            itemContainer.TryAddItemToContainer(itemToAdd);
            itemsSpawned++;
        }

        // Loot has been generated for this chest, from this point it will act as a normal chest
        lootGenerated = true;
    }
}
