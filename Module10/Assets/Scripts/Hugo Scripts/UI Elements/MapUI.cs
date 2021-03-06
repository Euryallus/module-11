using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Allows player to uncover map as they navigate the world. Will need adjustments once full world map is implemented, this version is very much a proof of concept
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class MapUI : UIPanel
{    
    [SerializeField]    private int revealRadius = 5;   // Radius of path reveled as player moves through world
    [SerializeField]    private Texture2D mapMask;      // Ref. to mask texture
                        private GameObject player;      // Ref to player
                        private Vector2 centre;         // Ref to player's position in 2D space
                        private CanvasGroup cg;         // Ref to canvas group of map
    

    protected override void Start()
    {
        base.Start();

        // Assigns refs to player and canvas group
        player = GameObject.FindGameObjectWithTag("Player");
        cg = gameObject.GetComponent<CanvasGroup>();

        // TEST ONLY - re-fills map when game starts
        for (int y = 0; y < mapMask.height; y++)
        {
            for (int x = 0; x < mapMask.width; x++)
            {
                Color color = new Color(0, 0, 0, 1);
                mapMask.SetPixel(x, y, color);
            }
        }
        mapMask.Apply();

        // Assigns centre of map to middle of texture
        centre = new Vector2(mapMask.width / 2, mapMask.height / 2);
    }

    private void Update()
    {
        // Adjusted by Joe to work with UIPanel base class/fix bugs related to showing/hiding panels

        // Checks if player presses M and no other menu is showing
        if (Input.GetKeyDown(KeyCode.M))
        {
            if(!showing && CanShowUIPanel())
            {
                // Change canvas group alpha to Show UI
                cg.alpha = 1.0f;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Map is now visible, apply changes made to texture since last opened & allow player to interact
                cg.blocksRaycasts = true;
                cg.interactable = true;
                mapMask.Apply();
                // Stop player from moving while map is open
                player.GetComponent<PlayerMovement>().StopMoving();

                showing = true;

                AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
            }
            else if(showing)
            {
                // Change canvas group alpha to Hide UI
                cg.alpha = 0.0f;

                Cursor.lockState = CursorLockMode.Locked;

                // Hides map & prevents interaction
                cg.blocksRaycasts = false;
                cg.interactable = false;
                // Allows player to move again
                player.GetComponent<PlayerMovement>().StartMoving();

                showing = false;

                AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
            }
        }

        float ratio = (1063f / 2f) / 500f;

        // Calculates player position & scales (Allows map to span entire world space needed)
        Vector2 playerPos = new Vector2(player.transform.position.x *ratio, player.transform.position.z * -ratio);
        // Sets what colour pixels will be set to
        Color color = Color.clear;

        // Cycles each pixel that should now be "revealed"
        for (int y = (int)playerPos.y - revealRadius; y < (int)playerPos.y + revealRadius; y++)
        {
            for (int x = (int)playerPos.x - revealRadius; x < (int)playerPos.x + revealRadius; x++)
            {
                // If pixel is not yet transparent & is within revealRadius
                if (mapMask.GetPixel(x, y).a != 0 && Vector3.Distance(playerPos, new Vector3(x, y)) <= (float)revealRadius)
                {
                    // Sets pixel to transparent to reveal map below (applied once map is opened with mapMask.Apply() )
                    mapMask.SetPixel((int)centre.x + x, (int)centre.y + y, color);
                }  
            }
        }
    }
}
