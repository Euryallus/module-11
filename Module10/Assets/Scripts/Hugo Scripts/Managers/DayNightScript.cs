using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Handles time & day / night progression
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class DayNightScript : MonoBehaviour
{
    [SerializeField]    private GameObject SunObject;               // Ref. to sun light (e.g. DirectionalLight)
    [SerializeField]    private Material skyboxMat;                 // Ref. to skybox material
    [SerializeField]    private float timeOfDay         = 0;        // Saves time of day (0 to 24)
    [SerializeField]    private float timeProgression   = 50f;      // Time modifier
    [SerializeField]    private float cloudScrollSpeed  = 0.05f;    // Speed at which clouds move
    [SerializeField]    private float fogStartPointNight   = 5f;     // Fog density at night
    [SerializeField]    private float fogStartPointDay     = 50f;   // Fog density 
    [SerializeField]    private float ambientLightNight = 0.5f;     // Intensity of ambient light at night (default brightness of scene)
    [SerializeField]    private Color dayFogColour;                 // Colour of fog durng the day
    [SerializeField]    private Color nightFogColour;               // Colour of fog at night

    [HideInInspector]   Vector4 cloudOffset;                        // Vector used to scroll clouds

    // ### NOTE! ###
    // The skybox texture was created using a tutorial by Tim Coster (https://timcoster.com/2019/09/03/unity-shadergraph-skybox-quick-tutorial/)
    
    // "Assets/Shaders/Sky/CustomLighting.hlsl" uses code copied directly from the tutorial, as I do not know how Unity's scripted shaders work
    
    // THIS script was developed by myself, and works in tandem with the skybox to create a day / night cycle

    void Start()
    {
        // Sets default position of sun (mid day)
        SunObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        timeOfDay = 12f;

        //Resets cloud offset
        cloudOffset = new Vector4(0, 0, 0, 0);

        RenderSettings.ambientIntensity = 1.0f;
        RenderSettings.fogDensity = fogStartPointDay;
        RenderSettings.fogColor = dayFogColour;
    }

    void Update()
    {
        // Rotates sun by delta time * time multiplier
        SunObject.transform.Rotate(new Vector3(Time.deltaTime * timeProgression, 0, 0));
        timeOfDay += (Time.deltaTime * timeProgression) / 360f * 24;

        //Resets time of day (keeps time to 24 hour frame
        if(timeOfDay >= 24.0f)
        {
            timeOfDay = timeOfDay - 24.0f;
        }

        // Checks if sun is setting - if so, lerp between day & night settings for fog and light intensity
        if(timeOfDay > 20.0f)
        {
            RenderSettings.ambientIntensity =   Mathf.Lerp(RenderSettings.ambientIntensity, ambientLightNight, Time.deltaTime * (1f / timeProgression));
            RenderSettings.fogStartDistance =         Mathf.Lerp(RenderSettings.fogStartDistance, fogStartPointNight, Time.deltaTime * (1f / timeProgression));
            RenderSettings.fogColor =           Color.Lerp(RenderSettings.fogColor, nightFogColour, Time.deltaTime * (1f / timeProgression));
        }
        // Checks if sun is rising - if so, lerp between night and day settings for fog and light intensity
        if(timeOfDay < 6.0f)
        {
            RenderSettings.ambientIntensity =   Mathf.Lerp(RenderSettings.ambientIntensity, 1.0f, Time.deltaTime * (1f / timeProgression));
            RenderSettings.fogDensity =         Mathf.Lerp(RenderSettings.fogStartDistance, fogStartPointDay, Time.deltaTime * (1f / timeProgression));
            RenderSettings.fogColor =           Color.Lerp(RenderSettings.fogColor, dayFogColour, Time.deltaTime * (1f / timeProgression));
        }
        // If sun has risen, "snap" all values to what they should be during the day
        if(timeOfDay > 6.0f && timeOfDay < 20.0f)
        {
            RenderSettings.ambientIntensity =   1.0f;
            RenderSettings.fogDensity =         fogStartPointDay;
            RenderSettings.fogColor =           dayFogColour;
        }

        // Increases cloud ofset (causes movement)
        cloudOffset.y += Time.deltaTime * cloudScrollSpeed;
        // Updates cloud vector in Skybox texture
        skyboxMat.SetVector("_CloudOffset", cloudOffset);

    }
}
