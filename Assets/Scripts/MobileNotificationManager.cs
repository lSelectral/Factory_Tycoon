# if UNITY_ANDROID
using System;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MobileNotificationManager : MonoBehaviour
{
    AndroidNotificationChannel androidNotificationChannel;
    AndroidNotificationChannel andoridImportantNotificationChannel;

    AndroidNotification returnToGameNotification;
    AndroidNotification animalHungryNotification;
    AndroidNotification needLoveNotification;
    AndroidNotification storageLowNotification;

    void Start()
    {
        #region Notification Channels

        androidNotificationChannel = new AndroidNotificationChannel
        {
            Id = "default_channel",
            Name = "Default Channel",
            Description = "For standard notifications",
            Importance = Importance.Default,
            EnableVibration = true,
            EnableLights = false,
            LockScreenVisibility = LockScreenVisibility.Public,
            CanBypassDnd = false,
            CanShowBadge = true,
        };
        AndroidNotificationCenter.RegisterNotificationChannel(androidNotificationChannel);

        andoridImportantNotificationChannel = new AndroidNotificationChannel
        {
            Id = "important_channel",
            Name = "Important Channel",
            Description = "Used for important game notifications",
            Importance = Importance.High,
            EnableLights = true,
            EnableVibration = true,
            CanBypassDnd = false,
            CanShowBadge = true,
            LockScreenVisibility = LockScreenVisibility.Public,
        };
        AndroidNotificationCenter.RegisterNotificationChannel(andoridImportantNotificationChannel);

        #endregion

        AndroidNotification androidNotification = new AndroidNotification
        {
            Title = "Farm is waiting for you",
            Text = "C'mon and earn tons of money",
            LargeIcon = "icon_main_large",
            FireTime = DateTime.Now.AddSeconds(30),
        };
        int identifier = AndroidNotificationCenter.SendNotification(androidNotification, "default_channel");

        needLoveNotification = new AndroidNotification
        {
            Title = "Your farm need some love",
            Text = "Return to your farm and speed up the progress",
            LargeIcon = "icon_love_large",
            SmallIcon = "icon_love_small",
            FireTime = DateTime.Now.AddSeconds(5),
            RepeatInterval = TimeSpan.FromSeconds(3),
            SortKey = "_a",
            Group = "idle_farm"
        };
        AndroidNotificationCenter.SendNotification(needLoveNotification, "important_channel");

        storageLowNotification = new AndroidNotification
        {
            Title = "",
            Text = "",
            LargeIcon = "",
            SmallIcon = "",
            FireTime = DateTime.Now.AddSeconds(30),
            RepeatInterval = TimeSpan.FromSeconds(10),
            SortKey = "_b",
            Group = "idle_farm"
        };

        AndroidNotificationCenter.NotificationReceivedCallback notificationReceivedCallback = delegate (AndroidNotificationIntentData data)
        {
            Debug.Log("RECEIVED");
            if (Application.isPlaying && SceneManager.GetSceneByName("MainScene") == SceneManager.GetActiveScene())
            {
                PopupManager.Instance.PopupPanel("Notification","Notification received by device");
            }
        };

        AndroidNotificationCenter.OnNotificationReceived += notificationReceivedCallback;
    }

    private void OnApplicationQuit()
    {
        AndroidNotificationCenter.SendNotification(needLoveNotification, "important_channel");
    }
}
#endif
