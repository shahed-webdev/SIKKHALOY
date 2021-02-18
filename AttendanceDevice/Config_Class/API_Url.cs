using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    public static class ApiUrl
    {
        public static readonly string EndPoint = "http://localhost:19362/";
        public async static Task<bool> CheckInterNet()
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
