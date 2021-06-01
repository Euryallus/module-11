using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ButtonPressInputType
{
    DepressOnHover, // The button will press down when it's hovered over
    DepressOnClick  // The button will press down when it's clicked
}

// ||=======================================================================||
// || PressEffectButton: A button with a 2.5D effect that visually          ||
// ||   'presses' down when it is clicked or hovered over.                  ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/PressEffectButton                              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[RequireComponent(typeof(Image))]
public class PressEffectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Note: Pivots must be set to (0.5, 0.5)")]

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Button                 button;                                          // The actual button component that handles click events
    [SerializeField] private GameObject             buttonTopGameObject;                             // The GameObject used as the top of the button that presses down
    [SerializeField] private float                  pressSpeed = 120f;                               // How quick the press animation will be
    [SerializeField] private ButtonPressInputType   inputType = ButtonPressInputType.DepressOnClick; // Whether the button will press when hovered or clicked
    [SerializeField] private Color                  buttonColour = Color.grey;                       // The base colour of the button, also partially determines 'shadow' colour of the button's side
    [SerializeField] private Color                  buttonShadowTint = new Color(0.8f, 0.8f, 0.8f);  // The base colour is multiplied by this colour to get the shadow/side colour

    #endregion

    private Vector3 startPos;               // The default position of the top of the button
    private Vector3 targetPos;              // The position the top of the button should move towards
    private bool    interactable = true;    // Whether the button can currently be pressed

    private void Start()
    {
        // Get the default button top position, which is also its default target position so it stays in place
        startPos = buttonTopGameObject.transform.localPosition;
        targetPos = startPos;
    }

    private void Update()
    {
        // Always move the top of the button towards its target position
        buttonTopGameObject.transform.localPosition = Vector3.MoveTowards(buttonTopGameObject.transform.localPosition, targetPos, Time.unscaledDeltaTime * pressSpeed);
    }

    private void OnValidate()
    {
        #if (UNITY_EDITOR)
            // Update the button's colour when a value is changed in the inspector so it's displayed with the buttonColour/buttonShadowTint that will be used in-game
            SetButtonColour(buttonColour);
        #endif
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
        this.interactable = interactable;
    }

    public void SetButtonColour(Color colour)
    {
        buttonColour = colour;

        // Set the main image's colour (which makes up the 'side' of the button) to the tinted shadow colour
        GetComponent<Image>().color = buttonColour * buttonShadowTint;

        // Set the top of the button to the main colour
        if (buttonTopGameObject != null)
        {
            buttonTopGameObject.GetComponent<Image>().color = buttonColour;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Visually 'press' the button on hover if the input type is DepressOnHover
        if (inputType == ButtonPressInputType.DepressOnHover && interactable)
        {
            Press();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Visually restore the button to its default state on hover end if the input type is DepressOnHover
        if (inputType == ButtonPressInputType.DepressOnHover)
        {
            Depress();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Visually 'press' the button on click if the input type is DepressOnClick
        if (inputType == ButtonPressInputType.DepressOnClick && interactable)
        {
            Press();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Visually restore the button to its default state on mouse release if the input type is DepressOnClick
        if (inputType == ButtonPressInputType.DepressOnClick)
        {
            Depress();
        }
    }

    private void Press()
    {
        // Move the top of the button towards the centre of its parent, i.e. move it down slightly to create the press effect
        targetPos = Vector3.zero;
    }

    private void Depress()
    {
        // Move the top of the button back towards its starting position
        targetPos = startPos;
    }
}
