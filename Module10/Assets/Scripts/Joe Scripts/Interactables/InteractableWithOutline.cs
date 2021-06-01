using UnityEngine;

// ||=======================================================================||
// || InteractableWithOutline: An InteractableObject that displays an       ||
// ||   outline while being hovered over.                                   ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// Note: The Outline component used in this script was taken from the Unity asset store and was written by Chris Nolet:
// https://assetstore.unity.com/packages/tools/particles-effects/quick-outline-115488

[RequireComponent(typeof(Outline))]
public class InteractableWithOutline : InteractableObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] protected string interactionSound = "buttonClickMain1"; // id of the sound to be played when the object is interacted with
                                                                             //  (using an id instead of SoundClass so a default value can easily be set)

    #endregion

    private Outline outline; //The outline that is enabled/disabled depending on if the player is hovering

    protected override void Start()
    {
        base.Start();

        // Get the outline component and hide it by default.
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    public override void Interact()
    {
        base.Interact();

        if (!string.IsNullOrEmpty(interactionSound))
        {
            // Play the interaction sound if one was set
            AudioManager.Instance.PlaySoundEffect2D(interactionSound);
        }
    }

    public override void StartHoverInRange()
    {
        base.StartHoverInRange();

        // Enable the outline while hovering in range
        outline.enabled = true;
    }

    public override void EndHoverInRange()
    {
        base.EndHoverInRange();

        // Disable the outline when not hovering
        outline.enabled = false;
    }
}
