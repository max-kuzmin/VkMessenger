using FFImageLoading.Config;
using FFImageLoading.DataResolvers;
using FFImageLoading.Work;
using System.Net;
using System.Net.Http;
using Tizen.Network.Connection;

namespace ru.MaxKuzmin.VkMessenger
{
    public class ProxiedDataResolverFactory : DataResolverFactory
    {
        public override IDataResolver GetResolver(string identifier, ImageSource source, TaskParameter parameters, Configuration configuration)
        {
            if (source == ImageSource.Url)
            {
                string proxyAddress = ConnectionManager.GetProxy(AddressFamily.IPv4);
                if (!string.IsNullOrEmpty(proxyAddress))
                {
                    configuration.HttpClient = new HttpClient(new HttpClientHandler
                    {
                        Proxy = new WebProxy(proxyAddress, true)
                    });
                }
            }

            return base.GetResolver(identifier, source, parameters, configuration);
        }
    }
}
