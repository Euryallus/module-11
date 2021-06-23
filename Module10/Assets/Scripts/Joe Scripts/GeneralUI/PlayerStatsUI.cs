using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider foodLevelSlider;
    [SerializeField] private Slider breathLevelSlider;
    [SerializeField] private CanvasGroup breathCanvasGroup;
    [SerializeField] private Animator healthSliderAnimator;
    [SerializeField] private Animator foodSliderAnimator;
    [SerializeField] private Animator foodSliderBGAnimator;
    [SerializeField] private Animator breathSliderAnimator;
    [SerializeField] private Animator breathSliderBGAnimator;

    public Slider HealthSlider { get { return healthSlider; } }
    public Slider FoodLevelSlider { get { return foodLevelSlider; } }
    public Slider BreathLevelSlider { get { return breathLevelSlider; } }
    public CanvasGroup BreathCanvasGroup { get { return breathCanvasGroup; } }
    public Animator HealthSliderAnimator { get { return healthSliderAnimator; } }
    public Animator FoodSliderAnimator { get { return foodSliderAnimator; } }
    public Animator FoodSliderBGAnimator { get { return foodSliderBGAnimator; } }
    public Animator BreathSliderAnimator { get { return breathSliderAnimator; } }
    public Animator BreathSliderBGAnimator { get { return breathSliderBGAnimator; } }
}
