using System.Net;
using Tizen.Network.Connection;

namespace ru.MaxKuzmin.VkMessenger
{
    public class ProxyWebClient : WebClient
    {
        public ProxyWebClient()
        {
            if (ConnectionManager.CurrentConnection.Type == ConnectionType.Ethernet)
            {
                string proxyAddress = ConnectionManager.GetProxy(AddressFamily.IPv4);
                Proxy = new WebProxy(proxyAddress, true);
            }
        }
    }
}
