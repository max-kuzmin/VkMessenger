using System;
using System.Net;
using System.Threading.Tasks;
using Tizen.Network.Connection;

namespace ru.MaxKuzmin.VkMessenger
{
    public static class Network
    {
        public static event EventHandler OnConnected;

        private static bool isConnected = false;
        private static bool isWaiting = false;
        private static bool isStarted = false;

        /// <summary>
        /// Helper for stopping network by throwing web exception
        /// </summary>
        public static void ThrowIfDisconnected()
        {
            if (!isConnected)
                throw new WebException();
        }

        /// <summary>
        /// Stop networking. Called when app paused
        /// </summary>
        public static void Stop()
        {
            isConnected = false;
            isWaiting = false;
            isStarted = false;
        }

        /// <summary>
        /// Start networking. Called when app started or resumed
        /// </summary>
        public static void Start()
        {
            isStarted = true;
            StartWaiting();
        }

        /// <summary>
        /// Start waiting for reconnect
        /// </summary>
        public static async void StartWaiting()
        {
            if (isWaiting || !isStarted)
                return;

            isConnected = false;
            isWaiting = true;

            while (isWaiting)
            {
                if (ConnectionManager.CurrentConnection.State == ConnectionState.Connected)
                {
                    isConnected = true;
                    isWaiting = false;
                    OnConnected?.Invoke(null, new EventArgs());
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }
}
