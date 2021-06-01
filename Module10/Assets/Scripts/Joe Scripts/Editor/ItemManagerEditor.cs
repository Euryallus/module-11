using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;

// ||=======================================================================||
// || ItemManagerEditor: Custom editor that displays the ItemManager's      ||
// ||   items and recipes in a more visually appealing grid with clearer    ||
// ||   icons and text (see the ItemManager prefab).                        ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[CustomEditor(typeof(ItemManager))]
public class ItemManagerEditor : Editor
{
    private int         columnCount;                            // Number of columns to use when displaying items/crafting recipes
    private Vector2     itemScrollPosition;                     // Position of the horizontal scrollbar when the items display overflows the inspector window
    private Vector2     recipeScrollPosition;                   // Position of the horizontal scrollbar when the recipes display overflows the inspector window
                                                                   
    private bool        itemArrayMatchesPrefab      = true;     // If true, there are no changes made to the ItemManager's items array that aren't applied to the base prefab
    private bool        recipeArrayMatchesPrefab    = true;     // If true, there are no changes made to the ItemManager's recipes array that aren't applied to the base prefab
    private List<int>   itemModificationIndexes;                // Contains all indexes in the ItemManager's items array where modifications have been made that aren't applied to the base prefab
    private List<int>   recipeModificationIndexes;              // Contains all indexes in the ItemManager's recipes array where modifications have been made that aren't applied to the base prefab

    // GUI Styles (see SetupGUIStyles function)
    private bool        stylesAreSetup;
    private GUIStyle    smallLabelStyle, mediumLabelStyle, warningLabelStyle, largeLabelStyle;
    private GUIStyle    boxStyle = new GUIStyle(), warningBoxStyle = new GUIStyle();

    private static bool itemsExpanded   = true;     // Whether the grid of items added to the ItemManager should be shown
    private static bool recipesExpanded = true;     // Whether the grid of recipes added to the ItemManager should be shown
    private static bool baseInspectorExpanded;      // Whether the default ItemManager inspector UI should be shown
                                                       
    private const int   MaxColumns = 4;             // Number of columns to use when displaying items/crafting recipes

    public override void OnInspectorGUI()
    {
        // Setup custom GUI styles if that haven't been already
        if (!stylesAreSetup)
        {
            SetupGUIStyles();
        }

        // Update the object being edited
        serializedObject.Update();

        ItemManager itemManager = (ItemManager)target;

        // Check if the current prefab instance has any modifications from the base prefab
        CheckForPrefabPropertyModifications(itemManager);

        // Start checking for changes, e.g. adding an item
        EditorGUI.BeginChangeCheck();

        // ITEM MANAGER HEADER
        // =====================

        DrawItemManagerHeader();

        EditorGUILayout.Space(15.0f);

        // ITEMS HEADER
        // ==============

        DrawItemsHeader();

        // ITEMS DISPLAY/SELECTION
        // =========================

        DrawItemsDisplaySection(itemManager);

        // RECIPES HEADER
        // ==============

        DrawRecipesHeader();

        // RECIPES DISPLAY/SELECTION
        // ===========================

        DrawRecipesDisplaySection(itemManager);

        // BASE INSPECTOR GUI
        // ====================

        if (GUILayout.Button(baseInspectorExpanded ? "Hide Standard Arrays" : "Show Standard Arrays", GUILayout.Width(163.0f), GUILayout.Height(25.0f)))
        {
            baseInspectorExpanded = !baseInspectorExpanded;
        }

        // Draw the default editor GUI if the above button was pressed to expand it
        if (baseInspectorExpanded)
        {
            base.OnInspectorGUI();
        }

        // Stop checking for changes
        EditorGUI.EndChangeCheck();

        if(GUI.changed)
        {
            // Mark the script/editor as dirty so the engine knows changes were made
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(itemManager.gameObject.scene);
        }

        // Apply any properties that were modified
        serializedObject.ApplyModifiedProperties();
    }

    private void SetupGUIStyles()
    {
        // Setup all custom GUI styles to be used in the editor window

        // Small standard label
        smallLabelStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 10,
            alignment = TextAnchor.LowerLeft,
            fixedHeight = 9.0f
        };
        smallLabelStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f);

        // Medium standard label
        mediumLabelStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 15,
            alignment = TextAnchor.LowerLeft
        };

        // Large standard label
        largeLabelStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 18,
            alignment = TextAnchor.LowerLeft
        };

        // Label for warning text
        warningLabelStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            alignment = TextAnchor.LowerLeft,
            wordWrap = true
        };
        warningLabelStyle.normal.textColor = new Color(0.8f, 0.98f, 1.0f);

        // Box used as the background for grid elements by default
        boxStyle.normal.background = GetColourTexture(new Color(1.0f, 1.0f, 1.0f, 0.9f));

        // Box used as the background for grid elements that were modified from the base prefab
        warningBoxStyle.normal.background = GetColourTexture(new Color(0.8f, 0.98f, 1.0f, 0.9f));

        // Mark styles as setup so they aren't reinitialised each time the GUI is drawn
        stylesAreSetup = true;
    }

    #region Inspector GUI Drawing
    //===========================

    private void DrawItemManagerHeader()
    {
        // Add labels showing this editor is for the ItemManager
        EditorGUILayout.Space(15.0f);
        EditorGUILayout.LabelField("Item Manager", largeLabelStyle);
        EditorGUILayout.LabelField("Please open the prefab to make changes.", mediumLabelStyle);
    }

    private void DrawItemsHeader()
    {
        GUILayout.BeginHorizontal();

        // Label showing this is the items section
        EditorGUILayout.LabelField("Items", largeLabelStyle, GUILayout.Width(80.0f), GUILayout.Height(22.0f));

        // Button that expands/collapses the items display area
        if (GUILayout.Button(itemsExpanded ? "Hide" : "Show", GUILayout.Width(80.0f), GUILayout.Height(25.0f)))
        {
            itemsExpanded = !itemsExpanded;
        }

        GUILayout.EndHorizontal();

        // If there are unapplied changes to the items array on the prefab, show warning text
        EditorGUILayout.LabelField(itemArrayMatchesPrefab ? "" : "Warning: The items array for this prefab instance contains changes not applied to the base prefab. Consider reverting changes.", warningLabelStyle);

        EditorGUILayout.Space(itemArrayMatchesPrefab ? 0.0f : 10.0f);
    }

    private void DrawRecipesHeader()
    {
        GUILayout.BeginHorizontal();

        // Label showing this is the recipes section
        EditorGUILayout.LabelField("Recipes", largeLabelStyle, GUILayout.Width(80.0f), GUILayout.Height(22.0f));

        // Button that expands/collapses the recipes display area
        if (GUILayout.Button(recipesExpanded ? "Hide" : "Show", GUILayout.Width(80.0f), GUILayout.Height(25.0f)))
        {
            recipesExpanded = !recipesExpanded;
        }

        GUILayout.EndHorizontal();

        // If there are unapplied changes to the recipes array on the prefab, show warning text
        EditorGUILayout.LabelField(recipeArrayMatchesPrefab ? "" : "Warning: The recipes array for this prefab instance contains changes not applied to the base prefab. Consider reverting changes.", warningLabelStyle);

        EditorGUILayout.Space(recipeArrayMatchesPrefab ? 0.0f : 10.0f);
    }

    private void DrawItemsDisplaySection(ItemManager itemManager)
    {
        columnCount = 0;
        Item[] items = itemManager.Items;

        // Only display items if the section is expanded and the items array is not null
        if (itemsExpanded && items != null)
        {
            // Scroll area containing all item previews
            itemScrollPosition = EditorGUILayout.BeginScrollView(itemScrollPosition, GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();

            // Loop through all items in the array, + 1 since an 'add' button also needs to be created
            for (int i = 0; i < (items.Length + 1); i++)
            {
                if (i < items.Length)
                {
                    // Draw a preview for each item
                    DrawItemPreview(itemManager, items, i);
                }
                else
                {
                    // Create an add button as the final element
                    DrawAddItemButton(itemManager);
                }

                GUILayout.Space(8.0f);

                // Keep track of how many columns have been added for the current row
                columnCount++;

                if (columnCount == MaxColumns)
                {
                    // Start a new row when max columns is reached
                    GUILayout.EndHorizontal();
                    GUILayout.Space(8.0f);
                    GUILayout.BeginHorizontal();
                    columnCount = 0;
                }

            }

            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(15.0f);
        }
        else if (items == null)
        {
            // No items to display, just draw an 'add' button
            DrawAddItemButton(itemManager);
        }
    }

    private void DrawRecipesDisplaySection(ItemManager itemManager)
    {
        columnCount = 0;
        CraftingRecipe[] recipes = itemManager.CraftingRecipes;

        // Only display recipes if the section is expanded and the items array is not null
        if (recipesExpanded && recipes != null)
        {
            // Scroll area containing all recipe previews
            recipeScrollPosition = EditorGUILayout.BeginScrollView(recipeScrollPosition, GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();

            // Loop through all recipes in the array, + 1 since an 'add' button also needs to be created
            for (int i = 0; i < (recipes.Length + 1); i++)
            {
                if (i < recipes.Length)
                {
                    // Draw a preview for each recipe
                    DrawRecipePreview(itemManager, recipes, i);
                }
                else
                {
                    // Create an add button as the final element
                    DrawAddRecipeButton(itemManager);
                }

                GUILayout.Space(8.0f);

                // Keep track of how many columns have been added for the current row
                columnCount++;

                if (columnCount == MaxColumns)
                {
                    // Start a new row when max columns is reached
                    GUILayout.EndHorizontal();
                    GUILayout.Space(8.0f);
                    GUILayout.BeginHorizontal();
                    columnCount = 0;
                }

            }

            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(15.0f);
        }
        else if (recipes == null)
        {
            // No recipes to display, just draw an 'add' button
            DrawAddRecipeButton(itemManager);
        }
    }

    private void DrawAddItemButton(ItemManager itemManager)
    {
        // Button for adding a new item to the items array
        if (GUILayout.Button("Add", GUILayout.Width(82.0f), GUILayout.Height(72.0f)))
        {
            AddItem(itemManager);
        }
    }

    private void DrawAddRecipeButton(ItemManager itemManager)
    {
        // Button for adding a new recipe to the recipes array
        if (GUILayout.Button("Add", GUILayout.Width(82.0f), GUILayout.Height(72.0f)))
        {
            AddRecipe(itemManager);
        }
    }

    private void DrawItemPreview(ItemManager itemManager, Item[] items, int index)
    {
        Item currentItem = items[index];

        // AREA 1: Start of the main item display area
        GUILayout.BeginVertical(itemModificationIndexes.Contains(index) ? warningBoxStyle : boxStyle, GUILayout.Width(80.0f));

        GUILayout.Space(5.0f);

        // Label showing the name if the item being previewed, or "None" for null items
        GUILayout.Label(currentItem ? currentItem.UIName : "None", smallLabelStyle, GUILayout.Width(78.0f));

        // AREA 2: Area containing the item icon and change/delete buttons
        GUILayout.BeginHorizontal(); 

        Texture2D textureToShow;

        // Either show the item sprite's texture, or a warning icon if the item/sprite is null
        if (currentItem != null && currentItem.Sprite != null)
        {
            textureToShow = currentItem.Sprite.texture;
        }
        else
        {
            textureToShow = Resources.Load<Sprite>("WarningIcon").texture;
        }
        GUILayout.Label(textureToShow, GUILayout.Width(55.0f), GUILayout.Height(55.0f));

        // AREA 3: Area containing just the change and delete buttons
        GUILayout.BeginVertical();

        // Field for selecting the desired item type
        items[index] = EditorGUILayout.ObjectField("", items[index], typeof(Item), false, GUILayout.Height(35.0f), GUILayout.Width(22.0f)) as Item;

        // Button for removing the item being previewed
        if (GUILayout.Button("X", GUILayout.Width(22.0f), GUILayout.Height(20.0f)))
        {
            RemoveItemAtIndex(itemManager, index);
        }

        GUILayout.EndVertical();   // End of AREA 1
        GUILayout.EndHorizontal(); // End of AREA 2
        GUILayout.EndVertical();   // End of AREA 3
    }

    private void DrawRecipePreview(ItemManager itemManager, CraftingRecipe[] recipes, int index)
    {
        CraftingRecipe currentRecipe = recipes[index];

        // AREA 1: Start of the main recipe display area
        GUILayout.BeginVertical(recipeModificationIndexes.Contains(index) ? warningBoxStyle : boxStyle, GUILayout.Width(80.0f));

        GUILayout.Space(5.0f);

        // Label showing the name if the recipe being previewed, or "None" for null recipes
        GUILayout.Label((currentRecipe && currentRecipe.ResultItem.Item) ? currentRecipe.ResultItem.Item.UIName : "None", smallLabelStyle, GUILayout.Width(78.0f));

        // AREA 2: Area containing the recipe result icon and change/delete buttons
        GUILayout.BeginHorizontal();

        Texture2D textureToShow;

        // Either show the recipe result item sprite's texture, or a warning icon if the recipe/result item/sprite is null
        if (currentRecipe != null && currentRecipe.ResultItem.Item != null && currentRecipe.ResultItem.Item.Sprite != null)
        {
            textureToShow = currentRecipe.ResultItem.Item.Sprite.texture;
        }
        else
        {
            textureToShow = Resources.Load<Sprite>("WarningIcon").texture;
        }
        GUILayout.Label(textureToShow, GUILayout.Width(55.0f), GUILayout.Height(55.0f));

        // AREA 3: Area containing just the change and delete buttons
        GUILayout.BeginVertical();

        // Field for selecting the desired recipe type
        recipes[index] = EditorGUILayout.ObjectField("", currentRecipe, typeof(CraftingRecipe), false, GUILayout.Height(35.0f), GUILayout.Width(22.0f)) as CraftingRecipe;

        // Button for removing the recipe being previewed
        if (GUILayout.Button("X", GUILayout.Width(22.0f), GUILayout.Height(20.0f)))
        {
            RemoveRecipeAtIndex(itemManager, index);
        }

        GUILayout.EndVertical();   // End of AREA 1
        GUILayout.EndHorizontal(); // End of AREA 2
        GUILayout.EndVertical();   // End of AREA 3
    }

    #endregion // End of Inspector GUI Drawing
    //========================================

    #region Adding/Removing Items & Recipes
    //=====================================

    private void AddItem(ItemManager itemManager)
    {
        // Converts the items array to a list, adds a null item, then converts back to an array

        List<Item> itemList = itemManager.Items.ToList();

        itemList.Add(null);

        itemManager.Items = itemList.ToArray();
    }

    private void AddRecipe(ItemManager itemManager)
    {
        // Converts the recipes array to a list, adds a null recipe, then converts back to an array

        List<CraftingRecipe> recipeList = itemManager.CraftingRecipes.ToList();

        recipeList.Add(null);

        itemManager.CraftingRecipes = recipeList.ToArray();
    }

    private void RemoveItemAtIndex(ItemManager itemManager, int index)
    {
        // Converts the items array to a list, removes the item at the given index, then converts back to an array

        List<Item> itemList = itemManager.Items.ToList();

        itemList.RemoveAt(index);

        itemManager.Items = itemList.ToArray();
    }

    private void RemoveRecipeAtIndex(ItemManager itemManager, int index)
    {
        // Converts the recipes array to a list, removes the recipe at the given index, then converts back to an array

        List<CraftingRecipe> recipeList = itemManager.CraftingRecipes.ToList();

        recipeList.RemoveAt(index);

        itemManager.CraftingRecipes = recipeList.ToArray();
    }

    #endregion // End of Adding/Removing Items & Recipes
    //==================================================

    private void CheckForPrefabPropertyModifications(ItemManager itemManager)
    {
        itemModificationIndexes     = new List<int>();
        recipeModificationIndexes   = new List<int>();

        // Get info about all properties that were modified from the base prefab
        var propertyModifications = PrefabUtility.GetPropertyModifications(itemManager.gameObject);

        if (propertyModifications != null)
        {
            // Modified properties were found

            foreach (var modification in propertyModifications)
            {
                // Get the path of each modified property
                string path = modification.propertyPath;

                if (path.Contains("items.Array.data"))
                {
                    // Some data from the items array was modified

                    // Get an integer from the path, this is the index of the modified item in the array
                    // Regex expression taken from: https:// stackoverflow.com/questions/4734116/find-and-extract-a-number-from-a-string
                    itemModificationIndexes.Add(int.Parse(Regex.Match(path, @"\d+").Value));

                    // The items array in the current inspector window does not match the prefab
                    itemArrayMatchesPrefab = false;
                }
                else if (path.Contains("craftingRecipes.Array.data"))
                {
                    // Some data from the recipes array was modified

                    // Get an integer from the path, this is the index of the modified recipe in the array
                    // Regex expression taken from: https:// stackoverflow.com/questions/4734116/find-and-extract-a-number-from-a-string
                    recipeModificationIndexes.Add(int.Parse(Regex.Match(path, @"\d+").Value));

                    // The recipes array in the current inspector window does not match the prefab
                    recipeArrayMatchesPrefab = false;
                }
            }
        }
    }

    private Texture2D GetColourTexture(Color colour)
    {
        // Create a texture
        Texture2D texture = new Texture2D(128, 128);

        // Fill it with pixels of the given colour
        for (int y = 0; y < texture.height; ++y)
        {
            for (int x = 0; x < texture.width; ++x)
            {
                texture.SetPixel(x, y, colour);
            }
        }

        // Apply/return the created texture
        texture.Apply();
        return texture;
    }
}
