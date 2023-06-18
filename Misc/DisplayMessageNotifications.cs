using EFT.Communications;
using SIT.Core.Configuration;
using System;
using System.Reflection;

namespace SIT.Core.Misc
{
    internal static class DisplayMessageNotifications
    {
        public static Type MessageNotificationType { get; set; }

        public static void DisplayMessageNotification(string message)
        {
            if (MessageNotificationType == null)
            {
                return;
            }

            if (PluginConfigSettings.Instance != null)
            {
                if (!PluginConfigSettings.Instance.CoopSettings.SETTING_ShowFeed)
                {
                    // if the user has disabled the feed, prevent display message logging
                    return;
                }
            }

            var o = MessageNotificationType.GetMethod("DisplayMessageNotification", BindingFlags.Static | BindingFlags.Public);
            if (o != null)
            {
                o.Invoke("DisplayMessageNotification", new object[] { message, ENotificationDurationType.Default, ENotificationIconType.Default, null });
            }

        }
    }
}
