using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

// All possible types of messages that can be shown as a notification
public enum NotificationMessageType
{
    PlayerTooFull,

    ItemRequiredForDoor,
    DoorUnlocked,
    CantOpenDoorManually,

    ItemCannotBePlaced,
    CantDestroyObject,

    CantAffordItem,

    SaveSuccess,
    AutoSaveSuccess,
    SaveError
}

// ||=======================================================================||
// || NotificationManager: Queues notifications and displays them with an   ||
// ||   animated UI popup.                                                  ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/NotificationManager                            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private GameObject prefabNotificationPanel;

    #endregion

    private Transform                   notificationParentTransform;    // The transform to use as a parent for notification UI elements
    private GameObject                  activeNotificationGameObj;      // The notification GameObject currently being shown, if any
    private QueuedNotification          activeNotification;             // The notification currently being shown, if any
    private Queue<QueuedNotification>   queuedNotifications;            // All notifications that are queued, ready to be shown

    // Dictionary that defines which text will be shown for each notification message type
    //   Note: '*' symbols will be replaced with parameters that are passed when a notification is first queued
    private readonly Dictionary<NotificationMessageType, string> notificationTextDict = new Dictionary<NotificationMessageType, string>()
    {
        { NotificationMessageType.PlayerTooFull,       "You're too full to eat that!" },

        { NotificationMessageType.ItemRequiredForDoor, "* is required to unlock this door." },
        { NotificationMessageType.DoorUnlocked,        "The door was unlocked with *" },
        { NotificationMessageType.CantOpenDoorManually,"This door cannot be opened or closed manually." },

        { NotificationMessageType.ItemCannotBePlaced,  "The held item cannot be placed there." },
        { NotificationMessageType.CantDestroyObject,   "This object cannot be destroyed." },

        { NotificationMessageType.CantAffordItem,      "You cannot purchase this item - * * required." },

        { NotificationMessageType.SaveSuccess,         "Progress saved successfully.\nSpawn point set." },
        { NotificationMessageType.AutoSaveSuccess,     "Your progress has been auto-saved.\nSpawn point set." },
        { NotificationMessageType.SaveError,           "Error: Progress could not be saved." }
    };

    private void Awake()
    {
        // Ensure that an instance of the class does not already exist
        if (Instance == null)
        {
            // Set this class as the instance and ensure that it stays when changing scenes
            Instance = this;
        }
        // If there is an existing instance that is not this, destroy the GameObject this script is connected to
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        queuedNotifications = new Queue<QueuedNotification>();
    }

    private void Start()
    {
        // Find the notifications parent
        notificationParentTransform = GameObject.FindGameObjectWithTag("JoeCanvas").transform.Find("NotificationsParent");
    }

    private void Update()
    {
        if(activeNotificationGameObj == null)
        {
            // No notification is currently being shown
            activeNotification = null;

            if(queuedNotifications.Count > 0)
            {
                // There is at least one notification in the queue, show it
                ShowNotification(queuedNotifications.Dequeue());
            }
        }
    }

    public void AddNotificationToQueue(NotificationMessageType messageType, string[] parameters = null, string soundName = "notification1")
    {
        // Create a queued notification with the given message/parameters
        QueuedNotification notificationToAdd = new QueuedNotification()
        {
            MessageType = messageType,
            Parameters = parameters,
            SoundName = soundName
        };

        if(activeNotification != null && NotificationsAreTheSame(notificationToAdd, activeNotification))
        {
            // The notification already being shown matches the one being added, no need to add it
            return;
        }

        for (int i = 0; i < queuedNotifications.Count; i++)
        {
            if(NotificationsAreTheSame(notificationToAdd, queuedNotifications.ElementAt(i)))
            {
                // A matching notification is already in the queue, no need to add this one
                return;
            }
        }

        // There are no matching notifications already showing/queued, add the new one to the queue
        queuedNotifications.Enqueue(notificationToAdd);
    }

    private void ShowNotification(QueuedNotification notification)
    {
        if (notificationTextDict.ContainsKey(notification.MessageType))
        {
            // Instantiate the notification UI popup
            GameObject notificationGameObj = Instantiate(prefabNotificationPanel, notificationParentTransform);

            // Get the text to be shown
            string textToShow = notificationTextDict[notification.MessageType];

            if (notification.Parameters != null)
            {
                for (int i = 0; i < notification.Parameters.Length; i++)
                {
                    // This notification has custom parameters, replace each '*' symbol with a parameter
                    int replaceIndex = textToShow.IndexOf("*");

                    textToShow = textToShow.Remove(replaceIndex, 1);
                    textToShow = textToShow.Insert(replaceIndex, notification.Parameters[i]);
                }
            }

            // Set the text on the notification GameObject
            notificationGameObj.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = textToShow;

            // Play a sound if one was set
            if (!string.IsNullOrEmpty(notification.SoundName))
            {
                AudioManager.Instance.PlaySoundEffect2D(notification.SoundName);
            }

            // This is now the active notification
            activeNotificationGameObj   = notificationGameObj;
            activeNotification          = notification;
        }
        else
        {
            Debug.LogError("Notification text type not added to dictionary: " + notification.MessageType);
        }
    }

    private bool NotificationsAreTheSame(QueuedNotification notification1, QueuedNotification notification2)
    {
        if(notification1.MessageType == notification2.MessageType)
        {
            // Notifications have the same message type, parameters also need to be compared

            if(notification1.Parameters != null && notification2.Parameters != null)
            {
                // Find the minimum number of parameters shared between each notification
                int parameterCount = Mathf.Min(notification1.Parameters.Length, notification2.Parameters.Length);

                // Loop through shared parameters
                for (int i = 0; i < parameterCount; i++)
                {
                    if(notification1.Parameters[i] != notification2.Parameters[i])
                    {
                        // Different parameter found, notifications are not the same
                        return false;
                    }
                }

                // Notifications have the same parameters and message
                return true;
            }
            else
            {
                // Same message, both have no parameters
                return true;
            }
        }
        
        // Different messages, notifications are not the same
        return false;
    }
}

// Data for notifications that have been queued
public class QueuedNotification
{
    public NotificationMessageType  MessageType;    // The message type to be displayed
    public string[]                 Parameters;     // Array of strings to replace any '*' symbols in the message text
    public string                   SoundName;      // Name of the sound effect to play when the notification is shown
}
