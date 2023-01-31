using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace SmsService
{
    public class SmsProviderMram : ISmsProvider
    {
        private const string HostUrl = "https://api.greenweb.com.bd/api.php?json";
        private const string ApiKey = "90282210471675095047ee665e3d0ba098844814cab35e133dc4";
        public int GetSmsBalance()
        {
            // Create Url
            const string actionUrl = "/getBalance";

            var address = new Uri(HostUrl + ApiKey + actionUrl);

            // Create the web request
            var request = WebRequest.Create(address) as HttpWebRequest;

            // Set type to POST
            request.Method = "GET";
            request.ContentType = "text/xml";

            try
            {
                using (var response = request.GetResponse())
                {
                    dynamic responseObject = ParseResponse(response);

                    if (responseObject.isError == "true")
                    {
                        throw new Exception(string.Format("Sms Sending was failed. Because: {0}",
                            responseObject.message));
                    }
                    else
                    {
                        // Get the response stream
                        return (int)responseObject.AvailableExternalSmsCount;
                    }
                }
            }
            catch (WebException e)
            {
                dynamic responseObject = ParseResponse(e.Response);

                if (responseObject.isError == "true")
                {
                    throw new Exception("Sms Sending was failed. Because: " + responseObject.message);
                }
            }

            return 0;

        }

        public string SendSms(string massage, string number)
        {
            const string actionUrl = ""; // your powers ms site url; register the ip first
            var request = HttpWebRequest.Create(HostUrl + actionUrl);
            var smsText = Uri.EscapeDataString(massage);
            var receiversParam = number;
            var dataFormat = "token={0}&smsdata={1}";


            var urlEncodedData = string.Format(dataFormat, ApiKey, smsText);
            var data = Encoding.ASCII.GetBytes(urlEncodedData);

            request.Method = "post";
            request.Proxy = null;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;


            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            try
            {
                using (var response = request.GetResponse())
                {
                    dynamic responseObject = ParseResponse(response);

                    if (responseObject.isError == "true")
                    {
                        throw new Exception(string.Format("Sms Sending was failed. Because: {0}",
                            responseObject.message));
                    }
                    else
                    {
                        return responseObject.insertedSmsIds;
                    }
                }
            }
            catch (WebException e)
            {
                dynamic responseObject = ParseResponse(e.Response);

                if (responseObject.isError == "true")
                {
                    throw new Exception("Sms Sending was failed. Because: " + responseObject.message);
                }
            }

            return string.Empty;
        }

        private static object ParseResponse(WebResponse r)
        {
            var response = (HttpWebResponse)r;

            var responseStream = response.GetResponseStream();

            if (responseStream == null) throw new Exception("Response stream found null.");

            using (var responseReader = new StreamReader(responseStream))
            {
                var responseString = responseReader.ReadToEnd();

                try
                {
                    return JsonConvert.DeserializeObject(responseString);
                }
                catch
                {
                    throw new Exception(
                        $"The sms service calling was unsuccessful with code:{(int)response.StatusCode}[{response.StatusCode}]");
                }
            }
        }
    }
}