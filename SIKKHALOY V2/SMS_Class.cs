using SmsService;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Education
{
    public class SMS_Class
    {
        // private static string url = "http://loopsitbd.powersms.net.bd/httpapi/";
        // private static string userId = "Sikkhaloy";
        // private static string password = "Sikkhaloy@SMS_345";
        private SmsServiceBuilder SmsService { get; }

        public int SMSBalance { get; }

        private readonly SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

        public SMS_Class(string schoolId)
        {
            var getSmsBalanceCmd = new SqlCommand("SELECT SMS_Balance FROM SMS WHERE (SchoolID = @SchoolID)", _con);
            getSmsBalanceCmd.Parameters.AddWithValue("@SchoolID", schoolId);

            var smsProviderCmd = new SqlCommand("SELECT TOP(1) SmsProvider FROM  SikkhaloySetting", _con);


            _con.Open();
            this.SMSBalance = Convert.ToInt32(getSmsBalanceCmd.ExecuteScalar());
            var smsProvider = smsProviderCmd.ExecuteScalar().ToString();
            _con.Close();

            var isProviderExist = Enum.TryParse<ProviderEnum>(smsProvider, out var provider);

            SmsService = isProviderExist ? new SmsServiceBuilder(provider) : new SmsServiceBuilder(ProviderEnum.BanglaPhone);
        }

        public Get_Validation SMS_Validation(string number, string text)
        {
            var isValid = true;
            var validationMessage = "";

            if (!SmsValidator.IsValidNumber(number))
            {
                isValid = false;
                validationMessage += "Invalid Mobile Number";
            }


            //if (!Enumerable.Range(1, 450).Contains(Text.Length))
            //{
            //    IsValid = false;
            //    Validation_Message += "SMS Text Length Out Of Range. Maximum Size Is (450 character) ";
            //}

            if (isValid)
            {
                validationMessage = "Valid";
            }

            var v = new Get_Validation
            {
                Validation = isValid,
                Message = validationMessage
            };

            return v;
        }

        public Guid SMS_Send(string number, string text, string smsPurpose)
        {
            var smsSendId = Guid.NewGuid();
            var responseMessage = "";
            var isError = false;

            // try
            // {
            //     var url_Action = "sendsms"; // your powersms site url; register the ip first
            //     var request = HttpWebRequest.Create(url + url_Action);
            //
            //     var smsText = Uri.EscapeDataString(Text);
            //     var receiversParam = Number;
            //     var dataFormat = "userId={0}&password={1}&smsText={2}&commaSeperatedReceiverNumbers={3}";
            //     var urlEncodedData = string.Format(dataFormat, userId, password, smsText, receiversParam);
            //     var data = Encoding.ASCII.GetBytes(urlEncodedData);
            //
            //     request.Method = "post";
            //     request.Proxy = null;
            //     request.ContentType = "application/x-www-form-urlencoded";
            //     request.ContentLength = data.Length;
            //
            //     using (var requestStream = request.GetRequestStream())
            //     {
            //         requestStream.Write(data, 0, data.Length);
            //     }
            //
            //     HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //
            //     // Get the response stream
            //     StreamReader reader = new StreamReader(response.GetResponseStream());
            //
            //     dynamic Json_Obj = JsonConvert.DeserializeObject(reader.ReadToEnd());
            //     if (Json_Obj.isError == "False")
            //     {
            //         //ShowLabel.Text = Json_Obj.message + Json_Obj.isError + Json_Obj.insertedSmsIds;
            //         responseMessage = Json_Obj.message;
            //     }
            // }
            // catch
            // {
            //     isError = true;
            // }

            responseMessage = SmsService.SendSms(text, number);
            isError = !SmsService.IsSuccess;
            if (!isError)
            {
                var sendRecordCmd = new SqlCommand("INSERT INTO [SMS_Send_Record] ([SMS_Send_ID], [PhoneNumber], [TextSMS], [TextCount], [SMSCount], [PurposeOfSMS], [Status], [Date], [SMS_Response]) VALUES (@SMS_Send_ID, @PhoneNumber, @TextSMS, @TextCount, @SMSCount, @PurposeOfSMS, @Status, Getdate(), @SMS_Response)", _con);
                sendRecordCmd.Parameters.AddWithValue("@SMS_Send_ID", smsSendId);
                sendRecordCmd.Parameters.AddWithValue("@PhoneNumber", number);
                sendRecordCmd.Parameters.AddWithValue("@TextSMS", text);
                sendRecordCmd.Parameters.AddWithValue("@TextCount", text.Length);
                sendRecordCmd.Parameters.AddWithValue("@SMSCount", SMS_Conut(text));
                sendRecordCmd.Parameters.AddWithValue("@PurposeOfSMS", smsPurpose);
                sendRecordCmd.Parameters.AddWithValue("@Status", "Sent");
                sendRecordCmd.Parameters.AddWithValue("@SMS_Response", responseMessage);

                _con.Open();
                sendRecordCmd.ExecuteNonQuery();
                _con.Close();

                return smsSendId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public int SMS_Conut(string text)
        {
            // string gsm7bitChars = "@£$¥èéùìòÇ\\nØø\\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\\\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà";
            // string gsm7bitExChar = "\\^{}\\\\\\[~\\]|€";
            //
            // Regex gsm7bitRegExp = new Regex(@"^[" + gsm7bitChars + "]*$");
            // Regex gsm7bitExRegExp = new Regex(@"^[" + gsm7bitChars + gsm7bitExChar + "]*$");
            // Regex gsm7bitExOnlyRegExp = new Regex(@"^[\\" + gsm7bitExChar + "]*$");
            //
            // string sms = Text.Replace(Environment.NewLine, " ").Trim();
            // int _results = 0;
            //
            // for (int ctr = 0; ctr <= sms.Length - 1; ctr++)
            // {
            //     if (gsm7bitExOnlyRegExp.IsMatch(sms[ctr].ToString()))
            //     {
            //         _results++;
            //     }
            // }
            //
            // double SMS_Count = 0;
            // double SMS_Length = 0;
            //
            // if (gsm7bitRegExp.IsMatch(sms))
            // {
            //     SMS_Length = sms.Length + _results;
            //     if (SMS_Length > 160)
            //     {
            //         SMS_Count = Math.Ceiling(SMS_Length / 153);
            //     }
            //     else
            //     {
            //         SMS_Count = 1;
            //     }
            // }
            // else if (gsm7bitExRegExp.IsMatch(sms))
            // {
            //     SMS_Length = sms.Length + _results;
            //     if (SMS_Length > 160)
            //     {
            //         SMS_Count = Math.Ceiling(SMS_Length / 153);
            //     }
            //     else
            //     {
            //         SMS_Count = 1;
            //     }
            // }
            // else
            // {
            //     SMS_Length = sms.Length;
            //     if (SMS_Length > 70)
            //     {
            //         SMS_Count = Math.Ceiling(SMS_Length / 67);
            //     }
            //     else
            //     {
            //         SMS_Count = 1;
            //     }
            // }

            // return (int)SMS_Count;
            return SmsValidator.TotalSmsCount(text);
        }

        public int SMS_Length(string Text)
        {
            string gsm7bitChars = "@£$¥èéùìòÇ\\nØø\\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\\\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà";
            string gsm7bitExChar = "\\^{}\\\\\\[~\\]|€";

            Regex gsm7bitRegExp = new Regex(@"^[" + gsm7bitChars + "]*$");
            Regex gsm7bitExRegExp = new Regex(@"^[" + gsm7bitChars + gsm7bitExChar + "]*$");
            Regex gsm7bitExOnlyRegExp = new Regex(@"^[\\" + gsm7bitExChar + "]*$");

            string sms = Text.Replace(Environment.NewLine, " ").Trim();
            int _results = 0;

            for (int ctr = 0; ctr <= sms.Length - 1; ctr++)
            {
                if (gsm7bitExOnlyRegExp.IsMatch(sms[ctr].ToString()))
                {
                    _results++;
                }
            }

            int sms_count = 0;
            if (gsm7bitRegExp.IsMatch(sms))
            {
                sms_count = sms.Length + _results;
            }
            else if (gsm7bitExRegExp.IsMatch(sms))
            {
                sms_count = sms.Length + _results;
            }
            else
            {
                sms_count = sms.Length;
            }

            return sms_count;
        }

        public int SMS_GetBalance()
        {
            // var url_Action = "accountinfo";
            // var info = "package";
            // var dataFormat = "?userId={0}&password={1}&info={2}";
            // var url_Data = string.Format(dataFormat, userId, password, info);
            // int Balance = 0;
            //
            // try
            // {
            //     Uri address = new Uri(url + url_Action + url_Data);
            //
            //     // Create the web request
            //     HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
            //
            //     // Set type to POST
            //     request.Method = "GET";
            //     request.ContentType = "text/xml";
            //
            //     HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //
            //     // Get the response stream
            //     StreamReader reader = new StreamReader(response.GetResponseStream());
            //
            //     dynamic Json_Obj = JsonConvert.DeserializeObject(reader.ReadToEnd());
            //
            //     Balance += (int)Json_Obj.AvailableExternalSmsCount;
            // }
            // catch
            // {
            //     Balance = 0;
            // }
            // return Balance;

            return SmsService.SmsBalance();
        }
    }

    public class Get_Validation
    {
        public bool Validation { get; set; }
        public string Message { get; set; }
    }
}