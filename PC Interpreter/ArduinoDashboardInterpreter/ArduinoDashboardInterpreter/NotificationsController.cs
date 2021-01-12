using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter
{
    public class NotificationsController
    {
        List<NotificationType> notificationList = new List<NotificationType>();
        NotificationType lastNotification = NotificationType.Off;
        List<NotificationType> lockedNotificationList = new List<NotificationType>();

        public delegate void NewNotificationAddedDelegate(NotificationType notification, NotificationPriority priority);
        public event NewNotificationAddedDelegate NewNotificationAdded;

        public delegate void CurrentNotificationChangedDelegate(NotificationType notification, NotificationPriority priority);
        public event CurrentNotificationChangedDelegate CurrentNotificationChanged;
    
        public enum NotificationType
        {
            //
            // NOTIFICATION COLOR IDS:
            //
            //     0 - OFF
            //  1-10 - BLUE INFO
            // 11-20 - ORANGE WARNING
            // 21+   - RED ALERT
            Off = 0,
            // INFO

            // WARNING

            // ALERT
            BrakeLowPressure = 21
        }

        public enum NotificationPriority
        {
            None,
            Info,
            Warning,
            Alert
        }

        public NotificationPriority GetNotificationPriority(NotificationType notification)
        {
            int id = (int)notification;
            if (id == 0) return NotificationPriority.None;
            if (id <= 10) return NotificationPriority.Info;
            if (id <= 20) return NotificationPriority.Warning;
            if (id > 20) return NotificationPriority.Alert;
            return NotificationPriority.None;
        }

        public NotificationType GetCurrentNotification()
        {
            NotificationType id = NotificationType.Off;
            NotificationPriority priority = NotificationPriority.None;
            foreach(NotificationType itemId in notificationList)
            {
                NotificationPriority itemPriority = GetNotificationPriority(itemId);
                if ((int)itemPriority > (int)priority)
                {
                    id = itemId;
                    priority = itemPriority;
                }
            }
            if (lastNotification != id) CurrentNotificationChanged?.Invoke(id, priority);
            lastNotification = id;
            return id;
        }

        private bool NotificationIsSelected(NotificationType notification)
        {
            foreach (NotificationType item in notificationList) if (item == notification) return true;
            return false;
        }

        private bool NotificationIsLocked(NotificationType notification)
        {
            foreach (NotificationType item in lockedNotificationList) if (item == notification) return true;
            return false;
        }

        public bool SetNotification(NotificationType notification, bool enabled)
        {
            if (enabled)
            {
                if (NotificationIsSelected(notification) || NotificationIsLocked(notification)) return false;
                notificationList.Add(notification);
                NewNotificationAdded?.Invoke(notification, GetNotificationPriority(notification));
                return true;
            }
            else
            {
                if (!NotificationIsSelected(notification)) return false;
                notificationList.Remove(notification);
                return true;
            }
        }

        public bool SetNotificationLock(NotificationType notification, bool lockEnabled)
        {
            if (lockEnabled)
            {
                if (NotificationIsLocked(notification)) return false;
                lockedNotificationList.Add(notification);
                return true;
            }
            else
            {
                if (!NotificationIsLocked(notification)) return false;
                lockedNotificationList.Remove(notification);
                return true;
            }
        }
    }
}
