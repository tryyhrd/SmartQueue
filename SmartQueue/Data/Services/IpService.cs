using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SmartQueue.Data.Services
{
    public class IpService
    {
        private static readonly string[] IgnoredAdapterNames = new[]
        {
            "radmin", "hamachi", "virtualbox", "vmware", "hyper-v",
            "bluetooth", "tun", "tap", "docker", "wsl", "zero tier"
        };
        public string GetIpAddress()
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var adapter in nics)
            {
                if (adapter.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                if (IgnoredAdapterNames.Any(keyword =>
                    adapter.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    adapter.Description.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    continue;
                }

                var ipProperties = adapter.GetIPProperties();

                foreach (var unicast in ipProperties.UnicastAddresses)
                {
                    var ip = unicast.Address;

                    if (ip.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(ip))
                        continue;

                    return ip.ToString();
                }
            }

            return "localhost";
        }
    }
}
