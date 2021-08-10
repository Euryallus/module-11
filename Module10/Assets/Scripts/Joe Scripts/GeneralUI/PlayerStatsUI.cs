using UnityEngine;
using UnityEngine.UI;

// Contains references to all player stats UI that needs to be accessed by the player stats script. Variables are grouped here to keep PlayerStats cleaner 

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private Slider      healthSlider;              // Slider displaying player health
    [SerializeField] private Slider      foodLevelSlider;           // Slider displaying food level (how full the player is)
    [SerializeField] private Slider      breathLevelSlider;         // Slider displaying remaining breath when underwater
    [SerializeField] private CanvasGroup breathCanvasGroup;         // Canvas group containing the breath slider

    [SerializeField] private Animator    healthSliderAnimator;      // Handles animations for the health slider fill
    [SerializeField] private Animator    foodSliderAnimator;        // Handles animations for the food slider fill
    [SerializeField] private Animator    foodSliderBGAnimator;      // Handles animations for the food slider background image
    [SerializeField] private Animator    breathSliderAnimator;      // Handles animations for the breath slider fill
    [SerializeField] private Animator    breathSliderBGAnimator;    // Handles animations for the breath slider background image

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
