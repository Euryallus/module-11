using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ||=======================================================================||
// || PressEffectButton: A button with a 2.5D effect that visually          ||
// ||   'presses' down when it is clicked or hovered over.                  ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/PressEffectButton                              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Added more customisation options: The button's side colour can be   ||
// ||  dynamically changed, and the button can raise when hovered over.     ||
// ||=======================================================================||

public enum ButtonPressInputType
{
    DepressOnHover, // The button will press down when it's hovered over
    DepressOnClick  // The button will press down when it's clicked
}

[RequireComponent(typeof(Image))]
public class PressEffectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Note: Pivots must be set to (0.5, 0.5)")]

    [SerializeField] private Button                 button;                                          // The actual button component that handles click events
    [SerializeField] private GameObject             buttonTopGameObject;                             // The GameObject used as the top of the button that presses down
    [SerializeField] private Image                  buttonTopImage;                                  // Image that makes up the 'top' of the button
    [SerializeField] private Image                  buttonSideImage;                                 // Image that makes up the 'side' of the button
    [SerializeField] private float                  pressSpeed = 120f;                               // How quick the press animation will be
    [SerializeField] private ButtonPressInputType   inputType = ButtonPressInputType.DepressOnClick; // Whether the button will press when hovered or clicked

    [SerializeField] private Color                  buttonColour = Color.grey;                       // The base colour of the button, also partially determines 'shadow' colour of the button's side
    [SerializeField] private Color                  buttonDisabledColour = Color.grey;               // The base colour of the button when not interactable
    [SerializeField] private Color                  buttonShadowTint = new Color(0.8f, 0.8f, 0.8f);  // The base colour is multiplied by this colour to get the shadow/side colour

    [SerializeField] private bool                   changeSideColourHover;  // Whether the side of the button should change colour when the button is hovered over
    [SerializeField] private Color                  hoverSideColour;        // If changeSideColourHover = true, the colour to use

    [SerializeField] private bool                   raiseButtonOnHover;     // Whether the button should visually raise when hovered over
    [SerializeField] private float                  hoverRaiseAmount;       // If raiseButtonOnHover = true, how much the button should raise

    #endregion

    #region Properties

    public Button   Button                  { get { return button; } }
    public bool     ChangeSideColourHover   { get { return changeSideColourHover; } set { changeSideColourHover = value; } }

    #endregion

    private Vector3 startPos;                       // The default position of the top of the button
    private Vector3 targetPos;                      // The position the top of the button should move towards
    private bool    interactable = true;            // Whether the button can currently be pressed
    private Color   defaultButtonColour;            // Keeps track of the button's original colour so it can be restored when needed
    private bool    setupComplete;                  // Whether the setup process is complete

    private float   pressSpeedMultiplier = 1.0f;    // pressSpeed is multiplied by this value when determining how quickly the top of the button should move
                                                    //  The value is adjusted depending on if the button is being pressed down or raised up, raise movements are slower


    private void Awake()
    {
        Setup();
    }

    public void Setup()
    {
        if(!setupComplete)
        {
            // Store the original colour of the button - this needs to happen in setup before any other
            //   code runs because the colour of the button may be changed on start from another script
            
            defaultButtonColour = buttonColour;
            setupComplete = true;
        }
    }

    private void Start()
    {
        // Get the default button top position, which is also its default target position so it stays in place
        startPos = buttonTopGameObject.transform.localPosition;
        targetPos = startPos;
    }

    private void Update()
    {
        // Always move the top of the button towards its target position
        buttonTopGameObject.transform.localPosition = Vector3.MoveTowards(buttonTopGameObject.transform.localPosition, targetPos, Time.unscaledDeltaTime * pressSpeed  * pressSpeedMultiplier);
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
        // Set whether the button can be interacted with
        button.interactable = interactable;
        this.interactable = interactable;

        // Update the button's colour depending on the interactable value
        if (interactable)
        {
            SetButtonColour(defaultButtonColour);
        }
        else
        {
            SetButtonColour(buttonDisabledColour);
        }
    }

    public void SetButtonColour(Color colour)
    {
        buttonColour = colour;

        // Set the top of the button to the main colour
        if (buttonTopImage != null)
        {
            buttonTopImage.color = buttonColour;
        }

        SetButtonSideColour();
    }

    public void SetButtonSideColour(bool useMainColour = true, Color colour = default)
    {
        if (buttonSideImage != null)
        {
            if (useMainColour)
            {
                colour = buttonColour * buttonShadowTint;
            }

            // Set the main image's colour (which makes up the 'side' of the button) to the tinted shadow colour
            buttonSideImage.color = colour;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(interactable)
        {
            if (inputType == ButtonPressInputType.DepressOnHover)
            {
                // Visually 'press' the button on hover if the input type is DepressOnHover

                Press();
            }
            else if (raiseButtonOnHover)
            {
                // Visually 'raise' the button on hover if raiseButtonOnHover = true

                Raise(hoverRaiseAmount);
            }

            if (changeSideColourHover)
            {
                // Change the button's side colour on hover if changeSideColourHover = true

                SetButtonSideColour(false, hoverSideColour);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Visually restore the button to its default state on hover end if it was pressed down or raised

        if (inputType == ButtonPressInputType.DepressOnHover)
        {
            Depress();
        }
        else if(raiseButtonOnHover)
        {
            Depress();
        }

        // Also restore the original side colour
        if (changeSideColourHover)
        {
            SetButtonSideColour();
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
        pressSpeedMultiplier = 1.0f;
    }

    private void Depress()
    {
        // Move the top of the button back towards its starting position
        targetPos = startPos;
        pressSpeedMultiplier = 1.0f;
    }

    private void Raise(float raiseAmount)
    {
        // Move the top of the button up slightly to visually raise the button
        targetPos = new Vector3(startPos.x, startPos.y + raiseAmount, startPos.z);

        // A lower value is used for pressSpeedMultiplier when raising so the raise animation happens more slowly than the press one
        pressSpeedMultiplier = 0.25f;
    }
}
