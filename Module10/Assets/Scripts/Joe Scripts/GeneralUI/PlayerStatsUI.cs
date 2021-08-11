using UnityEngine;
using UnityEngine.UI;

// ||=======================================================================||
// || PlayerStatsUI: Contains references to all  UI elements for easy       ||
// ||   from the PlayerStats script, which is on the Player prefab.         ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/PlayerHotbarAndInfo                            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class PlayerStatsUI : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Slider      healthSlider;              // Slider displaying player health
    [SerializeField] private Slider      foodLevelSlider;           // Slider displaying food level (how full the player is)
    [SerializeField] private Slider      breathLevelSlider;         // Slider displaying remaining breath when underwater
    [SerializeField] private CanvasGroup breathCanvasGroup;         // Canvas group containing the breath slider

    [SerializeField] private Animator    healthSliderAnimator;      // Handles animations for the health slider fill
    [SerializeField] private Animator    foodSliderAnimator;        // Handles animations for the food slider fill
    [SerializeField] private Animator    foodSliderBGAnimator;      // Handles animations for the food slider background image
    [SerializeField] private Animator    breathSliderAnimator;      // Handles animations for the breath slider fill
    [SerializeField] private Animator    breathSliderBGAnimator;    // Handles animations for the breath slider background image

    #endregion

    public Slider       HealthSlider            { get { return healthSlider; } }
    public Slider       FoodLevelSlider         { get { return foodLevelSlider; } }
    public Slider       BreathLevelSlider       { get { return breathLevelSlider; } }
    public CanvasGroup  BreathCanvasGroup       { get { return breathCanvasGroup; } }
    public Animator     HealthSliderAnimator    { get { return healthSliderAnimator; } }
    public Animator     FoodSliderAnimator      { get { return foodSliderAnimator; } }
    public Animator     FoodSliderBGAnimator    { get { return foodSliderBGAnimator; } }
    public Animator     BreathSliderAnimator    { get { return breathSliderAnimator; } }
    public Animator     BreathSliderBGAnimator  { get { return breathSliderBGAnimator; } }
}
