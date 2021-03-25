using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    public static class ApiUrl
    {
        public static readonly string EndPoint = "http://localhost:19362/";
      
        public static async Task<bool> IsServerUnavailable()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }

            try
            {
                using (var client = new WebClient())

                using (await client.OpenReadTaskAsync(EndPoint))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }


        public static async Task<bool> IsNoNetConnection()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }

            try
            {
                using (var client = new WebClient())

                using (await client.OpenReadTaskAsync("https://google.com"))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }
    }
}
