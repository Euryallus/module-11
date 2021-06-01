using UnityEngine;
using TMPro;

// ||=======================================================================||
// || ShopTalkPanel: A UI panel shown when the player interacts with a      ||
// ||   ShopNPC to introduce the shop type and allow the player to start    ||
// ||   buying items if they choose.                                        ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Shops/ShopTalkPanel                            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class ShopTalkPanel : UIPanel
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TextMeshProUGUI shopNameText;  // UI text displaying the name of the shop being accessed
    [SerializeField] private GameObject      buyUIPrefab;   // Prefab of the panel used to purchase items

    #endregion

    private ShopNPC     currentNPC; // The NPC being talked to

    protected override void Start()
    {
        base.Start();

        // Hide the talk panel until the player interacts with a shop NPC
        Hide();
    }

    void Update()
    {
        if (showing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // If esc is pressed while the panel is being shown, exit/hide it
                ButtonLeave();
            }
        }
    }

    public void ShowAndSetup(ShopNPC npc)
    {
        // Shop the UI panel
        Show();

        // Set the shop name text based on the shop type being accessed
        shopNameText.text = npc.ShopType.UIName;

        // Keep a reference to the NPC the player interacted with to open this panel,
        //   as they will also be used when setting up the shop interface
        currentNPC = npc;
    }

    public void ButtonBuy()
    {
        // The buy button used to show the panel that allows the player to purchase items

        // Hide this UI panel
        Hide();

        // Instantiate/show the buy panel
        GameObject buyPanel = Instantiate(buyUIPrefab, GameObject.FindGameObjectWithTag("JoeCanvas").transform);

        // Setup the buy panel with the currentNPC so it can display items from the correct shop type
        ShopBuyPanel panelScript = buyPanel.GetComponent<ShopBuyPanel>();
        panelScript.Setup(currentNPC);

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    public void ButtonLeave()
    {
        // Stop interacting with the NPC used to acess this talk panel
        currentNPC.StopInteracting();

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
    }
}
