using EFT.Communications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

            var o = MessageNotificationType.GetMethod("DisplayMessageNotification", BindingFlags.Static | BindingFlags.Public);
            if (o != null)
            {
                o.Invoke("DisplayMessageNotification", new object[] { message, ENotificationDurationType.Default, ENotificationIconType.Default, null });
            }

        }
    }
}
