using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDashboardInterpreter
{
    public class NotificationsController
    {
        const int NOTIFICATION_DURATION = 5;

        List<NotificationType> notificationList = new List<NotificationType>();
        NotificationType lastNotification = NotificationType.Off;
        List<NotificationType> lockedNotificationList = new List<NotificationType>();
        DateTime lastNotificationSwitch = DateTime.Now;

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
            TrailerAttached = 1,
            ShortDeliveryTime = 2,
            NewJob = 3,
            CargoDelivered = 4,
            Tollgate = 5,
            TruckRefueled = 6,
            // WARNING
            RestNeeded = 11,
            LowLevelOfFuel = 12,
            VehicleDamaged = 13,
            TrailerDamaged = 14,
            Fined = 15,
            // ALERT
            LowBrakePressure = 21,
            BrakesLocked = 22,
            RestTimeLimitExceeded = 23,
            ReleaseHandbrake = 24
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
            if (lastNotification != NotificationType.Off && (DateTime.Now - lastNotificationSwitch).TotalSeconds > NOTIFICATION_DURATION)
            {
                TurnOffNotification(lastNotification, true);
                lastNotification = NotificationType.Off;
            }
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
            if (lastNotification != id)
            {
                lastNotificationSwitch = DateTime.Now;
                CurrentNotificationChanged?.Invoke(id, priority);
            }
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

        public void ResetNotificationSystem()
        {
            lastNotification = NotificationType.Off;
            notificationList.Clear();
            lockedNotificationList.Clear();
        }

        public bool TurnOnNotification(NotificationType notification, bool condition, bool lockTheNotification = true)
        {
            if (!condition) return false;
            if (NotificationIsSelected(notification) || NotificationIsLocked(notification)) return false;
            notificationList.Add(notification);
            if (lockTheNotification) LockTheNotification(notification, true);
            NewNotificationAdded?.Invoke(notification, GetNotificationPriority(notification));
            return true;
        }

        public bool TurnOffNotification(NotificationType notification, bool condition)
        {
            if (!condition || !NotificationIsSelected(notification)) return false;
            notificationList.Remove(notification);
            return true;
        }

        public bool LockTheNotification(NotificationType notification, bool condition)
        {
            if (!condition || NotificationIsLocked(notification)) return false;
            lockedNotificationList.Add(notification);
            return true;
        }

        public bool UnlockTheNotification(NotificationType notification, bool condition)
        {
            if (!condition || !NotificationIsLocked(notification)) return false;
            lockedNotificationList.Remove(notification);
            return true;
        }
    }
}
