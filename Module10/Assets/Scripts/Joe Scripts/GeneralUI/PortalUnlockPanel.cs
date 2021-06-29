using UnityEngine;
using TMPro;

public class PortalUnlockPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI    unlockText;
    [SerializeField] private GameObject         infoTextGameObj;

    public void Setup(string unlockAreaName, bool showInfoText)
    {
        unlockText.text = unlockAreaName + " Portal Unlocked!";
        infoTextGameObj.SetActive(showInfoText);
    }
}