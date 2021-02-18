using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    public static class Device_PingTest
    {
        public async static Task<bool> PingHostAsync(string nameOrAddress)
        {
            bool pingable = false;

            try
            {
                using (Ping pinger = new Ping())
                {
                    PingReply reply = await pinger.SendPingAsync(nameOrAddress, LocalData.Instance.institution.PingTimeOut);
                    pingable = reply.Status == IPStatus.Success;
                }
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }

            return pingable;
        }

        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;

            try
            {
                using (Ping pinger = new Ping())
                {
                    PingReply reply = pinger.Send(nameOrAddress, LocalData.Instance.institution.PingTimeOut);
                    pingable = reply.Status == IPStatus.Success;
                }
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }

            return pingable;
        }
    }
}
