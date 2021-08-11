using UnityEngine;
using TMPro;

// ||=======================================================================||
// || PortalUnlockPanel: A UI popup shown when a portal is unlocked.        ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/PortalUnlockPanel                              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class PortalUnlockPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI    unlockText;         // Text displaying which portal was unlocked
    [SerializeField] private GameObject         infoTextGameObj;    // Text that tells the player where they can find the portal

    public void Setup(string unlockAreaName, bool showInfoText)
    {
        // Sets up panel text with the given values

        unlockText.text = unlockAreaName + " Portal Unlocked!";
        infoTextGameObj.SetActive(showInfoText);
    }
}