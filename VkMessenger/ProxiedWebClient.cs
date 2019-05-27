using System.Net;
using Tizen.Network.Connection;

namespace ru.MaxKuzmin.VkMessenger
{
    public class ProxiedWebClient : WebClient
    {
        public ProxiedWebClient()
        {
            if (ConnectionManager.CurrentConnection.Type == ConnectionType.Bluetooth)
            {
                string proxyAddress = ConnectionManager.GetProxy(AddressFamily.IPv4);
                Proxy = new WebProxy(proxyAddress, true);
            }
        }
    }
}
