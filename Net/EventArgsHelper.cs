using System;
using System.ComponentModel;
using System.Net;

namespace Leayal.Net
{
    internal static class EventArgsHelper
    {
        internal static DownloadProgressChangedEventArgs GetDownloadProgressChangedEventArgs(object userToken, long bytesReceived, long totalBytesToReceive)
        {
            return GetDownloadProgressChangedEventArgs((int)((100F * bytesReceived) / totalBytesToReceive), userToken, bytesReceived, totalBytesToReceive);
        }

        internal static DownloadProgressChangedEventArgs GetDownloadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive)
        {
            return (DownloadProgressChangedEventArgs)Activator.CreateInstance(typeof(DownloadProgressChangedEventArgs),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new object[] { progressPercentage, userToken, bytesReceived, totalBytesToReceive }, null);
        }

        internal static DownloadDataCompletedEventArgs GetDownloadDataCompletedEventArgs(byte[] bytes, System.Exception ex, bool cancelled, object usertoken)
        {
            return (DownloadDataCompletedEventArgs)Activator.CreateInstance(typeof(DownloadDataCompletedEventArgs),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new object[] { bytes, ex, cancelled, usertoken }, null);
        }

        internal static DownloadStringCompletedEventArgs GetDownloadStringCompletedEventArgs(string str, System.Exception ex, bool cancelled, object usertoken)
        {
            return (DownloadStringCompletedEventArgs)Activator.CreateInstance(typeof(DownloadStringCompletedEventArgs),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new object[] { str, ex, cancelled, usertoken }, null);
        }

        private static System.Reflection.FieldInfo prop_BytesReceived, prop_TotalBytesToReceive, prop_ProgressPercentage;
        internal static void SetBytesReceived(this DownloadProgressChangedEventArgs eventArgs, long value)
        {
            if (prop_BytesReceived == null)
                prop_BytesReceived = eventArgs.GetType().GetField("m_BytesReceived", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop_BytesReceived != null)
                prop_BytesReceived.SetValue(eventArgs, value);
        }

        internal static void SetTotalBytesToReceive(this DownloadProgressChangedEventArgs eventArgs, long value)
        {
            if (prop_TotalBytesToReceive == null)
                prop_TotalBytesToReceive = eventArgs.GetType().GetField("m_TotalBytesToReceive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop_TotalBytesToReceive != null)
                prop_TotalBytesToReceive.SetValue(eventArgs, value);
        }

        internal static void SetProgressPercentage(this ProgressChangedEventArgs eventArgs, int value)
        {
            if (value < 0 || value > 100)
                throw new InvalidOperationException();
            if (prop_ProgressPercentage == null)
                prop_ProgressPercentage = eventArgs.GetType().GetField("progressPercentage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop_ProgressPercentage != null)
                prop_ProgressPercentage.SetValue(eventArgs, value);
            //progressPercentage
        }

        internal static void CalculateProgressPercentage(this DownloadProgressChangedEventArgs eventArgs)
        {
            if (prop_ProgressPercentage == null)
                prop_ProgressPercentage = eventArgs.GetType().GetField("progressPercentage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop_ProgressPercentage != null)
                if (eventArgs.TotalBytesToReceive > 0)
                    prop_ProgressPercentage.SetValue(eventArgs, System.Convert.ToInt32((eventArgs.BytesReceived * 100d) / eventArgs.TotalBytesToReceive));
            //progressPercentage
        }
    }
}
